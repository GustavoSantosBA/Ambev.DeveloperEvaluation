using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for SaleCancelledEvent that logs sale cancellations with impact analysis
/// </summary>
public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleCancelledEventHandler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the SaleCancelledEvent with comprehensive impact logging
    /// </summary>
    /// <param name="notification">The sale cancelled event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("SaleCancelled-{SaleId}", notification.Sale.Id);

        try
        {
            // Log main cancellation event with WARNING level
            _logger.LogWarning(
                "SALE CANCELLED - ID: {SaleId}, Number: {SaleNumber}, Customer: {CustomerName} ({CustomerId}), " +
                "Branch: {BranchName} ({BranchId}), Cancelled Amount: {CancelledAmount:C}, " +
                "Original Sale Date: {SaleDate}, Cancelled At: {CancelledAt}, Reason: {CancellationReason}",
                notification.Sale.Id,
                notification.Sale.SaleNumber,
                notification.Sale.CustomerName,
                notification.Sale.CustomerId,
                notification.Sale.BranchName,
                notification.Sale.BranchId,
                notification.CancelledAmount,
                notification.Sale.SaleDate,
                notification.OccurredAt,
                notification.CancellationReason ?? "No reason specified");

            // Log business impact
            LogCancellationImpact(notification);

            // Log all items that were part of the cancelled sale
            LogCancelledItems(notification);

            _logger.LogWarning("Sale cancellation event processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SaleCancelledEvent for Sale ID: {SaleId}", notification.Sale.Id);
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs the business impact of the sale cancellation
    /// </summary>
    private void LogCancellationImpact(SaleCancelledEvent notification)
    {
        var sale = notification.Sale;
        var totalItems = sale.Items.Sum(i => i.Quantity);
        var totalDiscountLost = sale.Items.Sum(i => (i.UnitPrice * i.Quantity) - i.Total);

        _logger.LogWarning(
            "Cancellation Business Impact - Sale ID: {SaleId}, Revenue Lost: {RevenueLost:C}, " +
            "Total Items Affected: {TotalItems}, Unique Products: {UniqueProducts}, " +
            "Total Discount Lost: {DiscountLost:C}, Sale Duration: {SaleDuration}",
            sale.Id,
            notification.CancelledAmount,
            totalItems,
            sale.Items.Count,
            totalDiscountLost,
            notification.OccurredAt - sale.CreatedAt);

        // Log if this was a high-value cancellation
        if (notification.CancelledAmount > 1000) // Configurable threshold
        {
            _logger.LogError(
                "HIGH-VALUE SALE CANCELLED - Sale ID: {SaleId}, Amount: {CancelledAmount:C} - " +
                "Requires management attention",
                sale.Id,
                notification.CancelledAmount);
        }
    }

    /// <summary>
    /// Logs all items that were part of the cancelled sale
    /// </summary>
    private void LogCancelledItems(SaleCancelledEvent notification)
    {
        _logger.LogWarning("Items in cancelled sale {SaleId}:", notification.Sale.Id);

        foreach (var (item, index) in notification.Sale.Items.Select((item, index) => (item, index + 1)))
        {
            var itemValue = item.UnitPrice * item.Quantity;
            var discountAmount = itemValue - item.Total;

            _logger.LogWarning(
                "Cancelled Item #{ItemNumber} - {ProductName} ({ProductId}): {Quantity} units " +
                "@ {UnitPrice:C} each, Discount: {DiscountAmount:C} ({Discount}%), " +
                "Lost Revenue: {Total:C}, Status: {ItemStatus}",
                index,
                item.ProductName,
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                discountAmount,
                item.Discount,
                item.Total,
                item.IsCancelled ? "Already Cancelled" : "Active (Now Cancelled)");
        }
    }
}