using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyPicPay.Models.Enums;
    
namespace EasyPicPay.Models;

public sealed record WalletEntity
{
    [Key]
    public Guid Id { get; private set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; private set; }
    
    [Required]
    [MaxLength(14)]    
    public string IdTaxDoc { get; private set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; private set; }
    
    [Required]
    public string PasswordHash { get; private set; }
    
    [Required]
    public UserType UserType { get; private set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    private WalletEntity(){}

    public WalletEntity(string name, string cpfCnpj, string email, 
        string passwordHash, UserType userType)
    {
        Id = Guid.NewGuid();
        Name = name;
        IdTaxDoc = cpfCnpj;
        Email = email;
        PasswordHash = passwordHash;
        UserType = userType;
        Balance = 0;
        CreatedAt = DateTime.UtcNow;
        
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