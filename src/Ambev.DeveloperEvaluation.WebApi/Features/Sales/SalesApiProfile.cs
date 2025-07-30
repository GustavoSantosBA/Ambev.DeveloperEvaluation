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
        
        CreateMap<GetSaleResult, GetSaleResponse>();
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
        CreateMap<Guid, CancelSaleCommand>()
            .ConstructUsing(id => new CancelSaleCommand(id));

        CreateMap<CancelSaleResult, CancelSaleResponse>();
    }

    /// <summary>
    /// Configures mappings for CancelSaleItem operations
    /// </summary>
    private void ConfigureCancelSaleItemMappings()
    {
        CreateMap<CancelSaleItemResult, CancelSaleItemResponse>();
    }
}