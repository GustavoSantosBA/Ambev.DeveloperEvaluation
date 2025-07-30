namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Response model for CancelSaleItem operation
/// </summary>
public class CancelSaleItemResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the sale.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the sale number.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product identifier that was cancelled.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name that was cancelled.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity that was cancelled.
    /// </summary>
    public int CancelledQuantity { get; set; }

    /// <summary>
    /// Gets or sets the amount that was cancelled.
    /// </summary>
    public decimal CancelledAmount { get; set; }

    /// <summary>
    /// Gets or sets the new total amount of the sale after cancellation.
    /// </summary>
    public decimal NewSaleTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the date when the item was cancelled.
    /// </summary>
    public DateTime CancelledAt { get; set; }

    /// <summary>
    /// Indicates whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }
}