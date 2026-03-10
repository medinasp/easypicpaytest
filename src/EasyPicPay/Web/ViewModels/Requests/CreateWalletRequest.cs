using System.ComponentModel.DataAnnotations;
using EasyPicPay.Entities.Enums;

namespace EasyPicPay.Web.ViewModels.Requests;

public record CreateWalletRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
    [RegularExpression(@"^\d{11}$|^\d{14}$", ErrorMessage = "CPF deve ter 11 dígitos ou CNPJ 14 dígitos")]
    public string CpfCnpj { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Senha deve conter ao menos uma letra maiúscula, uma minúscula e um número")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tipo de usuário é obrigatório")]
    [EnumDataType(typeof(UserType), ErrorMessage = "Tipo de usuário inválido")]
    public UserType UserType { get; init; }
}