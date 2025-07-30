using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the SaleItem entity.
/// Tests cover entity creation, business logic validation, discount rules,
/// and state changes to ensure the SaleItem entity behaves correctly.
/// </summary>
public class SaleItemTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests that a new SaleItem is created with correct default values.
    /// </summary>
    [Fact(DisplayName = "New SaleItem should have correct default values")]
    public void Given_NewSaleItem_When_Created_Then_ShouldHaveCorrectDefaults()
    {
        // Act
        var saleItem = new SaleItem();

        // Assert
        saleItem.IsCancelled.Should().BeFalse();
        saleItem.Discount.Should().Be(0);
        saleItem.Total.Should().Be(0);
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that a valid SaleItem passes validation.
    /// </summary>
    [Fact(DisplayName = "Valid SaleItem should pass validation")]
    public void Given_ValidSaleItem_When_Validated_Then_ShouldBeValid()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ApplyDiscountRules(); // Apply business rules

        // Act
        var result = saleItem.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Discount Rules Tests

    /// <summary>
    /// Tests that items with quantity less than 4 have no discount.
    /// </summary>
    [Theory(DisplayName = "Items with quantity less than 4 should have no discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_SaleItemWithQuantityLessThan4_When_DiscountRulesApplied_Then_ShouldHaveNoDiscount(int quantity)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateSaleItemWithQuantity(quantity);

        // Act
        saleItem.ApplyDiscountRules();

        // Assert
        saleItem.Discount.Should().Be(0);
    }

    /// <summary>
    /// Tests that items with quantity 4-9 have 10% discount.
    /// </summary>
    [Theory(DisplayName = "Items with quantity 4-9 should have 10% discount")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Given_SaleItemWithQuantity4To9_When_DiscountRulesApplied_Then_ShouldHave10PercentDiscount(int quantity)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateSaleItemWithQuantity(quantity);

        // Act
        saleItem.ApplyDiscountRules();

        // Assert
        saleItem.Discount.Should().Be(SaleBusinessRules.StandardDiscountPercentage);
    }

    /// <summary>
    /// Tests that items with quantity 10-20 have 20% discount.
    /// </summary>
    [Theory(DisplayName = "Items with quantity 10-20 should have 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_SaleItemWithQuantity10To20_When_DiscountRulesApplied_Then_ShouldHave20PercentDiscount(int quantity)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateSaleItemWithQuantity(quantity);

        // Act
        saleItem.ApplyDiscountRules();

        // Assert
        saleItem.Discount.Should().Be(SaleBusinessRules.HighDiscountPercentage);
    }

    /// <summary>
    /// Tests that items with quantity above 20 throw exception.
    /// </summary>
    [Fact(DisplayName = "Items with quantity above 20 should throw exception")]
    public void Given_SaleItemWithQuantityAbove20_When_DiscountRulesApplied_Then_ShouldThrowException()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateSaleItemWithQuantity(21);

        // Act & Assert
        var act = () => saleItem.ApplyDiscountRules();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"*not possible to sell above {SaleBusinessRules.MaxQuantityPerItem} identical items*");
    }

    #endregion

    #region Total Calculation Tests

    /// <summary>
    /// Tests that total is calculated correctly without discount.
    /// </summary>
    [Fact(DisplayName = "Total should be calculated correctly without discount")]
    public void Given_SaleItemWithoutDiscount_When_TotalCalculated_Then_ShouldBeQuantityTimesUnitPrice()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 2,
            UnitPrice = 10.50m,
            Discount = 0
        };

        // Act
        saleItem.CalculateTotal();

        // Assert
        saleItem.Total.Should().Be(21.00m);
    }

    /// <summary>
    /// Tests that total is calculated correctly with discount.
    /// </summary>
    [Fact(DisplayName = "Total should be calculated correctly with discount")]
    public void Given_SaleItemWithDiscount_When_TotalCalculated_Then_ShouldApplyDiscount()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 5,
            UnitPrice = 10.00m,
            Discount = 10m // 10% discount
        };

        // Act
        saleItem.CalculateTotal();

        // Assert
        saleItem.Total.Should().Be(45.00m); // 50 - (50 * 0.1) = 45
    }

    /// <summary>
    /// Tests that ApplyDiscountRules calculates total automatically.
    /// </summary>
    [Fact(DisplayName = "ApplyDiscountRules should calculate total automatically")]
    public void Given_SaleItem_When_DiscountRulesApplied_Then_ShouldCalculateTotal()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 5,
            UnitPrice = 10.00m
        };

        // Act
        saleItem.ApplyDiscountRules();

        // Assert
        saleItem.Total.Should().BeGreaterThan(0);
        saleItem.Discount.Should().Be(SaleBusinessRules.StandardDiscountPercentage);
    }

    #endregion

    #region Cancel Method Tests

    /// <summary>
    /// Tests that Cancel method sets IsCancelled to true.
    /// </summary>
    [Fact(DisplayName = "Cancel should set IsCancelled to true")]
    public void Given_SaleItem_When_Cancelled_Then_ShouldSetIsCancelledToTrue()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();

        // Act
        saleItem.Cancel();

        // Assert
        saleItem.IsCancelled.Should().BeTrue();
    }

    #endregion

    #region Edge Cases Tests

    /// <summary>
    /// Tests that zero quantity doesn't cause division by zero or negative values.
    /// </summary>
    [Fact(DisplayName = "Zero quantity should result in zero total")]
    public void Given_SaleItemWithZeroQuantity_When_TotalCalculated_Then_ShouldBeZero()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 0,
            UnitPrice = 10.00m,
            Discount = 0
        };

        // Act
        saleItem.CalculateTotal();

        // Assert
        saleItem.Total.Should().Be(0);
    }

    /// <summary>
    /// Tests that maximum discount (100%) results in zero total.
    /// </summary>
    [Fact(DisplayName = "Maximum discount should result in zero total")]
    public void Given_SaleItemWithMaximumDiscount_When_TotalCalculated_Then_ShouldBeZero()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 2,
            UnitPrice = 10.00m,
            Discount = 100m // 100% discount
        };

        // Act
        saleItem.CalculateTotal();

        // Assert
        saleItem.Total.Should().Be(0);
    }

    #endregion
}