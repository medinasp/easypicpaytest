using EasyPicPay.Entities.Enums;
namespace EasyPicPay.Application.DTOs.Requests;

public record CreateWalletRequest(string Name, string CpfCnpj, string Email, string Password, UserType UserType);