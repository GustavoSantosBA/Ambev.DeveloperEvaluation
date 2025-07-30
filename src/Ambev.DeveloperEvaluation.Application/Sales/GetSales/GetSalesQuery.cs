using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Query for retrieving a list of sales with pagination and filtering support
/// </summary>
public record GetSalesQuery : IRequest<GetSalesResult>
{
    /// <summary>
    /// Page number (starting from 1)
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Optional customer ID filter
    /// </summary>
    public Guid? CustomerId { get; }

    /// <summary>
    /// Optional branch ID filter
    /// </summary>
    public Guid? BranchId { get; }

    /// <summary>
    /// Optional start date filter
    /// </summary>
    public DateTime? StartDate { get; }

    /// <summary>
    /// Optional end date filter
    /// </summary>
    public DateTime? EndDate { get; }

    /// <summary>
    /// Initializes a new instance of GetSalesQuery
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="branchId">Optional branch ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    public GetSalesQuery(
        int page = 1,
        int pageSize = 10,
        Guid? customerId = null,
        Guid? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Page = page;
        PageSize = pageSize;
        CustomerId = customerId;
        BranchId = branchId;
        StartDate = startDate;
        EndDate = endDate;
    }
}