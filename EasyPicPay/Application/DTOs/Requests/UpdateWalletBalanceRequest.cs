namespace EasyPicPay.Application.DTOs.Requests;

public record UpdateWalletBalanceRequest(Guid WalletId, decimal Amount);