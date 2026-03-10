using System.ComponentModel.DataAnnotations;

namespace EasyPicPay.Web.ViewModels.Requests;

public record LoginRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; init; } = string.Empty;
}