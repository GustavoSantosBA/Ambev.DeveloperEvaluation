using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Profile for mapping between Sale entity and CancelSaleResult
/// </summary>
public class CancelSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CancelSale operation
    /// </summary>
    public CancelSaleProfile()
    {
        CreateMap<Sale, CancelSaleResult>()
            .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));
    }
}