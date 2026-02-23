using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
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
    private ILogger<WalletService> _logger;
    private IWalletService _walletService;
    private ITransactionService _transactionService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _logger = NullLogger<WalletService>.Instance;
        _walletService = new WalletService(_context, _logger);
        _transactionService = new TransactionService(_context, NullLogger<TransactionService>.Instance, _walletService);
    }

    // Dispose the DbContext after each test to satisfy the IDisposable requirement
    [TearDown]
    public void TearDown()
    {
        
        
        
        _context?.Dispose();
    }

    [Test]
    public async Task CreateWallet_ValidData_ReturnsWallet()
    {
        var result = await _walletService.CreateWalletAsync("John Doe", "12345678901", "john@example.com", "P@ssw0rd", UserType.Common);

// IsNotNull
        Assert.That(result, Is.Not.Null);

// AreEqual
        Assert.That(result.Name, Is.EqualTo("John Doe"));
        Assert.That(result.Email, Is.EqualTo("john@example.com"));

// IsTrue
        Assert.That(_context.Wallets.Any(w => w.Id == result.Id), Is.True);

// AreEqual numérico
        Assert.That(result.Balance, Is.EqualTo(0));        
        
        // Assert.That(result, Is.Not.Null);
        // Assert.IsNotNull(result);
        // Assert.AreEqual("John Doe", result.Name);
        // Assert.AreEqual("john@example.com", result.Email);
        // Assert.IsTrue(_context.Wallets.Any(w => w.Id == result.Id));
    }

    [Test]
    public void CreateWallet_ExistingEmail_ThrowsBusinessException()
    {
        _walletService.CreateWalletAsync("Jane", "98765432109", "duplicate@example.com", "P@ssw0rd", UserType.Common).Wait();
        Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync("John", "12345678901", "duplicate@example.com", "P@ssw0rd", UserType.Common));
    }

    [Test]
    public void CreateWallet_ExistingCpfCnpj_ThrowsBusinessException()
    {
        _walletService.CreateWalletAsync("Jane", "12345678901", "unique@example.com", "P@ssw0rd", UserType.Common).Wait();
        Assert.ThrowsAsync<BusinessException>(async () =>
            await _walletService.CreateWalletAsync("John", "12345678901", "unique@example.com", "P@ssw0rd", UserType.Common));
    }

    [Test]
    public async Task GetBalance_ExistingWallet_ReturnsZero()
    {
        var wallet = await _walletService.CreateWalletAsync("Alice", "11122233344", "alice@example.com", "P@ssw0rd", UserType.Common);
        var balance = await _walletService.GetBalanceAsync(wallet.Id);
        Assert.That(balance, Is.EqualTo(0));
    }

    [Test]
    public async Task TransferMoney_ValidTransaction_UpdatesBalancesAndCreatesTransaction()
    {
        // Arrange
        var payer = await _walletService.CreateWalletAsync("Payer", "payerCpf", "payer@example.com", "P@ssw0rd", UserType.Common);
        var payee = await _walletService.CreateWalletAsync("Payee", "payeeCpf", "payee@example.com", "P@ssw0rd", UserType.Common);
        await _context.SaveChangesAsync();

        // Give the payer an initial balance of 100 using the Credit method (public in the entity)
        var payerEntity = await _context.Wallets.FindAsync(payer.Id);
        payerEntity?.Credit(100);
        await _context.SaveChangesAsync();

        // Act
        var transaction = await _transactionService.CreateTransactionAsync(payer.Id, payee.Id, 30);
        // Assert.IsNotNull(transaction);
        // Assert.AreEqual(payer.Id, transaction.PayerId);
        // Assert.AreEqual(payee.Id, transaction.PayeeId);
        // Assert.AreEqual(30, transaction.Amount);
        
        Assert.That(transaction, Is.Not.Null);
        Assert.That(payer.Id, Is.EqualTo(transaction.PayerId));
        Assert.That(payee.Id, Is.EqualTo(transaction.PayeeId));
        Assert.That(payee.Id, Is.EqualTo(transaction.PayeeId));        
        
        

        // Assert
        var updatedPayer = await _context.Wallets.FindAsync(payer.Id);
        var updatedPayee = await _context.Wallets.FindAsync(payee.Id);
        Assert.That(70, Is.EqualTo(updatedPayer.Balance));
        Assert.That(30, Is.EqualTo(updatedPayee.Balance));
        
        // Assert.AreEqual(70, updatedPayer.Balance); // 100 - 30
        // Assert.AreEqual(30, updatedPayee.Balance); // 0 + 30
    }

    [Test]
    public async Task TransferMoney_InsufficientBalance_ThrowsBusinessException()
    {
        // Arrange
        var payer = await _walletService.CreateWalletAsync("Payer", "payerCpf", "payer@example.com", "P@ssw0rd", UserType.Common);
        var payee = await _walletService.CreateWalletAsync("Payee", "payeeCpf", "payee@example.com", "P@ssw0rd", UserType.Common);
        await _context.SaveChangesAsync();

        // Set payer's balance to a low value (20)
        var payerEntity = await _context.Wallets.FindAsync(payer.Id);
        payerEntity?.Credit(20);
        await _context.SaveChangesAsync();

        // Act & Assert
        Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payer.Id, payee.Id, 50));
    }

    [Test]
    public async Task TransferMoney_NonExistingWallet_ThrowsBusinessException()
    {
        Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(Guid.NewGuid(), Guid.NewGuid(), 10));
    }

    // [Test]
    // public async Task TransferMoney_SenderCannotSendMoney_ThrowsBusinessException()
    // {
    //     // Arrange
    //     var wallet = await _walletService.CreateWalletAsync("Merchant", "merchantCpf", "merchant@example.com", "P@ssw0rd", UserType.Merchant);
    //     await _context.SaveChangesAsync();
    //
    //     var merchant = await _context.Wallets.FindAsync(wallet.Id);
    //     // The entity exposes a method (or property) that can be toggled to block sending money.
    //     // Assuming a public setter exists; otherwise use the domain method that disables it.
    //     merchant.CanSendMoney = false; // adjust if the API differs
    //     await _context.SaveChangesAsync();
    //
    //     // Act & Assert
    //     Assert.ThrowsAsync<BusinessException>(async () =>
    //         await _transactionService.CreateTransactionAsync(merchant.Id, Guid.NewGuid(), 10));
    // }
    
    [Test]
    public async Task TransferMoney_SenderCannotSendMoney_ThrowsBusinessException()
    {
        // Arrange - Merchant já não pode enviar dinheiro por definição
        var merchant = await _walletService.CreateWalletAsync("Merchant", "merchantCpf12", "merchant@example.com", "P@ssw0rd", UserType.Merchant);
        var payee = await _walletService.CreateWalletAsync("Payee", "payeeCpf1234", "payee@example.com", "P@ssw0rd", UserType.Common);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThatAsync(async () =>
                await _transactionService.CreateTransactionAsync(merchant.Id, payee.Id, 10),
            Throws.TypeOf<BusinessException>());
    }    
}