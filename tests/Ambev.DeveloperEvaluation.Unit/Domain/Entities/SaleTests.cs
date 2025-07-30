using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale entity.
/// Tests cover entity creation, business logic, validation rules,
/// and state changes to ensure the Sale entity behaves correctly.
/// </summary>
public class SaleTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests that a new Sale is created with correct default values.
    /// </summary>
    [Fact(DisplayName = "New Sale should have correct default values")]
    public void Given_NewSale_When_Created_Then_ShouldHaveCorrectDefaults()
    {
        // Act
        var sale = new Sale();

        // Assert
        sale.Status.Should().Be(SaleStatus.Active);
        sale.Items.Should().NotBeNull().And.BeEmpty();
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.SaleDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.UpdatedAt.Should().BeNull();
        sale.TotalAmount.Should().Be(0);
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that a valid Sale passes validation.
    /// </summary>
    [Fact(DisplayName = "Valid Sale should pass validation")]
    public void Given_ValidSale_When_Validated_Then_ShouldBeValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that an invalid Sale fails validation.
    /// </summary>
    [Fact(DisplayName = "Invalid Sale should fail validation")]
    public void Given_InvalidSale_When_Validated_Then_ShouldBeInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateInvalidSale();

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Cancel Method Tests

    /// <summary>
    /// Tests that cancelling a sale updates status and timestamp.
    /// </summary>
    [Fact(DisplayName = "Cancel should update status and timestamp")]
    public void Given_ActiveSale_When_Cancelled_Then_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var originalUpdateTime = sale.UpdatedAt;

        // Act
        sale.Cancel();

        // Assert
        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.UpdatedAt.Should().NotBe(originalUpdateTime);
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region AddItem Method Tests

    /// <summary>
    /// Tests that adding an item to sale calculates total correctly.
    /// </summary>
    [Fact(DisplayName = "AddItem should add item and calculate total")]
    public void Given_EmptySale_When_ItemAdded_Then_ShouldAddItemAndCalculateTotal()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleTestData.GenerateValidSaleItem();

        // Act
        sale.AddItem(item);

        // Assert
        sale.Items.Should().Contain(item);
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().BeGreaterThan(0);
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that adding multiple items calculates total correctly.
    /// </summary>
    [Fact(DisplayName = "AddItem should handle multiple items correctly")]
    public void Given_Sale_When_MultipleItemsAdded_Then_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item1 = SaleTestData.GenerateSaleItemWithQuantity(2);
        var item2 = SaleTestData.GenerateSaleItemWithQuantity(3);

        // Act
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Assert
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(item1.Total + item2.Total);
    }

    /// <summary>
    /// Tests that adding item with excessive quantity throws exception.
    /// </summary>
    [Fact(DisplayName = "AddItem should throw exception for excessive quantity")]
    public void Given_Sale_When_ItemWithExcessiveQuantityAdded_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleTestData.GenerateSaleItemWithExcessiveQuantity();

        // Act & Assert
        var act = () => sale.AddItem(item);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*not possible to sell above*identical items*");
    }

    #endregion

    #region CalculateTotalAmount Method Tests

    /// <summary>
    /// Tests that CalculateTotalAmount updates total correctly.
    /// </summary>
    [Fact(DisplayName = "CalculateTotalAmount should update total correctly")]
    public void Given_SaleWithItems_When_CalculateTotalAmountCalled_Then_ShouldUpdateTotal()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(2);
        var expectedTotal = sale.Items.Where(i => !i.IsCancelled).Sum(i => i.Total);

        // Act
        sale.CalculateTotalAmount();

        // Assert
        sale.TotalAmount.Should().Be(expectedTotal);
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that CalculateTotalAmount excludes cancelled items.
    /// </summary>
    [Fact(DisplayName = "CalculateTotalAmount should exclude cancelled items")]
    public void Given_SaleWithCancelledItems_When_CalculateTotalAmountCalled_Then_ShouldExcludeCancelledItems()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(3);
        var itemToCancel = sale.Items.First();
        itemToCancel.Cancel();

        // Act
        sale.CalculateTotalAmount();

        // Assert
        var expectedTotal = sale.Items.Where(i => !i.IsCancelled).Sum(i => i.Total);
        sale.TotalAmount.Should().Be(expectedTotal);
        sale.TotalAmount.Should().NotBe(sale.Items.Sum(i => i.Total));
    }

    #endregion

    #region ModifyItem Method Tests

    /// <summary>
    /// Tests that ModifyItem updates existing item correctly.
    /// </summary>
    [Fact(DisplayName = "ModifyItem should update existing item")]
    public void Given_SaleWithItem_When_ItemModified_Then_ShouldUpdateItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1);
        var item = sale.Items.First();
        var originalTotal = sale.TotalAmount;
        var newQuantity = 5;
        var newUnitPrice = 15.50m;

        // Act
        sale.ModifyItem(item.ProductId, newQuantity, newUnitPrice);

        // Assert
        item.Quantity.Should().Be(newQuantity);
        item.UnitPrice.Should().Be(newUnitPrice);
        sale.TotalAmount.Should().NotBe(originalTotal);
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that ModifyItem does nothing for non-existent item.
    /// </summary>
    [Fact(DisplayName = "ModifyItem should do nothing for non-existent item")]
    public void Given_Sale_When_NonExistentItemModified_Then_ShouldDoNothing()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1);
        var originalTotal = sale.TotalAmount;
        var nonExistentProductId = Guid.NewGuid();

        // Act
        sale.ModifyItem(nonExistentProductId, 5, 15.50m);

        // Assert
        sale.TotalAmount.Should().Be(originalTotal);
    }

    /// <summary>
    /// Tests that ModifyItem ignores cancelled items.
    /// </summary>
    [Fact(DisplayName = "ModifyItem should ignore cancelled items")]
    public void Given_SaleWithCancelledItem_When_CancelledItemModified_Then_ShouldIgnoreItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1);
        var item = sale.Items.First();
        item.Cancel();
        var originalTotal = sale.TotalAmount;

        // Act
        sale.ModifyItem(item.ProductId, 5, 15.50m);

        // Assert
        sale.TotalAmount.Should().Be(originalTotal);
    }

    #endregion

    #region CancelItem Method Tests

    /// <summary>
    /// Tests that CancelItem cancels existing item and recalculates total.
    /// </summary>
    [Fact(DisplayName = "CancelItem should cancel item and recalculate total")]
    public void Given_SaleWithItem_When_ItemCancelled_Then_ShouldCancelItemAndRecalculateTotal()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(2);
        var itemToCancel = sale.Items.First();
        var originalTotal = sale.TotalAmount;

        // Act
        sale.CancelItem(itemToCancel.ProductId);

        // Assert
        itemToCancel.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().BeLessThan(originalTotal);
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that CancelItem does nothing for non-existent item.
    /// </summary>
    [Fact(DisplayName = "CancelItem should do nothing for non-existent item")]
    public void Given_Sale_When_NonExistentItemCancelled_Then_ShouldDoNothing()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1);
        var originalTotal = sale.TotalAmount;
        var nonExistentProductId = Guid.NewGuid();

        // Act
        sale.CancelItem(nonExistentProductId);

        // Assert
        sale.TotalAmount.Should().Be(originalTotal);
        sale.Items.Should().AllSatisfy(item => item.IsCancelled.Should().BeFalse());
    }

    /// <summary>
    /// Tests that CancelItem ignores already cancelled items.
    /// </summary>
    [Fact(DisplayName = "CancelItem should ignore already cancelled items")]
    public void Given_SaleWithCancelledItem_When_AlreadyCancelledItemCancelled_Then_ShouldIgnoreItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1);
        var item = sale.Items.First();
        item.Cancel();
        var originalTotal = sale.TotalAmount;

        // Act
        sale.CancelItem(item.ProductId);

        // Assert
        sale.TotalAmount.Should().Be(originalTotal);
    }

    #endregion

    #region Business Rules Tests

    /// <summary>
    /// Tests that sale with all items cancelled has zero total.
    /// </summary>
    [Fact(DisplayName = "Sale with all cancelled items should have zero total")]
    public void Given_SaleWithAllItemsCancelled_When_TotalCalculated_Then_ShouldBeZero()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(3);

        // Act
        foreach (var item in sale.Items)
        {
            sale.CancelItem(item.ProductId);
        }

        // Assert
        sale.TotalAmount.Should().Be(0);
        sale.Items.Should().AllSatisfy(item => item.IsCancelled.Should().BeTrue());
    }

    /// <summary>
    /// Tests that empty sale has zero total.
    /// </summary>
    [Fact(DisplayName = "Empty sale should have zero total")]
    public void Given_EmptySale_When_TotalCalculated_Then_ShouldBeZero()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.CalculateTotalAmount();

        // Assert
        sale.TotalAmount.Should().Be(0);
    }

    #endregion
}