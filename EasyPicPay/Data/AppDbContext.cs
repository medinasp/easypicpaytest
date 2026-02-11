using EasyPicPay.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyPicPay.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet <WalletEntity> Wallets { get; set; }
    public DbSet <WalletEntity> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<WalletEntity>(entity =>
        {
            entity.HasIndex(e => e.Email)
                .IsUnique();
                
            entity.HasIndex(e => e.IdTaxDoc)
                .IsUnique();
                
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.IdTaxDoc)
                .IsRequired()
                .HasMaxLength(14);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.PasswordHash)
                .IsRequired();
                
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });
            
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.PayerId);
            entity.HasIndex(e => e.PayeeId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(e => e.AuthorizationCode)
                .HasMaxLength(100);
                
            entity.Property(e => e.FailureReason)
                .HasMaxLength(500);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            // Relacionamentos
            entity.HasOne(e => e.Payer)
                .WithMany()
                .HasForeignKey(e => e.PayerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Payee)
                .WithMany()
                .HasForeignKey(e => e.PayeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}