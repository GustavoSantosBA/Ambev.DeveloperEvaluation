using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Bogus;

namespace Ambev.DeveloperEvaluation.Integration.TestData;

/// <summary>
/// Provides test data for Sales integration tests
/// </summary>
public static class SaleIntegrationTestData
{
    /// <summary>
    /// Configures the Faker to generate valid CreateSaleRequest entities.
    /// </summary>
    private static readonly Faker<CreateSaleRequest> CreateSaleRequestFaker = new Faker<CreateSaleRequest>()
        .RuleFor(r => r.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(r => r.SaleDate, f => f.Date.Recent(30))
        .RuleFor(r => r.CustomerId, f => f.Random.Guid())
        .RuleFor(r => r.CustomerName, f => f.Company.CompanyName())
        .RuleFor(r => r.BranchId, f => f.Random.Guid())
        .RuleFor(r => r.BranchName, f => f.Address.City())
        .RuleFor(r => r.Items, f => CreateSaleItemRequestFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Configures the Faker to generate valid CreateSaleItemRequest entities.
    /// </summary>
    private static readonly Faker<CreateSaleItemRequest> CreateSaleItemRequestFaker = new Faker<CreateSaleItemRequest>()
        .RuleFor(r => r.ProductId, f => f.Random.Guid())
        .RuleFor(r => r.ProductName, f => f.Commerce.ProductName())
        .RuleFor(r => r.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(r => r.UnitPrice, f => f.Random.Decimal(1, 1000));

    /// <summary>
    /// Configures the Faker to generate valid UpdateSaleRequest entities.
    /// </summary>
    private static readonly Faker<UpdateSaleRequest> UpdateSaleRequestFaker = new Faker<UpdateSaleRequest>()
        .RuleFor(r => r.SaleDate, f => f.Date.Recent(30))
        .RuleFor(r => r.CustomerId, f => f.Random.Guid())
        .RuleFor(r => r.CustomerName, f => f.Company.CompanyName())
        .RuleFor(r => r.BranchId, f => f.Random.Guid())
        .RuleFor(r => r.BranchName, f => f.Address.City())
        .RuleFor(r => r.Items, f => UpdateSaleItemRequestFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Configures the Faker to generate valid UpdateSaleItemRequest entities.
    /// </summary>
    private static readonly Faker<UpdateSaleItemRequest> UpdateSaleItemRequestFaker = new Faker<UpdateSaleItemRequest>()
        .RuleFor(r => r.ProductId, f => f.Random.Guid())
        .RuleFor(r => r.ProductName, f => f.Commerce.ProductName())
        .RuleFor(r => r.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(r => r.UnitPrice, f => f.Random.Decimal(1, 1000));

    #region CreateSale Test Data

    /// <summary>
    /// Generates a valid CreateSaleRequest with randomized data.
    /// </summary>
    public static CreateSaleRequest GenerateValidCreateSaleRequest()
    {
        return CreateSaleRequestFaker.Generate();
    }

    /// <summary>
    /// Generates an invalid CreateSaleRequest for testing validation.
    /// </summary>
    public static CreateSaleRequest GenerateInvalidCreateSaleRequest()
    {
        return new CreateSaleRequest
        {
            SaleNumber = string.Empty, // Invalid
            SaleDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
            CustomerId = Guid.Empty, // Invalid
            CustomerName = string.Empty, // Invalid
            BranchId = Guid.Empty, // Invalid
            BranchName = string.Empty, // Invalid
            Items = new List<CreateSaleItemRequest>() // Invalid - empty list
        };
    }

    /// <summary>
    /// Generates a CreateSaleRequest with duplicate sale number.
    /// </summary>
    public static CreateSaleRequest GenerateCreateSaleRequestWithDuplicateNumber(string saleNumber)
    {
        var request = CreateSaleRequestFaker.Generate();
        request.SaleNumber = saleNumber;
        return request;
    }

    /// <summary>
    /// Generates a CreateSaleRequest with items exceeding maximum quantity.
    /// </summary>
    public static CreateSaleRequest GenerateCreateSaleRequestWithExcessiveQuantity()
    {
        var request = CreateSaleRequestFaker.Generate();
        request.Items.First().Quantity = SaleBusinessRules.MaxQuantityPerItem + 1;
        return request;
    }

    #endregion

    #region UpdateSale Test Data

    /// <summary>
    /// Generates a valid UpdateSaleRequest with randomized data.
    /// </summary>
    public static UpdateSaleRequest GenerateValidUpdateSaleRequest()
    {
        return UpdateSaleRequestFaker.Generate();
    }

    /// <summary>
    /// Generates an invalid UpdateSaleRequest for testing validation.
    /// </summary>
    public static UpdateSaleRequest GenerateInvalidUpdateSaleRequest()
    {
        return new UpdateSaleRequest
        {
            SaleDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
            CustomerId = Guid.Empty, // Invalid
            CustomerName = string.Empty, // Invalid
            BranchId = Guid.Empty, // Invalid
            BranchName = string.Empty, // Invalid
            Items = new List<UpdateSaleItemRequest>() // Invalid - empty list
        };
    }

    #endregion

    #region GetSales Test Data

    /// <summary>
    /// Generates a valid GetSalesRequest with randomized data.
    /// </summary>
    public static GetSalesRequest GenerateValidGetSalesRequest()
    {
        var faker = new Faker();
        return new GetSalesRequest
        {
            Page = faker.Random.Int(1, 10),
            PageSize = faker.Random.Int(10, 50),
            CustomerId = faker.Random.Guid(),
            BranchId = faker.Random.Guid(),
            StartDate = faker.Date.Recent(30),
            EndDate = faker.Date.Recent(1)
        };
    }

    /// <summary>
    /// Generates an invalid GetSalesRequest for testing validation.
    /// </summary>
    public static GetSalesRequest GenerateInvalidGetSalesRequest()
    {
        return new GetSalesRequest
        {
            Page = 0, // Invalid
            PageSize = 0 // Invalid
        };
    }

    #endregion
}