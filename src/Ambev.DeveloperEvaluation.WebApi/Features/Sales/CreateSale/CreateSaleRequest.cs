using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Request model for creating a new sale
/// </summary>
public class CreateSaleRequest
{
    /// <summary>
    /// Gets or sets the sale number. Must be unique in the system.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the sale was made.
    /// If not provided, will default to current UTC time.
    /// </summary>
    public DateTime? SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the customer external identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name for denormalized reference.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch external identifier where the sale was made.
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
    public List<CreateSaleItemRequest> Items { get; set; } = new();
}