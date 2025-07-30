using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Response model for UpdateSale operation
/// </summary>
public class UpdateSaleResponse
{
    /// <summary>
    /// The unique identifier of the updated sale
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The sale number of the updated sale
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// The total amount of the sale
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The date when the sale was made
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// The customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The branch name where the sale was made
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// The number of items in the sale
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// The date when the sale was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The status of the sale
    /// </summary>
    public SaleStatus Status { get; set; }
}