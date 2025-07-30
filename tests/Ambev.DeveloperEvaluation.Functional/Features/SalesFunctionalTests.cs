using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Functional.Setup;
using Ambev.DeveloperEvaluation.Functional.TestData;
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

namespace Ambev.DeveloperEvaluation.Functional.Features;

/// <summary>
/// Functional tests for Sales features covering complete business scenarios
/// These tests validate the system behavior from end-user perspective
/// </summary>
public class SalesFunctionalTests : BaseFunctionalTest
{
    public SalesFunctionalTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region Complete Sale Lifecycle Scenarios

    /// <summary>
    /// Tests the complete lifecycle of a sale from creation to cancellation
    /// </summary>
    [Fact(DisplayName = "Complete sale lifecycle should work end-to-end")]
    public async Task Scenario_CompleteSaleLifecycle_ShouldWorkEndToEnd()
    {
        // Arrange
        await CleanDatabaseAsync();

        // Step 1: Create a sale
        var createRequest = SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.CreateRequest();
        var createJsonContent = new StringContent(JsonConvert.SerializeObject(createRequest), Encoding.UTF8, "application/json");

        var createResponse = await HttpClient.PostAsync("/api/sales", createJsonContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(createResponseContent);
        var saleId = createApiResponse!.Data!.Id;

        // Step 2: Retrieve the created sale
        var getResponse = await HttpClient.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var getApiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<GetSaleResponse>>(getResponseContent);
        
        getApiResponse!.Data!.SaleNumber.Should().Be(createRequest.SaleNumber);
        getApiResponse.Data.TotalAmount.Should().Be(SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.ExpectedTotal);

        // Step 3: Update the sale
        var updateRequest = SaleFunctionalTestData.SaleUpdateScenario.CreateUpdateRequest();
        var updateJsonContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

        var updateResponse = await HttpClient.PutAsync($"/api/sales/{saleId}", updateJsonContent);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify the update
        var getUpdatedResponse = await HttpClient.GetAsync($"/api/sales/{saleId}");
        var getUpdatedContent = await getUpdatedResponse.Content.ReadAsStringAsync();
        var getUpdatedApiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<GetSaleResponse>>(getUpdatedContent);
        
        getUpdatedApiResponse!.Data!.CustomerName.Should().Be("Cliente Atualizado");

        // Step 5: Cancel the sale
        var cancelResponse = await HttpClient.DeleteAsync($"/api/sales/{saleId}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Verify the cancellation
        using var dbContext = GetDbContext();
        var cancelledSale = await dbContext.Sales.FindAsync(saleId);
        cancelledSale!.Status.Should().Be(Domain.Enums.SaleStatus.Cancelled);
    }

    #endregion

    #region Business Rules Scenarios

    /// <summary>
    /// Tests the discount business rules for small retail purchases
    /// </summary>
    [Fact(DisplayName = "Small retail purchase should not receive discount")]
    public async Task Scenario_SmallRetailPurchase_ShouldNotReceiveDiscount()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.SmallRetailPurchaseScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(responseContent);
        
        apiResponse!.Data!.TotalAmount.Should().Be(SaleFunctionalTestData.SmallRetailPurchaseScenario.ExpectedTotal);

        // Verify in database
        using var dbContext = GetDbContext();
        var sale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == apiResponse.Data.Id);
        
        sale!.Items.First().Discount.Should().Be(SaleFunctionalTestData.SmallRetailPurchaseScenario.ExpectedDiscount);
    }

    /// <summary>
    /// Tests the discount business rules for bulk purchases
    /// </summary>
    [Fact(DisplayName = "Bulk purchase should receive 10% discount")]
    public async Task Scenario_BulkPurchase_ShouldReceive10PercentDiscount()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(responseContent);
        
        apiResponse!.Data!.TotalAmount.Should().Be(SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.ExpectedTotal);

