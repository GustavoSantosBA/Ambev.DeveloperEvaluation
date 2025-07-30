using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for SaleCreatedEvent that logs the sale creation with detailed information
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
    /// Handles the SaleCreatedEvent with comprehensive logging
    /// </summary>
    /// <param name="notification">The sale created event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("SaleCreated-{SaleId}", notification.Sale.Id);

        try
        {
            // Log main sale creation event
            _logger.LogInformation(
                "Sale successfully created - ID: {SaleId}, Number: {SaleNumber}, Customer: {CustomerName} ({CustomerId}), " +
                "Branch: {BranchName} ({BranchId}), Total: {TotalAmount:C}, Items: {ItemsCount}, Date: {SaleDate}, " +
                "Created At: {CreatedAt}",
                notification.Sale.Id,
                notification.Sale.SaleNumber,
                notification.Sale.CustomerName,
                notification.Sale.CustomerId,
                notification.Sale.BranchName,
                notification.Sale.BranchId,
                notification.Sale.TotalAmount,
                notification.Sale.Items.Count,
                notification.Sale.SaleDate,
                notification.OccurredAt);

            // Log business metrics
            LogBusinessMetrics(notification);

            // Log each item with business rule analysis
            LogSaleItems(notification);

            _logger.LogInformation("Sale creation event processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SaleCreatedEvent for Sale ID: {SaleId}", notification.Sale.Id);
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs business metrics and analytics for the created sale
    /// </summary>
    private void LogBusinessMetrics(SaleCreatedEvent notification)
    {
        var sale = notification.Sale;
        var totalDiscount = sale.Items.Sum(i => (i.UnitPrice * i.Quantity) - i.Total);
        var averageItemValue = sale.Items.Count > 0 ? sale.TotalAmount / sale.Items.Count : 0;
        var discountedItems = sale.Items.Count(i => i.Discount > 0);

        _logger.LogInformation(
            "Sale Business Metrics - Sale ID: {SaleId}, Total Discount Applied: {TotalDiscount:C}, " +
            "Average Item Value: {AverageItemValue:C}, Items with Discount: {DiscountedItems}/{TotalItems}, " +
            "Discount Percentage: {DiscountPercentage:P2}",
            sale.Id,
            totalDiscount,
            averageItemValue,
            discountedItems,
            sale.Items.Count,
            sale.TotalAmount > 0 ? totalDiscount / (sale.TotalAmount + totalDiscount) : 0);
    }

    /// <summary>
    /// Logs detailed information about each sale item
    /// </summary>
    private void LogSaleItems(SaleCreatedEvent notification)
    {
        foreach (var (item, index) in notification.Sale.Items.Select((item, index) => (item, index + 1)))
        {
            var discountAmount = (item.UnitPrice * item.Quantity) - item.Total;
            var businessRule = GetAppliedBusinessRule(item.Quantity, item.Discount);

            _logger.LogInformation(
                "Sale Item #{ItemNumber} - Product: {ProductName} ({ProductId}), Qty: {Quantity}, " +
                "Unit Price: {UnitPrice:C}, Discount: {Discount}% ({DiscountAmount:C}), " +
                "Total: {Total:C}, Business Rule: {BusinessRule}",
                index,
                item.ProductName,
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.Discount,
                discountAmount,
                item.Total,
                businessRule);
        }
    }

    /// <summary>
    /// Determines which business rule was applied based on quantity and discount
    /// </summary>
    private static string GetAppliedBusinessRule(int quantity, decimal discount)
    {
        return quantity switch
        {
            < 4 when discount == 0 => "No discount (< 4 items)",
            >= 4 and < 10 when discount == 10 => "10% discount (4-9 items)",
            >= 10 and <= 20 when discount == 20 => "20% discount (10-20 items)",
            > 20 => "Invalid - exceeds maximum limit",
            _ => $"Unexpected rule application (Qty: {quantity}, Discount: {discount}%)"
        };
    }
}