using Microsoft.EntityFrameworkCore;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Data;
using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;
using EasyPicPay.Exceptions;

namespace EasyPicPay.Application.Services;

public class WalletService(AppDbContext context, ILogger<WalletService> logger) : IWalletService
{
    public async Task<WalletEntity> CreateWalletAsync(string name, string cpfCnpj, 
        string email, string password, UserType userType)
    {
        try
        {
            // Verifica se email/CPF já existe
            var exists = await context.Wallets
                .AnyAsync(w => w.Email == email || w.IdTaxDoc == cpfCnpj);
                
            if (exists)
                throw new BusinessException("Email ou CPF/CNPJ já cadastrado");

            // Hash da senha (simplificado)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var wallet = new WalletEntity(name, cpfCnpj, email, passwordHash, userType);
            
            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Wallet criada: {WalletId}", wallet.Id);
            return wallet;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar wallet");
            throw;
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
        var wallet = await GetWalletByIdAsync(walletId);
        return wallet?.Balance ?? 0;
    }

    public async Task<bool> WalletExistsAsync(Guid walletId)
    {
        return await context.Wallets
            .AnyAsync(w => w.Id == walletId);
    }    
}