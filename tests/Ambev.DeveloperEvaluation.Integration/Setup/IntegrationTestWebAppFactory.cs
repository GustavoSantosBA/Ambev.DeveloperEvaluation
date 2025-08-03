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
    /// Configures the web host for testing - VERSÃO SIMPLIFICADA
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpar configurações existentes e adicionar as de teste
            config.Sources.Clear();
            config.AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true);
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", ConnectionString)
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover o DbContext existente
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Adicionar DbContext de teste
            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            // Remover autenticação JWT e usar autenticação de teste
            var authDescriptors = services.Where(d => d.ServiceType.Name.Contains("Authentication")).ToList();
            foreach (var authDescriptor in authDescriptors)
            {
                services.Remove(authDescriptor);
            }

            services.AddAuthentication("Test")
                .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });
        
        // Configurar logging mínimo para testes
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error); // Só erros
        });

        // Migrar banco após configuração dos serviços
        builder.ConfigureServices(services =>
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
            }
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
        return base.Services.CreateScope();
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