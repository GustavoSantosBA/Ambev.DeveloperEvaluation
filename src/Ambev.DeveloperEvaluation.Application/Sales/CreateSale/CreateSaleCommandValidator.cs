using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand that implements business rules and data validation.
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateSaleCommandValidator class.
    /// Defines validation rules for all properties of CreateSaleCommand.
    /// </summary>
    public CreateSaleCommandValidator()
    {
        RuleFor(command => command.SaleNumber)
            .NotEmpty()
            .WithMessage("Sale number is required")
            .MaximumLength(50)
            .WithMessage("Sale number cannot exceed 50 characters");

        RuleFor(command => command.SaleDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(command => command.SaleDate.HasValue)
            .WithMessage("Sale date cannot be in the future");

        RuleFor(command => command.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(command => command.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MaximumLength(100)
            .WithMessage("Customer name cannot exceed 100 characters");

        RuleFor(command => command.BranchId)
            .NotEmpty()
            .WithMessage("Branch ID is required");

        RuleFor(command => command.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(100)
            .WithMessage("Branch name cannot exceed 100 characters");

        RuleFor(command => command.Items)
            .NotEmpty()
            .WithMessage("Sale must contain at least one item");

        RuleForEach(command => command.Items)
            .SetValidator(new CreateSaleItemCommandValidator());
    }
}