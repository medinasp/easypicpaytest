using System.ComponentModel.DataAnnotations;

namespace EasyPicPay.Application.DTOs.Requests;

public record CreateTransactionRequest(
    [property: Required] Guid PayerId,
    [property: Required] Guid PayeeId,
    [property: Required] decimal Amount
);