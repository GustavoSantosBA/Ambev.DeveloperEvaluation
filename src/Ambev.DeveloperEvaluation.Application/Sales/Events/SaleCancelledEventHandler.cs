using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for SaleCancelledEvent that logs the sale cancellation
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
    /// Handles the SaleCancelledEvent
    /// </summary>
    /// <param name="notification">The sale cancelled event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Sale Cancelled - Sale ID: {SaleId}, Sale Number: {SaleNumber}, Customer: {CustomerName}, " +
            "Branch: {BranchName}, Cancelled Amount: {CancelledAmount:C}, Cancelled At: {CancelledAt}, " +
            "Reason: {CancellationReason}",
            notification.Sale.Id,
            notification.Sale.SaleNumber,
            notification.Sale.CustomerName,
            notification.Sale.BranchName,
            notification.CancelledAmount,
            notification.OccurredAt,
            notification.CancellationReason ?? "Not specified");

        return Task.CompletedTask;
    }
}