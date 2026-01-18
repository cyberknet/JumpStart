using JumpStart.DemoApp.Api.Data;
using JumpStart.DemoApp.Data;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.DemoApp.Api.Repositories;

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