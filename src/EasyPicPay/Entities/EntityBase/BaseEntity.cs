namespace EasyPicPay.Entities.EntityBase;

public abstract class BaseEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected init; } = DateTime.UtcNow;
}