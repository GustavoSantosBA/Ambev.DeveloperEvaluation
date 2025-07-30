using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Consolidated audit handler that processes all sale-related events for compliance and metrics
/// </summary>
public class SalesAuditEventHandler : 
    INotificationHandler<SaleCreatedEvent>,
    INotificationHandler<SaleModifiedEvent>,
    INotificationHandler<SaleCancelledEvent>,
    INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<SalesAuditEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SalesAuditEventHandler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SalesAuditEventHandler(ILogger<SalesAuditEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles SaleCreatedEvent for audit purposes
    /// </summary>
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogAuditEvent("SALE_CREATED", notification.Sale.Id, notification.Sale.SaleNumber, 
            notification.Sale.TotalAmount, notification.OccurredAt, 
            $"Customer: {notification.Sale.CustomerName}, Branch: {notification.Sale.BranchName}");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles SaleModifiedEvent for audit purposes
    /// </summary>
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        LogAuditEvent("SALE_MODIFIED", notification.Sale.Id, notification.Sale.SaleNumber,
            notification.Sale.TotalAmount, notification.OccurredAt,
            $"Type: {notification.ModificationType}, Details: {notification.ModificationDetails}");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles SaleCancelledEvent for audit purposes
    /// </summary>
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        LogAuditEvent("SALE_CANCELLED", notification.Sale.Id, notification.Sale.SaleNumber,
            notification.CancelledAmount, notification.OccurredAt,
            $"Reason: {notification.CancellationReason ?? "Not specified"}");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles ItemCancelledEvent for audit purposes
    /// </summary>
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        LogAuditEvent("ITEM_CANCELLED", notification.Sale.Id, notification.Sale.SaleNumber,
            notification.CancelledAmount, notification.OccurredAt,
            $"Product: {notification.CancelledItem.ProductName}, Quantity: {notification.CancelledQuantity}");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs standardized audit events for compliance and monitoring
    /// </summary>
    private void LogAuditEvent(string eventType, Guid saleId, string saleNumber, 
        decimal amount, DateTime timestamp, string details)
    {
        _logger.LogInformation(
            "[AUDIT] Event: {EventType}, Sale ID: {SaleId}, Sale Number: {SaleNumber}, " +
            "Amount: {Amount:C}, Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}, Details: {Details}",
            eventType, saleId, saleNumber, amount, timestamp, details);
    }
}