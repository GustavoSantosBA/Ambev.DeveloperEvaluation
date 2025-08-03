using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of SaleRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public SaleRepository(DefaultContext context)
    {
        // FIX: Ensure the context is not null.
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Creates a new sale in the database
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale</returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // FIX: Check if the sale object is null before using it.
        if (sale == null)
        {
            throw new ArgumentNullException(nameof(sale));
        }

        // The error is likely happening on the next line if _context.Sales is null,
        // but this is very unlikely if the context is injected correctly.
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Retrieves a sale by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a sale by its sale number
    /// </summary>
    /// <param name="saleNumber">The sale number to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    /// <summary>
    /// Retrieves all sales for a specific customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of sales for the customer</returns>
    public async Task<IEnumerable<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all sales for a specific branch
    /// </summary>
    /// <param name="branchId">The branch identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of sales for the branch</returns>
    public async Task<IEnumerable<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves sales within a date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of sales within the date range</returns>
    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all sales with pagination support
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of sales for the specified page</returns>
    public async Task<IEnumerable<Sale>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves sales with advanced filtering and pagination
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="branchId">Optional branch ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of filtered sales for the specified page</returns>
    public async Task<IEnumerable<Sale>> GetFilteredAsync(
        int page = 1,
        int pageSize = 10,
        Guid? customerId = null,
        Guid? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        return await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of sales matching the specified filters
    /// </summary>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="branchId">Optional branch ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of filtered sales</returns>
    public async Task<int> GetFilteredCountAsync(
        Guid? customerId = null,
        Guid? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing sale in the database
    /// </summary>
    /// <param name="sale">The sale to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale</returns>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Deletes a sale from the database
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the sale was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets the total count of sales
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total number of sales</returns>
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sales.CountAsync(cancellationToken);
    }
}