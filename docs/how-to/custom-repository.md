# How-To: Create a Custom Repository

Learn how to extend JumpStart's base repositories with custom query methods for your specific domain needs.

## When to Use Custom Repositories

Add custom repository methods when you need to:

- Execute complex queries specific to your domain
- Encapsulate business logic related to data access
- Implement specialized filtering or sorting
- Create reusable query patterns
- Abstract database-specific operations

## Basic Custom Repository

### 1. Define the Interface

Extend the base repository interface with your custom methods:

```csharp
using JumpStart.Repositories;

namespace MyApp.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IList<Product>> GetLowStockProductsAsync(int threshold);
    Task<IList<Product>> GetProductsByCategoryAsync(Guid categoryId);
    Task<Product?> GetBySkuAsync(string sku);
    Task<decimal> GetAveragePriceAsync();
    Task<IList<Product>> SearchAsync(string searchTerm);
}
```

### 2. Implement the Repository

Extend the base repository class and implement your custom methods:

```csharp
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context) : base(context)
    {
    }

    public async Task<IList<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await Context.Set<Product>()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public async Task<IList<Product>> GetProductsByCategoryAsync(Guid categoryId)
    {
        return await Context.Set<Product>()
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await Context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<decimal> GetAveragePriceAsync()
    {
        return await Context.Set<Product>()
            .AverageAsync(p => p.Price);
    }

    public async Task<IList<Product>> SearchAsync(string searchTerm)
    {
        return await Context.Set<Product>()
            .Where(p => p.Name.Contains(searchTerm) || 
                       p.Description.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
```

### 3. Register the Repository

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### 4. Use the Repository

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<Product>> GetProductsNeedingRestockAsync()
    {
        return await _repository.GetLowStockProductsAsync(threshold: 10);
    }
}
```

## Advanced Patterns

### Complex Queries with Includes

Load related entities efficiently:

```csharp
public async Task<IList<Product>> GetProductsWithCategoryAsync()
{
    return await Context.Set<Product>()
        .Include(p => p.Category)
        .OrderBy(p => p.Name)
        .ToListAsync();
}

public async Task<Product?> GetProductWithDetailsAsync(Guid id)
{
    return await Context.Set<Product>()
        .Include(p => p.Category)
        .Include(p => p.Reviews)
        .Include(p => p.Supplier)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

### Specification Pattern

For complex, reusable query logic:

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
}

public class ProductsByPriceRangeSpec : ISpecification<Product>
{
    public ProductsByPriceRangeSpec(decimal minPrice, decimal maxPrice)
    {
        Criteria = p => p.Price >= minPrice && p.Price <= maxPrice;
    }

    public Expression<Func<Product, bool>> Criteria { get; }
    public List<Expression<Func<Product, object>>> Includes { get; } = new();
}

public async Task<IList<Product>> GetBySpecificationAsync(ISpecification<Product> spec)
{
    var query = Context.Set<Product>().Where(spec.Criteria);
    
    query = spec.Includes.Aggregate(query, (current, include) => 
        current.Include(include));
    
    return await query.ToListAsync();
}
```

### Dynamic Filtering

Build queries dynamically based on parameters:

```csharp
public async Task<IList<Product>> GetFilteredProductsAsync(ProductFilter filter)
{
    var query = Context.Set<Product>().AsQueryable();

    if (!string.IsNullOrWhiteSpace(filter.Name))
        query = query.Where(p => p.Name.Contains(filter.Name));

    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.Price >= filter.MinPrice.Value);

    if (filter.MaxPrice.HasValue)
        query = query.Where(p => p.Price <= filter.MaxPrice.Value);

    if (filter.CategoryId.HasValue)
        query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

    if (filter.InStock.HasValue)
        query = query.Where(p => p.StockQuantity > 0 == filter.InStock.Value);

    // Apply sorting
    query = filter.SortBy switch
    {
        "name" => query.OrderBy(p => p.Name),
        "price" => query.OrderBy(p => p.Price),
        "stock" => query.OrderBy(p => p.StockQuantity),
        _ => query.OrderBy(p => p.CreatedOn)
    };

    if (filter.Descending)
        query = query.Reverse();

    return await query.ToListAsync();
}

public class ProductFilter
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "name";
    public bool Descending { get; set; }
}
```

### Pagination with Filtering

Combine custom filtering with pagination:

```csharp
public async Task<PagedResult<Product>> GetFilteredPagedAsync(
    ProductFilter filter,
    int page,
    int pageSize)
{
    var query = Context.Set<Product>().AsQueryable();

    // Apply filters (same as above)
    if (!string.IsNullOrWhiteSpace(filter.Name))
        query = query.Where(p => p.Name.Contains(filter.Name));

    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.Price >= filter.MinPrice.Value);

    // ... other filters

    // Get total count before pagination
    var totalItems = await query.CountAsync();

    // Apply pagination
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<Product>
    {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
    };
}
```

### Aggregate Operations

Custom methods for statistics and calculations:

```csharp
public async Task<ProductStatistics> GetStatisticsAsync()
{
    var products = await Context.Set<Product>().ToListAsync();

    return new ProductStatistics
    {
        TotalProducts = products.Count,
        AveragePrice = products.Average(p => p.Price),
        MinPrice = products.Min(p => p.Price),
        MaxPrice = products.Max(p => p.Price),
        TotalValue = products.Sum(p => p.Price * p.StockQuantity),
        LowStockCount = products.Count(p => p.StockQuantity <= 10)
    };
}

