namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Response model for sale item in GetSale operation
/// </summary>
public class GetSaleItemResult
{
    /// <summary>
    /// The product external identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The product name (denormalized)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The quantity of items sold
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The unit price of the product
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The discount percentage applied (0-100)
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// The total amount for this item (calculated)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Whether this item is cancelled
    /// </summary>
    public bool IsCancelled { get; set; }
}