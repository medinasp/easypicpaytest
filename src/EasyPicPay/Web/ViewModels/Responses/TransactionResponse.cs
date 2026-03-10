namespace EasyPicPay.Web.ViewModels.Responses;

public record TransactionResponse(
    Guid Id,
    Guid PayerId,
    Guid PayeeId,
    decimal Amount,
    DateTimeOffset Timestamp);