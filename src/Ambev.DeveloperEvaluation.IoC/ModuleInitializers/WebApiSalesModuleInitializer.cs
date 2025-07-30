using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Module initializer for Sales WebApi-related dependencies
/// </summary>
public class WebApiSalesModuleInitializer : IModuleInitializer
{
    /// <summary>
    /// Registers all Sales WebApi-related dependencies in the IoC container
    /// </summary>
    /// <param name="builder">The web application builder</param>
    public void Initialize(WebApplicationBuilder builder)
    {
        RegisterApiProfiles(builder);
        RegisterApiValidators(builder);
    }

    /// <summary>
    /// Registers AutoMapper profiles for Sales API layer
    /// </summary>
    private static void RegisterApiProfiles(WebApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(typeof(SalesApiProfile));
    }

    /// <summary>
    /// Registers Sales API request validators
    /// </summary>
    private static void RegisterApiValidators(WebApplicationBuilder builder)
    {
        // Create Sale validators
        builder.Services.AddScoped<CreateSaleRequestValidator>();
        builder.Services.AddScoped<CreateSaleItemRequestValidator>();
        
        // Get Sale validators
        builder.Services.AddScoped<GetSaleRequestValidator>();
        builder.Services.AddScoped<GetSalesRequestValidator>();
        
        // Update Sale validators
        builder.Services.AddScoped<UpdateSaleRequestValidator>();
        builder.Services.AddScoped<UpdateSaleItemRequestValidator>();
        
        // Cancel Sale validators
        builder.Services.AddScoped<CancelSaleRequestValidator>();
        builder.Services.AddScoped<CancelSaleItemRequestValidator>();
    }
}