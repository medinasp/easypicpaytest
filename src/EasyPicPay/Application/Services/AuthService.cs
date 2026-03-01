using EasyPicPay.Application.Interfaces;
using EasyPicPay.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security;
using System.Text;
using EasyPicPay.Web.ViewModels.Requests;

namespace EasyPicPay.Application.Services;

public class AuthService(
    AppDbContext db,
    // REMOVA: IPasswordHasher<WalletEntity> hasher,  ← não precisa mais
    IConfiguration cfg)
    : IAuthService
{
    public string SignIn(LoginRequest request)
    {
        // MANTÉM — busca pelo e-mail
        var user = db.Wallets.FirstOrDefault(w => w.Email == request.Email);
        if (user == null)
            throw new SecurityException("Credenciais inválidas.");

        // TROCA — de IPasswordHasher para BCrypt
        var senhaCorreta = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!senhaCorreta)
            throw new SecurityException("Credenciais inválidas.");
        
        // senha digitada "MinhaSenh@123" + hash do banco → BCrypt.Verify() → true ou false
        // O BCrypt não "descriptografa" o hash — ele refaz o processo de hash na senha digitada e compara com o que está salvo.        

        // MANTÉM — geração do JWT inteiramente igual
        var jwtKey = cfg["Jwt:Key"] 
                     ?? throw new InvalidOperationException("Jwt:Key não configurada no appsettings.json");
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer:            cfg["Jwt:Issuer"],
            audience:          cfg["Jwt:Audience"],
            expires:           DateTime.UtcNow.AddHours(1),
            claims:            claims,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // MANTÉM — método de verificação de claims inteiramente igual
    public bool IsAuthorized(string token, string requiredPolicy)
    {
        var handler   = new JwtSecurityTokenHandler();
        var tokenInfo = handler.CanReadToken(token) ? handler.ReadJwtToken(token) : null;
        if (tokenInfo == null) return false;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenInfo.Claims));
        return principal.FindFirst(requiredPolicy) != null;
    }
}