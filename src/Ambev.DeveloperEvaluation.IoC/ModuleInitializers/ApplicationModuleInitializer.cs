using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class ApplicationModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        // Security services
        builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        
        // Sales command and query handlers are automatically registered by MediatR
        // when AddMediatR is called in Program.cs, but we can register them explicitly
        // if needed for specific scenarios
        
        // Domain event handlers
        builder.Services.AddScoped<INotificationHandler<SaleCreatedEvent>, SaleCreatedEventHandler>();
        builder.Services.AddScoped<INotificationHandler<SaleModifiedEvent>, SaleModifiedEventHandler>();
        builder.Services.AddScoped<INotificationHandler<SaleCancelledEvent>, SaleCancelledEventHandler>();
        builder.Services.AddScoped<INotificationHandler<ItemCancelledEvent>, ItemCancelledEventHandler>();
        
        // Audit event handler (handles multiple events)
        builder.Services.AddScoped<SalesAuditEventHandler>();
        
        // Register the audit handler for each event type it handles
        builder.Services.AddScoped<INotificationHandler<SaleCreatedEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<SaleModifiedEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<SaleCancelledEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
        builder.Services.AddScoped<INotificationHandler<ItemCancelledEvent>>(provider => 
            provider.GetRequiredService<SalesAuditEventHandler>());
    }
}