using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// AutoMapper profile for Sales API mappings between Request/Response DTOs and Application layer Commands/Results
/// </summary>
public class SalesApiProfile : Profile
{
    /// <summary>
    /// Initializes mappings for all Sales operations
    /// </summary>
    public SalesApiProfile()
    {
        ConfigureCreateSaleMappings();
        ConfigureGetSaleMappings();
        ConfigureGetSalesMappings();
        ConfigureUpdateSaleMappings();
        ConfigureCancelSaleMappings();
        ConfigureCancelSaleItemMappings();
    }

    /// <summary>
    /// Configures mappings for CreateSale operations
    /// </summary>
    private void ConfigureCreateSaleMappings()
    {
        CreateMap<CreateSaleRequest, CreateSaleCommand>();
        CreateMap<CreateSaleItemRequest, CreateSaleItemCommand>();
        CreateMap<CreateSaleResult, CreateSaleResponse>();
    }

    /// <summary>
    /// Configures mappings for GetSale operations
    /// </summary>
    private void ConfigureGetSaleMappings()
    {
        CreateMap<Guid, GetSaleQuery>()
            .ConstructUsing(id => new GetSaleQuery(id));
        
        // CRITICAL FIX: Explicit mapping for all properties
        CreateMap<GetSaleResult, GetSaleResponse>()
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

        CreateMap<GetSaleItemResult, GetSaleItemResponse>();
    }

    /// <summary>
    /// Configures mappings for GetSales operations
    /// </summary>
    private void ConfigureGetSalesMappings()
    {
        CreateMap<GetSalesRequest, GetSalesQuery>()
            .ConstructUsing(src => new GetSalesQuery(
                src.Page,
                src.PageSize,
                src.CustomerId,
                src.BranchId,
                src.StartDate,
                src.EndDate));

        CreateMap<GetSalesResult, GetSalesResponse>()
            .ForMember(dest => dest.Sales, opt => opt.MapFrom(src => src.Sales));
    }

    /// <summary>
    /// Configures mappings for UpdateSale operations
    /// </summary>
    private void ConfigureUpdateSaleMappings()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Set manually in controller

        CreateMap<UpdateSaleItemRequest, UpdateSaleItemCommand>();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>();
    }

    /// <summary>
    /// Configures mappings for CancelSale operations
    /// </summary>
    private void ConfigureCancelSaleMappings()
    {
        // FIX: Mapear de CancelSaleRequest para CancelSaleCommand
        CreateMap<CancelSaleRequest, CancelSaleCommand>()
            .ConstructUsing(src => new CancelSaleCommand(src.Id));

        // E também manter o mapeamento direto de Guid para CancelSaleCommand
        CreateMap<Guid, CancelSaleCommand>()
            .ConstructUsing(id => new CancelSaleCommand(id));

        CreateMap<CancelSaleResult, CancelSaleResponse>()
            .ForMember(dest => dest.SaleNumber, opt => opt.MapFrom(src => src.SaleNumber)); // Explicit mapping
    }

    /// <summary>
    /// Configures mappings for CancelSaleItem operations
    /// </summary>
    private void ConfigureCancelSaleItemMappings()
    {
        CreateMap<CancelSaleItemResult, CancelSaleItemResponse>();
    }
}