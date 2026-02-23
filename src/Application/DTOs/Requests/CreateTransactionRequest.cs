namespace EasyPicPay.Application.DTOs.Requests;

public record CreateTransactionRequest(Guid PayerId, Guid PayeeId, decimal Amount, DateTimeOffset Timestamp);