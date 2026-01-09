using JumpStart.DemoApp.Data;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.DemoApp.Services;

/// <summary>
/// Repository interface for Product entities.
/// Extends ISimpleRepository with custom product-specific queries.
/// </summary>
public interface IProductRepository : ISimpleRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<Product?> GetProductBySkuAsync(string sku);
}

/// <summary>
/// Repository implementation for Product entities using JumpStart framework.
/// Provides CRUD operations plus custom queries for products.
/// </summary>
public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context, ISimpleUserContext? userContext = null)
        : base(context, userContext)
    {
    }

    /// <summary>
    /// Gets all products within a specified price range.
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets products with stock quantity at or below the specified threshold.
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active (non-deleted, IsActive=true) products.
    /// </summary>
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a product by its SKU (Stock Keeping Unit).
    /// </summary>
    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }
}