using EasyPicPay.Web.ViewModels.Requests;

namespace EasyPicPay.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Valida usuário e devolve um JWT (ou lança SecurityException).
    /// </summary>
    string SignIn(LoginRequest request);

    /// <summary>
    /// Verifica se o token contém a claim / role exigida.
    /// </summary>
    bool IsAuthorized(string token, string requiredPolicy);    
}