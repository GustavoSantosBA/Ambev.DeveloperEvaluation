using Ambev.DeveloperEvaluation.IoC;
using Microsoft.AspNetCore.Builder;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Module initializer for Sales WebApi-related dependencies
/// This module is currently empty as AutoMapper profiles and validators
/// are automatically discovered by the framework in Program.cs
/// </summary>
public class WebApiSalesModuleInitializer : IModuleInitializer
{
    /// <summary>
    /// Registers all Sales WebApi-related dependencies in the IoC container
    /// </summary>
    /// <param name="builder">The web application builder</param>
    public void Initialize(WebApplicationBuilder builder)
    {
        // No explicit registrations needed
        // AutoMapper profiles are discovered automatically by AddAutoMapper() in Program.cs
        // Request validators are used directly in controllers without DI registration
    }
}