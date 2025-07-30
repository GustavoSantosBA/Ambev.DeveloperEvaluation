using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Handler for processing GetSalesQuery requests
/// </summary>
public class GetSalesHandler : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetSalesHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public GetSalesHandler(
        ISaleRepository saleRepository,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetSalesQuery request
    /// </summary>
    /// <param name="request">The GetSales query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The paginated sales list with metadata</returns>
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSalesValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get filtered sales with pagination
        var sales = await _saleRepository.GetFilteredAsync(
            request.Page,
            request.PageSize,
            request.CustomerId,
            request.BranchId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        // Get total count for pagination metadata
        var totalCount = await _saleRepository.GetFilteredCountAsync(
            request.CustomerId,
            request.BranchId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        // Map sales to result DTOs
        var mappedSales = _mapper.Map<List<GetSaleResult>>(sales);

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasPreviousPage = request.Page > 1;
        var hasNextPage = request.Page < totalPages;

        return new GetSalesResult
        {
            Sales = mappedSales,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage
        };
    }
}