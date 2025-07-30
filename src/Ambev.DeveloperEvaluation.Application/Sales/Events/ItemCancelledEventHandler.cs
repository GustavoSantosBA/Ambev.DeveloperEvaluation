using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for ItemCancelledEvent that logs item cancellations with detailed analysis
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
    /// Handles the ItemCancelledEvent with comprehensive analysis
    /// </summary>
    /// <param name="notification">The item cancelled event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("ItemCancelled-{SaleId}-{ProductId}", 
            notification.Sale.Id, notification.CancelledItem.ProductId);

        try
        {
            // Log main item cancellation event
            _logger.LogWarning(
                "ITEM CANCELLED - Sale ID: {SaleId}, Sale Number: {SaleNumber}, " +
                "Product: {ProductName} ({ProductId}), Cancelled Qty: {CancelledQuantity}, " +
                "Unit Price: {UnitPrice:C}, Cancelled Amount: {CancelledAmount:C}, " +
                "New Sale Total: {NewSaleTotalAmount:C}, Cancelled At: {CancelledAt}, " +
                "Reason: {CancellationReason}",
                notification.Sale.Id,
                notification.Sale.SaleNumber,
                notification.CancelledItem.ProductName,
                notification.CancelledItem.ProductId,
                notification.CancelledQuantity,
                notification.CancelledItem.UnitPrice,
                notification.CancelledAmount,
                notification.NewSaleTotalAmount,
                notification.OccurredAt,
                notification.CancellationReason ?? "No reason specified");

            // Log impact analysis
            LogCancellationImpact(notification);

            // Log remaining items in sale
            LogRemainingSaleItems(notification);

            _logger.LogInformation("Item cancellation event processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ItemCancelledEvent for Sale ID: {SaleId}, Product ID: {ProductId}", 
                notification.Sale.Id, notification.CancelledItem.ProductId);
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs the impact analysis of the item cancellation
    /// </summary>
    private void LogCancellationImpact(ItemCancelledEvent notification)
    {
        var sale = notification.Sale;
        var originalAmount = notification.NewSaleTotalAmount + notification.CancelledAmount;
        var impactPercentage = originalAmount > 0 ? (notification.CancelledAmount / originalAmount) * 100 : 0;
        var remainingActiveItems = sale.Items.Count(i => !i.IsCancelled);
        var totalCancelledItems = sale.Items.Count(i => i.IsCancelled);

        _logger.LogWarning(
            "Cancellation Impact Analysis - Sale ID: {SaleId}, Revenue Impact: {CancelledAmount:C} " +
            "({ImpactPercentage:F2}% of original sale), Remaining Items: {RemainingItems}, " +
            "Total Cancelled Items: {TotalCancelledItems}, Original Amount: {OriginalAmount:C}",
            sale.Id,
            notification.CancelledAmount,
            impactPercentage,
            remainingActiveItems,
            totalCancelledItems,
            originalAmount);

        // Check if this leaves the sale in a critical state
        if (remainingActiveItems == 0)
        {
            _logger.LogError(
                "CRITICAL: Sale {SaleId} has no remaining active items after cancellation - " +
                "Consider cancelling entire sale",
                sale.Id);
        }
        else if (notification.NewSaleTotalAmount < 10) // Configurable threshold
        {
            _logger.LogWarning(
                "LOW-VALUE SALE WARNING: Sale {SaleId} total is now {NewTotal:C} - " +
                "Below minimum threshold",
                sale.Id,
                notification.NewSaleTotalAmount);
        }

        // Log discount impact
        var cancelledDiscount = (notification.CancelledItem.UnitPrice * notification.CancelledQuantity) - notification.CancelledAmount;
        if (cancelledDiscount > 0)
        {
            _logger.LogInformation(
                "Discount Impact - Sale ID: {SaleId}, Lost Discount: {LostDiscount:C} " +
                "({DiscountPercentage}% on {CancelledQuantity} items)",
                sale.Id,
                cancelledDiscount,
                notification.CancelledItem.Discount,
                notification.CancelledQuantity);
        }
    }

    /// <summary>
    /// Logs information about remaining items in the sale
    /// </summary>
    private void LogRemainingSaleItems(ItemCancelledEvent notification)
    {
        var activeItems = notification.Sale.Items.Where(i => !i.IsCancelled).ToList();
        
        if (activeItems.Any())
        {
            _logger.LogInformation(
                "Remaining active items in sale {SaleId} (Total: {RemainingTotal:C}):",
                notification.Sale.Id,
                activeItems.Sum(i => i.Total));

            foreach (var item in activeItems.Take(5)) // Log first 5 to avoid spam
            {
                _logger.LogInformation(
                    "Active: {ProductName} ({ProductId}) - {Quantity} x {UnitPrice:C} = {Total:C}",
                    item.ProductName,
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.Total);
            }

            if (activeItems.Count > 5)
            {
                _logger.LogInformation("... and {AdditionalItems} more items", activeItems.Count - 5);
            }
        }
    }
}