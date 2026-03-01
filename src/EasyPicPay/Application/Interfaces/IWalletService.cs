using EasyPicPay.Entities;
using EasyPicPay.Entities.Enums;

namespace EasyPicPay.Application.Interfaces;

public interface IWalletService
{
    Task<WalletEntity> CreateWalletAsync(string name, string cpfCnpj, 
        string email, string password, UserType userType);
    Task<WalletEntity?> GetWalletByIdAsync(Guid id);
    Task<decimal> GetBalanceAsync(Guid walletId);
    Task<bool> WalletExistsAsync(Guid walletId);
    
}