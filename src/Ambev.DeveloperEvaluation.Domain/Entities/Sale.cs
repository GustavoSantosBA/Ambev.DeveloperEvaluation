using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Common.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale in the system following DDD principles.
/// Uses External Identities pattern with denormalization for cross-domain references.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique sale number identifier
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the sale was made
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the customer external identifier (External Identities pattern)
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name (denormalized for performance)
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch external identifier where the sale was made
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the branch name (denormalized for performance)
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total sale amount (calculated from all items)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the sale status (Active/Cancelled)
    /// </summary>
    public SaleStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the collection of items in this sale
    /// </summary>
    public List<SaleItem> Items { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the Sale class
    /// </summary>
    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        Status = SaleStatus.Active;
        SaleDate = DateTime.UtcNow;
        Items = new List<SaleItem>(); // Garantir inicialização
    }

    /// <summary>
    /// Validates the sale entity using FluentValidation
    /// </summary>
    /// <returns>Validation result with errors if any</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    /// <summary>
    /// Cancels the entire sale
    /// </summary>
    public void Cancel()
    {
        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates the total amount based on active items
    /// </summary>
    public void CalculateTotalAmount()
    {
        // Verificação de segurança para evitar NullReferenceException
        if (Items == null)
        {
            Items = new List<SaleItem>();
        }
        
        TotalAmount = Items.Where(item => !item.IsCancelled).Sum(item => item.Total);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an item to the sale with business rule validation
    /// </summary>
    /// <param name="item">The sale item to add</param>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public void AddItem(SaleItem item)
    {
        // Verificação de segurança
        if (Items == null)
        {
            Items = new List<SaleItem>();
        }
        
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        
        // Business rule validation is handled in SaleItem.ApplyDiscountRules()
        item.ApplyDiscountRules();
        Items.Add(item);
        CalculateTotalAmount();
    }

    /// <summary>
    /// Modifies an existing item in the sale
    /// </summary>
    /// <param name="productId">The product ID to modify</param>
    /// <param name="newQuantity">The new quantity</param>
    /// <param name="newUnitPrice">The new unit price</param>
    public void ModifyItem(Guid productId, int newQuantity, decimal newUnitPrice)
    {
        // Verificação de segurança
        if (Items == null)
        {
            Items = new List<SaleItem>();
            return;
        }
        
        var item = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (item != null)
        {
            item.Quantity = newQuantity;
            item.UnitPrice = newUnitPrice;
            item.ApplyDiscountRules();
            CalculateTotalAmount();
        }
    }

    /// <summary>
    /// Cancels a specific item in the sale
    /// </summary>
    /// <param name="productId">The product ID to cancel</param>
    public void CancelItem(Guid productId)
    {
        // Verificação de segurança
        if (Items == null)
        {
            Items = new List<SaleItem>();
            return;
        }
        
        var item = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (item != null)
        {
            item.Cancel();
            CalculateTotalAmount();
        }
    }
}

