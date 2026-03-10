using Microsoft.AspNetCore.Mvc;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Application.Util;
using EasyPicPay.Web.ViewModels.Requests;

namespace EasyPicPay.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Realiza login e retorna um JWT Bearer token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // ✅ Valida ModelState
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            logger.LogInformation("Tentativa de login: {Email}", request.Email);

            var token = authService.SignIn(request);

            logger.LogInformation("Login bem-sucedido: {Email}", request.Email);

            return Ok(new { token });
        }
        catch (System.Security.SecurityException ex)
        {
            logger.LogWarning(ex, "Falha no login: {Email}", request.Email);
            return Unauthorized(new { error = "Credenciais inválidas." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado no login: {Email}", request.Email);
            return StatusCode(500, new { error = ConstMessages.InternalError });
        }
    }
}