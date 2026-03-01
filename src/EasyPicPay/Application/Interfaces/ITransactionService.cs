using EasyPicPay.Entities;

namespace EasyPicPay.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionEntity> CreateTransactionAsync(Guid payerId, Guid payeeId, decimal amount);
    Task<TransactionEntity?> GetTransactionByIdAsync(Guid id);    
}