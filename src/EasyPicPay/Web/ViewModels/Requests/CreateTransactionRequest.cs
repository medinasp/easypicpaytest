using System.ComponentModel.DataAnnotations;
using EasyPicPay.Web.Validators;


namespace EasyPicPay.Web.ViewModels.Requests;

public record CreateTransactionRequest
{
    [Required(ErrorMessage = "PayerId é obrigatório")]
    [NotSameAs(nameof(PayeeId), ErrorMessage = "Pagador e recebedor não podem ser iguais")]
    public Guid PayerId { get; init; }

    [Required(ErrorMessage = "PayeeId é obrigatório")]
    public Guid PayeeId { get; init; }


    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, 1000000, ErrorMessage = "Valor deve estar entre R$ 0,01 e R$ 1.000.000,00")]
    public decimal Amount { get; init; }
}