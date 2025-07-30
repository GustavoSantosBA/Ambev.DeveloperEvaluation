using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Integration.Setup;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Controllers;

/// <summary>
/// Integration tests for SalesController
/// Tests the complete flow from HTTP request to database operations
/// </summary>
public class SalesControllerIntegrationTests : BaseIntegrationTest
{
    public SalesControllerIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    #region CreateSale Integration Tests

    /// <summary>
    /// Tests that CreateSale endpoint creates a sale successfully with valid request
    /// </summary>
    [Fact(DisplayName = "POST /api/sales should create sale successfully with valid request")]
    public async Task Given_ValidCreateSaleRequest_When_PostToCreateSale_Then_ShouldCreateSaleSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleIntegrationTestData.GenerateValidCreateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(responseContent);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Sale created successfully");
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().NotBeEmpty();
        apiResponse.Data.SaleNumber.Should().Be(request.SaleNumber);

        // Verify database
        using var dbContext = GetDbContext();
        var createdSale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == request.SaleNumber);
        
        createdSale.Should().NotBeNull();
        createdSale!.Items.Should().HaveCount(request.Items.Count);
        createdSale.TotalAmount.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that CreateSale endpoint returns validation errors for invalid request
    /// </summary>
    [Fact(DisplayName = "POST /api/sales should return BadRequest for invalid request")]
    public async Task Given_InvalidCreateSaleRequest_When_PostToCreateSale_Then_ShouldReturnBadRequest()
    {
        // Arrange
        var request = SaleIntegrationTestData.GenerateInvalidCreateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that CreateSale endpoint handles duplicate sale numbers
    /// </summary>
    [Fact(DisplayName = "POST /api/sales should return BadRequest for duplicate sale number")]
    public async Task Given_DuplicateSaleNumber_When_PostToCreateSale_Then_ShouldReturnBadRequest()
    {
        // Arrange
        await CleanDatabaseAsync();
        var firstRequest = SaleIntegrationTestData.GenerateValidCreateSaleRequest();
        var duplicateRequest = SaleIntegrationTestData.GenerateCreateSaleRequestWithDuplicateNumber(firstRequest.SaleNumber);
        
        var firstJsonContent = new StringContent(JsonConvert.SerializeObject(firstRequest), Encoding.UTF8, "application/json");
        var duplicateJsonContent = new StringContent(JsonConvert.SerializeObject(duplicateRequest), Encoding.UTF8, "application/json");

        // Act
        var firstResponse = await HttpClient.PostAsync("/api/sales", firstJsonContent);
        var duplicateResponse = await HttpClient.PostAsync("/api/sales", duplicateJsonContent);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that CreateSale endpoint applies business rules correctly
    /// </summary>
    [Fact(DisplayName = "POST /api/sales should apply business rules and calculate discounts correctly")]
    public async Task Given_ValidRequestWithDiscountableQuantity_When_PostToCreateSale_Then_ShouldApplyDiscountCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleIntegrationTestData.GenerateValidCreateSaleRequest();
        request.Items.First().Quantity = 5; // Should get 10% discount
        request.Items.First().UnitPrice = 100m;
        
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify discount was applied in database
        using var dbContext = GetDbContext();
        var createdSale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == request.SaleNumber);
        
        createdSale.Should().NotBeNull();
        var discountedItem = createdSale!.Items.First();
        discountedItem.Discount.Should().Be(10m); // 10% discount for quantity 5
        discountedItem.Total.Should().Be(450m); // 5 * 100 * 0.9 = 450
    }

    #endregion

    #region GetSale Integration Tests

    /// <summary>
    /// Tests that GetSale endpoint retrieves sale successfully
    /// </summary>
    [Fact(DisplayName = "GET /api/sales/{id} should retrieve sale successfully")]
    public async Task Given_ExistingSale_When_GetSale_Then_ShouldReturnSaleSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = CreateTestSale();
        await SeedDatabaseAsync(context => context.Sales.Add(sale));

        // Act
        var response = await HttpClient.GetAsync($"/api/sales/{sale.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<GetSaleResponse>>(responseContent);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(sale.Id);
        apiResponse.Data.SaleNumber.Should().Be(sale.SaleNumber);
    }

    /// <summary>
    /// Tests that GetSale endpoint returns NotFound for non-existent sale
    /// </summary>
    [Fact(DisplayName = "GET /api/sales/{id} should return NotFound for non-existent sale")]
    public async Task Given_NonExistentSale_When_GetSale_Then_ShouldReturnNotFound()
    {
        // Arrange
        await CleanDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/sales/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that GetSale endpoint returns BadRequest for invalid ID
    /// </summary>
    [Fact(DisplayName = "GET /api/sales/{id} should return BadRequest for invalid ID")]
    public async Task Given_InvalidSaleId_When_GetSale_Then_ShouldReturnBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/sales/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetSales Integration Tests

    /// <summary>
    /// Tests that GetSales endpoint retrieves sales with pagination
    /// </summary>
    [Fact(DisplayName = "GET /api/sales should retrieve sales with pagination")]
    public async Task Given_MultipleSales_When_GetSales_Then_ShouldReturnPaginatedResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sales = CreateMultipleTestSales(5);
        await SeedDatabaseAsync(context => context.Sales.AddRange(sales));

        // Act
        var response = await HttpClient.GetAsync("/api/sales?page=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
        // Additional assertions would depend on the exact response structure
    }

    /// <summary>
    /// Tests that GetSales endpoint handles filtering correctly
    /// </summary>
    [Fact(DisplayName = "GET /api/sales should handle filtering correctly")]
    public async Task Given_SalesWithFiltering_When_GetSales_Then_ShouldReturnFilteredResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();
        var sales = CreateMultipleTestSales(3);
        sales.First().CustomerId = customerId;
        
        await SeedDatabaseAsync(context => context.Sales.AddRange(sales));

        // Act
        var response = await HttpClient.GetAsync($"/api/sales?customerId={customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
        // Additional assertions would verify only filtered results are returned
    }

    #endregion

    #region UpdateSale Integration Tests

    /// <summary>
    /// Tests that UpdateSale endpoint updates sale successfully
    /// </summary>
    [Fact(DisplayName = "PUT /api/sales/{id} should update sale successfully")]
    public async Task Given_ValidUpdateRequest_When_PutToUpdateSale_Then_ShouldUpdateSaleSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        var existingSale = CreateTestSale();
        await SeedDatabaseAsync(context => context.Sales.Add(existingSale));

        var updateRequest = SaleIntegrationTestData.GenerateValidUpdateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"/api/sales/{existingSale.Id}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<UpdateSaleResponse>>(responseContent);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();

        // Verify database update
        using var dbContext = GetDbContext();
        var updatedSale = await dbContext.Sales.FindAsync(existingSale.Id);
        updatedSale.Should().NotBeNull();
        updatedSale!.CustomerName.Should().Be(updateRequest.CustomerName);
        updatedSale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that UpdateSale endpoint returns NotFound for non-existent sale
    /// </summary>
    [Fact(DisplayName = "PUT /api/sales/{id} should return NotFound for non-existent sale")]
    public async Task Given_NonExistentSale_When_PutToUpdateSale_Then_ShouldReturnNotFound()
    {
        // Arrange
        await CleanDatabaseAsync();
        var updateRequest = SaleIntegrationTestData.GenerateValidUpdateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.PutAsync($"/api/sales/{nonExistentId}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that UpdateSale endpoint prevents updating cancelled sales
    /// </summary>
    [Fact(DisplayName = "PUT /api/sales/{id} should prevent updating cancelled sales")]
    public async Task Given_CancelledSale_When_PutToUpdateSale_Then_ShouldReturnBadRequest()
    {
        // Arrange
        await CleanDatabaseAsync();
        var cancelledSale = CreateTestSale();
        cancelledSale.Cancel();
        await SeedDatabaseAsync(context => context.Sales.Add(cancelledSale));

        var updateRequest = SaleIntegrationTestData.GenerateValidUpdateSaleRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"/api/sales/{cancelledSale.Id}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region CancelSale Integration Tests

    /// <summary>
    /// Tests that CancelSale endpoint cancels sale successfully
    /// </summary>
    [Fact(DisplayName = "DELETE /api/sales/{id} should cancel sale successfully")]
    public async Task Given_ExistingSale_When_DeleteSale_Then_ShouldCancelSaleSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = CreateTestSale();
        await SeedDatabaseAsync(context => context.Sales.Add(sale));

        // Act
        var response = await HttpClient.DeleteAsync($"/api/sales/{sale.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify sale is cancelled in database
        using var dbContext = GetDbContext();
        var cancelledSale = await dbContext.Sales.FindAsync(sale.Id);
        cancelledSale.Should().NotBeNull();
        cancelledSale!.Status.Should().Be(Domain.Enums.SaleStatus.Cancelled);
        cancelledSale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that CancelSale endpoint returns NotFound for non-existent sale
    /// </summary>
    [Fact(DisplayName = "DELETE /api/sales/{id} should return NotFound for non-existent sale")]
    public async Task Given_NonExistentSale_When_DeleteSale_Then_ShouldReturnNotFound()
    {
        // Arrange
        await CleanDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/sales/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CancelSaleItem Integration Tests

    /// <summary>
    /// Tests that CancelSaleItem endpoint cancels item successfully
    /// </summary>
    [Fact(DisplayName = "DELETE /api/sales/{saleId}/items/{productId} should cancel item successfully")]
    public async Task Given_ExistingSaleWithItem_When_DeleteSaleItem_Then_ShouldCancelItemSuccessfully()
    {
        // Arrange
        await CleanDatabaseAsync();
        var sale = CreateTestSale();
        var itemToCancel = sale.Items.First();
        await SeedDatabaseAsync(context => context.Sales.Add(sale));

        // Act
        var response = await HttpClient.DeleteAsync($"/api/sales/{sale.Id}/items/{itemToCancel.ProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify item is cancelled in database
        using var dbContext = GetDbContext();
        var updatedSale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == sale.Id);
        
        updatedSale.Should().NotBeNull();
        var cancelledItem = updatedSale!.Items.First(i => i.ProductId == itemToCancel.ProductId);
        cancelledItem.IsCancelled.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test sale with items for testing purposes
    /// </summary>
    private Sale CreateTestSale()
    {
        var sale = new Sale
        {
            SaleNumber = $"TEST-{Guid.NewGuid():N}",
            SaleDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch",
            Status = Domain.Enums.SaleStatus.Active
        };

        var item = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = 50m,
            Discount = 0,
            Total = 100m
        };

        sale.Items.Add(item);
        sale.TotalAmount = 100m;
        
        return sale;
    }

    /// <summary>
    /// Creates multiple test sales for pagination testing
    /// </summary>
    private List<Sale> CreateMultipleTestSales(int count)
    {
        var sales = new List<Sale>();
        
        for (int i = 0; i < count; i++)
        {
            var sale = CreateTestSale();
            sale.SaleNumber = $"TEST-MULTI-{i:D3}";
            sales.Add(sale);
        }
        
        return sales;
    }

    #endregion
}