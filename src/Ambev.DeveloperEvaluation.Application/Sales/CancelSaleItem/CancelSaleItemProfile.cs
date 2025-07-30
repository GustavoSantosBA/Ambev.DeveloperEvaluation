using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Profile for mapping between Sale/SaleItem entities and CancelSaleItemResult
/// </summary>
public class CancelSaleItemProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CancelSaleItem operation
    /// </summary>
    public CancelSaleItemProfile()
    {
        CreateMap<Sale, CancelSaleItemResult>()
            .ForMember(dest => dest.SaleId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NewSaleTotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));
    }
}