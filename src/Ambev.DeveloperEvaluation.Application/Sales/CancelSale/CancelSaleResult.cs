namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Response model for CancelSale operation
/// </summary>
public class CancelSaleResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the cancelled sale.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sale number of the cancelled sale.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the sale was cancelled.
    /// </summary>
    public DateTime CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch name where the sale was made.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount that was cancelled.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Indicates whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }
}