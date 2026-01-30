using AutoMapper;
using Correlate;
using JumpStart.Api.Controllers;
using JumpStart.DemoApp.Api.Repositories;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.DemoApp.Controllers;

/// <summary>
/// API controller for managing products using DTOs.
/// Inherits all CRUD operations from ApiControllerBase.
/// Uses AutoMapper for entity-DTO conversions.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController 
    : ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>
{
    public ProductsController(IProductRepository repository, IMapper mapper, ILogger<ProductsController> logger, ICorrelationContextAccessor correlationAccessor) 
        : base(repository, mapper, logger, correlationAccessor)
    {
    }

    // Inherits:
    // GET api/products - Get all with pagination (returns ProductDto)
    // GET api/products/{id} - Get by ID (returns ProductDto)
    // POST api/products - Create (accepts CreateProductDto, returns ProductDto)
    // PUT api/products/{id} - Update (accepts UpdateProductDto, returns ProductDto)
    // DELETE api/products/{id} - Delete (soft delete)

    /// <summary>
    /// Custom endpoint: Get products by price range.
    /// Returns DTOs instead of entities.
    /// </summary>
    [HttpGet("by-price-range")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByPriceRange(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice)
    {
        var products = await _repository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        var dtos = _mapper.Map<List<ProductDto>>(products);
        return Ok(dtos);
    }

    /// <summary>
    /// Custom endpoint: Get low stock products.
    /// Returns DTOs instead of entities.
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStock([FromQuery] int threshold = 10)
    {
        var products = await _repository.GetLowStockProductsAsync(threshold);
        var dtos = _mapper.Map<List<ProductDto>>(products);
        return Ok(dtos);
    }

    /// <summary>
    /// Custom endpoint: Get active products only.
    /// Returns DTOs instead of entities.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActive()
    {
        var products = await _repository.GetActiveProductsAsync();
        var dtos = _mapper.Map<List<ProductDto>>(products);
        return Ok(dtos);
    }
}
