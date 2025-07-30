using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Ambev.DeveloperEvaluation.Integration.Setup;

/// <summary>
/// Custom WebApplicationFactory for integration tests with test database setup
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("ambev_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    /// <summary>
    /// Gets the database connection string for tests
    /// </summary>
    public string ConnectionString => _dbContainer.GetConnectionString();

    /// <summary>
    /// Configures the web host for testing
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database
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

        builder.UseEnvironment("Testing");
        
        // Suppress logging during tests to reduce noise
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
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