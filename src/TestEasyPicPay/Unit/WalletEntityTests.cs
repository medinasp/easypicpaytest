using Xunit;
using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;

namespace TestEasyPicPay.Unit;

/// <summary>
/// Testes unitários da WalletEntity.
/// Foco: Lógica de domínio isolada, SEM banco de dados.
/// </summary>
public class WalletEntityTests
{
    #region Testes de Debit/Credit

    [Fact]
    public void Debit_WithSufficientBalance_DecreasesBalance()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);
        wallet.Credit(100m);

        // Act
        wallet.Debit(30m);

        // Assert
        Assert.Equal(70m, wallet.Balance);
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);
        wallet.Credit(50m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => wallet.Debit(100m));
        Assert.Equal("Saldo insuficiente", exception.Message);
    }

    [Fact]
    public void Debit_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);
        wallet.Credit(100m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wallet.Debit(-10m));
    }

    [Fact]
    public void Debit_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wallet.Debit(0m));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50.50)]
    [InlineData(1000)]
    public void Credit_WithValidAmount_IncreasesBalance(decimal amount)
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);

        // Act
        wallet.Credit(amount);

        // Assert
        Assert.Equal(amount, wallet.Balance);
    }

    [Fact]
    public void Credit_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wallet.Credit(-10m));
    }

    #endregion

    #region Testes de CanSendMoney

    [Fact]
    public void CanSendMoney_CommonUser_ReturnsTrue()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Common);

        // Act
        var canSend = wallet.CanSendMoney();

        // Assert
        Assert.True(canSend);
    }

    [Fact]
    public void CanSendMoney_Merchant_ReturnsFalse()
    {
        // Arrange
        var wallet = CreateValidWallet(UserType.Merchant);

        // Act
        var canSend = wallet.CanSendMoney();

        // Assert
        Assert.False(canSend);
    }

    #endregion

    #region Testes de Validação no Construtor

    [Fact]
    public void Constructor_WithValidData_CreatesWallet()
    {
        // Act
        var wallet = CreateValidWallet(UserType.Common);

        // Assert
        Assert.NotNull(wallet);
        Assert.Equal("João Silva", wallet.Name);
        Assert.Equal("12345678901", wallet.IdTaxDoc);
        Assert.Equal("joao@test.com", wallet.Email);
        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new WalletEntity("", "12345678901", "joao@test.com", "hash123", UserType.Common)
        );
        Assert.Equal("Nome é obrigatório", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new WalletEntity("   ", "12345678901", "joao@test.com", "hash123", UserType.Common)
        );
    }

    [Theory]
    [InlineData("123")]           // Muito curto
    [InlineData("12345")]         // Muito curto
    [InlineData("123456789")]     // 9 dígitos
    [InlineData("1234567890")]    // 10 dígitos
    [InlineData("123456789012")]  // 12 dígitos
    [InlineData("12345678901234567")] // Muito longo
    public void Constructor_WithInvalidCpfCnpj_ThrowsArgumentException(string invalidDoc)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new WalletEntity("João Silva", invalidDoc, "joao@test.com", "hash123", UserType.Common)
        );
        Assert.Equal("CPF/CNPJ inválido", exception.Message);
    }

    [Theory]
    [InlineData("12345678901")]   // CPF válido (11 dígitos)
    [InlineData("12345678000190")] // CNPJ válido (14 dígitos)
    public void Constructor_WithValidCpfCnpj_CreatesWallet(string validDoc)
    {
        // Act
        var wallet = new WalletEntity("João Silva", validDoc, "joao@test.com", "hash123", UserType.Common);

        // Assert
        Assert.NotNull(wallet);
        Assert.Equal(validDoc, wallet.IdTaxDoc);
    }

    #endregion

    #region Helper Methods

    private static WalletEntity CreateValidWallet(UserType userType)
    {
        return new WalletEntity(
            "João Silva",
            "12345678901",
            "joao@test.com",
            "hash123",
            userType
        );
    }

    #endregion
}