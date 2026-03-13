using Xunit;
using EasyPicPay.Entities;

namespace TestEasyPicPay.Unit;

/// <summary>
/// Testes unitários da TransactionEntity.
/// Foco: Validações e lógica de domínio.
/// </summary>
public class TransactionEntityTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesTransaction()
    {
        // Arrange
        var payerId = Guid.NewGuid();
        var payeeId = Guid.NewGuid();
        var amount = 100m;

        // Act
        var transaction = new TransactionEntity(payerId, payeeId, amount);

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(payerId, transaction.PayerId);
        Assert.Equal(payeeId, transaction.PayeeId);
        Assert.Equal(amount, transaction.Amount);
        Assert.True(transaction.IsPending);
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var payerId = Guid.NewGuid();
        var payeeId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TransactionEntity(payerId, payeeId, 0m));
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var payerId = Guid.NewGuid();
        var payeeId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TransactionEntity(payerId, payeeId, -50m));
    }

    [Fact]
    public void Constructor_WithSamePayerAndPayee_ThrowsInvalidOperationException()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new TransactionEntity(walletId, walletId, 100m)
        );
        Assert.Equal("Não é possível transferir para si mesmo", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyPayerId_ThrowsArgumentException()
    {
        // Arrange
        var payeeId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TransactionEntity(Guid.Empty, payeeId, 100m));
    }

    [Fact]
    public void Constructor_WithEmptyPayeeId_ThrowsArgumentException()
    {
        // Arrange
        var payerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TransactionEntity(payerId, Guid.Empty, 100m));
    }
}