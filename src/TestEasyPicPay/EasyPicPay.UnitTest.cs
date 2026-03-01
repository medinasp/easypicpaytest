using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; 
using Microsoft.Extensions.Logging;
using EasyPicPay.Application.Services;
using EasyPicPay.Data;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestEasyPicPay;

[TestFixture]
public class Tests
{
    private AppDbContext _context;
    private ILogger<WalletService> _walletLogger;
    private ILogger<TransactionService> _transactionLogger;
    private IWalletService _walletService;
    private ITransactionService _transactionService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => 
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // 👈 LINHA CRÍTICA
            .Options;

        _context = new AppDbContext(options);
        _walletLogger = NullLogger<WalletService>.Instance;
        _transactionLogger = NullLogger<TransactionService>.Instance;
        _walletService = new WalletService(_context, _walletLogger);
        _transactionService = new TransactionService(_context, _transactionLogger, _walletService);
    }    

    [TearDown]
    public void TearDown() => _context.Dispose();

    #region WalletService Tests

    [Test]
    public async Task CreateWallet_ValidData_ReturnsWallet()
    {
        // Act
        var result = await _walletService.CreateWalletAsync(
            "John Doe", 
            "12345678901", 
            "john@example.com", 
            "P@ssw0rd", 
            UserType.Common);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("John Doe"));
            Assert.That(result.Email, Is.EqualTo("john@example.com"));
            Assert.That(result.IdTaxDoc, Is.EqualTo("12345678901"));
            Assert.That(result.UserType, Is.EqualTo(UserType.Common));
            Assert.That(result.Balance, Is.EqualTo(0));
            Assert.That(_context.Wallets.Any(w => w.Id == result.Id), Is.True);
        });
    }

    [Test]
    public async Task CreateWallet_ExistingEmail_ThrowsBusinessException()
    {
        // Arrange
        await _walletService.CreateWalletAsync(
            "Jane", 
            "98765432109", 
            "duplicate@example.com", 
            "P@ssw0rd", 
            UserType.Common);

        // Act & Assert - CORRIGIDO: sem await na exceção
        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync(
                "John", 
                "12345678901", 
                "duplicate@example.com",
                "P@ssw0rd", 
                UserType.Common));
        
        Assert.That(ex.Message, Is.EqualTo("Email ou CPF/CNPJ já cadastrado"));
    }

    [Test]
    public async Task CreateWallet_ExistingCpfCnpj_ThrowsBusinessException()
    {
        // Arrange
        var cpfExistente = "12345678901";
        await _walletService.CreateWalletAsync(
            "Jane", 
            cpfExistente, 
            "jane@email.com",
            "P@ssw0rd", 
            UserType.Common);

        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync(
                "John", 
                cpfExistente,
                "john@email.com",
                "P@ssw0rd", 
                UserType.Common));
        
        Assert.That(ex.Message, Is.EqualTo("Email ou CPF/CNPJ já cadastrado"));
    }

    [Test]
    public async Task GetWalletById_ExistingWallet_ReturnsWallet()
    {
        // Arrange
        var created = await _walletService.CreateWalletAsync(
            "Alice", 
            "11122233344", 
            "alice@example.com", 
            "P@ssw0rd", 
            UserType.Common);

        // Act
        var result = await _walletService.GetWalletByIdAsync(created.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task GetWalletById_NonExistingWallet_ReturnsNull()
    {
        // Act
        var result = await _walletService.GetWalletByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetBalance_ExistingWallet_ReturnsZero()
    {
        // Arrange
        var wallet = await _walletService.CreateWalletAsync(
            "Alice", 
            "11122233344", 
            "alice@example.com", 
            "P@ssw0rd", 
            UserType.Common);

        // Act
        var balance = await _walletService.GetBalanceAsync(wallet.Id);

        // Assert
        Assert.That(balance, Is.EqualTo(0));
    }

    [Test]
    public async Task GetBalance_NonExistingWallet_ReturnsZero()
    {
        // Act
        var balance = await _walletService.GetBalanceAsync(Guid.NewGuid());

        // Assert
        Assert.That(balance, Is.EqualTo(0));
    }

    [Test]
    public async Task WalletExists_ExistingWallet_ReturnsTrue()
    {
        // Arrange
        var wallet = await _walletService.CreateWalletAsync(
            "Bob", 
            "55566677788", 
            "bob@example.com", 
            "P@ssw0rd", 
            UserType.Common);

        // Act
        var exists = await _walletService.WalletExistsAsync(wallet.Id);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task WalletExists_NonExistingWallet_ReturnsFalse()
    {
        // Act
        var exists = await _walletService.WalletExistsAsync(Guid.NewGuid());

        // Assert
        Assert.That(exists, Is.False);
    }

    #endregion

    #region TransactionService Tests

    private async Task<WalletEntity> CreateWalletWithBalance(string name, string cpf, string email, decimal balance)
    {
        var wallet = await _walletService.CreateWalletAsync(name, cpf, email, "P@ssw0rd", UserType.Common);
        var entity = await _context.Wallets.FindAsync(wallet.Id);
        entity?.Credit(balance);
        await _context.SaveChangesAsync();
        return wallet;
    }

    [Test]
    public async Task CreateTransaction_ValidData_ReturnsTransactionAndUpdatesBalances()
    {
        // Arrange
        var payer = await CreateWalletWithBalance("Payer", "11111111111", "payer@test.com", 100);
        var payee = await CreateWalletWithBalance("Payee", "22222222222", "payee@test.com", 50);
        var amount = 30;

        // Act
        var transaction = await _transactionService.CreateTransactionAsync(payer.Id, payee.Id, amount);

        // Assert
        var updatedPayer = await _context.Wallets.FindAsync(payer.Id);
        var updatedPayee = await _context.Wallets.FindAsync(payee.Id);

        Assert.Multiple(() =>
        {
            Assert.That(transaction, Is.Not.Null);
            Assert.That(transaction.PayerId, Is.EqualTo(payer.Id));
            Assert.That(transaction.PayeeId, Is.EqualTo(payee.Id));
            Assert.That(transaction.Amount, Is.EqualTo(amount));
            Assert.That(updatedPayer!.Balance, Is.EqualTo(70));
            Assert.That(updatedPayee!.Balance, Is.EqualTo(80));
        });
    }

    [Test]
    public async Task CreateTransaction_PayerNotFound_ThrowsBusinessException()
    {
        // Arrange
        var payee = await _walletService.CreateWalletAsync("Payee", "22222222222", "payee@test.com", "P@ssw0rd", UserType.Common);

        // Act & Assert - CORRIGIDO: sem await na exceção
        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(Guid.NewGuid(), payee.Id, 50));

        Assert.That(ex.Message, Is.EqualTo("Pagador não encontrado"));
    }

    [Test]
    public async Task CreateTransaction_PayeeNotFound_ThrowsBusinessException()
    {
        // Arrange
        var payer = await _walletService.CreateWalletAsync("Payer", "11111111111", "payer@test.com", "P@ssw0rd", UserType.Common);

        // Act & Assert - CORRIGIDO: sem await na exceção
        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payer.Id, Guid.NewGuid(), 50));

        Assert.That(ex.Message, Is.EqualTo("Recebedor não encontrado"));
    }

    [Test]
    public async Task CreateTransaction_SenderIsMerchant_ThrowsBusinessException()
    {
        // Arrange
        var merchant = await _walletService.CreateWalletAsync("Merchant", "33333333333", "merchant@test.com", "P@ssw0rd", UserType.Merchant);
        var payee = await _walletService.CreateWalletAsync("Payee", "44444444444", "payee@test.com", "P@ssw0rd", UserType.Common);
        
        // Give merchant balance
        var merchantEntity = await _context.Wallets.FindAsync(merchant.Id);
        merchantEntity?.Credit(100);
        await _context.SaveChangesAsync();

        // Act & Assert - CORRIGIDO: sem await na exceção
        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(merchant.Id, payee.Id, 50));

        Assert.That(ex.Message, Is.EqualTo("Lojistas não podem enviar dinheiro"));
    }

    [Test]
    public async Task CreateTransaction_InsufficientBalance_ThrowsBusinessException()
    {
        // Arrange
        var payer = await CreateWalletWithBalance("Payer", "11111111111", "payer@test.com", 30);
        var payee = await _walletService.CreateWalletAsync("Payee", "22222222222", "payee@test.com", "P@ssw0rd", UserType.Common);

        // Act & Assert - CORRIGIDO: sem await na exceção
        var ex = Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payer.Id, payee.Id, 50));

        Assert.That(ex.Message, Is.EqualTo("Saldo insuficiente"));
    }

    [Test]
    public async Task GetTransactionById_ExistingTransaction_ReturnsTransactionWithNavigation()
    {
        // Arrange
        var payer = await CreateWalletWithBalance("Payer", "11111111111", "payer@test.com", 100);
        var payee = await _walletService.CreateWalletAsync("Payee", "22222222222", "payee@test.com", "P@ssw0rd", UserType.Common);
        
        var created = await _transactionService.CreateTransactionAsync(payer.Id, payee.Id, 30);

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(created.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(created.Id));
            Assert.That(result.Payer, Is.Not.Null);
            Assert.That(result.Payee, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetTransactionById_NonExistingTransaction_ReturnsNull()
    {
        // Act
        var result = await _transactionService.GetTransactionByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion
}