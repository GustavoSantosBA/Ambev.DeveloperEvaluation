using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Main AutoMapper profile for Sale entities and all related operations
/// Consolidates all sale-related mappings in one place for better organization
/// </summary>
public class SalesProfile : Profile
{
    /// <summary>
    /// Initializes all mappings for Sale entities and commands/results
    /// </summary>
    public SalesProfile()
    {
        ConfigureSaleEntityMappings();
        ConfigureCreateSaleMappings();
        ConfigureUpdateSaleMappings();
        ConfigureGetSaleMappings();
        ConfigureCancelSaleMappings();
        ConfigureCancelSaleItemMappings();
    }

    /// <summary>
    /// Configures basic entity-to-entity mappings
    /// </summary>
    private void ConfigureSaleEntityMappings()
    {
        // Sale entity mappings
        CreateMap<Sale, Sale>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Preserve existing ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Preserve creation date
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // SaleItem entity mappings
        CreateMap<SaleItem, SaleItem>()
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore()); // Preserve cancellation status
    }

    /// <summary>
    /// Configures mappings for CreateSale operations
    /// </summary>
    private void ConfigureCreateSaleMappings()
    {
        // Command to Entity mappings
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.SaleStatus.Active))
            .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => src.SaleDate ?? DateTime.UtcNow))
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber)) // Garantir mapeamento explícito
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Calculated later
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<CreateSaleItemCommand, SaleItem>()
            .ForMember(dest => dest.Discount, opt => opt.Ignore()) // Applied by business rules
            .ForMember(dest => dest.Total, opt => opt.Ignore()) // Calculated later
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => false));

        // Entity to Result mappings
        CreateMap<Sale, CreateSaleResult>()
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber)) // Garantir mapeamento explícito
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count(i => !i.IsCancelled)));
    }

    /// <summary>
    /// Configures mappings for UpdateSale operations
    /// </summary>
    private void ConfigureUpdateSaleMappings()
    {
        // Entity to Result mappings
        CreateMap<Sale, UpdateSaleResult>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count(i => !i.IsCancelled)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow));

        CreateMap<UpdateSaleItemCommand, SaleItem>()
            .ForMember(dest => dest.Discount, opt => opt.Ignore()) // Applied by business rules
            .ForMember(dest => dest.Total, opt => opt.Ignore()) // Calculated later
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => false));
    }

    /// <summary>
    /// Configures mappings for GetSale operations
    /// </summary>
    private void ConfigureGetSaleMappings()
    {
        // CRITICAL FIX: Explicit mapping for all properties
        CreateMap<Sale, GetSaleResult>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber))
            .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => src.SaleDate))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.BranchName))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleItem, GetSaleItemResult>();
    }

    /// <summary>
    /// Configures mappings for CancelSale operations
    /// </summary>
    private void ConfigureCancelSaleMappings()
    {
        CreateMap<Sale, CancelSaleResult>()
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));
    }

    /// <summary>
    /// Configures mappings for CancelSaleItem operations
    /// </summary>
    private void ConfigureCancelSaleItemMappings()
    {
        CreateMap<Sale, CancelSaleItemResult>()
            .ForMember(dest => dest.SaleId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NewSaleTotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.CancelledQuantity, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.CancelledAmount, opt => opt.Ignore()); // Set manually in handler
    }
}