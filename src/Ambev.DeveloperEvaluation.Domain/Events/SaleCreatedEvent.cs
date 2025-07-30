using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event triggered when a sale is created
/// </summary>
public class SaleCreatedEvent : INotification
{
    /// <summary>
    /// The sale that was created
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of SaleCreatedEvent
    /// </summary>
    /// <param name="sale">The created sale</param>
    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
        OccurredAt = DateTime.UtcNow;
    }
}