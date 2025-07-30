using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event triggered when a sale is modified
/// </summary>
public class SaleModifiedEvent : INotification
{
    /// <summary>
    /// The sale that was modified
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// The type of modification that occurred
    /// </summary>
    public string ModificationType { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Additional details about the modification
    /// </summary>
    public string? ModificationDetails { get; }

    /// <summary>
    /// Initializes a new instance of SaleModifiedEvent
    /// </summary>
    /// <param name="sale">The modified sale</param>
    /// <param name="modificationType">The type of modification</param>
    /// <param name="modificationDetails">Additional details about the modification</param>
    public SaleModifiedEvent(Sale sale, string modificationType, string? modificationDetails = null)
    {
        Sale = sale;
        ModificationType = modificationType;
        ModificationDetails = modificationDetails;
        OccurredAt = DateTime.UtcNow;
    }
}