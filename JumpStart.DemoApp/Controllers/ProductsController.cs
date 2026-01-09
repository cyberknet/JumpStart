using JumpStart.Api.Controllers;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.DemoApp.Controllers;

/// <summary>
/// API controller for managing products.
/// Inherits all CRUD operations from SimpleApiControllerBase.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : SimpleApiControllerBase<Product, IProductRepository>
{
    public ProductsController(IProductRepository repository) : base(repository)
    {
    }

    // Inherits:
    // GET api/products - Get all with pagination
    // GET api/products/{id} - Get by ID
    // POST api/products - Create
    // PUT api/products/{id} - Update
    // DELETE api/products/{id} - Delete (soft delete)

    /// <summary>
    /// Custom endpoint: Get products by price range.
    /// </summary>
    [HttpGet("by-price-range")]
    public async Task<ActionResult<IEnumerable<Product>>> GetByPriceRange(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice)
    {
        var products = await Repository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        return Ok(products);
    }

    /// <summary>
    /// Custom endpoint: Get low stock products.
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Product>>> GetLowStock([FromQuery] int threshold = 10)
    {
        var products = await Repository.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }

    /// <summary>
    /// Custom endpoint: Get active products only.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Product>>> GetActive()
    {
        var products = await Repository.GetActiveProductsAsync();
        return Ok(products);
    }
}
