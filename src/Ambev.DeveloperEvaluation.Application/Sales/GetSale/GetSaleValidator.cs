using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Validator for GetSaleQuery
/// </summary>
public class GetSaleValidator : AbstractValidator<GetSaleQuery>
{
    /// <summary>
    /// Initializes validation rules for GetSaleQuery
    /// </summary>
    public GetSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}