using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Setup;

/// <summary>
/// Custom WebApplicationFactory for functional tests with isolated database
/// </summary>
public class FunctionalTestWebAppFactory : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("ambev_functional_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    /// <summary>
    /// Gets the database connection string for tests
    /// </summary>
    public string ConnectionString => _dbContainer.GetConnectionString();

    /// <summary>
    /// Creates the host builder for functional testing
    /// </summary>
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseEnvironment("FunctionalTesting");
                
                webBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Functional.json", optional: true, reloadOnChange: true);
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", ConnectionString)
                    });
                });
                
                webBuilder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add functional test database
                    services.AddDbContext<DefaultContext>(options =>
                    {
                        options.UseNpgsql(ConnectionString);
                    });

                    // Build service provider and migrate database
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                    context.Database.Migrate();
                });
                
                // Configure logging for functional tests
                webBuilder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });
    }

    /// <summary>
    /// Initialize the test container
    /// </summary>
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    /// <summary>
    /// Cleanup the test container
    /// </summary>
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Creates a scope for database operations
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Gets the database context for test setup
    /// </summary>
    public DefaultContext GetDbContext()
    {
        var scope = CreateScope();
        return scope.ServiceProvider.GetRequiredService<DefaultContext>();
    }
}