using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
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
        RegisterHandlers(builder);
        RegisterEventHandlers(builder);
        RegisterValidators(builder);
        RegisterProfiles(builder);
    }

    /// <summary>
    /// Registers Sales repositories
    /// </summary>
    private static void RegisterRepositories(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();
    }

    /// <summary>
    /// Registers Sales command and query handlers
    /// </summary>
    private static void RegisterHandlers(WebApplicationBuilder builder)
    {
        // Create Sale handlers
        builder.Services.AddScoped<IRequestHandler<CreateSaleCommand, CreateSaleResult>, CreateSaleHandler>();
        
        // Get Sale handlers
        builder.Services.AddScoped<IRequestHandler<GetSaleQuery, GetSaleResult>, GetSaleHandler>();
        builder.Services.AddScoped<IRequestHandler<GetSalesQuery, GetSalesResult>, GetSalesHandler>();
        
        // Update Sale handlers
        builder.Services.AddScoped<IRequestHandler<UpdateSaleCommand, UpdateSaleResult>, UpdateSaleHandler>();
        
        // Cancel Sale handlers
        builder.Services.AddScoped<IRequestHandler<CancelSaleCommand, CancelSaleResult>, CancelSaleHandler>();
        builder.Services.AddScoped<IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>, CancelSaleItemHandler>();
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
        builder.Services.AddScoped<INotificationHandler<SaleCreatedEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<SaleModifiedEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<SaleCancelledEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<ItemCancelledEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
    }

    /// <summary>
    /// Registers Sales validators
    /// </summary>
    private static void RegisterValidators(WebApplicationBuilder builder)
    {
        // Command validators
        builder.Services.AddScoped<CreateSaleCommandValidator>();
        builder.Services.AddScoped<UpdateSaleCommandValidator>();
        builder.Services.AddScoped<CancelSaleValidator>();
        builder.Services.AddScoped<CancelSaleItemValidator>();
        builder.Services.AddScoped<GetSaleValidator>();
        builder.Services.AddScoped<GetSalesValidator>();
        
        // Item validators
        builder.Services.AddScoped<CreateSaleItemCommandValidator>();
        builder.Services.AddScoped<UpdateSaleItemCommandValidator>();
    }

    /// <summary>
    /// Registers AutoMapper profiles for Sales
    /// </summary>
    private static void RegisterProfiles(WebApplicationBuilder builder)
    {
        // Application layer profiles
        builder.Services.AddAutoMapper(typeof(SalesProfile));
        builder.Services.AddAutoMapper(typeof(CreateSaleProfile));
        builder.Services.AddAutoMapper(typeof(GetSaleProfile));
        builder.Services.AddAutoMapper(typeof(GetSalesProfile));
        builder.Services.AddAutoMapper(typeof(UpdateSaleProfile));
        builder.Services.AddAutoMapper(typeof(CancelSaleProfile));
        builder.Services.AddAutoMapper(typeof(CancelSaleItemProfile));
    }
}