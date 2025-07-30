using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command for creating a new sale.
/// </summary>
/// <remarks>
/// This command is used to capture the required data for creating a sale,
/// including sale number, customer information, branch information, and sale items.
/// It implements <see cref="IRequest{TResponse}"/> to initiate the request
/// that returns a <see cref="CreateSaleResult"/>.
/// 
/// The data provided in this command is validated using the
/// <see cref="CreateSaleCommandValidator"/> which extends
/// <see cref="AbstractValidator{T}"/> to ensure that the fields are correctly
/// populated and follow the required business rules.
/// </remarks>
public class CreateSaleCommand : IRequest<CreateSaleResult>
{
    /// <summary>
    /// Gets or sets the sale number.
    /// Must be unique in the system.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the sale was made.
    /// If not provided, will default to current UTC time.
    /// </summary>
    public DateTime? SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the customer external identifier.
    /// Following the External Identities pattern with denormalization.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name for denormalized reference.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch external identifier where the sale was made.
    /// Following the External Identities pattern with denormalization.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the branch name for denormalized reference.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of items to be included in this sale.
    /// Must contain at least one item.
    /// </summary>
    public List<CreateSaleItemCommand> Items { get; set; } = new();

    /// <summary>
    /// Validates the command using FluentValidation rules.
    /// </summary>
    /// <returns>Validation result with any errors found</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new CreateSaleCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}