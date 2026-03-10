using EasyPicPay.Application.Interfaces;
using EasyPicPay.Data;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Entities;
using Microsoft.EntityFrameworkCore;
using EasyPicPay.Application.Util;

namespace EasyPicPay.Application.Services;

public class TransactionService(
    AppDbContext context,
    ILogger<TransactionService> logger)
    : ITransactionService
{
    public async Task<TransactionEntity> CreateTransactionAsync(Guid payerId, Guid payeeId, decimal amount)
    {
        // 1. Inicia transação explícita no banco (obrigatório para FOR UPDATE funcionar)
        // Garante que todas as operações sejam atômicas (tudo ou nada)
        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            // 2. Busca e TRAVA as wallets no banco usando FOR UPDATE (Lock Pessimista)
            // O FOR UPDATE bloqueia as linhas para outras transações até o COMMIT
            // Outras transações que tentarem ler essas mesmas linhas ficarão ESPERANDO
            var payer = await context.Wallets
                .FromSqlRaw(
                    "SELECT * FROM \"Wallets\" WHERE \"Id\" = {0} FOR UPDATE", 
                    payerId
                )
                .FirstOrDefaultAsync();
            
            // 3. Busca e TRAVA a wallet do recebedor (também com FOR UPDATE)
            // Importante: travamos AMBAS as wallets antes de qualquer validação
            var payee = await context.Wallets
                .FromSqlRaw(
                    "SELECT * FROM \"Wallets\" WHERE \"Id\" = {0} FOR UPDATE", 
                    payeeId
                )
                .FirstOrDefaultAsync();

            // 4. Validações básicas de existência
            // Agora que as linhas estão travadas, podemos validar com segurança
            if (payer == null)
                throw new BusinessException(ConstMessages.PayerNotFound);
            
            if (payee == null)
                throw new BusinessException(ConstMessages.PayeeNotFound);

            // 5. Validações de regras de negócio
            // Como as linhas estão travadas, temos garantia que os dados não mudaram
            if (!payer.CanSendMoney())
                throw new BusinessException(ConstMessages.MerchantCannotSend);
            
            if (payer.Balance < amount)
                throw new BusinessException(ConstMessages.InsufficientBalance);

            // 6. Cria a entidade de transação
            // Registra a operação que será realizada
            var transaction = new TransactionEntity(payerId, payeeId, amount);
            context.Transactions.Add(transaction);

            // 7. Atualiza os saldos na memória
            // As entidades ainda estão sendo rastreadas pelo EF Core
            payer.Debit(amount);  // Debita do pagador
            payee.Credit(amount); // Credita ao recebedor

            // 8. Persiste todas as mudanças atomicamente
            // SaveChangesAsync executa todos os INSERTs e UPDATEs em uma única operação
            // Se qualquer operação falhar, nada é salvo (atomicidade)
            await context.SaveChangesAsync();
            
            // 9. Confirma a transação e LIBERA os locks
            // COMMIT libera as linhas travadas para outras transações
            // Só após o COMMIT as mudanças ficam visíveis para outras transações
            await dbTransaction.CommitAsync();

            logger.LogInformation(
                "Transação {TransactionId} criada com sucesso. Pagador: {PayerId}, Recebedor: {PayeeId}, Valor: {Amount}", 
                transaction.Id, payerId, payeeId, amount
            );
            
            return transaction;
        }
        catch (Exception ex)
        {
            // 10. Em caso de erro, desfaz TUDO e libera os locks
            // ROLLBACK descarta todas as mudanças e libera as linhas travadas
            // Garante que o banco volta ao estado anterior à transação
            await dbTransaction.RollbackAsync();
            
            logger.LogError(
                ex, 
                "Erro ao criar transação entre Pagador: {PayerId} e Recebedor: {PayeeId}. Valor: {Amount}", 
                payerId, payeeId, amount
            );
            
            // 11. Repropaga BusinessException (erros de validação)
            // Outros erros são encapsulados em BusinessException genérica
            if (ex is BusinessException)
                throw;
            
            throw new BusinessException(ConstMessages.InternalError);
        }
    }

    public async Task<TransactionEntity?> GetTransactionByIdAsync(Guid id)
    {
        // Busca transação com dados relacionados (Payer e Payee)
        // AsNoTracking melhora performance pois não precisa rastrear mudanças
        return await context.Transactions
            .Include(t => t.Payer)
            .Include(t => t.Payee)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}