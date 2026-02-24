using EasyPicPay.Entities.Enums;
namespace EasyPicPay.Web.ViewModels.Requests;

public record CreateWalletRequest(string Name, string CpfCnpj, string Email, string Password, UserType UserType);