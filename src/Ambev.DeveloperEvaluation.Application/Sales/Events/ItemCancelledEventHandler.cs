using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for ItemCancelledEvent that logs the item cancellation
/// </summary>
public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of ItemCancelledEventHandler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the ItemCancelledEvent
    /// </summary>
    /// <param name="notification">The item cancelled event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Item Cancelled - Sale ID: {SaleId}, Sale Number: {SaleNumber}, Product: {ProductName} (ID: {ProductId}), " +
            "Cancelled Quantity: {CancelledQuantity}, Cancelled Amount: {CancelledAmount:C}, " +
            "New Sale Total: {NewSaleTotalAmount:C}, Cancelled At: {CancelledAt}, Reason: {CancellationReason}",
            notification.Sale.Id,
            notification.Sale.SaleNumber,
            notification.CancelledItem.ProductName,
            notification.CancelledItem.ProductId,
            notification.CancelledQuantity,
            notification.CancelledAmount,
            notification.NewSaleTotalAmount,
            notification.OccurredAt,
            notification.CancellationReason ?? "Not specified");

        return Task.CompletedTask;
    }
}