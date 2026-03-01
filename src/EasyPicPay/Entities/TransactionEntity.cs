using EasyPicPay.Entities.Enums;
using EasyPicPay.Entities.EntityBase;

namespace EasyPicPay.Entities;

public class TransactionEntity : BaseEntity
{
    public Guid PayerId { get; private set; }
    
    public Guid PayeeId { get; private set; }
    
    public decimal Amount { get; private set; }
    
    public string Status { get; private set; } = nameof(TransactionStatus.Pending);
    
    public string? AuthorizationCode { get; private set; }
    
    public bool NotificationSent { get; private set; }
    
    public string? FailureReason { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }
    
    public WalletEntity? Payer { get; private set; }
    public WalletEntity? Payee { get; private set; }
    
    // Construtor privado para EF Core
    private TransactionEntity() { }
    
    // Construtor principal
    public TransactionEntity(Guid payerId, Guid payeeId, decimal amount)
    {
        Id = Guid.NewGuid();
        PayerId = payerId;
        PayeeId = payeeId;
        Amount = amount;
        Status = nameof(TransactionStatus.Pending);
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
        if (Status != nameof(TransactionStatus.Pending))
            throw new InvalidOperationException($"Transação não está pendente. Status atual: {Status}");
            
        Status = nameof(TransactionStatus.Completed);
        AuthorizationCode = authorizationCode;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Fail(string reason)
    {
        if (Status != nameof(TransactionStatus.Pending))
            throw new InvalidOperationException($"Transação não está pendente. Status atual: {Status}");
            
        Status = nameof(TransactionStatus.Failed);
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