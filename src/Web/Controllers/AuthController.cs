using Microsoft.AspNetCore.Mvc;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Web.ViewModels.Requests;

namespace EasyPicPay.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Realiza login e retorna um JWT Bearer token.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = authService.SignIn(request);
            return Ok(new { token });
        }
        catch (System.Security.SecurityException)
        {
            // Não revelamos o motivo exato por segurança
            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }
}