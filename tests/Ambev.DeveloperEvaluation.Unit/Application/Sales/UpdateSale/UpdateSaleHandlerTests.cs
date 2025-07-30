using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.UpdateSale;

/// <summary>
/// Contains unit tests for the UpdateSaleHandler class.
/// Tests cover successful updates, validation errors, business rule violations,
/// and various edge cases.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper);
    }

    #region Successful Update Tests

    /// <summary>
    /// Tests that a valid command updates a sale successfully.
    /// </summary>
    [Fact(DisplayName = "Handle should update sale successfully with valid command")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldUpdateSaleSuccessfully()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSaleWithItems();
        var expectedResult = new UpdateSaleResult
        {
            Id = existingSale.Id,
            SaleNumber = existingSale.SaleNumber,
            TotalAmount = existingSale.TotalAmount
        };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        await _saleRepository.Received(1).UpdateAsync(existingSale, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that business rules are applied to all items during update.
    /// </summary>
    [Fact(DisplayName = "Handle should apply business rules to all items during update")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldApplyBusinessRulesToItems()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSaleWithItems(2);
        var originalTotal = existingSale.TotalAmount;

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingSale.TotalAmount.Should().BeGreaterThan(0);
        existingSale.UpdatedAt.Should().NotBeNull();
        await _saleRepository.Received(1).UpdateAsync(existingSale, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that invalid command throws validation exception.
    /// </summary>
    [Fact(DisplayName = "Handle should throw ValidationException for invalid command")]
    public async Task Given_InvalidCommand_When_Handle_Then_ShouldThrowValidationException()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateInvalidUpdateSaleCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule Tests

    /// <summary>
    /// Tests that non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Handle should throw KeyNotFoundException for non-existent sale")]
    public async Task Given_NonExistentSale_When_Handle_Then_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.Id} not found");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that updating cancelled sale throws InvalidOperationException.
    /// </summary>
    [Fact(DisplayName = "Handle should throw InvalidOperationException for cancelled sale")]
    public async Task Given_CancelledSale_When_Handle_Then_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var cancelledSale = SaleTestData.GenerateCancelledSale();
        
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(cancelledSale);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update a cancelled sale");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Repository Interaction Tests

    /// <summary>
    /// Tests that repository methods are called with correct parameters.
    /// </summary>
    [Fact(DisplayName = "Handle should call repository methods with correct parameters")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSaleWithItems();

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).UpdateAsync(existingSale, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Property Update Tests

    /// <summary>
    /// Tests that sale properties are updated correctly from command.
    /// </summary>
    [Fact(DisplayName = "Handle should update sale properties from command")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldUpdateSaleProperties()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSaleWithItems();
        var originalCustomerName = existingSale.CustomerName;

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingSale.SaleDate.Should().Be(command.SaleDate);
        existingSale.CustomerId.Should().Be(command.CustomerId);
        existingSale.CustomerName.Should().Be(command.CustomerName);
        existingSale.BranchId.Should().Be(command.BranchId);
        existingSale.BranchName.Should().Be(command.BranchName);
        existingSale.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Mapping Tests

    /// <summary>
    /// Tests that AutoMapper is called with correct objects.
    /// </summary>
    [Fact(DisplayName = "Handle should call mapper with correct objects")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldCallMapperWithCorrectObjects()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSaleWithItems();
        var result = new UpdateSaleResult();

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapper.Received(1).Map<UpdateSaleResult>(existingSale);
    }

    #endregion

    #region Cancellation Token Tests

    /// <summary>
    /// Tests that cancellation token is properly handled.
    /// </summary>
    [Fact(DisplayName = "Handle should handle cancellation token properly")]
    public async Task Given_CancellationRequested_When_Handle_Then_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidUpdateSaleCommand();
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}