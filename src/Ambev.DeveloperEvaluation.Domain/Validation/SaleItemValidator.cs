using Ambev.DeveloperEvaluation.Domain.Constants;
using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for SaleItem entity with business rule validation
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(item => item.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100)
            .WithMessage("Product name cannot exceed 100 characters");

        RuleFor(item => item.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(SaleBusinessRules.MaxQuantityPerItem)
            .WithMessage($"Cannot sell above {SaleBusinessRules.MaxQuantityPerItem} identical items");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero");

        RuleFor(item => item.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount cannot be negative")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount cannot exceed 100%");

        // Business rule: No discount for quantities below minimum
        RuleFor(item => item)
            .Must(item => item.Quantity >= SaleBusinessRules.MinQuantityForDiscount || item.Discount == 0)
            .WithMessage($"Purchases below {SaleBusinessRules.MinQuantityForDiscount} items cannot have a discount");

        RuleFor(item => item.Total)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount cannot be negative");
    }
}