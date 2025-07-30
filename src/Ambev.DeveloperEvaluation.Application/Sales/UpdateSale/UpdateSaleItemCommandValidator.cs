using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleItemCommand that implements business rules for sale items.
/// </summary>
public class UpdateSaleItemCommandValidator : AbstractValidator<UpdateSaleItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleItemCommandValidator class.
    /// Defines validation rules for all properties of UpdateSaleItemCommand including business rules.
    /// </summary>
    public UpdateSaleItemCommandValidator()
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
            .LessThanOrEqualTo(20)
            .WithMessage("Cannot sell above 20 identical items (business rule)");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero");
    }
}