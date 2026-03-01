namespace EasyPicPay.Web.ViewModels.Requests;

public record UpdateWalletBalanceRequest(Guid WalletId, decimal Amount);