        // Verify discount was applied
        using var dbContext = GetDbContext();
        var sale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == apiResponse.Data.Id);
        
        sale!.Items.First().Discount.Should().Be(SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.ExpectedDiscount);
    }

    /// <summary>
    /// Tests the discount business rules for high volume purchases
    /// </summary>
    [Fact(DisplayName = "High volume purchase should receive 20% discount")]
    public async Task Scenario_HighVolumePurchase_ShouldReceive20PercentDiscount()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.HighVolumePurchaseScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(responseContent);
        
        apiResponse!.Data!.TotalAmount.Should().Be(SaleFunctionalTestData.HighVolumePurchaseScenario.ExpectedTotal);

        // Verify discount was applied
        using var dbContext = GetDbContext();
        var sale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == apiResponse.Data.Id);
        
        sale!.Items.First().Discount.Should().Be(SaleFunctionalTestData.HighVolumePurchaseScenario.ExpectedDiscount);
    }

    /// <summary>
    /// Tests mixed purchase scenario with different discount levels
    /// </summary>
    [Fact(DisplayName = "Mixed purchase should apply different discounts per item")]
    public async Task Scenario_MixedPurchase_ShouldApplyDifferentDiscountsPerItem()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.MixedPurchaseScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(responseContent);
        
        apiResponse!.Data!.TotalAmount.Should().Be(SaleFunctionalTestData.MixedPurchaseScenario.ExpectedTotal);

        // Verify different discounts were applied
        using var dbContext = GetDbContext();
        var sale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == apiResponse.Data.Id);
        
        sale!.Items.Should().HaveCount(2);
        
        var firstItem = sale.Items.First(i => i.Quantity == 2);
        var secondItem = sale.Items.First(i => i.Quantity == 8);
        
        firstItem.Discount.Should().Be(0); // No discount for quantity 2
        secondItem.Discount.Should().Be(10); // 10% discount for quantity 8
    }

    #endregion

    #region Validation Scenarios

    /// <summary>
    /// Tests that excessive quantity is properly rejected
    /// </summary>
    [Fact(DisplayName = "Purchase with excessive quantity should be rejected")]
    public async Task Scenario_ExcessiveQuantityPurchase_ShouldBeRejected()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.InvalidPurchaseScenario.CreateRequestWithExcessiveQuantity();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that future dates are properly rejected
    /// </summary>
    [Fact(DisplayName = "Purchase with future date should be rejected")]
    public async Task Scenario_FutureDatePurchase_ShouldBeRejected()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.InvalidPurchaseScenario.CreateRequestWithFutureDate();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/sales", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Customer Journey Scenarios

    /// <summary>
    /// Tests a typical customer journey with multiple interactions
    /// </summary>
    [Fact(DisplayName = "Customer journey with multiple sales should work correctly")]
    public async Task Scenario_CustomerJourneyWithMultipleSales_ShouldWorkCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var customerId = Guid.NewGuid();

        // Step 1: Customer makes first small purchase
        var firstPurchase = SaleFunctionalTestData.SmallRetailPurchaseScenario.CreateRequest();
        firstPurchase.CustomerId = customerId;
        firstPurchase.SaleNumber = "CUSTOMER-001";
        
        var firstJsonContent = new StringContent(JsonConvert.SerializeObject(firstPurchase), Encoding.UTF8, "application/json");
        var firstResponse = await HttpClient.PostAsync("/api/sales", firstJsonContent);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 2: Customer makes second bulk purchase
        var secondPurchase = SaleFunctionalTestData.BulkPurchaseWithDiscountScenario.CreateRequest();
        secondPurchase.CustomerId = customerId;
        secondPurchase.SaleNumber = "CUSTOMER-002";
        
        var secondJsonContent = new StringContent(JsonConvert.SerializeObject(secondPurchase), Encoding.UTF8, "application/json");
        var secondResponse = await HttpClient.PostAsync("/api/sales", secondJsonContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 3: Retrieve customer's sales history
        var salesResponse = await HttpClient.GetAsync($"/api/sales?customerId={customerId}");
        salesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert customer has both sales
        using var dbContext = GetDbContext();
        var customerSales = await dbContext.Sales
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();
        
        customerSales.Should().HaveCount(2);
        customerSales.Should().Contain(s => s.SaleNumber == "CUSTOMER-001");
        customerSales.Should().Contain(s => s.SaleNumber == "CUSTOMER-002");
    }

    #endregion

    #region Item Cancellation Scenarios

    /// <summary>
    /// Tests partial cancellation of sale items
    /// </summary>
    [Fact(DisplayName = "Partial item cancellation should update sale total correctly")]
    public async Task Scenario_PartialItemCancellation_ShouldUpdateSaleTotalCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.MixedPurchaseScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Step 1: Create sale with multiple items
        var createResponse = await HttpClient.PostAsync("/api/sales", jsonContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(createResponseContent);
        var saleId = createApiResponse!.Data!.Id;

        // Step 2: Get product ID to cancel
        using var dbContext = GetDbContext();
        var sale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);
        var productIdToCancel = sale!.Items.First().ProductId;
        var originalTotal = sale.TotalAmount;

        // Step 3: Cancel one item
        var cancelResponse = await HttpClient.DeleteAsync($"/api/sales/{saleId}/items/{productIdToCancel}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify total was recalculated
        var updatedSale = await dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);
        
        updatedSale!.TotalAmount.Should().BeLessThan(originalTotal);
        updatedSale.Items.First(i => i.ProductId == productIdToCancel).IsCancelled.Should().BeTrue();
    }

    #endregion

    #region Error Recovery Scenarios

    /// <summary>
    /// Tests system behavior when attempting to update a cancelled sale
    /// </summary>
    [Fact(DisplayName = "Attempt to update cancelled sale should be prevented")]
    public async Task Scenario_UpdateCancelledSale_ShouldBePrevented()
    {
        // Arrange
        await CleanDatabaseAsync();
        var request = SaleFunctionalTestData.SmallRetailPurchaseScenario.CreateRequest();
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Step 1: Create sale
        var createResponse = await HttpClient.PostAsync("/api/sales", jsonContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createApiResponse = JsonConvert.DeserializeObject<ApiResponseWithData<CreateSaleResponse>>(createResponseContent);
        var saleId = createApiResponse!.Data!.Id;

        // Step 2: Cancel the sale
        var cancelResponse = await HttpClient.DeleteAsync($"/api/sales/{saleId}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Attempt to update the cancelled sale
        var updateRequest = SaleFunctionalTestData.SaleUpdateScenario.CreateUpdateRequest();
        var updateJsonContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

        var updateResponse = await HttpClient.PutAsync($"/api/sales/{saleId}", updateJsonContent);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Performance Scenarios

    /// <summary>
    /// Tests system performance with high-volume operations
    /// </summary>
    [Fact(DisplayName = "High volume sales creation should complete within reasonable time")]
    public async Task Scenario_HighVolumeSalesCreation_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await CleanDatabaseAsync();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int salesCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < salesCount; i++)
        {
            var request = SaleFunctionalTestData.SmallRetailPurchaseScenario.CreateRequest();
            request.SaleNumber = $"PERF-{i:D3}";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            tasks.Add(HttpClient.PostAsync("/api/sales", jsonContent));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.Created));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete within 10 seconds
    }

    #endregion
}