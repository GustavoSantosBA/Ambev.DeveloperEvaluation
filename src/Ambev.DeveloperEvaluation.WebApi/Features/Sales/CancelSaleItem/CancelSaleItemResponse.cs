namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

/// <summary>
/// Response model for CancelSaleItem operation
/// </summary>
public class CancelSaleItemResponse
{
    /// <summary>
    /// The unique identifier of the sale
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// The sale number
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// The product identifier that was cancelled
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The product name that was cancelled
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The quantity that was cancelled
    /// </summary>
    public int CancelledQuantity { get; set; }

    /// <summary>
    /// The amount that was cancelled
    /// </summary>
    public decimal CancelledAmount { get; set; }

    /// <summary>
    /// The new total amount of the sale after cancellation
    /// </summary>
    public decimal NewSaleTotalAmount { get; set; }

    /// <summary>
    /// The date when the item was cancelled
    /// </summary>
    public DateTime CancelledAt { get; set; }

    /// <summary>
    /// Indicates whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }
}