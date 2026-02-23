namespace EasyPicPay.Application.DTOs.Responses;

public record CreateTransactionResponse
{
    public Guid TransactionId { get; init; }
    public string Message     { get; init; }
}