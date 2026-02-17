using System.ComponentModel.DataAnnotations;

namespace EasyPicPay.Application.DTOs;

public record TransactionDto(
    Guid Id,
    Guid PayerId,
    Guid PayeeId,
    decimal Amount,
    DateTime CreatedAt)
{
    [Required] public Guid Id { get; init; }
    [Required] public Guid PayerId { get; init; }
    [Required] public Guid PayeeId { get; init; }
    [Required] public decimal Amount { get; init; }
}