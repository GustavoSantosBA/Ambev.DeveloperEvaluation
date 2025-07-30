using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleItemValidator class.
/// Tests cover validation of all sale item properties including product information,
/// quantity, pricing, discount rules, and business logic validation.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator;

    public SaleItemValidatorTests()
    {
        _validator = new SaleItemValidator();
    }

    #region Valid SaleItem Tests

    /// <summary>
    /// Tests that validation passes when all sale item properties are valid.
    /// </summary>
    [Fact(DisplayName = "Valid sale item should pass all validation rules")]
    public void Given_ValidSaleItem_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ApplyDiscountRules(); // Apply business rules to set proper values

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region ProductId Validation Tests

    /// <summary>
    /// Tests that validation fails when product ID is empty.
    /// </summary>
    [Fact(DisplayName = "Empty product ID should fail validation")]
    public void Given_EmptyProductId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
              .WithErrorMessage("Product ID is required");
    }

    /// <summary>
    /// Tests that validation passes when product ID is valid.
    /// </summary>
    [Fact(DisplayName = "Valid product ID should pass validation")]
    public void Given_ValidProductId_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductId = Guid.NewGuid();

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    #endregion

    #region ProductName Validation Tests

    /// <summary>
    /// Tests that validation fails when product name is empty.
    /// </summary>
    [Fact(DisplayName = "Empty product name should fail validation")]
    public void Given_EmptyProductName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductName = string.Empty;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
              .WithErrorMessage("Product name is required");
    }

    /// <summary>
    /// Tests that validation fails when product name is null.
    /// </summary>
    [Fact(DisplayName = "Null product name should fail validation")]
    public void Given_NullProductName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductName = null!;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
              .WithErrorMessage("Product name is required");
    }

    /// <summary>
    /// Tests that validation fails when product name exceeds maximum length.
    /// </summary>
    [Fact(DisplayName = "Product name exceeding maximum length should fail validation")]
    public void Given_ProductNameExceedingMaxLength_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductName = new string('A', 101); // 101 characters, exceeds 100 limit

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
              .WithErrorMessage("Product name cannot exceed 100 characters");
    }

    /// <summary>
    /// Tests that validation passes when product name is at maximum length.
    /// </summary>
    [Fact(DisplayName = "Product name at maximum length should pass validation")]
    public void Given_ProductNameAtMaxLength_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.ProductName = new string('A', 100); // Exactly 100 characters

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductName);
    }

    #endregion

    #region Quantity Validation Tests

    /// <summary>
    /// Tests that validation fails when quantity is zero.
    /// </summary>
    [Fact(DisplayName = "Zero quantity should fail validation")]
    public void Given_ZeroQuantity_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = 0;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Quantity must be greater than zero");
    }

    /// <summary>
    /// Tests that validation fails when quantity is negative.
    /// </summary>
    [Fact(DisplayName = "Negative quantity should fail validation")]
    public void Given_NegativeQuantity_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = -1;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Quantity must be greater than zero");
    }

    /// <summary>
    /// Tests that validation fails when quantity exceeds maximum allowed.
    /// </summary>
    [Fact(DisplayName = "Quantity exceeding maximum should fail validation")]
    public void Given_QuantityExceedingMaximum_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = SaleBusinessRules.MaxQuantityPerItem + 1;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage($"Cannot sell above {SaleBusinessRules.MaxQuantityPerItem} identical items");
    }

    /// <summary>
    /// Tests that validation passes for valid quantity values.
    /// </summary>
    [Theory(DisplayName = "Valid quantity should pass validation")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)] // Maximum allowed
    public void Given_ValidQuantity_When_Validated_Then_ShouldNotHaveError(int quantity)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = quantity;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    #endregion

    #region UnitPrice Validation Tests

    /// <summary>
    /// Tests that validation fails when unit price is zero.
    /// </summary>
    [Fact(DisplayName = "Zero unit price should fail validation")]
    public void Given_ZeroUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.UnitPrice = 0;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
              .WithErrorMessage("Unit price must be greater than zero");
    }

    /// <summary>
    /// Tests that validation fails when unit price is negative.
    /// </summary>
    [Fact(DisplayName = "Negative unit price should fail validation")]
    public void Given_NegativeUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.UnitPrice = -10.50m;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
              .WithErrorMessage("Unit price must be greater than zero");
    }

    /// <summary>
    /// Tests that validation passes when unit price is positive.
    /// </summary>
    [Fact(DisplayName = "Positive unit price should pass validation")]
    public void Given_PositiveUnitPrice_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.UnitPrice = 25.99m;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UnitPrice);
    }

    #endregion

    #region Discount Validation Tests

    /// <summary>
    /// Tests that validation fails when discount is negative.
    /// </summary>
    [Fact(DisplayName = "Negative discount should fail validation")]
    public void Given_NegativeDiscount_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Discount = -5;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Discount)
              .WithErrorMessage("Discount cannot be negative");
    }

    /// <summary>
    /// Tests that validation fails when discount exceeds 100%.
    /// </summary>
    [Fact(DisplayName = "Discount exceeding 100% should fail validation")]
    public void Given_DiscountExceeding100Percent_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Discount = 101;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Discount)
              .WithErrorMessage("Discount cannot exceed 100%");
    }

    /// <summary>
    /// Tests that validation passes for valid discount values.
    /// </summary>
    [Theory(DisplayName = "Valid discount should pass validation")]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Given_ValidDiscount_When_Validated_Then_ShouldNotHaveError(decimal discount)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Discount = discount;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Discount);
    }

    #endregion

    #region Business Rule: Discount Validation Tests

    /// <summary>
    /// Tests that validation fails when item has discount but quantity is below minimum.
    /// </summary>
    [Theory(DisplayName = "Discount with quantity below minimum should fail validation")]
    [InlineData(1, 10)]
    [InlineData(2, 5)]
    [InlineData(3, 15)]
    public void Given_DiscountWithQuantityBelowMinimum_When_Validated_Then_ShouldHaveError(int quantity, decimal discount)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = quantity;
        saleItem.Discount = discount;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage($"Purchases below {SaleBusinessRules.MinQuantityForDiscount} items cannot have a discount");
    }

    /// <summary>
    /// Tests that validation passes when item has no discount and quantity is below minimum.
    /// </summary>
    [Theory(DisplayName = "No discount with quantity below minimum should pass validation")]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    public void Given_NoDiscountWithQuantityBelowMinimum_When_Validated_Then_ShouldNotHaveError(int quantity, decimal discount)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = quantity;
        saleItem.Discount = discount;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    /// <summary>
    /// Tests that validation passes when item has discount and quantity meets minimum.
    /// </summary>
    [Theory(DisplayName = "Discount with quantity meeting minimum should pass validation")]
    [InlineData(4, 10)]
    [InlineData(10, 20)]
    [InlineData(15, 25)]
    public void Given_DiscountWithQuantityMeetingMinimum_When_Validated_Then_ShouldNotHaveError(int quantity, decimal discount)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Quantity = quantity;
        saleItem.Discount = discount;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Total Validation Tests

    /// <summary>
    /// Tests that validation fails when total is negative.
    /// </summary>
    [Fact(DisplayName = "Negative total should fail validation")]
    public void Given_NegativeTotal_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Total = -10.50m;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Total)
              .WithErrorMessage("Total amount cannot be negative");
    }

    /// <summary>
    /// Tests that validation passes when total is zero or positive.
    /// </summary>
    [Theory(DisplayName = "Zero or positive total should pass validation")]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(100.99)]
    public void Given_ZeroOrPositiveTotal_When_Validated_Then_ShouldNotHaveError(decimal total)
    {
        // Arrange
        var saleItem = SaleTestData.GenerateValidSaleItem();
        saleItem.Total = total;

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Total);
    }

    #endregion

    #region Complex Validation Scenarios

    /// <summary>
    /// Tests that validation can handle multiple errors simultaneously.
    /// </summary>
    [Fact(DisplayName = "Sale item with multiple errors should fail validation with all errors")]
    public void Given_SaleItemWithMultipleErrors_When_Validated_Then_ShouldHaveAllErrors()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            ProductId = Guid.Empty, // Invalid
            ProductName = string.Empty, // Invalid
            Quantity = 0, // Invalid
            UnitPrice = -10, // Invalid
            Discount = -5, // Invalid
            Total = -100 // Invalid
        };

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
        result.ShouldHaveValidationErrorFor(x => x.Discount);
        result.ShouldHaveValidationErrorFor(x => x.Total);
    }

    /// <summary>
    /// Tests edge case with maximum allowed values.
    /// </summary>
    [Fact(DisplayName = "Sale item with maximum allowed values should pass validation")]
    public void Given_SaleItemWithMaximumAllowedValues_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = new string('A', 100), // Maximum length
            Quantity = SaleBusinessRules.MaxQuantityPerItem, // Maximum quantity
            UnitPrice = decimal.MaxValue,
            Discount = 100, // Maximum discount
            Total = 0 // Can be zero when 100% discount
        };

        // Act
        var result = _validator.TestValidate(saleItem);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}