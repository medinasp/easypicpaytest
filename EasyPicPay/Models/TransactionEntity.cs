using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyPicPay.Models.Enums;

namespace EasyPicPay.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; private set; }
    
    [Required]
    public Guid PayerId { get; private set; }
    
    [Required]
    public Guid PayeeId { get; private set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; private set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; private set; } = TransactionStatus.Pending.ToString();
    
    [MaxLength(100)]
    public string? AuthorizationCode { get; private set; }
    
    public bool NotificationSent { get; private set; }
    
    [MaxLength(500)]
    public string? FailureReason { get; private set; }
    
    [Required]
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }
    
    public WalletEntity? Payer { get; private set; }
    public WalletEntity? Payee { get; private set; }
    
    // Construtor privado para EF Core
    private Transaction() { }
    
    // Construtor principal
    public Transaction(Guid payerId, Guid payeeId, decimal amount)
    {
        Id = Guid.NewGuid();
        PayerId = payerId;
        PayeeId = payeeId;
        Amount = amount;
        Status = TransactionStatus.Pending.ToString();
        CreatedAt = DateTime.UtcNow;
        
        Validate();
    }
    
    private void Validate()
    {
        if (Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero", nameof(Amount));
            
        if (PayerId == Guid.Empty)
            throw new ArgumentException("Pagador inválido", nameof(PayerId));
            
        if (PayeeId == Guid.Empty)
            throw new ArgumentException("Recebedor inválido", nameof(PayeeId));
            
        if (PayerId == PayeeId)
            throw new InvalidOperationException("Não é possível transferir para si mesmo");
    }
    
    // Métodos de domínio
    public void Complete(string authorizationCode)
    {
        if (Status != TransactionStatus.Pending.ToString())
            throw new InvalidOperationException($"Transação não está pendente. Status atual: {Status}");
            
        Status = TransactionStatus.Completed.ToString();
        AuthorizationCode = authorizationCode;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Fail(string reason)
    {
        if (Status != TransactionStatus.Pending.ToString())
            throw new InvalidOperationException($"Transação não está pendente. Status atual: {Status}");
            
        Status = TransactionStatus.Failed.ToString();
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkNotificationAsSent()
    {
        NotificationSent = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsCompleted => Status == nameof(TransactionStatus.Completed);
    public bool IsFailed => Status == nameof(TransactionStatus.Failed);
    public bool IsPending => Status == nameof(TransactionStatus.Pending);
}