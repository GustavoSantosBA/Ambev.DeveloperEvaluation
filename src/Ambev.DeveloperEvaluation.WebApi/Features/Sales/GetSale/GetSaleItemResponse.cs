namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>
/// Response model for sale item in GetSale operation
/// </summary>
public class GetSaleItemResponse
{
    /// <summary>
    /// The product external identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The product name
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
    /// The discount percentage applied
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// The total amount for this item
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Whether this item is cancelled
    /// </summary>
    public bool IsCancelled { get; set; }
}