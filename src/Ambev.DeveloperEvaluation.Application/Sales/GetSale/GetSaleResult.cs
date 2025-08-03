using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Response model for GetSale operation
/// </summary>
public class GetSaleResult
{
    /// <summary>
    /// The unique identifier of the sale
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The sale number
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// The date when the sale was made
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// The customer external identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// The customer name (denormalized)
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The branch external identifier where the sale was made
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// The branch name (denormalized)
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// The total sale amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The sale status (Active/Cancelled)
    /// </summary>
    public SaleStatus Status { get; set; }

    /// <summary>
    /// The date when the sale was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date when the sale was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The collection of items in this sale
    /// </summary>
    public List<GetSaleItemResult> Items { get; set; } = new();
}