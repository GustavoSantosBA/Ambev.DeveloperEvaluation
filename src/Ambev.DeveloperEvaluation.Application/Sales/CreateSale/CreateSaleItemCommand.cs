namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Represents an item to be included in a sale creation command.
/// </summary>
public class CreateSaleItemCommand
{
    /// <summary>
    /// Gets or sets the product external identifier.
    /// Following the External Identities pattern with denormalization.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name for denormalized reference.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of items to be sold.
    /// Must be between 1 and 20 (business rule).
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// Must be greater than zero.
    /// </summary>
    public decimal UnitPrice { get; set; }
}