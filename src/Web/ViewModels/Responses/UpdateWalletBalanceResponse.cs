namespace EasyPicPay.Web.ViewModels.Responses;

public record UpdateWalletBalanceResponse
{
    public decimal NewBalance { get; init; }
};