using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Constants;

/// <summary>
/// Constantes para regras de negócio de vendas
/// Elimina magic numbers e centraliza configurações
/// </summary>
public static class SaleBusinessRules
{
    /// <summary>
    /// Quantidade mínima para aplicar desconto
    /// </summary>
    public const int MinQuantityForDiscount = 4;

    /// <summary>
    /// Quantidade mínima para desconto maior
    /// </summary>
    public const int MinQuantityForHigherDiscount = 10;

    /// <summary>
    /// Quantidade máxima permitida por item
    /// </summary>
    public const int MaxQuantityPerItem = 20;

    /// <summary>
    /// Percentual de desconto padrão
    /// </summary>
    public const decimal StandardDiscountPercentage = 10m;

    /// <summary>
    /// Percentual de desconto alto
    /// </summary>
    public const decimal HighDiscountPercentage = 20m;

    /// <summary>
    /// Tamanho máximo do nome do cliente
    /// </summary>
    public const int MaxCustomerNameLength = 100;

    /// <summary>
    /// Tamanho máximo do nome do produto
    /// </summary>
    public const int MaxProductNameLength = 100;

    /// <summary>
    /// Tamanho máximo do nome da filial
    /// </summary>
    public const int MaxBranchNameLength = 100;
}
