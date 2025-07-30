using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Bogus;

namespace Ambev.DeveloperEvaluation.Integration.TestData;

/// <summary>
/// Provides test data for Sale repository integration tests
/// </summary>
public static class SaleRepositoryTestData
{
    /// <summary>
    /// Configures the Faker to generate valid Sale entities for repository tests.
    /// </summary>
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Recent(30))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Address.City())
        .RuleFor(s => s.Status, f => SaleStatus.Active)
        .RuleFor(s => s.TotalAmount, f => f.Random.Decimal(10, 1000))
        .RuleFor(s => s.CreatedAt, f => f.Date.Recent(1))
        .RuleFor(s => s.UpdatedAt, f => null);

    /// <summary>
    /// Configures the Faker to generate valid SaleItem entities for repository tests.
    /// </summary>
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(si => si.ProductId, f => f.Random.Guid())
        .RuleFor(si => si.ProductName, f => f.Commerce.ProductName())
        .RuleFor(si => si.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(si => si.UnitPrice, f => f.Random.Decimal(1, 100))
        .RuleFor(si => si.Discount, f => 0)
        .RuleFor(si => si.IsCancelled, f => false)
        .FinishWith((f, si) => 
        {
            si.ApplyDiscountRules();
        });

    /// <summary>
    /// Generates a valid Sale entity with items for repository testing.
    /// </summary>
    public static Sale GenerateValidSaleWithItems(int itemCount = 2)
    {
        var sale = SaleFaker.Generate();
        var items = SaleItemFaker.Generate(itemCount);
        
        foreach (var item in items)
        {
            sale.Items.Add(item);
        }
        
        sale.CalculateTotalAmount();
        return sale;
    }

    /// <summary>
    /// Generates multiple Sale entities for bulk testing.
    /// </summary>
    public static List<Sale> GenerateMultipleSalesWithItems(int saleCount, int itemsPerSale = 2)
    {
        var sales = new List<Sale>();
        
        for (int i = 0; i < saleCount; i++)
        {
            var sale = GenerateValidSaleWithItems(itemsPerSale);
            sale.SaleNumber = $"BULK-{i:D5}";
            sales.Add(sale);
        }
        
        return sales;
    }

    /// <summary>
    /// Generates a Sale with specific customer ID for filtering tests.
    /// </summary>
    public static Sale GenerateSaleWithCustomerId(Guid customerId)
    {
        var sale = GenerateValidSaleWithItems();
        sale.CustomerId = customerId;
        return sale;
    }

    /// <summary>
    /// Generates a Sale with specific branch ID for filtering tests.
    /// </summary>
    public static Sale GenerateSaleWithBranchId(Guid branchId)
    {
        var sale = GenerateValidSaleWithItems();
        sale.BranchId = branchId;
        return sale;
    }

    /// <summary>
    /// Generates a Sale with specific sale date for date range tests.
    /// </summary>
    public static Sale GenerateSaleWithDate(DateTime saleDate)
    {
        var sale = GenerateValidSaleWithItems();
        sale.SaleDate = saleDate;
        return sale;
    }

    /// <summary>
    /// Generates a cancelled Sale for status filtering tests.
    /// </summary>
    public static Sale GenerateCancelledSale()
    {
        var sale = GenerateValidSaleWithItems();
        sale.Cancel();
        return sale;
    }

    /// <summary>
    /// Generates a Sale with specific sale number for uniqueness tests.
    /// </summary>
    public static Sale GenerateSaleWithNumber(string saleNumber)
    {
        var sale = GenerateValidSaleWithItems();
        sale.SaleNumber = saleNumber;
        return sale;
    }
}