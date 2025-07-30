using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Setup;

/// <summary>
/// Base class for repository integration tests with real database
/// </summary>
public abstract class BaseRepositoryIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    protected DefaultContext DbContext { get; private set; } = null!;
    protected string ConnectionString => _dbContainer.GetConnectionString();

    protected BaseRepositoryIntegrationTest()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("ambev_repository_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Initialize the test container and database context
    /// </summary>
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        DbContext = new DefaultContext(options);
        await DbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Cleanup the test container and dispose context
    /// </summary>
    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }
        
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Cleans all data from the database tables
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        // Clean in correct order due to foreign key constraints
        DbContext.Sales.RemoveRange(DbContext.Sales);
        DbContext.Users.RemoveRange(DbContext.Users);
        
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a fresh database context for testing
    /// </summary>
    protected DefaultContext CreateFreshContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new DefaultContext(options);
    }
}