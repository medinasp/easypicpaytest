using System.ComponentModel.DataAnnotations;

namespace EasyPicPay.Web.ViewModels.Requests;

public record UpdateWalletBalanceRequest
{
    [Required(ErrorMessage = "WalletId é obrigatório")]
    public Guid WalletId { get; init; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(-1000000, 1000000, ErrorMessage = "Valor deve estar entre -R$ 1.000.000,00 e R$ 1.000.000,00")]
    public decimal Amount { get; init; }
}