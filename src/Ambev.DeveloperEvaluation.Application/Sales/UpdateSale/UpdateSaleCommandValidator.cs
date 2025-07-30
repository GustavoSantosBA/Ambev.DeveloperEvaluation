using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand that implements business rules and data validation.
/// </summary>
public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleCommandValidator class.
    /// Defines validation rules for all properties of UpdateSaleCommand.
    /// </summary>
    public UpdateSaleCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");

        RuleFor(command => command.SaleDate)
            .NotEmpty()
            .WithMessage("Sale date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
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
            .SetValidator(new UpdateSaleItemCommandValidator());
    }
}