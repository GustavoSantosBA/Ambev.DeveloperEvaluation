using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event triggered when a sale item is cancelled
/// </summary>
public class ItemCancelledEvent : INotification
{
    /// <summary>
    /// The sale containing the cancelled item
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// The item that was cancelled
    /// </summary>
    public SaleItem CancelledItem { get; }

    /// <summary>
    /// The quantity that was cancelled
    /// </summary>
    public int CancelledQuantity { get; }

    /// <summary>
    /// The amount that was cancelled
    /// </summary>
    public decimal CancelledAmount { get; }

    /// <summary>
    /// The new total amount of the sale after cancellation
    /// </summary>
    public decimal NewSaleTotalAmount { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Reason for item cancellation if provided
    /// </summary>
    public string? CancellationReason { get; }

    /// <summary>
    /// Initializes a new instance of ItemCancelledEvent
    /// </summary>
    /// <param name="sale">The sale containing the cancelled item</param>
    /// <param name="cancelledItem">The item that was cancelled</param>
    /// <param name="cancelledQuantity">The quantity that was cancelled</param>
    /// <param name="cancelledAmount">The amount that was cancelled</param>
    /// <param name="cancellationReason">The reason for cancellation</param>
    public ItemCancelledEvent(Sale sale, SaleItem cancelledItem, int cancelledQuantity, decimal cancelledAmount, string? cancellationReason = null)
    {
        Sale = sale;
        CancelledItem = cancelledItem;
        CancelledQuantity = cancelledQuantity;
        CancelledAmount = cancelledAmount;
        NewSaleTotalAmount = sale.TotalAmount;
        CancellationReason = cancellationReason;
        OccurredAt = DateTime.UtcNow;
    }
}