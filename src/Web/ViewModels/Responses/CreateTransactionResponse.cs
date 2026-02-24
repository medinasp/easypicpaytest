namespace EasyPicPay.Web.ViewModels.Responses;

public record CreateTransactionResponse
{
    public Guid TransactionId { get; init; }
    public string Message     { get; init; }
}