using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleValidator class.
/// Tests cover validation of all sale properties including sale number, dates,
/// customer information, branch information, status, items, and total amount.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator;

    public SaleValidatorTests()
    {
        _validator = new SaleValidator();
    }

    #region Valid Sale Tests

    /// <summary>
    /// Tests that validation passes when all sale properties are valid.
    /// This test verifies that a sale with valid properties passes all validation rules.
    /// </summary>
    [Fact(DisplayName = "Valid sale should pass all validation rules")]
    public void Given_ValidSale_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(2);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region SaleNumber Validation Tests

    /// <summary>
    /// Tests that validation fails when sale number is empty.
    /// </summary>
    [Fact(DisplayName = "Empty sale number should fail validation")]
    public void Given_EmptySaleNumber_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleNumber = string.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber)
              .WithErrorMessage("Sale number is required");
    }

    /// <summary>
    /// Tests that validation fails when sale number is null.
    /// </summary>
    [Fact(DisplayName = "Null sale number should fail validation")]
    public void Given_NullSaleNumber_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleNumber = null!;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber)
              .WithErrorMessage("Sale number is required");
    }

    /// <summary>
    /// Tests that validation fails when sale number exceeds maximum length.
    /// </summary>
    [Fact(DisplayName = "Sale number exceeding maximum length should fail validation")]
    public void Given_SaleNumberExceedingMaxLength_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleNumber = new string('A', 51); // 51 characters, exceeds 50 limit

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber)
              .WithErrorMessage("Sale number cannot exceed 50 characters");
    }

    /// <summary>
    /// Tests that validation passes when sale number is at maximum length.
    /// </summary>
    [Fact(DisplayName = "Sale number at maximum length should pass validation")]
    public void Given_SaleNumberAtMaxLength_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleNumber = new string('A', 50); // Exactly 50 characters

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SaleNumber);
    }

    #endregion

    #region SaleDate Validation Tests

    /// <summary>
    /// Tests that validation fails when sale date is empty/default.
    /// </summary>
    [Fact(DisplayName = "Default sale date should fail validation")]
    public void Given_DefaultSaleDate_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleDate = default(DateTime);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleDate)
              .WithErrorMessage("Sale date is required");
    }

    /// <summary>
    /// Tests that validation fails when sale date is in the future.
    /// </summary>
    [Fact(DisplayName = "Future sale date should fail validation")]
    public void Given_FutureSaleDate_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleDate)
              .WithErrorMessage("Sale date cannot be in the future");
    }

    /// <summary>
    /// Tests that validation passes when sale date is current time.
    /// </summary>
    [Fact(DisplayName = "Current sale date should pass validation")]
    public void Given_CurrentSaleDate_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        // Usar data passada por 1 segundo para evitar problemas de timing
        sale.SaleDate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SaleDate);
    }

    /// <summary>
    /// Tests that validation passes when sale date is in the past.
    /// </summary>
    [Fact(DisplayName = "Past sale date should pass validation")]
    public void Given_PastSaleDate_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SaleDate);
    }

    #endregion

    #region Customer Validation Tests

    /// <summary>
    /// Tests that validation fails when customer ID is empty.
    /// </summary>
    [Fact(DisplayName = "Empty customer ID should fail validation")]
    public void Given_EmptyCustomerId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.CustomerId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
              .WithErrorMessage("Customer ID is required");
    }

    /// <summary>
    /// Tests that validation fails when customer name is empty.
    /// </summary>
    [Fact(DisplayName = "Empty customer name should fail validation")]
    public void Given_EmptyCustomerName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.CustomerName = string.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
              .WithErrorMessage("Customer name is required");
    }

    /// <summary>
    /// Tests that validation fails when customer name exceeds maximum length.
    /// </summary>
    [Fact(DisplayName = "Customer name exceeding maximum length should fail validation")]
    public void Given_CustomerNameExceedingMaxLength_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.CustomerName = new string('A', 101); // 101 characters, exceeds 100 limit

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
              .WithErrorMessage("Customer name cannot exceed 100 characters");
    }

    #endregion

    #region Branch Validation Tests

    /// <summary>
    /// Tests that validation fails when branch ID is empty.
    /// </summary>
    [Fact(DisplayName = "Empty branch ID should fail validation")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.BranchId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchId)
              .WithErrorMessage("Branch ID is required");
    }

    /// <summary>
    /// Tests that validation fails when branch name is empty.
    /// </summary>
    [Fact(DisplayName = "Empty branch name should fail validation")]
    public void Given_EmptyBranchName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.BranchName = string.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchName)
              .WithErrorMessage("Branch name is required");
    }

    /// <summary>
    /// Tests that validation fails when branch name exceeds maximum length.
    /// </summary>
    [Fact(DisplayName = "Branch name exceeding maximum length should fail validation")]
    public void Given_BranchNameExceedingMaxLength_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.BranchName = new string('A', 101); // 101 characters, exceeds 100 limit

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchName)
              .WithErrorMessage("Branch name cannot exceed 100 characters");
    }

    #endregion

    #region Status Validation Tests

    /// <summary>
    /// Tests that validation fails when status is invalid enum value.
    /// </summary>
    [Fact(DisplayName = "Invalid status should fail validation")]
    public void Given_InvalidStatus_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.Status = (SaleStatus)999; // Invalid enum value

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("Sale status é inválido");
    }

    /// <summary>
    /// Tests that validation passes for valid status values.
    /// </summary>
    [Theory(DisplayName = "Valid status should pass validation")]
    [InlineData(SaleStatus.Active)]
    [InlineData(SaleStatus.Cancelled)]
    public void Given_ValidStatus_When_Validated_Then_ShouldNotHaveError(SaleStatus status)
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.Status = status;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    #endregion

    #region Items Validation Tests

    /// <summary>
    /// Tests that validation fails when sale has no items.
    /// </summary>
    [Fact(DisplayName = "Sale without items should fail validation")]
    public void Given_SaleWithoutItems_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Items.Clear();

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Sale must contain at least one item");
    }

    /// <summary>
    /// Tests that validation fails when items collection is null.
    /// </summary>
    [Fact(DisplayName = "Sale with null items should fail validation")]
    public void Given_SaleWithNullItems_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Items = null!;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Sale must contain at least one item");
    }

    /// <summary>
    /// Tests that validation includes item validation for each item.
    /// </summary>
    [Fact(DisplayName = "Sale should validate each item")]
    public void Given_SaleWithInvalidItem_When_Validated_Then_ShouldHaveItemError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems(1); // Começar com uma venda válida
        
        // Tornar o item inválido
        var item = sale.Items.First();
        item.ProductName = string.Empty; // Tornar inválido
        
        // Recalcular total para manter a venda válida em outros aspectos
        sale.CalculateTotalAmount();

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        // Verificar que há erro de validação relacionado aos itens
        // O erro específico será em Items[0].ProductName
        result.ShouldHaveValidationErrorFor("Items[0].ProductName")
              .WithErrorMessage("Product name is required");
              
        // Garantir que o total não está causando erro adicional
        result.ShouldNotHaveValidationErrorFor(x => x.TotalAmount);
    }

    #endregion

    #region TotalAmount Validation Tests

    /// <summary>
    /// Tests that validation fails when total amount is zero.
    /// </summary>
    [Fact(DisplayName = "Zero total amount should fail validation")]
    public void Given_ZeroTotalAmount_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.TotalAmount = 0;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount)
              .WithErrorMessage("Total amount must be greater than zero");
    }

    /// <summary>
    /// Tests that validation fails when total amount is negative.
    /// </summary>
    [Fact(DisplayName = "Negative total amount should fail validation")]
    public void Given_NegativeTotalAmount_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.TotalAmount = -10.50m;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount)
              .WithErrorMessage("Total amount must be greater than zero");
    }

    /// <summary>
    /// Tests that validation passes when total amount is positive.
    /// </summary>
    [Fact(DisplayName = "Positive total amount should pass validation")]
    public void Given_PositiveTotalAmount_When_Validated_Then_ShouldNotHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.TotalAmount = 100.50m;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TotalAmount);
    }

    #endregion

    #region Complex Validation Scenarios

    /// <summary>
    /// Tests that validation can handle multiple errors simultaneously.
    /// </summary>
    [Fact(DisplayName = "Sale with multiple errors should fail validation with all errors")]
    public void Given_SaleWithMultipleErrors_When_Validated_Then_ShouldHaveAllErrors()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = string.Empty, // Invalid
            SaleDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
            CustomerId = Guid.Empty, // Invalid
            CustomerName = string.Empty, // Invalid
            BranchId = Guid.Empty, // Invalid
            BranchName = string.Empty, // Invalid
            TotalAmount = 0, // Invalid
            Items = new List<SaleItem>() // Invalid - empty list
        };

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber);
        result.ShouldHaveValidationErrorFor(x => x.SaleDate);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor(x => x.BranchId);
        result.ShouldHaveValidationErrorFor(x => x.BranchName);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    #endregion
}