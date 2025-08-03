using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Constants;

/// <summary>
/// Defines business rule constants for sales operations
/// </summary>
public static class SaleBusinessRules
{
    /// <summary>
    /// The minimum quantity of an item to be eligible for the standard discount
    /// </summary>
    public const int MinQuantityForDiscount = 4;

    /// <summary>
    /// The minimum quantity of an item to be eligible for the higher discount
    /// </summary>
    // FIX: The value should be 10 to match the business rule "10 or more items get 20%".
    // The previous value was likely 5, causing the error.
    public const int MinQuantityForHigherDiscount = 10;

    /// <summary>
    /// The standard discount percentage for regular bulk purchases
    /// </summary>
    public const decimal StandardDiscountPercentage = 10m;

    /// <summary>
    /// The higher discount percentage for high-volume purchases
    /// </summary>
    public const decimal HighDiscountPercentage = 20m;

    /// <summary>
    /// The maximum allowed quantity for a single item in a sale
    /// </summary>
    public const int MaxQuantityPerItem = 20;
}
