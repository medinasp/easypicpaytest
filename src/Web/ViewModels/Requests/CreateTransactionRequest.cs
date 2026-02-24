namespace EasyPicPay.Web.ViewModels.Requests;

public record CreateTransactionRequest(Guid PayerId, Guid PayeeId, decimal Amount, DateTimeOffset Timestamp);