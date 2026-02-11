using EasyPicPay.Models;

namespace EasyPicPay.Application.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(Guid payerId, Guid payeeId, decimal amount);
    Task<bool> AuthorizeTransactionAsync(Guid transactionId);
    Task CompleteTransactionAsync(Guid transactionId, string authorizationCode);
    Task FailTransactionAsync(Guid transactionId, string reason);
    Task<Transaction?> GetTransactionByIdAsync(Guid id);    
}