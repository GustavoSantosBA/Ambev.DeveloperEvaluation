using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Validator for CancelSaleItemCommand
/// </summary>
public class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    /// <summary>
    /// Initializes validation rules for CancelSaleItemCommand
    /// </summary>
    public CancelSaleItemValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("Sale ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");
    }
}