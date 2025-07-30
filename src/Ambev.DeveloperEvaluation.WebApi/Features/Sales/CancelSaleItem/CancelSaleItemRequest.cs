namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

/// <summary>
/// Request model for cancelling a specific item in a sale
/// </summary>
public class CancelSaleItemRequest
{
    /// <summary>
    /// The unique identifier of the sale containing the item
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// The unique identifier of the product item to cancel
    /// </summary>
    public Guid ProductId { get; set; }
}