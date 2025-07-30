using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for SaleModifiedEvent that logs the sale modification
/// </summary>
public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleModifiedEventHandler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the SaleModifiedEvent
    /// </summary>
    /// <param name="notification">The sale modified event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sale Modified - Sale ID: {SaleId}, Sale Number: {SaleNumber}, Modification Type: {ModificationType}, " +
            "Customer: {CustomerName}, Branch: {BranchName}, Total Amount: {TotalAmount:C}, " +
            "Items Count: {ItemsCount}, Modified At: {ModifiedAt}, Details: {ModificationDetails}",
            notification.Sale.Id,
            notification.Sale.SaleNumber,
            notification.ModificationType,
            notification.Sale.CustomerName,
            notification.Sale.BranchName,
            notification.Sale.TotalAmount,
            notification.Sale.Items.Count(i => !i.IsCancelled),
            notification.OccurredAt,
            notification.ModificationDetails ?? "N/A");

        return Task.CompletedTask;
    }
}