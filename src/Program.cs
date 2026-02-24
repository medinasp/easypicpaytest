using EasyPicPay.Application.Interfaces;
using EasyPicPay.Application.Services;
using EasyPicPay.Data;
using EasyPicPay.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// -------------------------------------------------------
// 1️⃣ JWT
// -------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// -------------------------------------------------------
// 2️⃣ Banco de dados
// -------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------------------------------------
// 3️⃣ Serviços
// -------------------------------------------------------
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("EasyPicPay API")
               .WithTheme(ScalarTheme.DeepSpace)
                // Outros temas
                // ScalarTheme.Default
                // ScalarTheme.Alternative
                // ScalarTheme.Moon
                // ScalarTheme.Purple
                // ScalarTheme.Solarized
                // ScalarTheme.BluePlanet
                // ScalarTheme.DeepSpace
                // ScalarTheme.Saturn
                // ScalarTheme.Kepler
                // ScalarTheme.Mars
                // ScalarTheme.None
               .WithHttpBearerAuthentication(bearer => { });
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();