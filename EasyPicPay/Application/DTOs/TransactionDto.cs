namespace EasyPicPay.Application.DTOs;

public record TransactionDto(
    Guid Id,
    Guid PayerId,
    Guid PayeeId,
    decimal Amount,
    DateTimeOffset Timestamp);