using EasyPicPay.Entities.Enums;
using EasyPicPay.Entities.EntityBase;
    
namespace EasyPicPay.Entities;

public class WalletEntity : BaseEntity
{
    public string Name { get; private set; }
    
    public string IdTaxDoc { get; private set; }
    
    public string Email { get; private set; }
    
    public string PasswordHash { get; private set; }
    
    public UserType UserType { get; private set; }
    
    public decimal Balance { get; private set; }
    
    private WalletEntity(){}

    public WalletEntity(string name, string cpfCnpj, string email, 
        string passwordHash, UserType userType)
    {
        Name = name;
        IdTaxDoc = cpfCnpj;
        Email = email;
        PasswordHash = passwordHash;
        UserType = userType;
        Balance = 0;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Nome é obrigatório");
            
        if (!IsValidCpfCnpj(IdTaxDoc))
            throw new ArgumentException("CPF/CNPJ inválido");
    }
    
    public bool CanSendMoney() => UserType == UserType.Common;
    
    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");
            
        if (Balance < amount)
            throw new InvalidOperationException("Saldo insuficiente");
            
        Balance -= amount;
    }
    
    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");
            
        Balance += amount;
    }
    
    private static bool IsValidCpfCnpj(string idTaxDoc)
    {
        return !string.IsNullOrWhiteSpace(idTaxDoc) && 
               (idTaxDoc.Length == 11 || idTaxDoc.Length == 14);
    }
}