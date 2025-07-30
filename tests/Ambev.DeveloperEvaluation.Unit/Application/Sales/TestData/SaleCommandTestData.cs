using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

/// <summary>
/// Provides methods for generating test data for Sales Commands using the Bogus library.
/// This class centralizes all Sales command test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class SaleCommandTestData
{
    /// <summary>
    /// Configures the Faker to generate valid CreateSaleCommand entities.
    /// </summary>
    private static readonly Faker<CreateSaleCommand> CreateSaleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(30))
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => f.Address.City())
        .RuleFor(c => c.Items, f => CreateSaleItemCommandFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Configures the Faker to generate valid CreateSaleItemCommand entities.
    /// </summary>
    private static readonly Faker<CreateSaleItemCommand> CreateSaleItemCommandFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(c => c.ProductId, f => f.Random.Guid())
        .RuleFor(c => c.ProductName, f => f.Commerce.ProductName())
        .RuleFor(c => c.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(c => c.UnitPrice, f => f.Random.Decimal(1, 1000));

    /// <summary>
    /// Configures the Faker to generate valid UpdateSaleCommand entities.
    /// </summary>
    private static readonly Faker<UpdateSaleCommand> UpdateSaleCommandFaker = new Faker<UpdateSaleCommand>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(30))
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => f.Address.City())
        .RuleFor(c => c.Items, f => UpdateSaleItemCommandFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Configures the Faker to generate valid UpdateSaleItemCommand entities.
    /// </summary>
    private static readonly Faker<UpdateSaleItemCommand> UpdateSaleItemCommandFaker = new Faker<UpdateSaleItemCommand>()
        .RuleFor(c => c.ProductId, f => f.Random.Guid())
        .RuleFor(c => c.ProductName, f => f.Commerce.ProductName())
        .RuleFor(c => c.Quantity, f => f.Random.Int(1, SaleBusinessRules.MaxQuantityPerItem))
        .RuleFor(c => c.UnitPrice, f => f.Random.Decimal(1, 1000));

    #region CreateSale Command Data

    /// <summary>
    /// Generates a valid CreateSaleCommand with randomized data.
    /// </summary>
    /// <returns>A valid CreateSaleCommand with randomly generated data.</returns>
    public static CreateSaleCommand GenerateValidCreateSaleCommand()
    {
        return CreateSaleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates a valid CreateSaleItemCommand with randomized data.
    /// </summary>
    /// <returns>A valid CreateSaleItemCommand with randomly generated data.</returns>
    public static CreateSaleItemCommand GenerateValidCreateSaleItemCommand()
    {
        return CreateSaleItemCommandFaker.Generate();
    }

    /// <summary>
    /// Generates an invalid CreateSaleCommand for testing validation.
    /// </summary>
    /// <returns>An invalid CreateSaleCommand.</returns>
    public static CreateSaleCommand GenerateInvalidCreateSaleCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = string.Empty, // Invalid
            SaleDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
            CustomerId = Guid.Empty, // Invalid
            CustomerName = string.Empty, // Invalid
            BranchId = Guid.Empty, // Invalid
            BranchName = string.Empty, // Invalid
            Items = new List<CreateSaleItemCommand>() // Invalid - empty list
        };
    }

    /// <summary>
    /// Generates a CreateSaleCommand with duplicate sale number for testing business rules.
    /// </summary>
    /// <param name="saleNumber">The duplicate sale number</param>
    /// <returns>A CreateSaleCommand with duplicate sale number.</returns>
    public static CreateSaleCommand GenerateCreateSaleCommandWithDuplicateNumber(string saleNumber)
    {
        var command = CreateSaleCommandFaker.Generate();
        command.SaleNumber = saleNumber;
        return command;
    }

    #endregion

    #region UpdateSale Command Data

    /// <summary>
    /// Generates a valid UpdateSaleCommand with randomized data.
    /// </summary>
    /// <returns>A valid UpdateSaleCommand with randomly generated data.</returns>
    public static UpdateSaleCommand GenerateValidUpdateSaleCommand()
    {
        return UpdateSaleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates an invalid UpdateSaleCommand for testing validation.
    /// </summary>
    /// <returns>An invalid UpdateSaleCommand.</returns>
    public static UpdateSaleCommand GenerateInvalidUpdateSaleCommand()
    {
        return new UpdateSaleCommand
        {
            Id = Guid.Empty, // Invalid
            SaleDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
            CustomerId = Guid.Empty, // Invalid
            CustomerName = string.Empty, // Invalid
            BranchId = Guid.Empty, // Invalid
            BranchName = string.Empty, // Invalid
            Items = new List<UpdateSaleItemCommand>() // Invalid - empty list
        };
    }

    #endregion

    #region GetSale Query Data

    /// <summary>
    /// Generates a valid GetSaleQuery with randomized data.
    /// </summary>
    /// <returns>A valid GetSaleQuery with randomly generated data.</returns>
    public static GetSaleQuery GenerateValidGetSaleQuery()
    {
        return new GetSaleQuery(Guid.NewGuid());
    }

    /// <summary>
    /// Generates an invalid GetSaleQuery for testing validation.
    /// </summary>
    /// <returns>An invalid GetSaleQuery.</returns>
    public static GetSaleQuery GenerateInvalidGetSaleQuery()
    {
        return new GetSaleQuery(Guid.Empty);
    }

    #endregion

    #region GetSales Query Data

    /// <summary>
    /// Generates a valid GetSalesQuery with randomized data.
    /// </summary>
    /// <returns>A valid GetSalesQuery with randomly generated data.</returns>
    public static GetSalesQuery GenerateValidGetSalesQuery()
    {
        var faker = new Faker();
        return new GetSalesQuery(
            page: faker.Random.Int(1, 10),
            pageSize: faker.Random.Int(10, 50),
            customerId: faker.Random.Guid(),
            branchId: faker.Random.Guid(),
            startDate: faker.Date.Recent(30),
            endDate: faker.Date.Recent(1)
        );
    }

    /// <summary>
    /// Generates an invalid GetSalesQuery for testing validation.
    /// </summary>
    /// <returns>An invalid GetSalesQuery.</returns>
    public static GetSalesQuery GenerateInvalidGetSalesQuery()
    {
        return new GetSalesQuery(
            page: 0, // Invalid
            pageSize: 0, // Invalid
            customerId: null,
            branchId: null,
            startDate: null,
            endDate: null
        );
    }

    #endregion

    #region CancelSale Command Data

    /// <summary>
    /// Generates a valid CancelSaleCommand with randomized data.
    /// </summary>
    /// <returns>A valid CancelSaleCommand with randomly generated data.</returns>
    public static CancelSaleCommand GenerateValidCancelSaleCommand()
    {
        return new CancelSaleCommand(Guid.NewGuid());
    }

    /// <summary>
    /// Generates an invalid CancelSaleCommand for testing validation.
    /// </summary>
    /// <returns>An invalid CancelSaleCommand.</returns>
    public static CancelSaleCommand GenerateInvalidCancelSaleCommand()
    {
        return new CancelSaleCommand(Guid.Empty);
    }

    #endregion

    #region CancelSaleItem Command Data

    /// <summary>
    /// Generates a valid CancelSaleItemCommand with randomized data.
    /// </summary>
    /// <returns>A valid CancelSaleItemCommand with randomly generated data.</returns>
    public static CancelSaleItemCommand GenerateValidCancelSaleItemCommand()
    {
        return new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid());
    }

    /// <summary>
    /// Generates an invalid CancelSaleItemCommand for testing validation.
    /// </summary>
    /// <returns>An invalid CancelSaleItemCommand.</returns>
    public static CancelSaleItemCommand GenerateInvalidCancelSaleItemCommand()
    {
        return new CancelSaleItemCommand(Guid.Empty, Guid.Empty);
    }

    #endregion
}