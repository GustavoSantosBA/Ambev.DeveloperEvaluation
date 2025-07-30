using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

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
    /// Applies business rules for quantity-based discounts
    /// Business Rules:
    /// - Purchases above 4 identical items have a 10% discount
    /// - Purchases between 10 and 20 identical items have a 20% discount
    /// - It's not possible to sell above 20 identical items
    /// - Purchases below 4 items cannot have a discount
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to sell more than 20 items</exception>
    public void ApplyDiscountRules()
    {
        // Business rule: Maximum 20 items per product
        if (Quantity > 20)
        {
            throw new InvalidOperationException("It's not possible to sell above 20 identical items");
        }

        // Apply discount based on quantity
        if (Quantity < 4)
        {
            // Business rule: No discount for less than 4 items
            Discount = 0;
        }
        else if (Quantity >= 4 && Quantity < 10)
        {
            // Business rule: 10% discount for 4-9 items
            Discount = 10;
        }
        else if (Quantity >= 10 && Quantity <= 20)
        {
            // Business rule: 20% discount for 10-20 items
            Discount = 20;
        }

        CalculateTotal();
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