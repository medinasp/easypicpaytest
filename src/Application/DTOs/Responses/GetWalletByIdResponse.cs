using EasyPicPay.Entities.Enums;

namespace EasyPicPay.Application.DTOs.Responses;

public record GetWalletByIdResponse
{
    public string Name {get; init; }
    public string Email {get; init; }
    public decimal Balance  {get; init; }
    public UserType UserType  {get; init; }
}