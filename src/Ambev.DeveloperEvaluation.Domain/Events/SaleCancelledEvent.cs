using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event triggered when a sale is cancelled
/// </summary>
public class SaleCancelledEvent : INotification
{
    /// <summary>
    /// The sale that was cancelled
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// The total amount that was cancelled
    /// </summary>
    public decimal CancelledAmount { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Reason for cancellation if provided
    /// </summary>
    public string? CancellationReason { get; }

    /// <summary>
    /// Initializes a new instance of SaleCancelledEvent
    /// </summary>
    /// <param name="sale">The cancelled sale</param>
    /// <param name="cancellationReason">The reason for cancellation</param>
    public SaleCancelledEvent(Sale sale, string? cancellationReason = null)
    {
        Sale = sale;
        CancelledAmount = sale.TotalAmount;
        CancellationReason = cancellationReason;
        OccurredAt = DateTime.UtcNow;
    }
}