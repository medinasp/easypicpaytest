using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using EasyPicPay.Application.Services;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Entities.Enums;
using EasyPicPay.Data;
using TestEasyPicPay.Infrastructure;

namespace TestEasyPicPay.Integration;

[Collection("DatabaseTests")]  
public class TransactionServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private AppDbContext _context = null!;
    private TransactionService _transactionService = null!;
    private WalletService _walletService = null!;

    public TransactionServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _context = _fixture.CreateDbContext();
        
        var logger = NullLogger<TransactionService>.Instance;
        var walletLogger = NullLogger<WalletService>.Instance;

        _transactionService = new TransactionService(_context, logger);
        _walletService = new WalletService(_context, walletLogger);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    #region Helper Methods

    private async Task<Guid> CreateWalletAsync(string name, string cpfCnpj, string email, UserType userType)
    {
        var wallet = await _walletService.CreateWalletAsync(name, cpfCnpj, email, "SenhaForte@123", userType);
        return wallet.Id;
    }

    private async Task AddBalanceAsync(Guid walletId, decimal amount)
    {
        using var setupContext = _fixture.CreateDbContext();
    
        var rowsAffected = await setupContext.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Wallets"" SET ""Balance"" = ""Balance"" + {0} WHERE ""Id"" = {1}",
            amount, walletId
        );
    
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Wallet {walletId} não encontrada");
        
        // ✅ CRÍTICO: limpa o cache do contexto principal
        // Sem isso, o FromSqlRaw do TransactionService retorna
        // a entidade cacheada com Balance = 0
        _context.ChangeTracker.Clear();
    }
    
    #endregion

    #region Testes

    [Fact]
    public async Task CreateTransaction_WithValidData_SavesToDatabase()
    {
        var payerId = await CreateWalletAsync("João Silva", "12345678901", "joao@test.com", UserType.Common);
        var payeeId = await CreateWalletAsync("Maria Santos", "98765432100", "maria@test.com", UserType.Common);
        
        await AddBalanceAsync(payerId, 1000m);

        var transaction = await _transactionService.CreateTransactionAsync(payerId, payeeId, 250m);

        Assert.NotNull(transaction);
        Assert.Equal(250m, transaction.Amount);

        using var verifyContext = _fixture.CreateDbContext();
        var payer = await verifyContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == payerId);
        var payee = await verifyContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == payeeId);

        Assert.Equal(750m, payer.Balance);
        Assert.Equal(250m, payee.Balance);
    }

    [Fact]
    public async Task CreateTransaction_WithInsufficientBalance_ThrowsBusinessException()
    {
        var payerId = await CreateWalletAsync("Pedro Costa", "11122233344", "pedro@test.com", UserType.Common);
        var payeeId = await CreateWalletAsync("Ana Lima", "55566677788", "ana@test.com", UserType.Common);
        
        await AddBalanceAsync(payerId, 50m);

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payerId, payeeId, 100m)
        );

        Assert.Contains("Saldo insuficiente", exception.Message);

        using var verifyContext = _fixture.CreateDbContext();
        var payer = await verifyContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == payerId);
        Assert.Equal(50m, payer.Balance);
    }

    [Fact]
    public async Task CreateTransaction_WithMerchantAsPayer_ThrowsBusinessException()
    {
        var merchantId = await CreateWalletAsync("Loja XYZ", "12345678000190", "loja@test.com", UserType.Merchant);
        var customerId = await CreateWalletAsync("Cliente ABC", "99988877766", "cliente@test.com", UserType.Common);
        
        await AddBalanceAsync(merchantId, 500m);

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(merchantId, customerId, 100m)
        );

        Assert.Contains("Lojistas não podem enviar dinheiro", exception.Message);
    }

    [Fact]
    public async Task CreateTransaction_WithNonExistentPayer_ThrowsBusinessException()
    {
        var payeeId = await CreateWalletAsync("Maria Santos", "98765432100", "maria@test.com", UserType.Common);
        var fakePayerId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(fakePayerId, payeeId, 100m)
        );

        Assert.Contains("Pagador não encontrado", exception.Message);
    }

    [Fact]
    public async Task CreateTransaction_WithNonExistentPayee_ThrowsBusinessException()
    {
        var payerId = await CreateWalletAsync("João Silva", "12345678901", "joao@test.com", UserType.Common);
        await AddBalanceAsync(payerId, 1000m);
        
        var fakePayeeId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payerId, fakePayeeId, 100m)
        );

        Assert.Contains("Recebedor não encontrado", exception.Message);
    }

    [Fact]
    public async Task CreateTransaction_WhenErrorOccurs_RollsBackChanges()
    {
        var payerId = await CreateWalletAsync("João Silva", "12345678901", "joao@test.com", UserType.Common);
        await AddBalanceAsync(payerId, 1000m);
        
        var fakePayeeId = Guid.NewGuid();

        await Assert.ThrowsAsync<BusinessException>(async () =>
            await _transactionService.CreateTransactionAsync(payerId, fakePayeeId, 100m)
        );

        using var verifyContext = _fixture.CreateDbContext();
        var payer = await verifyContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == payerId);
        Assert.Equal(1000m, payer.Balance);
    }

    [Fact]
    public async Task CreateTransaction_SavesTransactionInDatabase()
    {
        var payerId = await CreateWalletAsync("Carlos Souza", "33344455566", "carlos@test.com", UserType.Common);
        var payeeId = await CreateWalletAsync("Recebedor", "66677788899", "rec@test.com", UserType.Common);
        
        await AddBalanceAsync(payerId, 1000m);

        var transaction = await _transactionService.CreateTransactionAsync(payerId, payeeId, 300m);

        using var verifyContext = _fixture.CreateDbContext();
        var savedTransaction = await verifyContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);
        
        Assert.NotNull(savedTransaction);
        Assert.Equal(payerId, savedTransaction.PayerId);
        Assert.Equal(payeeId, savedTransaction.PayeeId);
        Assert.Equal(300m, savedTransaction.Amount);
    }

    #endregion
}