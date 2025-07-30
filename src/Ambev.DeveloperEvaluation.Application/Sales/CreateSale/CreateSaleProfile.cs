using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Profile for mapping CreateSale specific operations
/// Note: Main mappings are in SalesProfile, this provides additional customizations
/// </summary>
public class CreateSaleProfile : Profile
{
    /// <summary>
    /// Initializes additional mappings for CreateSale operation
    /// </summary>
    public CreateSaleProfile()
    {
        // Additional specific mappings if needed
        // Main mappings are handled in the consolidated SalesProfile
        
        // Custom mapping for complex scenarios
        CreateMap<CreateSaleCommand, Sale>()
            .AfterMap((src, dest, context) =>
            {
                // Ensure all items are properly initialized
                foreach (var item in dest.Items)
                {
                    if (item.Discount == 0 && item.Total == 0)
                    {
                        item.ApplyDiscountRules();
                    }
                }
            });
    }
}