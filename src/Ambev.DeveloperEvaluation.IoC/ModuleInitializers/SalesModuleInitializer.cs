using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Module initializer for Sales-related dependencies
/// </summary>
public class SalesModuleInitializer : IModuleInitializer
{
    /// <summary>
    /// Registers all Sales-related dependencies in the IoC container
    /// </summary>
    /// <param name="builder">The web application builder</param>
    public void Initialize(WebApplicationBuilder builder)
    {
        RegisterRepositories(builder);
        RegisterEventHandlers(builder);
    }

    /// <summary>
    /// Registers Sales repositories
    /// </summary>
    private static void RegisterRepositories(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();
    }

    /// <summary>
    /// Registers Sales domain event handlers
    /// </summary>
    private static void RegisterEventHandlers(WebApplicationBuilder builder)
    {
        // Domain event handlers
        builder.Services.AddScoped<INotificationHandler<SaleCreatedEvent>, SaleCreatedEventHandler>();
        builder.Services.AddScoped<INotificationHandler<SaleModifiedEvent>, SaleModifiedEventHandler>();
        builder.Services.AddScoped<INotificationHandler<SaleCancelledEvent>, SaleCancelledEventHandler>();
        builder.Services.AddScoped<INotificationHandler<ItemCancelledEvent>, ItemCancelledEventHandler>();
        
        // Audit event handler (handles multiple events)
        builder.Services.AddScoped<SalesAuditEventHandler>();
    }

    // Removed RegisterHandlers() - MediatR automatically discovers handlers via AddMediatR() in Program.cs
    // Removed RegisterValidators() - Validators are used directly, no DI registration needed
    // Removed RegisterProfiles() - AutoMapper automatically discovers profiles via AddAutoMapper() in Program.cs
}