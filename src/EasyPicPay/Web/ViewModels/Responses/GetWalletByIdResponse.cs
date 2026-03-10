using EasyPicPay.Entities.Enums;

namespace EasyPicPay.Web.ViewModels.Responses;

public record GetWalletByIdResponse(string Name, string Email, decimal Balance, UserType UserType);