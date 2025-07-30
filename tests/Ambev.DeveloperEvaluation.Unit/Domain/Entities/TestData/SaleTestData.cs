using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sale entity using the Bogus library.
/// This class centralizes all Sale test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class SaleTestData
{
    /// <summary>
    /// Configures the Faker to generate valid Sale entities.
    /// </summary>
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Recent(30))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Address.City())
        .RuleFor(s => s.Status, f => SaleStatus.Active)
        .RuleFor(s => s.CreatedAt, f => f.Date.Recent(1))
        .RuleFor(s => s.UpdatedAt, f => null);

    /// <summary>
    /// Configures the Faker to generate valid SaleItem entities.
    /// </summary>
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(si => si.ProductId, f => f.Random.Guid())
        .RuleFor(si => si.ProductName, f => f.Commerce.ProductName())
        .RuleFor(si => si.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(si => si.UnitPrice, f => f.Random.Decimal(1, 1000))
        .RuleFor(si => si.IsCancelled, f => false);

    /// <summary>
    /// Generates a valid Sale entity with randomized data.
    /// </summary>
    /// <returns>A valid Sale entity with randomly generated data.</returns>
    public static Sale GenerateValidSale()
    {
        return SaleFaker.Generate();
    }

    /// <summary>
    /// Generates a valid Sale entity with specified number of items.
    /// </summary>
    /// <param name="itemCount">Number of items to add to the sale</param>
    /// <returns>A valid Sale entity with the specified number of items.</returns>
    public static Sale GenerateValidSaleWithItems(int itemCount = 2)
    {
        var sale = SaleFaker.Generate();
        var items = SaleItemFaker.Generate(itemCount);
        
        foreach (var item in items)
        {
            sale.AddItem(item);
        }
        
        return sale;
    }

    /// <summary>
    /// Generates a valid SaleItem entity.
    /// </summary>
    /// <returns>A valid SaleItem entity.</returns>
    public static SaleItem GenerateValidSaleItem()
    {
        return SaleItemFaker.Generate();
    }

    /// <summary>
    /// Generates a SaleItem with specified quantity for testing discount rules.
    /// </summary>
    /// <param name="quantity">The quantity for the item</param>
    /// <returns>A SaleItem with the specified quantity.</returns>
    public static SaleItem GenerateSaleItemWithQuantity(int quantity)
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = quantity;
        return item;
    }

    /// <summary>
    /// Generates a Sale with invalid data for testing validation.
    /// </summary>
    /// <returns>A Sale with invalid data.</returns>
    public static Sale GenerateInvalidSale()
    {
        return new Sale
        {
            SaleNumber = string.Empty, // Invalid: empty sale number
            CustomerName = string.Empty, // Invalid: empty customer name
            BranchName = string.Empty, // Invalid: empty branch name
            CustomerId = Guid.Empty, // Invalid: empty GUID
            BranchId = Guid.Empty, // Invalid: empty GUID
            SaleDate = DateTime.UtcNow.AddDays(1) // Invalid: future date
        };
    }

    /// <summary>
    /// Generates a SaleItem with quantity exceeding maximum allowed.
    /// </summary>
    /// <returns>A SaleItem with invalid quantity.</returns>
    public static SaleItem GenerateSaleItemWithExcessiveQuantity()
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = SaleBusinessRules.MaxQuantityPerItem + 1;
        return item;
    }

    /// <summary>
    /// Generates a cancelled Sale for testing.
    /// </summary>
    /// <returns>A cancelled Sale entity.</returns>
    public static Sale GenerateCancelledSale()
    {
        var sale = GenerateValidSaleWithItems();
        sale.Cancel();
        return sale;
    }

    /// <summary>
    /// Generates a SaleItem with invalid product name for testing validation.
    /// </summary>
    /// <returns>A SaleItem with invalid product name.</returns>
    public static SaleItem GenerateSaleItemWithInvalidProductName()
    {
        var item = SaleItemFaker.Generate();
        item.ProductName = string.Empty;
        return item;
    }

    /// <summary>
    /// Generates a SaleItem with invalid discount for quantity for testing validation.
    /// </summary>
    /// <returns>A SaleItem with invalid discount rules.</returns>
    public static SaleItem GenerateSaleItemWithInvalidDiscountRule()
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = 2; // Below minimum for discount
        item.Discount = 10; // Has discount anyway
        return item;
    }

    /// <summary>
    /// Generates a Sale with future date for testing validation.
    /// </summary>
    /// <returns>A Sale with future date.</returns>
    public static Sale GenerateSaleWithFutureDate()
    {
        var sale = SaleFaker.Generate();
        sale.SaleDate = DateTime.UtcNow.AddDays(1);
        return sale;
    }

    /// <summary>
    /// Generates a Sale with long names for testing validation.
    /// </summary>
    /// <returns>A Sale with names exceeding maximum length.</returns>
    public static Sale GenerateSaleWithLongNames()
    {
        var sale = SaleFaker.Generate();
        sale.CustomerName = new string('A', 101);
        sale.BranchName = new string('B', 101);
        sale.SaleNumber = new string('C', 51);
        return sale;
    }
}