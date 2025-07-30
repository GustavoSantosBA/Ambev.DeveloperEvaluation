using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of UpdateSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the UpdateSaleCommand request
    /// </summary>
    /// <param name="command">The UpdateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale details</returns>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if the sale exists
        var existingSale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingSale == null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        // Check if sale is already cancelled
        if (existingSale.Status == Domain.Enums.SaleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale");

        // Update sale properties
        UpdateSaleFromCommand(existingSale, command);

        // Apply business rules to each item
        foreach (var item in existingSale.Items)
        {
            item.ApplyDiscountRules();
        }

        // Calculate total amount
        existingSale.CalculateTotalAmount();

        // Save the updated sale
        var updatedSale = await _saleRepository.UpdateAsync(existingSale, cancellationToken);
        var result = _mapper.Map<UpdateSaleResult>(updatedSale);
        
        return result;
    }

    /// <summary>
    /// Updates the existing sale entity with data from the command
    /// </summary>
    /// <param name="sale">The existing sale entity</param>
    /// <param name="command">The update command</param>
    private void UpdateSaleFromCommand(Sale sale, UpdateSaleCommand command)
    {
        // Update sale properties
        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;

        // Clear existing items and add new ones
        sale.Items.Clear();

        foreach (var itemCommand in command.Items)
        {
            var saleItem = new SaleItem
            {
                ProductId = itemCommand.ProductId,
                ProductName = itemCommand.ProductName,
                Quantity = itemCommand.Quantity,
                UnitPrice = itemCommand.UnitPrice
            };

            sale.Items.Add(saleItem);
        }

        // Update timestamp
        sale.UpdatedAt = DateTime.UtcNow;
    }
}