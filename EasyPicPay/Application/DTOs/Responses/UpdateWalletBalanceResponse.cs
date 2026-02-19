namespace EasyPicPay.Application.DTOs.Responses;

public record UpdateWalletBalanceResponse
{
    public decimal NewBalance { get; init; }
};