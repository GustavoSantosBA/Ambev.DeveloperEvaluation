using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);

            var app = builder.Build();
            ConfigurePipeline(app);

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        try
        {
            Log.Information("Configuring services");

            // Logging configuration
            builder.AddDefaultLogging();

            // Basic services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Database configuration
            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            // Health checks
            builder.AddBasicHealthChecks();

            // Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Dependencies registration
            builder.RegisterDependencies();

            // AutoMapper configuration
            builder.Services.AddAutoMapper(
                typeof(Program).Assembly, 
                typeof(ApplicationLayer).Assembly);

            // MediatR configuration
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error configuring services");
            throw;
        }
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        try
        {
            Log.Information("Configuring application pipeline");

            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseBasicHealthChecks();
            app.MapControllers();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error configuring application pipeline");
            throw;
        }
    }

    // Factory method for tests - creates a WebApplication instance
    public static WebApplication CreateApp(string[] args = null)
    {
        args ??= Array.Empty<string>();
        
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);
        
        var app = builder.Build();
        ConfigurePipeline(app);
        
        return app;
    }
}
