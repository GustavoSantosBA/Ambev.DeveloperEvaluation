using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.Commands;

/// <summary>
/// Contains unit tests for the CreateSaleCommand class.
/// Tests cover command creation, validation, and property assignments.
/// </summary>
public class CreateSaleCommandTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests that a new CreateSaleCommand is created with correct default values.
    /// </summary>
    [Fact(DisplayName = "New CreateSaleCommand should have correct default values")]
    public void Given_NewCreateSaleCommand_When_Created_Then_ShouldHaveCorrectDefaults()
    {
        // Act
        var command = new CreateSaleCommand();

        // Assert
        command.SaleNumber.Should().BeEmpty();
        command.CustomerName.Should().BeEmpty();
        command.BranchName.Should().BeEmpty();
        command.Items.Should().NotBeNull().And.BeEmpty();
        command.CustomerId.Should().Be(Guid.Empty);
        command.BranchId.Should().Be(Guid.Empty);
        command.SaleDate.Should().BeNull();
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that valid command passes validation.
    /// </summary>
    [Fact(DisplayName = "Valid CreateSaleCommand should pass validation")]
    public void Given_ValidCreateSaleCommand_When_Validated_Then_ShouldBeValid()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();

        // Act
        var result = command.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that invalid command fails validation.
    /// </summary>
    [Fact(DisplayName = "Invalid CreateSaleCommand should fail validation")]
    public void Given_InvalidCreateSaleCommand_When_Validated_Then_ShouldBeInvalid()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateInvalidCreateSaleCommand();

        // Act
        var result = command.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Property Assignment Tests

    /// <summary>
    /// Tests that all properties can be assigned correctly.
    /// </summary>
    [Fact(DisplayName = "CreateSaleCommand should allow property assignment")]
    public void Given_CreateSaleCommand_When_PropertiesAssigned_Then_ShouldRetainValues()
    {
        // Arrange
        var command = new CreateSaleCommand();
        var saleNumber = "TEST-001";
        var customerName = "Test Customer";
        var branchName = "Test Branch";
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var saleDate = DateTime.UtcNow;
        var item = SaleCommandTestData.GenerateValidCreateSaleItemCommand();

        // Act
        command.SaleNumber = saleNumber;
        command.CustomerName = customerName;
        command.BranchName = branchName;
        command.CustomerId = customerId;
        command.BranchId = branchId;
        command.SaleDate = saleDate;
        command.Items.Add(item);

        // Assert
        command.SaleNumber.Should().Be(saleNumber);
        command.CustomerName.Should().Be(customerName);
        command.BranchName.Should().Be(branchName);
        command.CustomerId.Should().Be(customerId);
        command.BranchId.Should().Be(branchId);
        command.SaleDate.Should().Be(saleDate);
        command.Items.Should().Contain(item);
    }

    #endregion
}