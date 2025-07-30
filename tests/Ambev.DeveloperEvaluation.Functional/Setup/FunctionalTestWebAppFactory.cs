using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Ambev.DeveloperEvaluation.Functional.Setup;

/// <summary>
/// Custom WebApplicationFactory for functional tests with isolated database
/// </summary>
public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
    /// Configures the web host for functional testing
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Functional.json", optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
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

        builder.UseEnvironment("FunctionalTesting");
        
        // Configure logging for functional tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
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