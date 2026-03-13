using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using EasyPicPay.Data;
using Xunit;

namespace TestEasyPicPay.Infrastructure;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private AppDbContext? _dbContext;

    public DatabaseFixture()
    {
        var builder = new PostgreSqlBuilder("repository:tag");
        _container = builder
            .WithImage("postgres:16-alpine")
            .WithDatabase("easypicpay_test")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _dbContext = CreateDbContext();
        await _dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
            await _dbContext.DisposeAsync();

        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Transactions\" CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Wallets\" CASCADE");
    }
}