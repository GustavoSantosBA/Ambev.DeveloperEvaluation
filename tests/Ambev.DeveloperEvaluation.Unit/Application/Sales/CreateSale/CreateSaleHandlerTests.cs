using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale;

/// <summary>
/// Contains unit tests for the CreateSaleHandler class.
/// Tests cover successful creation, validation errors, business rule violations,
/// and domain event publishing.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _publisher);
    }

    #region Successful Creation Tests

    /// <summary>
    /// Tests that a valid command creates a sale successfully.
    /// </summary>
    [Fact(DisplayName = "Handle should create sale successfully with valid command")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldCreateSaleSuccessfully()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var expectedResult = new CreateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount
        };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        await _saleRepository.Received(1).CreateAsync(sale, Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that business rules are applied to all items during creation.
    /// </summary>
    [Fact(DisplayName = "Handle should apply business rules to all items")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldApplyBusinessRulesToItems()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale();
        var item1 = SaleTestData.GenerateValidSaleItem();
        var item2 = SaleTestData.GenerateValidSaleItem();
        sale.Items.Add(item1);
        sale.Items.Add(item2);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify that ApplyDiscountRules was called (indirectly by checking total calculation)
        sale.TotalAmount.Should().BeGreaterThan(0);
        await _saleRepository.Received(1).CreateAsync(sale, Arg.Any<CancellationToken>());
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
        var command = SaleCommandTestData.GenerateInvalidCreateSaleCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule Tests

    /// <summary>
    /// Tests that duplicate sale number throws InvalidOperationException.
    /// </summary>
    [Fact(DisplayName = "Handle should throw InvalidOperationException for duplicate sale number")]
    public async Task Given_DuplicateSaleNumber_When_Handle_Then_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateValidSaleWithItems();
        var command = SaleCommandTestData.GenerateCreateSaleCommandWithDuplicateNumber(existingSale.SaleNumber);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(existingSale);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Sale with number {command.SaleNumber} already exists");
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Domain Event Tests

    /// <summary>
    /// Tests that SaleCreatedEvent is published after successful creation.
    /// </summary>
    [Fact(DisplayName = "Handle should publish SaleCreatedEvent after successful creation")]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldPublishSaleCreatedEvent()
    {
        // Arrange
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSaleWithItems();

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _publisher.Received(1).Publish(
            Arg.Is<SaleCreatedEvent>(e => e.Sale == sale),
            Arg.Any<CancellationToken>());
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
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSaleWithItems();

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(sale, Arg.Any<CancellationToken>());
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
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var result = new CreateSaleResult();

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapper.Received(1).Map<Sale>(command);
        _mapper.Received(1).Map<CreateSaleResult>(sale);
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
        var command = SaleCommandTestData.GenerateValidCreateSaleCommand();
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}