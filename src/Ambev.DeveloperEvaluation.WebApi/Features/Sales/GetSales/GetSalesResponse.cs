namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

/// <summary>
/// Response model for GetSales operation with pagination
/// </summary>
public class GetSalesResponse
{
    /// <summary>
    /// The list of sales for the current page
    /// </summary>
    public List<GetSaleResponse> Sales { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of sales
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }
}