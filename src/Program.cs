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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();          // cria o banco se não existir e atualiza o schema
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
//     app.MapScalarApiReference(options =>
//     {
//         options.WithTitle("EasyPicPay API")
//                .WithTheme(ScalarTheme.DeepSpace)
// //                 // Outros temas
// //                 // ScalarTheme.Default
// //                 // ScalarTheme.Alternative
// //                 // ScalarTheme.Moon
// //                 // ScalarTheme.Purple
// //                 // ScalarTheme.Solarized
// //                 // ScalarTheme.BluePlanet
// //                 // ScalarTheme.DeepSpace
// //                 // ScalarTheme.Saturn
// //                 // ScalarTheme.Kepler
// //                 // ScalarTheme.Mars
// //                 // ScalarTheme.None
//                .WithHttpBearerAuthentication(bearer => { });
//     });

// app.MapScalarApiReference(options =>
// {
//     options.WithTitle("EasyPicPay API")
//            .WithTheme(ScalarTheme.DeepSpace)
//            .AddHttpAuthentication("Bearer", scheme => { });
// });
//removido o scalar porque ele não é compatível com o .NET 10, e a documentação oficial do Scalar ainda não foi atualizada para refletir as mudanças no pipeline de middleware do .NET 10. O Scalar depende de pontos específicos no pipeline para funcionar corretamente, e as mudanças no .NET 10 podem ter quebrado essa compatibilidade. Até que o Scalar seja atualizado para suportar o .NET 10, é melhor removê-lo para evitar problemas de compatibilidade.
}

// app.UseHttpsRedirection();
//Por que remover no Docker? Porque seu container está configurado para rodar apenas em HTTP na porta 80 — você não tem certificado SSL configurado no container. Quando o middleware tenta redirecionar para HTTPS e não há nada escutando na porta 443, a requisição morre. Em produção real você resolveria isso com um proxy reverso como Nginx ou Traefik na frente do container, que cuida do SSL externamente.

// app.UseRouting();
//Por que remover no .NET 10? Porque o MapControllers() já chama o UseRouting() internamente de forma automática quando necessário. Chamá-lo explicitamente antes cria dois pontos de roteamento no pipeline, o que pode causar comportamento imprevisível — requisições sendo avaliadas duas vezes ou middleware de autenticação sendo executado fora de ordem.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();