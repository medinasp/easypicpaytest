using Microsoft.EntityFrameworkCore;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Data;
using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Application.Util;

namespace EasyPicPay.Application.Services;

public class WalletService(AppDbContext context, ILogger<WalletService> logger) 
    : IWalletService
{
    public async Task<WalletEntity> CreateWalletAsync(
        string name, string cpfCnpj, string email, string password, UserType userType)
    {
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var wallet = new WalletEntity(name, cpfCnpj, email, passwordHash, userType);
            
            context.Wallets.Add(wallet);
            await context.SaveChangesAsync(); // Banco valida unicidade

            logger.LogInformation(
                "Wallet criada: {WalletId}, Email: {Email}, UserType: {UserType}", 
                wallet.Id, email, userType);
            
            return wallet;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning("Tentativa de criar wallet duplicada: Email={Email}, CPF/CNPJ={CpfCnpj}", 
                email, cpfCnpj);
            throw new BusinessException(ConstMessages.EmailOrDocumentAlreadyExists);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar wallet: Email={Email}", email);
            throw new BusinessException(ConstMessages.InternalError);
        }
    }

    public async Task<WalletEntity?> GetWalletByIdAsync(Guid id)
    {
        return await context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<decimal> GetBalanceAsync(Guid walletId)
    {
        return await context.Wallets
            .Where(w => w.Id == walletId)
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> WalletExistsAsync(Guid walletId)
    {
        return await context.Wallets.AnyAsync(w => w.Id == walletId);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pgEx 
            && pgEx.SqlState == "23505";
    }
}