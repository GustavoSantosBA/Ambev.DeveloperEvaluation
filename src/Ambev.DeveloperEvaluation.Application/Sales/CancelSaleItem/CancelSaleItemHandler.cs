using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for processing CancelSaleItemCommand requests
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of CancelSaleItemHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="publisher">The MediatR publisher for domain events</param>
    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    /// <summary>
    /// Handles the CancelSaleItemCommand request
    /// </summary>
    /// <param name="request">The CancelSaleItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the item cancellation operation</returns>
    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get the sale
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        // Check if sale is cancelled
        if (sale.Status == Domain.Enums.SaleStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel items in a cancelled sale");

        // Find the item to cancel
        var itemToCancel = sale.Items.FirstOrDefault(i => i.ProductId == request.ProductId && !i.IsCancelled);
        if (itemToCancel == null)
            throw new KeyNotFoundException($"Product with ID {request.ProductId} not found in sale or already cancelled");

        // Store item details before cancellation for the result and event
        var cancelledQuantity = itemToCancel.Quantity;
        var cancelledAmount = itemToCancel.Total;
        var productName = itemToCancel.ProductName;

        // Cancel the item using domain method - this will update TotalAmount
        sale.CancelItem(request.ProductId);

        // Explicitly mark the entity as modified to ensure EF tracks the changes
        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish domain event
        await _publisher.Publish(new ItemCancelledEvent(updatedSale, itemToCancel, cancelledQuantity, cancelledAmount), cancellationToken);

        // Create and populate the result
        var result = _mapper.Map<CancelSaleItemResult>(updatedSale);
        result.ProductId = request.ProductId;
        result.ProductName = productName;
        result.CancelledQuantity = cancelledQuantity;
        result.CancelledAmount = cancelledAmount;

        return result;
    }
}