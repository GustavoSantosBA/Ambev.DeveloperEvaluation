using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Setup;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Integration tests for SaleRepository that test all database operations
/// with a real PostgreSQL database using Testcontainers
/// </summary>
public class SaleRepositoryIntegrationTests : BaseRepositoryIntegrationTest
{
    private readonly ISaleRepository _repository;

    public SaleRepositoryIntegrationTests()
    {
        _repository = new SaleRepository(DbContext);
    }

    #region CreateAsync Tests

    /// <summary>
    /// Tests that CreateAsync successfully persists a sale with items to the database
    /// </summary>
    [Fact(DisplayName = "CreateAsync should persist sale with items to database successfully")]
    public async Task Given_ValidSale_When_CreateAsync_Then_ShouldPersistToDatabase()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems(2);

        // Act
        var createdSale = await _repository.CreateAsync(sale);

        // Assert
        createdSale.Should().NotBeNull();
        createdSale.Id.Should().NotBeEmpty();
        createdSale.Items.Should().HaveCount(2);
        createdSale.TotalAmount.Should().BeGreaterThan(0);

        // Verify in fresh context
        using var freshContext = CreateFreshContext();
        var persistedSale = await freshContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == createdSale.Id);

        persistedSale.Should().NotBeNull();
        persistedSale!.SaleNumber.Should().Be(sale.SaleNumber);
        persistedSale.Items.Should().HaveCount(2);
        persistedSale.TotalAmount.Should().Be(createdSale.TotalAmount);
    }

    /// <summary>
    /// Tests that CreateAsync handles multiple sales concurrently
    /// </summary>
    [Fact(DisplayName = "CreateAsync should handle concurrent operations correctly")]
    public async Task Given_MultipleConcurrentSales_When_CreateAsync_Then_ShouldCreateAllSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        const int concurrentSales = 5;
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(concurrentSales);
        var tasks = new List<Task<Sale>>();

        // Act
        foreach (var sale in sales)
        {
            // Create separate context for each concurrent operation
            var freshContext = CreateFreshContext();
            var repo = new SaleRepository(freshContext);
            tasks.Add(repo.CreateAsync(sale));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentSales);
        results.Should().AllSatisfy(result => result.Id.Should().NotBeEmpty());

        // Verify all persisted
        using var verificationContext = CreateFreshContext();
        var persistedCount = await verificationContext.Sales.CountAsync();
        persistedCount.Should().Be(concurrentSales);
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// Tests that GetByIdAsync retrieves sale with items correctly
    /// </summary>
    [Fact(DisplayName = "GetByIdAsync should retrieve sale with items correctly")]
    public async Task Given_ExistingSale_When_GetByIdAsync_Then_ShouldReturnSaleWithItems()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems(3);
        var createdSale = await _repository.CreateAsync(sale);

        // Act
        var retrievedSale = await _repository.GetByIdAsync(createdSale.Id);

        // Assert
        retrievedSale.Should().NotBeNull();
        retrievedSale!.Id.Should().Be(createdSale.Id);
        retrievedSale.SaleNumber.Should().Be(createdSale.SaleNumber);
        retrievedSale.Items.Should().HaveCount(3);
        retrievedSale.TotalAmount.Should().Be(createdSale.TotalAmount);
    }

    /// <summary>
    /// Tests that GetByIdAsync returns null for non-existent sale
    /// </summary>
    [Fact(DisplayName = "GetByIdAsync should return null for non-existent sale")]
    public async Task Given_NonExistentSale_When_GetByIdAsync_Then_ShouldReturnNull()
    {
        // Arrange
        await CleanDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetBySaleNumberAsync Tests

    /// <summary>
    /// Tests that GetBySaleNumberAsync retrieves sale correctly
    /// </summary>
    [Fact(DisplayName = "GetBySaleNumberAsync should retrieve sale by sale number correctly")]
    public async Task Given_ExistingSale_When_GetBySaleNumberAsync_Then_ShouldReturnCorrectSale()
    {
        // Arrange
        await CleanDatabaseAsync();
        var saleNumber = "TEST-UNIQUE-001";
        var sale = SaleRepositoryTestData.GenerateSaleWithNumber(saleNumber);
        await _repository.CreateAsync(sale);

        // Act
        var retrievedSale = await _repository.GetBySaleNumberAsync(saleNumber);

        // Assert
        retrievedSale.Should().NotBeNull();
        retrievedSale!.SaleNumber.Should().Be(saleNumber);
        retrievedSale.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that GetBySaleNumberAsync returns null for non-existent sale number
    /// </summary>
    [Fact(DisplayName = "GetBySaleNumberAsync should return null for non-existent sale number")]
    public async Task Given_NonExistentSaleNumber_When_GetBySaleNumberAsync_Then_ShouldReturnNull()
    {
        // Arrange
        await CleanDatabaseAsync();

        // Act
        var result = await _repository.GetBySaleNumberAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByCustomerIdAsync Tests

    /// <summary>
    /// Tests that GetByCustomerIdAsync retrieves sales for specific customer
    /// </summary>
    [Fact(DisplayName = "GetByCustomerIdAsync should retrieve sales for specific customer")]
    public async Task Given_SalesFromMultipleCustomers_When_GetByCustomerIdAsync_Then_ShouldReturnOnlyCustomerSales()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var customerSales = new List<Sale>
        {
            SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId),
            SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId)
        };
        var otherSale = SaleRepositoryTestData.GenerateValidSaleWithItems();

        foreach (var sale in customerSales)
            await _repository.CreateAsync(sale);
        await _repository.CreateAsync(otherSale);

        // Act
        var results = await _repository.GetByCustomerIdAsync(customerId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(sale => sale.CustomerId.Should().Be(customerId));
    }

    /// <summary>
    /// Tests that GetByCustomerIdAsync orders results by sale date descending
    /// </summary>
    [Fact(DisplayName = "GetByCustomerIdAsync should order results by sale date descending")]
    public async Task Given_MultipleSalesForCustomer_When_GetByCustomerIdAsync_Then_ShouldOrderBySaleDateDescending()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var oldSale = SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId);
        oldSale.SaleDate = DateTime.UtcNow.AddDays(-10);
        var newSale = SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId);
        newSale.SaleDate = DateTime.UtcNow.AddDays(-1);

        await _repository.CreateAsync(oldSale);
        await _repository.CreateAsync(newSale);

        // Act
        var results = (await _repository.GetByCustomerIdAsync(customerId)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].SaleDate.Should().BeAfter(results[1].SaleDate);
    }

    #endregion

    #region GetByBranchIdAsync Tests

    /// <summary>
    /// Tests that GetByBranchIdAsync retrieves sales for specific branch
    /// </summary>
    [Fact(DisplayName = "GetByBranchIdAsync should retrieve sales for specific branch")]
    public async Task Given_SalesFromMultipleBranches_When_GetByBranchIdAsync_Then_ShouldReturnOnlyBranchSales()
    {
        // Arrange
        await CleanDatabaseAsync();
        var branchId = Guid.NewGuid();
        var branchSales = new List<Sale>
        {
            SaleRepositoryTestData.GenerateSaleWithBranchId(branchId),
            SaleRepositoryTestData.GenerateSaleWithBranchId(branchId)
        };
        var otherSale = SaleRepositoryTestData.GenerateValidSaleWithItems();

        foreach (var sale in branchSales)
            await _repository.CreateAsync(sale);
        await _repository.CreateAsync(otherSale);

        // Act
        var results = await _repository.GetByBranchIdAsync(branchId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(sale => sale.BranchId.Should().Be(branchId));
    }

    #endregion

    #region GetByDateRangeAsync Tests

    /// <summary>
    /// Tests that GetByDateRangeAsync retrieves sales within date range
    /// </summary>
    [Fact(DisplayName = "GetByDateRangeAsync should retrieve sales within specified date range")]
    public async Task Given_SalesInDifferentDates_When_GetByDateRangeAsync_Then_ShouldReturnOnlySalesInRange()
    {
        // Arrange
        await CleanDatabaseAsync();
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(-5);
        
        var saleInRange = SaleRepositoryTestData.GenerateSaleWithDate(DateTime.UtcNow.AddDays(-7));
        var saleBeforeRange = SaleRepositoryTestData.GenerateSaleWithDate(DateTime.UtcNow.AddDays(-15));
        var saleAfterRange = SaleRepositoryTestData.GenerateSaleWithDate(DateTime.UtcNow.AddDays(-2));

        await _repository.CreateAsync(saleInRange);
        await _repository.CreateAsync(saleBeforeRange);
        await _repository.CreateAsync(saleAfterRange);

        // Act
        var results = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        results.Should().ContainSingle();
        results.First().Id.Should().Be(saleInRange.Id);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// Tests that GetAllAsync retrieves sales with pagination
    /// </summary>
    [Fact(DisplayName = "GetAllAsync should retrieve sales with pagination correctly")]
    public async Task Given_MultipleSales_When_GetAllAsync_Then_ShouldReturnPaginatedResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(10);
        foreach (var sale in sales)
            await _repository.CreateAsync(sale);

        // Act
        var firstPage = await _repository.GetAllAsync(page: 1, pageSize: 3);
        var secondPage = await _repository.GetAllAsync(page: 2, pageSize: 3);

        // Assert
        firstPage.Should().HaveCount(3);
        secondPage.Should().HaveCount(3);
        
        var firstPageIds = firstPage.Select(s => s.Id).ToList();
        var secondPageIds = secondPage.Select(s => s.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);
    }

    /// <summary>
    /// Tests that GetAllAsync orders results by sale date descending
    /// </summary>
    [Fact(DisplayName = "GetAllAsync should order results by sale date descending")]
    public async Task Given_MultipleSales_When_GetAllAsync_Then_ShouldOrderBySaleDateDescending()
    {
        // Arrange
        await CleanDatabaseAsync();
        var oldSale = SaleRepositoryTestData.GenerateSaleWithDate(DateTime.UtcNow.AddDays(-10));
        var newSale = SaleRepositoryTestData.GenerateSaleWithDate(DateTime.UtcNow.AddDays(-1));

        await _repository.CreateAsync(oldSale);
        await _repository.CreateAsync(newSale);

        // Act
        var results = (await _repository.GetAllAsync(page: 1, pageSize: 10)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].SaleDate.Should().BeAfter(results[1].SaleDate);
    }

    #endregion

    #region GetFilteredAsync Tests

    /// <summary>
    /// Tests that GetFilteredAsync applies customer filter correctly
    /// </summary>
    [Fact(DisplayName = "GetFilteredAsync should filter by customer ID correctly")]
    public async Task Given_SalesWithDifferentCustomers_When_GetFilteredAsyncWithCustomerId_Then_ShouldReturnFilteredResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var customerSale = SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId);
        var otherSale = SaleRepositoryTestData.GenerateValidSaleWithItems();

        await _repository.CreateAsync(customerSale);
        await _repository.CreateAsync(otherSale);

        // Act
        var results = await _repository.GetFilteredAsync(customerId: customerId);

        // Assert
        results.Should().ContainSingle();
        results.First().CustomerId.Should().Be(customerId);
    }

    /// <summary>
    /// Tests that GetFilteredAsync applies multiple filters correctly
    /// </summary>
    [Fact(DisplayName = "GetFilteredAsync should apply multiple filters correctly")]
    public async Task Given_MultipleSales_When_GetFilteredAsyncWithMultipleFilters_Then_ShouldReturnCorrectResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Sale that matches all filters
        var matchingSale = SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId);
        matchingSale.BranchId = branchId;
        matchingSale.SaleDate = DateTime.UtcNow.AddDays(-5);

        // Sale that doesn't match customer filter
        var nonMatchingSale = SaleRepositoryTestData.GenerateValidSaleWithItems();
        nonMatchingSale.BranchId = branchId;
        nonMatchingSale.SaleDate = DateTime.UtcNow.AddDays(-5);

        await _repository.CreateAsync(matchingSale);
        await _repository.CreateAsync(nonMatchingSale);

        // Act
        var results = await _repository.GetFilteredAsync(
            customerId: customerId,
            branchId: branchId,
            startDate: startDate,
            endDate: endDate);

        // Assert
        results.Should().ContainSingle();
        results.First().Id.Should().Be(matchingSale.Id);
    }

    /// <summary>
    /// Tests that GetFilteredAsync handles pagination correctly
    /// </summary>
    [Fact(DisplayName = "GetFilteredAsync should handle pagination correctly")]
    public async Task Given_MultipleSales_When_GetFilteredAsyncWithPagination_Then_ShouldReturnPaginatedResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var sales = Enumerable.Range(0, 5)
            .Select(_ => SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId))
            .ToList();

        foreach (var sale in sales)
            await _repository.CreateAsync(sale);

        // Act
        var firstPage = await _repository.GetFilteredAsync(page: 1, pageSize: 2, customerId: customerId);
        var secondPage = await _repository.GetFilteredAsync(page: 2, pageSize: 2, customerId: customerId);

        // Assert
        firstPage.Should().HaveCount(2);
        secondPage.Should().HaveCount(2);
    }

    #endregion

    #region GetFilteredCountAsync Tests

    /// <summary>
    /// Tests that GetFilteredCountAsync returns correct count with filters
    /// </summary>
    [Fact(DisplayName = "GetFilteredCountAsync should return correct count with filters")]
    public async Task Given_MultipleSales_When_GetFilteredCountAsync_Then_ShouldReturnCorrectCount()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var customerSales = Enumerable.Range(0, 3)
            .Select(_ => SaleRepositoryTestData.GenerateSaleWithCustomerId(customerId))
            .ToList();
        var otherSale = SaleRepositoryTestData.GenerateValidSaleWithItems();

        foreach (var sale in customerSales)
            await _repository.CreateAsync(sale);
        await _repository.CreateAsync(otherSale);

        // Act
        var count = await _repository.GetFilteredCountAsync(customerId: customerId);

        // Assert
        count.Should().Be(3);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// Tests that UpdateAsync successfully updates sale properties
    /// </summary>
    [Fact(DisplayName = "UpdateAsync should update sale properties successfully")]
    public async Task Given_ExistingSale_When_UpdateAsync_Then_ShouldUpdatePropertiesCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems();
        var createdSale = await _repository.CreateAsync(sale);

        // Modify properties
        createdSale.CustomerName = "Updated Customer Name";
        createdSale.BranchName = "Updated Branch Name";
        createdSale.UpdatedAt = DateTime.UtcNow;

        // Act
        var updatedSale = await _repository.UpdateAsync(createdSale);

        // Assert
        updatedSale.Should().NotBeNull();
        updatedSale.CustomerName.Should().Be("Updated Customer Name");
        updatedSale.BranchName.Should().Be("Updated Branch Name");
        updatedSale.UpdatedAt.Should().NotBeNull();

        // Verify in fresh context
        using var freshContext = CreateFreshContext();
        var persistedSale = await freshContext.Sales.FindAsync(createdSale.Id);
        persistedSale!.CustomerName.Should().Be("Updated Customer Name");
        persistedSale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that UpdateAsync handles item modifications correctly
    /// </summary>
    [Fact(DisplayName = "UpdateAsync should handle item modifications correctly")]
    public async Task Given_SaleWithItems_When_UpdateAsyncWithItemChanges_Then_ShouldUpdateItemsCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems(2);
        var createdSale = await _repository.CreateAsync(sale);

        // Modify an item
        var itemToModify = createdSale.Items.First();
        itemToModify.Quantity = 10;
        itemToModify.ApplyDiscountRules();
        createdSale.CalculateTotalAmount();

        // Act
        var updatedSale = await _repository.UpdateAsync(createdSale);

        // Assert
        updatedSale.Should().NotBeNull();
        var updatedItem = updatedSale.Items.First(i => i.Id == itemToModify.Id);
        updatedItem.Quantity.Should().Be(10);
        updatedItem.Discount.Should().Be(20); // Should have 20% discount for quantity 10
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// Tests that DeleteAsync removes sale and returns true for existing sale
    /// </summary>
    [Fact(DisplayName = "DeleteAsync should remove existing sale and return true")]
    public async Task Given_ExistingSale_When_DeleteAsync_Then_ShouldRemoveSaleAndReturnTrue()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems();
        var createdSale = await _repository.CreateAsync(sale);

        // Act
        var result = await _repository.DeleteAsync(createdSale.Id);

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var deletedSale = await _repository.GetByIdAsync(createdSale.Id);
        deletedSale.Should().BeNull();
    }

    /// <summary>
    /// Tests that DeleteAsync returns false for non-existent sale
    /// </summary>
    [Fact(DisplayName = "DeleteAsync should return false for non-existent sale")]
    public async Task Given_NonExistentSale_When_DeleteAsync_Then_ShouldReturnFalse()
    {
        // Arrange
        await CleanDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// Tests that GetCountAsync returns correct total count
    /// </summary>
    [Fact(DisplayName = "GetCountAsync should return correct total count of sales")]
    public async Task Given_MultipleSales_When_GetCountAsync_Then_ShouldReturnCorrectCount()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(7);
        foreach (var sale in sales)
            await _repository.CreateAsync(sale);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(7);
    }

    /// <summary>
    /// Tests that GetCountAsync returns zero for empty database
    /// </summary>
    [Fact(DisplayName = "GetCountAsync should return zero for empty database")]
    public async Task Given_EmptyDatabase_When_GetCountAsync_Then_ShouldReturnZero()
    {
        // Arrange
        await CleanDatabaseAsync();

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region Cancellation Token Tests

    /// <summary>
    /// Tests that repository operations respect cancellation tokens
    /// </summary>
    [Fact(DisplayName = "Repository operations should respect cancellation tokens")]
    public async Task Given_CancellationToken_When_RepositoryOperation_Then_ShouldRespectCancellation()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.CreateAsync(sale, cts.Token));
    }

    #endregion

    #region Database Constraints Tests

    /// <summary>
    /// Tests that duplicate sale numbers are handled appropriately by database constraints
    /// </summary>
    [Fact(DisplayName = "Database should enforce sale number uniqueness")]
    public async Task Given_DuplicateSaleNumbers_When_CreateAsync_Then_ShouldThrowDbException()
    {
        // Arrange
        await CleanDatabaseAsync();
        var saleNumber = "DUPLICATE-TEST";
        var firstSale = SaleRepositoryTestData.GenerateSaleWithNumber(saleNumber);
        var duplicateSale = SaleRepositoryTestData.GenerateSaleWithNumber(saleNumber);

        await _repository.CreateAsync(firstSale);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => _repository.CreateAsync(duplicateSale));
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that bulk operations perform within acceptable time limits
    /// </summary>
    [Fact(DisplayName = "Bulk operations should complete within reasonable time")]
    public async Task Given_LargeNumberOfSales_When_BulkOperations_Then_ShouldCompleteInReasonableTime()
    {
        // Arrange
        await CleanDatabaseAsync();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int salesCount = 50;
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(salesCount, 1);

        // Act
        foreach (var sale in sales)
        {
            await _repository.CreateAsync(sale);
        }

        var allSales = await _repository.GetAllAsync(1, salesCount);
        stopwatch.Stop();

        // Assert
        allSales.Should().HaveCount(salesCount);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Should complete in less than 30 seconds
    }

    #endregion
}