public class ProductStatistics
{
    public int TotalProducts { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
}
```

### Bulk Operations

Efficient batch operations:

```csharp
public async Task<int> BulkUpdatePricesAsync(decimal percentageIncrease)
{
    var products = await Context.Set<Product>().ToListAsync();
    
    foreach (var product in products)
    {
        product.Price *= (1 + percentageIncrease / 100);
    }
    
    return await Context.SaveChangesAsync();
}

public async Task<int> BulkDeleteByCategoryAsync(Guid categoryId)
{
    return await Context.Set<Product>()
        .Where(p => p.CategoryId == categoryId)
        .ExecuteDeleteAsync(); // EF Core 7+
}

public async Task<int> BulkUpdateStockAsync(Dictionary<Guid, int> stockUpdates)
{
    var productIds = stockUpdates.Keys.ToList();
    var products = await Context.Set<Product>()
        .Where(p => productIds.Contains(p.Id))
        .ToListAsync();

    foreach (var product in products)
    {
        if (stockUpdates.TryGetValue(product.Id, out var newStock))
        {
            product.StockQuantity = newStock;
        }
    }

    return await Context.SaveChangesAsync();
}
```

## Testing Custom Repositories

### Unit Tests with In-Memory Database

```csharp
public class ProductRepositoryTests
{
    private DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ReturnsCorrectProducts()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        using var context = new ApplicationDbContext(options);
        var repository = new ProductRepository(context);

        var products = new[]
        {
            new Product { Name = "Product 1", StockQuantity = 5 },
            new Product { Name = "Product 2", StockQuantity = 15 },
            new Product { Name = "Product 3", StockQuantity = 8 }
        };

        foreach (var product in products)
        {
            await repository.AddAsync(product);
        }

        // Act
        var lowStockProducts = await repository.GetLowStockProductsAsync(threshold: 10);

        // Assert
        Assert.Equal(2, lowStockProducts.Count);
        Assert.Contains(lowStockProducts, p => p.Name == "Product 1");
        Assert.Contains(lowStockProducts, p => p.Name == "Product 3");
    }

    [Fact]
    public async Task GetBySkuAsync_WithExistingSku_ReturnsProduct()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        using var context = new ApplicationDbContext(options);
        var repository = new ProductRepository(context);

        var product = new Product { Name = "Test Product", Sku = "TEST-001" };
        await repository.AddAsync(product);

        // Act
        var result = await repository.GetBySkuAsync("TEST-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task SearchAsync_FindsMatchingProducts()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        using var context = new ApplicationDbContext(options);
        var repository = new ProductRepository(context);

        await repository.AddAsync(new Product { Name = "Widget", Description = "A test item" });
        await repository.AddAsync(new Product { Name = "Gadget", Description = "Another test" });
        await repository.AddAsync(new Product { Name = "Tool", Description = "Not matching" });

        // Act
        var results = await repository.SearchAsync("test");

        // Assert
        Assert.Equal(2, results.Count);
    }
}
```

## Best Practices

### Do's ?

- **Keep methods focused** - One method, one responsibility
- **Use async/await** consistently
- **Return interfaces** from repository methods
- **Add XML documentation** to custom methods
- **Use meaningful names** that describe the query
- **Include error handling** for edge cases
- **Test custom methods** thoroughly

### Don'ts ?

- **Don't return IQueryable** from repositories (breaks abstraction)
- **Don't include business logic** in repositories (only data access)
- **Don't forget to dispose** DbContext properly
- **Don't use `.Result` or `.Wait()`** on async methods
- **Don't expose DbContext** outside repositories
- **Don't create repositories** for every entity if base methods suffice

## Common Patterns

### Repository with Caching

```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly ProductRepository _innerRepository;
    private readonly IMemoryCache _cache;

    public CachedProductRepository(
        ProductRepository innerRepository,
        IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"Product_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Product? product))
            return product;

        product = await _innerRepository.GetByIdAsync(id);
        
        if (product != null)
        {
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(5));
        }

        return product;
    }
}
```

### Repository with Logging

```csharp
public class LoggingProductRepository : IProductRepository
{
    private readonly ProductRepository _innerRepository;
    private readonly ILogger<LoggingProductRepository> _logger;

    public LoggingProductRepository(
        ProductRepository innerRepository,
        ILogger<LoggingProductRepository> logger)
    {
        _innerRepository = innerRepository;
        _logger = logger;
    }

    public async Task<IList<Product>> GetLowStockProductsAsync(int threshold)
    {
        _logger.LogInformation(
            "Getting low stock products with threshold {Threshold}", 
            threshold);

        var stopwatch = Stopwatch.StartNew();
        
        var products = await _innerRepository.GetLowStockProductsAsync(threshold);
        
        stopwatch.Stop();
        
        _logger.LogInformation(
            "Retrieved {Count} low stock products in {Elapsed}ms",
            products.Count,
            stopwatch.ElapsedMilliseconds);

        return products;
    }
}
```

## Next Steps

- **[How-To: Pagination](pagination.md)** - Implement efficient pagination
- **[How-To: Use Advanced Entities](advanced-entities.md)** - Work with custom key types
- **[Core Concepts: Repository Pattern](../core-concepts.md#repository-pattern)** - Understand the basics
- **[API Development](../api-development.md)** - Use repositories in APIs

---

**Questions?** See [FAQ](../faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
