using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.Constants;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an individual item within a sale.
/// Contains product information and pricing details with business rule validation.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the product external identifier (External Identities pattern)
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name (denormalized for performance)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of items sold
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage applied (0-100)
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this item (calculated)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets whether this item is cancelled
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Initializes a new instance of the SaleItem class
    /// </summary>
    public SaleItem()
    {
        IsCancelled = false;
        Discount = 0;
    }

    /// <summary>
    /// Validates the sale item using FluentValidation
    /// </summary>
    /// <returns>Validation result with errors if any</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleItemValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    /// <summary>
    /// Applies business rules for quantity-based discounts using constants
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to sell more than maximum allowed items</exception>
    public void ApplyDiscountRules()
    {
        ValidateMaxQuantity();
        ApplyQuantityBasedDiscount();
        CalculateTotal();
    }

    /// <summary>
    /// Validates maximum quantity business rule
    /// </summary>
    private void ValidateMaxQuantity()
    {
        if (Quantity > SaleBusinessRules.MaxQuantityPerItem)
        {
            throw new InvalidOperationException(
                $"It's not possible to sell above {SaleBusinessRules.MaxQuantityPerItem} identical items");
        }
    }

    /// <summary>
    /// Applies quantity-based discount rules
    /// </summary>
    private void ApplyQuantityBasedDiscount()
    {
        if (Quantity < SaleBusinessRules.MinQuantityForDiscount)
        {
            Discount = 0;
        }
        else if (Quantity >= SaleBusinessRules.MinQuantityForDiscount &&
                 Quantity < SaleBusinessRules.MinQuantityForHigherDiscount)
        {
            Discount = SaleBusinessRules.StandardDiscountPercentage;
        }
        else if (Quantity >= SaleBusinessRules.MinQuantityForHigherDiscount)
        {
            Discount = SaleBusinessRules.HighDiscountPercentage;
        }
    }

    /// <summary>
    /// Calculates the total amount considering discount
    /// </summary>
    public void CalculateTotal()
    {
        var subtotal = UnitPrice * Quantity;
        var discountAmount = subtotal * (Discount / 100);
        Total = subtotal - discountAmount;
    }

    /// <summary>
    /// Cancels this specific item
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }
}