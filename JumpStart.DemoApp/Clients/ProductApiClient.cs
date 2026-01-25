using JumpStart.Api.Clients;
using JumpStart.DemoApp.Shared.DTOs;
using Refit;

namespace JumpStart.DemoApp.Clients;

/// <summary>
/// API client interface for product operations using DTOs with Refit.
/// </summary>
/// <remarks>
/// This interface inherits standard CRUD operations from IApiClient and adds
/// custom product-specific operations. Refit automatically generates the implementation.
/// </remarks>
/// <example>
/// <code>
/// // Register in Program.cs
/// builder.Services.AddSimpleApiClient&lt;IProductApiClient&gt;("https://api.example.com/api/products");
/// 
/// // Inject and use
/// @inject IProductApiClient ProductClient
/// var products = await ProductClient.GetByPriceRangeAsync(10.00m, 50.00m);
/// </code>
/// </example>
public interface IProductApiClient : IApiClient<ProductDto, CreateProductDto, UpdateProductDto>
{
    /// <summary>
    /// Gets products within a specified price range.
    /// </summary>
    /// <param name="minPrice">Minimum price filter.</param>
    /// <param name="maxPrice">Maximum price filter.</param>
    /// <returns>Collection of products within the price range.</returns>
    [Get("/by-price-range")]
    Task<IEnumerable<ProductDto>> GetByPriceRangeAsync([Query] decimal minPrice, [Query] decimal maxPrice);

    /// <summary>
    /// Gets products with low stock levels.
    /// </summary>
    /// <param name="threshold">Stock quantity threshold (default: 10).</param>
    /// <returns>Collection of products with stock below the threshold.</returns>
        [Get("/low-stock")]
        Task<IEnumerable<ProductDto>> GetLowStockAsync([Query] int threshold = 10);

        /// <summary>
        /// Gets all active products.
        /// </summary>
        /// <returns>Collection of active products.</returns>
        [Get("/active")]
        Task<IEnumerable<ProductDto>> GetActiveAsync();
    }
