namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

/// <summary>
/// Request model for getting sales with pagination and filters
/// </summary>
public class GetSalesRequest
{
    /// <summary>
    /// Page number (starting from 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional customer ID filter
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Optional branch ID filter
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Optional start date filter
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date filter
    /// </summary>
    public DateTime? EndDate { get; set; }
}