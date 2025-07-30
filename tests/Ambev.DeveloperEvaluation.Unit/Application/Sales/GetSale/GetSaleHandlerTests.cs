using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSale;

/// <summary>
/// Contains unit tests for the GetSaleHandler class.
/// Tests cover successful retrieval, validation errors, not found scenarios,
/// and various edge cases.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    #region Successful Retrieval Tests

    /// <summary>
    /// Tests that a valid query retrieves a sale successfully.
    /// </summary>
    [Fact(DisplayName = "Handle should retrieve sale successfully with valid query")]
    public async Task Given_ValidQuery_When_Handle_Then_ShouldRetrieveSaleSuccessfully()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var expectedResult = new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            CustomerName = sale.CustomerName,
            BranchName = sale.BranchName
        };

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that sale with items is retrieved with all item details.
    /// </summary>
    [Fact(DisplayName = "Handle should retrieve sale with all item details")]
    public async Task Given_SaleWithItems_When_Handle_Then_ShouldRetrieveAllItemDetails()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateValidSaleWithItems(3);
        var expectedResult = new GetSaleResult
        {
            Id = sale.Id,
            Items = new List<GetSaleItemResult>()
        };

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<GetSaleResult>(sale);
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that invalid query throws validation exception.
    /// </summary>
    [Fact(DisplayName = "Handle should throw ValidationException for invalid query")]
    public async Task Given_InvalidQuery_When_Handle_Then_ShouldThrowValidationException()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateInvalidGetSaleQuery();

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Not Found Tests

    /// <summary>
    /// Tests that non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Handle should throw KeyNotFoundException for non-existent sale")]
    public async Task Given_NonExistentSale_When_Handle_Then_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {query.Id} not found");
    }

    #endregion

    #region Repository Interaction Tests

    /// <summary>
    /// Tests that repository methods are called with correct parameters.
    /// </summary>
    [Fact(DisplayName = "Handle should call repository methods with correct parameters")]
    public async Task Given_ValidQuery_When_Handle_Then_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateValidSaleWithItems();

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Mapping Tests

    /// <summary>
    /// Tests that AutoMapper is called with correct objects.
    /// </summary>
    [Fact(DisplayName = "Handle should call mapper with correct objects")]
    public async Task Given_ValidQuery_When_Handle_Then_ShouldCallMapperWithCorrectObjects()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var result = new GetSaleResult();

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mapper.Received(1).Map<GetSaleResult>(sale);
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
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Edge Cases Tests

    /// <summary>
    /// Tests handling of sale with cancelled items.
    /// </summary>
    [Fact(DisplayName = "Handle should retrieve sale with cancelled items correctly")]
    public async Task Given_SaleWithCancelledItems_When_Handle_Then_ShouldRetrieveCorrectly()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateValidSaleWithItems(2);
        sale.Items.First().Cancel(); // Cancel one item

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests handling of cancelled sale.
    /// </summary>
    [Fact(DisplayName = "Handle should retrieve cancelled sale correctly")]
    public async Task Given_CancelledSale_When_Handle_Then_ShouldRetrieveCorrectly()
    {
        // Arrange
        var query = SaleCommandTestData.GenerateValidGetSaleQuery();
        var sale = SaleTestData.GenerateCancelledSale();

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
    }

    #endregion
}