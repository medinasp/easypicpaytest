using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using EasyPicPay.Application.Services;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Entities.Enums;
using TestEasyPicPay.Infrastructure;

namespace TestEasyPicPay.Integration;

/// <summary>
/// Testes de integração do WalletService.
/// Testa: Constraints UNIQUE do banco (email, CPF/CNPJ).
/// </summary>
public class WalletServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private WalletService _walletService = null!;

    public WalletServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();

        var context = _fixture.CreateDbContext();
        var logger = NullLogger<WalletService>.Instance;
        _walletService = new WalletService(context, logger);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    #region Testes de Criação

    [Fact]
    public async Task CreateWallet_WithValidData_SavesToDatabase()
    {
        // Act
        var wallet = await _walletService.CreateWalletAsync(
            "João Silva",
            "12345678901",
            "joao@test.com",
            "SenhaForte@123",
            UserType.Common
        );

        // Assert
        Assert.NotEqual(Guid.Empty, wallet.Id);
        Assert.Equal("João Silva", wallet.Name);
        Assert.Equal("12345678901", wallet.IdTaxDoc);
        Assert.Equal(0m, wallet.Balance);

        // Verifica que foi persistido
        var retrieved = await _walletService.GetWalletByIdAsync(wallet.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("joao@test.com", retrieved.Email);
    }

    #endregion

    #region Testes de Constraints UNIQUE (Banco)

    [Fact]
    public async Task CreateWallet_WithDuplicateEmail_ThrowsBusinessException()
    {
        // Arrange
        await _walletService.CreateWalletAsync(
            "João Silva",
            "12345678901",
            "joao@test.com",
            "Senha@123",
            UserType.Common
        );

        // Act & Assert - Tenta criar com mesmo email
        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync(
                "Maria Santos",
                "98765432100",
                "joao@test.com",  // ← Mesmo email!
                "Senha@456",
                UserType.Common
            )
        );

        Assert.Contains("Email ou CPF/CNPJ já cadastrado", exception.Message);
    }

    [Fact]
    public async Task CreateWallet_WithDuplicateCpf_ThrowsBusinessException()
    {
        // Arrange
        await _walletService.CreateWalletAsync(
            "João Silva",
            "12345678901",
            "joao@test.com",
            "Senha@123",
            UserType.Common
        );

        // Act & Assert - Tenta criar com mesmo CPF
        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync(
                "Pedro Costa",
                "12345678901",  // ← Mesmo CPF!
                "pedro@test.com",
                "Senha@789",
                UserType.Common
            )
        );

        Assert.Contains("Email ou CPF/CNPJ já cadastrado", exception.Message);
    }

    [Fact]
    public async Task CreateWallet_WithDuplicateCnpj_ThrowsBusinessException()
    {
        // Arrange
        await _walletService.CreateWalletAsync(
            "Loja ABC",
            "12345678000190",
            "loja1@test.com",
            "Senha@111",
            UserType.Merchant
        );

        // Act & Assert - Tenta criar com mesmo CNPJ
        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync(
                "Loja XYZ",
                "12345678000190",  // ← Mesmo CNPJ!
                "loja2@test.com",
                "Senha@222",
                UserType.Merchant
            )
        );

        Assert.Contains("Email ou CPF/CNPJ já cadastrado", exception.Message);
    }

    #endregion

    #region Testes de Consulta

    [Fact]
    public async Task GetWalletById_WhenExists_ReturnsWallet()
    {
        // Arrange
        var created = await _walletService.CreateWalletAsync(
            "Ana Lima",
            "55566677788",
            "ana@test.com",
            "Senha@333",
            UserType.Common
        );

        // Act
        var retrieved = await _walletService.GetWalletByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Ana Lima", retrieved.Name);
    }

    [Fact]
    public async Task GetWalletById_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var result = await _walletService.GetWalletByIdAsync(fakeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WalletExists_WhenExists_ReturnsTrue()
    {
        // Arrange
        var wallet = await _walletService.CreateWalletAsync(
            "Carlos Souza",
            "33344455566",
            "carlos@test.com",
            "Senha@444",
            UserType.Common
        );

        // Act
        var exists = await _walletService.WalletExistsAsync(wallet.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task WalletExists_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var exists = await _walletService.WalletExistsAsync(fakeId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetBalance_WhenWalletExists_ReturnsBalance()
    {
        // Arrange
        var wallet = await _walletService.CreateWalletAsync(
            "Ricardo Alves",
            "44455566677",
            "ricardo@test.com",
            "Senha@555",
            UserType.Common
        );

        // Act
        var balance = await _walletService.GetBalanceAsync(wallet.Id);

        // Assert
        Assert.Equal(0m, balance);
    }

    [Fact]
    public async Task GetBalance_WhenWalletNotExists_ReturnsZero()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var balance = await _walletService.GetBalanceAsync(fakeId);

        // Assert
        Assert.Equal(0m, balance);
    }

    #endregion
}