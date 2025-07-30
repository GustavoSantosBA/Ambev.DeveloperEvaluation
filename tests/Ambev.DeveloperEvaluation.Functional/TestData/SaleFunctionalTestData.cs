using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Constants;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Functional.TestData;

/// <summary>
/// Provides test data for functional testing scenarios
/// </summary>
public static class SaleFunctionalTestData
{
    /// <summary>
    /// Creates a complete sales scenario for a small retail purchase
    /// </summary>
    public static class SmallRetailPurchaseScenario
    {
        public static CreateSaleRequest CreateRequest()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"RETAIL-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "João Silva",
                BranchId = Guid.NewGuid(),
                BranchName = "Loja Centro SP",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Cerveja Brahma 350ml",
                        Quantity = 2,
                        UnitPrice = 3.50m
                    }
                }
            };
        }

        public static decimal ExpectedTotal => 7.00m; // 2 * 3.50, no discount
        public static decimal ExpectedDiscount => 0m;
    }

    /// <summary>
    /// Creates a bulk purchase scenario that should receive discount
    /// </summary>
    public static class BulkPurchaseWithDiscountScenario
    {
        public static CreateSaleRequest CreateRequest()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"BULK-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Maria Santos",
                BranchId = Guid.NewGuid(),
                BranchName = "Loja Norte RJ",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Cerveja Skol 600ml",
                        Quantity = 6, // Should get 10% discount
                        UnitPrice = 5.00m
                    }
                }
            };
        }

        public static decimal ExpectedTotal => 27.00m; // 6 * 5.00 * 0.9 (10% discount)
        public static decimal ExpectedDiscount => 10m;
    }

    /// <summary>
    /// Creates a high-volume purchase scenario with maximum discount
    /// </summary>
    public static class HighVolumePurchaseScenario
    {
        public static CreateSaleRequest CreateRequest()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"VOLUME-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Carlos Empresa LTDA",
                BranchId = Guid.NewGuid(),
                BranchName = "Centro de Distribuição SP",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Cerveja Heineken 330ml",
                        Quantity = 15, // Should get 20% discount
                        UnitPrice = 4.50m
                    }
                }
            };
        }

        public static decimal ExpectedTotal => 54.00m; // 15 * 4.50 * 0.8 (20% discount)
        public static decimal ExpectedDiscount => 20m;
    }

    /// <summary>
    /// Creates a mixed purchase scenario with multiple items
    /// </summary>
    public static class MixedPurchaseScenario
    {
        public static CreateSaleRequest CreateRequest()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"MIXED-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Ana Distribuidora",
                BranchId = Guid.NewGuid(),
                BranchName = "Filial Oeste SP",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Cerveja Brahma 350ml",
                        Quantity = 2, // No discount
                        UnitPrice = 3.50m
                    },
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Cerveja Skol 600ml",
                        Quantity = 8, // 10% discount
                        UnitPrice = 5.00m
                    }
                }
            };
        }

        public static decimal ExpectedTotal => 43.00m; // 7.00 + 36.00 (8 * 5.00 * 0.9)
    }

    /// <summary>
    /// Creates an invalid purchase scenario for testing validation
    /// </summary>
    public static class InvalidPurchaseScenario
    {
        public static CreateSaleRequest CreateRequestWithExcessiveQuantity()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"INVALID-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Cliente Teste",
                BranchId = Guid.NewGuid(),
                BranchName = "Loja Teste",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto Teste",
                        Quantity = 25, // Exceeds maximum of 20
                        UnitPrice = 10.00m
                    }
                }
            };
        }

        public static CreateSaleRequest CreateRequestWithFutureDate()
        {
            return new CreateSaleRequest
            {
                SaleNumber = $"FUTURE-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SaleDate = DateTime.UtcNow.AddDays(1), // Future date
                CustomerId = Guid.NewGuid(),
                CustomerName = "Cliente Futuro",
                BranchId = Guid.NewGuid(),
                BranchName = "Loja Futuro",
                Items = new List<CreateSaleItemRequest>
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto Futuro",
                        Quantity = 5,
                        UnitPrice = 10.00m
                    }
                }
            };
        }
    }

    /// <summary>
    /// Creates update scenarios for existing sales
    /// </summary>
    public static class SaleUpdateScenario
    {
        public static UpdateSaleRequest CreateUpdateRequest()
        {
            return new UpdateSaleRequest
            {
                SaleDate = DateTime.UtcNow.AddDays(-1),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Cliente Atualizado",
                BranchId = Guid.NewGuid(),
                BranchName = "Loja Atualizada",
                Items = new List<UpdateSaleItemRequest>
                {
                    new UpdateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto Atualizado",
                        Quantity = 4, // Should get 10% discount
                        UnitPrice = 8.00m
                    }
                }
            };
        }

        public static decimal ExpectedTotal => 28.80m; // 4 * 8.00 * 0.9
    }
}