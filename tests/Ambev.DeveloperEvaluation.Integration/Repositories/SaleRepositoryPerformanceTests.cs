using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Setup;
using Ambev.DeveloperEvaluation.Integration.TestData;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Contains performance tests for the SaleRepository.
/// </summary>
public class SaleRepositoryPerformanceTests : BaseIntegrationTest
{
    public SaleRepositoryPerformanceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        // O construtor da classe base já inicializa o que é necessário.
    }

    /// <summary>
    /// Tests concurrent read operations to ensure they perform well under load.
    /// </summary>
    [Fact(DisplayName = "Concurrent read operations should perform well")]
    public async Task Concurrent_read_operations_should_perform_well()
    {
        // Arrange
        await CleanDatabaseAsync();

        // Crie uma instância do repositório aqui para o setup inicial
        var initialRepo = Factory.Services.CreateScope().ServiceProvider.GetRequiredService<ISaleRepository>();
        var sale = SaleIntegrationTestData.GenerateValidSaleEntity();
        await initialRepo.CreateAsync(sale);

        const int readCount = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        var readTasks = Enumerable.Range(0, readCount).Select(async _ =>
        {
            using var scope = Factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISaleRepository>();
            return await repo.GetByIdAsync(sale.Id);
        }).ToList();

        var results = await Task.WhenAll(readTasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(readCount);
        results.Should().NotContainNulls();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "100 leituras concorrentes devem ser rápidas.");
    }
}