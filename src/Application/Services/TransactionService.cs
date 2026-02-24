using EasyPicPay.Application.Interfaces;
using EasyPicPay.Data;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyPicPay.Application.Services;

public class TransactionService(
    AppDbContext context,
    ILogger<TransactionService> logger,
    IWalletService walletService)
    : ITransactionService
{
    public async Task<TransactionEntity> CreateTransactionAsync(Guid payerId, Guid payeeId, decimal amount)
    {
        await using var transactionScope = await context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Validações básicas
            if (!await walletService.WalletExistsAsync(payerId))
                throw new BusinessException("Pagador não encontrado");
                
            if (!await walletService.WalletExistsAsync(payeeId))
                throw new BusinessException("Recebedor não encontrado");

            // 2. Busca wallets com lock para evitar race conditions
            var payer = await context.Wallets
                .FirstOrDefaultAsync(w => w.Id == payerId);
                
            var payee = await context.Wallets
                .FirstOrDefaultAsync(w => w.Id == payeeId);

            if (payer == null || payee == null)
                throw new BusinessException("Uma das wallets não foi encontrada");

            // 3. Valida regras de negócio
            if (!payer.CanSendMoney())
                throw new BusinessException("Lojistas não podem enviar dinheiro");
                
            if (payer.Balance < amount)
                throw new BusinessException("Saldo insuficiente");

            // 4. Cria transação
            var transaction = new TransactionEntity(payerId, payeeId, amount);
            context.Transactions.Add(transaction);

            // 5. Debita e credita (ainda não persiste)
            payer.Debit(amount);
            payee.Credit(amount);

            // 6. Persiste tudo atomicamente
            await context.SaveChangesAsync();
            await transactionScope.CommitAsync();
            
            logger.LogInformation("Transação {TransactionId} criada com sucesso", transaction.Id);
            return transaction;
        }
        catch (Exception ex)
        {
            await transactionScope.RollbackAsync();
            logger.LogError(ex, "Erro na transação entre {PayerId} e {PayeeId}", payerId, payeeId);
            
            if (ex is BusinessException) throw;
            throw new BusinessException("Erro ao processar transação");
        }
    }

    public async Task<TransactionEntity?> GetTransactionByIdAsync(Guid id)
    {
        return await context.Transactions
            .Include(t => t.Payer)
            .Include(t => t.Payee)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    // #region Ciclo de vida interno (Authorization / Complete / Fail)
    //
    // /// <summary>
    // /// Simulação de autorização externa – **privada**, usada apenas internamente.
    // /// </summary>
    // private async Task<bool> AuthorizeInternalAsync(Guid transactionId)
    // {
    //     // Simulação simples, pode ser substituída por chamada a outro serviço
    //     await Task.Delay(100);
    //     return new Random().Next(0, 10) > 2; // 80% de sucesso
    // }
    //
    // /// <summary>
    // /// Marca a transação como concluída. **Privado** – chamado apenas por outro
    // /// serviço interno que decida que a transação pode ser finalizada.
    // /// </summary>
    // private async Task CompleteInternalAsync(Guid transactionId, string authorizationCode)
    // {
    //     var transaction = await context.Transactions
    //         .FirstOrDefaultAsync(t => t.Id == transactionId);
    //
    //     if (transaction == null)
    //         throw new BusinessException("Transação não encontrada");
    //
    //     transaction.Complete(authorizationCode);
    //     await context.SaveChangesAsync();
    //
    //     logger.LogInformation("Transação {TransactionId} completada", transactionId);
    // }
    //
    // /// <summary>
    // /// Registra o motivo da falha e revome as movimentações feitas.
    // /// **Privado** – chamado apenas quando a lógica interna decidir que a
    // /// transação deve ser revertida.
    // /// </summary>
    // private async Task FailInternalAsync(Guid transactionId, string reason)
    // {
    //     await using var txScope = await context.Database.BeginTransactionAsync();
    //
    //     try
    //     {
    //         var transaction = await context.Transactions
    //             .Include(t => t.Payer)
    //             .Include(t => t.Payee)
    //             .FirstOrDefaultAsync(t => t.Id == transactionId);
    //
    //         if (transaction == null)
    //             throw new BusinessException("Transação não encontrada");
    //
    //         // Só tenta reverter se a transação ainda estiver pendente
    //         if (transaction.Status == nameof(TransactionStatus.Pending))
    //         {
    //             if (transaction.Payer != null) transaction.Payer.Credit(transaction.Amount);
    //             if (transaction.Payee != null) transaction.Payee.Debit(transaction.Amount);
    //         }
    //
    //         transaction.Fail(reason);
    //         await context.SaveChangesAsync();
    //         await txScope.CommitAsync();
    //
    //         logger.LogWarning("Transação {TransactionId} falhou: {Reason}", transactionId, reason);
    //     }
    //     catch (Exception ex)
    //     {
    //         await txScope.RollbackAsync();
    //         logger.LogError(ex,
    //             "Erro ao falhar transação {TransactionId}", transactionId);
    //         throw;
    //     }
    // }

    // #endregion    
}