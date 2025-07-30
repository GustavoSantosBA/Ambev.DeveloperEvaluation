using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Setup;

/// <summary>
/// Base class for functional tests providing common setup and utilities
/// </summary>
public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly FunctionalTestWebAppFactory Factory;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    /// <summary>
    /// Cleans the database before each test scenario
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        
        // Clean in proper order due to foreign key constraints
        context.Sales.RemoveRange(context.Sales);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds test data into the database for scenarios
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