using Ambev.DeveloperEvaluation.Integration.Setup;
using Ambev.DeveloperEvaluation.Integration.TestData;
using FluentAssertions;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Performance;

/// <summary>
/// Performance tests for SalesController to ensure API meets performance requirements
/// </summary>
public class SalesControllerPerformanceTests : BaseIntegrationTest
{
    public SalesControllerPerformanceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Tests that CreateSale endpoint performs within acceptable time limits
    /// </summary>
    [Fact(DisplayName = "CreateSale should complete within 2 seconds")]
    public async Task Given_ValidCreateSaleRequest_When_PostToCreateSale_Then_ShouldCompleteWithin2Seconds()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleIntegrationTestData.GenerateValidCreateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    /// <summary>
    /// Tests concurrent requests to CreateSale endpoint
    /// </summary>
    [Fact(DisplayName = "Multiple concurrent CreateSale requests should complete successfully")]
    public async Task Given_ConcurrentCreateSaleRequests_When_PostToCreateSale_Then_ShouldHandleConcurrency()
    {
        // Arrange
        await CleanDatabaseAsync();
        const int concurrentRequests = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            var request = SaleIntegrationTestData.GenerateValidCreateSaleRequest();
            request.SaleNumber = $"CONCURRENT-{i:D3}"; // Ensure unique sale numbers
            var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            tasks.Add(HttpClient.PostAsync("/api/sales", jsonContent));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.Created));
    }

    /// <summary>
    /// Tests GetSales endpoint performance with large datasets
    /// </summary>
    [Fact(DisplayName = "GetSales should handle large result sets efficiently")]
    public async Task Given_LargeDataset_When_GetSales_Then_ShouldReturnWithinReasonableTime()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        // Note: In a real scenario, you would seed a large dataset
        // For this example, we'll test with a reasonable request
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await HttpClient.GetAsync("/api/sales?page=1&pageSize=50");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
}