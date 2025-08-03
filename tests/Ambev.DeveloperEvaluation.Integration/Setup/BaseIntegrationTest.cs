using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Setup;

/// <summary>
/// Base class for integration tests providing common setup and utilities
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly IntegrationTestWebAppFactory Factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        //HttpClient.BaseAddress = new Uri("https://localhost:7181"); // Adjust as needed
    }

    /// <summary>
    /// Cleans the database before each test
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        
        // Remove all data but keep schema
        context.Sales.RemoveRange(context.Sales);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds test data into the database
    /// </summary>
    protected async Task SeedDatabaseAsync(Action<DefaultContext> seedAction)
    {
        using var scope = Factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        
        seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a fresh database context for assertions
    /// </summary>
    protected DefaultContext GetDbContext()
    {
        return Factory.GetDbContext();
    }
}