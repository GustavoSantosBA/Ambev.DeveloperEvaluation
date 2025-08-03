using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration.Setup;

/// <summary>
/// Startup class specifically for integration tests
/// </summary>
public class TestStartup
{
    public TestStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configures services for integration tests
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Basic services
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Database configuration
        services.AddDbContext<DefaultContext>(options =>
            options.UseNpgsql(
                Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
            )
        );

        // Authentication (simplified for tests)
        services.AddAuthentication("Test")
            .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

        // Dependencies registration
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddConfiguration(Configuration);
        // Substitua esta linha:
        // builder.Services.AddRange(services);

        // Pelo seguinte código:
        foreach (var service in services)
        {
            builder.Services.Add(service);
        }
        builder.RegisterDependencies();

        // Copy registered services back
        foreach (var service in builder.Services)
        {
            if (!services.Any(s => s.ServiceType == service.ServiceType))
            {
                services.Add(service);
            }
        }

        // AutoMapper configuration
        services.AddAutoMapper(
            typeof(TestStartup).Assembly,
            typeof(ApplicationLayer).Assembly);

        // MediatR configuration
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(ApplicationLayer).Assembly,
                typeof(TestStartup).Assembly
            );
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Migrate database
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        context.Database.Migrate();
    }

    /// <summary>
    /// Configures the HTTP request pipeline for tests
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ValidationExceptionMiddleware>();
        
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}