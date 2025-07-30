using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Setup;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Performance tests for SaleRepository to ensure database operations meet performance requirements
/// </summary>
public class SaleRepositoryPerformanceTests : BaseRepositoryIntegrationTest
{
    private readonly ISaleRepository _repository;

    public SaleRepositoryPerformanceTests()
    {
        _repository = new SaleRepository(DbContext);
    }

    /// <summary>
    /// Tests that single sale creation completes within acceptable time
    /// </summary>
    [Fact(DisplayName = "Single sale creation should complete within 500ms")]
    public async Task Given_SingleSale_When_CreateAsync_Then_ShouldCompleteQuickly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = SaleRepositoryTestData.GenerateValidSaleWithItems(3);
        var stopwatch = Stopwatch.StartNew();

        // Act
        await _repository.CreateAsync(sale);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    /// <summary>
    /// Tests that filtered queries with large datasets perform well
    /// </summary>
    [Fact(DisplayName = "Filtered queries should perform well with large datasets")]
    public async Task Given_LargeDataset_When_GetFilteredAsync_Then_ShouldPerformWell()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        
        // Create a larger dataset
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(100, 2);
        // Make some of them match our filter
        for (int i = 0; i < 20; i++)
        {
            sales[i].CustomerId = customerId;
        }

        // Bulk insert for setup
        foreach (var sale in sales)
        {
            await _repository.CreateAsync(sale);
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await _repository.GetFilteredAsync(
            page: 1, 
            pageSize: 10, 
            customerId: customerId);

        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(10);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    /// <summary>
    /// Tests concurrent read operations performance
    /// </summary>
    [Fact(DisplayName = "Concurrent read operations should perform well")]
    public async Task Given_ConcurrentReads_When_GetByIdAsync_Then_ShouldHandleConcurrencyWell()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sales = SaleRepositoryTestData.GenerateMultipleSalesWithItems(10);
        var createdSales = new List<Domain.Entities.Sale>();
        
        foreach (var sale in sales)
        {
            createdSales.Add(await _repository.CreateAsync(sale));
        }

        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();

        // Act
        foreach (var sale in createdSales)
        {
            tasks.Add(Task.Run(async () =>
            {
                // Create separate context for concurrent operations
                using var context = CreateFreshContext();
                var repo = new SaleRepository(context);
                await repo.GetByIdAsync(sale.Id);
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }
}