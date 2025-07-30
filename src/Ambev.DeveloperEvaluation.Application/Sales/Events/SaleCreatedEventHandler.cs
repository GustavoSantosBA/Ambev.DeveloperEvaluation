using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for SaleCreatedEvent that logs the sale creation
/// </summary>
public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleCreatedEventHandler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the SaleCreatedEvent
    /// </summary>
    /// <param name="notification">The sale created event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sale Created - Sale ID: {SaleId}, Sale Number: {SaleNumber}, Customer: {CustomerName}, " +
            "Branch: {BranchName}, Total Amount: {TotalAmount:C}, Items Count: {ItemsCount}, Created At: {CreatedAt}",
            notification.Sale.Id,
            notification.Sale.SaleNumber,
            notification.Sale.CustomerName,
            notification.Sale.BranchName,
            notification.Sale.TotalAmount,
            notification.Sale.Items.Count,
            notification.OccurredAt);

        // Log each item details
        foreach (var item in notification.Sale.Items)
        {
            _logger.LogInformation(
                "Sale Item Created - Sale ID: {SaleId}, Product: {ProductName} (ID: {ProductId}), " +
                "Quantity: {Quantity}, Unit Price: {UnitPrice:C}, Discount: {Discount}%, Total: {Total:C}",
                notification.Sale.Id,
                item.ProductName,
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.Discount,
                item.Total);
        }

        return Task.CompletedTask;
    }
}