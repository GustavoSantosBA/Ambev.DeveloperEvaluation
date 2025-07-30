using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling a specific item in a sale
/// </summary>
public record CancelSaleItemCommand : IRequest<CancelSaleItemResult>
{
    /// <summary>
    /// The unique identifier of the sale containing the item to cancel
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// The unique identifier of the product item to cancel
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Initializes a new instance of CancelSaleItemCommand
    /// </summary>
    /// <param name="saleId">The ID of the sale</param>
    /// <param name="productId">The ID of the product to cancel</param>
    public CancelSaleItemCommand(Guid saleId, Guid productId)
    {
        SaleId = saleId;
        ProductId = productId;
    }
}