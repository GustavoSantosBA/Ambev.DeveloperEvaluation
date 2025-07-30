using Ambev.DeveloperEvaluation.IoC.ModuleInitializers;
using Microsoft.AspNetCore.Builder;

namespace Ambev.DeveloperEvaluation.IoC;

/// <summary>
/// Extension methods for dependency injection setup
/// </summary>
public static class DependencyResolver
{
    /// <summary>
    /// Registers all application dependencies
    /// </summary>
    /// <param name="builder">The web application builder</param>
    public static void RegisterDependencies(this WebApplicationBuilder builder)
    {
        // Core modules
        new ApplicationModuleInitializer().Initialize(builder);
        new InfrastructureModuleInitializer().Initialize(builder);
        new WebApiModuleInitializer().Initialize(builder);
        
        // Sales module
        new SalesModuleInitializer().Initialize(builder);
        
        // Removed WebApiSalesModuleInitializer as it's not needed
        // AutoMapper and validators are discovered automatically
    }
}