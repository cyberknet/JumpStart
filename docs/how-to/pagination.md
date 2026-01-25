# How-To: Implement Pagination

Learn how to efficiently handle large datasets using JumpStart's built-in pagination support.

## Why Pagination?

Pagination prevents:
- **Memory issues** from loading thousands of records
- **Slow queries** that fetch entire tables
- **Poor user experience** with overwhelming data
- **Network bandwidth waste** sending unnecessary data

## Built-In Pagination

JumpStart repositories include pagination out of the box.

### Basic Usage

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<Product>> GetProductsAsync(int page = 1, int pageSize = 20)
    {
        return await _repository.GetPagedAsync(page, pageSize);
    }
}
```

### PagedResult Structure

```csharp
public class PagedResult<T>
{
    public IList<T> Items { get; set; }           // Current page items
    public int CurrentPage { get; set; }          // Current page number
    public int PageSize { get; set; }             // Items per page
    public int TotalItems { get; set; }           // Total count across all pages
    public int TotalPages { get; set; }           // Total number of pages
    public bool HasNextPage { get; set; }         // Can navigate forward
    public bool HasPreviousPage { get; set; }     // Can navigate backward
}
```

## API Pagination

### Controller with Pagination

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductsController(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets paginated list of products.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate parameters
        if (page < 1)
            return BadRequest("Page must be 1 or greater");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        // Get paginated data
        var result = await _repository.GetPagedAsync(page, pageSize);

        // Map to DTOs
        var dtoResult = new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IList<ProductDto>>(result.Items),
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage
        };

        return Ok(dtoResult);
    }
}
```

### Sample Response

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Widget",
      "price": 19.99
    },
    {
      "id": "7d8b2c45-9ae1-4f3b-8d6e-1c2a3b4c5d6e",
      "name": "Gadget",
      "price": 29.99
    }
  ],
  "currentPage": 1,
  "pageSize": 20,
  "totalItems": 45,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### API Client Usage

```csharp
public class ProductApiClient
{
    private readonly HttpClient _httpClient;

    public ProductApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize)
    {
        var response = await _httpClient.GetAsync($"/api/products?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
    }
}
```

## Custom Pagination

### Pagination with Filtering

```csharp

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResult<Product>> GetPagedWithFilterAsync(
        ProductFilter filter,
        int page,
        int pageSize);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, IUserContext? userContext)
        : base(context, userContext)
    {
    }

    public async Task<PagedResult<Product>> GetPagedWithFilterAsync(
        ProductFilter filter,
        int page,
        int pageSize)
    {
        // Start with base query
        var query = Context.Set<Product>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => p.Name.Contains(filter.Name));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        // Apply sorting
        query = filter.SortBy switch
        {
            "name" => filter.Descending 
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "price" => filter.Descending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        // Get total count
        var totalItems = await query.CountAsync();

        // Get page of items
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Build result
        return new PagedResult<Product>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page < (int)Math.Ceiling(totalItems / (double)pageSize),
            HasPreviousPage = page > 1
        };
    }
}

public class ProductFilter
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public string SortBy { get; set; } = "name";
    public bool Descending { get; set; }
}
```

### Using Filtered Pagination in API

```csharp
[HttpGet("search")]
public async Task<ActionResult<PagedResult<ProductDto>>> Search(
    [FromQuery] string? name = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] Guid? categoryId = null,
    [FromQuery] string sortBy = "name",
    [FromQuery] bool descending = false,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var filter = new ProductFilter
    {
        Name = name,
        MinPrice = minPrice,
        MaxPrice = maxPrice,
        CategoryId = categoryId,
        SortBy = sortBy,
        Descending = descending
    };

    var result = await _repository.GetPagedWithFilterAsync(filter, page, pageSize);
    
    return Ok(_mapper.Map<PagedResult<ProductDto>>(result));
}
```

## Blazor Server Pagination

### Component with Pagination

```razor
@page "/products"
@inject IProductRepository Repository
@inject IMapper Mapper

<h3>Products</h3>

@if (products == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Price</th>
                <th>Stock</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var product in products.Items)
            {
                <tr>
                    <td>@product.Name</td>
                    <td>@product.Price.ToString("C")</td>
                    <td>@product.StockQuantity</td>
                </tr>
            }
        </tbody>
    </table>

    <nav>
        <ul class="pagination">
            <li class="page-item @(products.HasPreviousPage ? "" : "disabled")">
                <button class="page-link" @onclick="() => LoadPageAsync(products.CurrentPage - 1)">
                    Previous
                </button>
            </li>

            @for (int i = 1; i <= products.TotalPages; i++)
            {
                var pageNumber = i; // Capture for closure
                <li class="page-item @(pageNumber == products.CurrentPage ? "active" : "")">
                    <button class="page-link" @onclick="() => LoadPageAsync(pageNumber)">
                        @pageNumber
                    </button>
                </li>
            }

            <li class="page-item @(products.HasNextPage ? "" : "disabled")">
                <button class="page-link" @onclick="() => LoadPageAsync(products.CurrentPage + 1)">
                    Next
                </button>
            </li>
        </ul>
    </nav>

    <p>
        Showing @((products.CurrentPage - 1) * products.PageSize + 1) 
        to @Math.Min(products.CurrentPage * products.PageSize, products.TotalItems)
        of @products.TotalItems items
    </p>
}

@code {
    private PagedResult<Product>? products;
    private int pageSize = 20;

    protected override async Task OnInitializedAsync()
    {
        await LoadPageAsync(1);
    }

    private async Task LoadPageAsync(int page)
    {
        products = await Repository.GetPagedAsync(page, pageSize);
    }
}
```

### Reusable Pagination Component

Create a reusable pagination component:

```razor
@typeparam TItem

<nav>
    <ul class="pagination">
        <li class="page-item @(PagedResult.HasPreviousPage ? "" : "disabled")">
            <button class="page-link" @onclick="OnPreviousPage" disabled="@(!PagedResult.HasPreviousPage)">
                Previous
            </button>
        </li>

        @if (ShowPageNumbers)
        {
            @for (int i = StartPage; i <= EndPage; i++)
            {
                var pageNumber = i;
                <li class="page-item @(pageNumber == PagedResult.CurrentPage ? "active" : "")">
                    <button class="page-link" @onclick="() => OnPageChanged(pageNumber)">
                        @pageNumber
                    </button>
                </li>
            }
        }

        <li class="page-item @(PagedResult.HasNextPage ? "" : "disabled")">
            <button class="page-link" @onclick="OnNextPage" disabled="@(!PagedResult.HasNextPage)">
                Next
            </button>
        </li>
    </ul>
</nav>

<p class="text-muted">
    Showing @StartItem to @EndItem of @PagedResult.TotalItems items
</p>

@code {
    [Parameter]
    public PagedResult<TItem> PagedResult { get; set; } = null!;

    [Parameter]
    public EventCallback<int> OnPageChange { get; set; }

    [Parameter]
    public bool ShowPageNumbers { get; set; } = true;

    [Parameter]
    public int MaxPageNumbers { get; set; } = 5;

    private int StartPage => Math.Max(1, PagedResult.CurrentPage - MaxPageNumbers / 2);
    private int EndPage => Math.Min(PagedResult.TotalPages, StartPage + MaxPageNumbers - 1);
    private int StartItem => (PagedResult.CurrentPage - 1) * PagedResult.PageSize + 1;
    private int EndItem => Math.Min(PagedResult.CurrentPage * PagedResult.PageSize, PagedResult.TotalItems);

    private async Task OnPageChanged(int page)
    {
        await OnPageChange.InvokeAsync(page);
    }

    private async Task OnPreviousPage()
    {
        if (PagedResult.HasPreviousPage)
            await OnPageChange.InvokeAsync(PagedResult.CurrentPage - 1);
    }

    private async Task OnNextPage()
    {
        if (PagedResult.HasNextPage)
            await OnPageChange.InvokeAsync(PagedResult.CurrentPage + 1);
    }
}
```

**Usage:**

```razor
<PaginationComponent TItem="Product" 
                     PagedResult="@products" 
                     OnPageChange="LoadPageAsync" />
```

## Performance Optimization

### Indexed Columns

Ensure columns used in ordering are indexed:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasIndex(e => e.Name);
        entity.HasIndex(e => e.Price);
        entity.HasIndex(e => e.CreatedOn);
    });
}
```

### Select Only Needed Columns

Project to DTOs in the query:

```csharp
public async Task<PagedResult<ProductSummaryDto>> GetPagedSummaryAsync(int page, int pageSize)
{
    var totalItems = await Context.Set<Product>().CountAsync();

    var items = await Context.Set<Product>()
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        })
        .ToListAsync();

    return new PagedResult<ProductSummaryDto>
    {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
    };
}
```

### Avoid Count Queries

For large datasets, counting can be expensive. Consider alternatives:

```csharp
// Option 1: Estimated count
public async Task<PagedResult<Product>> GetPagedWithEstimateAsync(int page, int pageSize)
{
    var items = await Context.Set<Product>()
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize + 1) // Take one extra
        .ToListAsync();

    var hasMore = items.Count > pageSize;
    if (hasMore)
        items = items.Take(pageSize).ToList();

    return new PagedResult<Product>
    {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        HasNextPage = hasMore,
        HasPreviousPage = page > 1,
        // Don't include total count
    };
}

// Option 2: Cached count
private int? _cachedTotalCount;
private DateTime? _cacheTime;

public async Task<PagedResult<Product>> GetPagedWithCachedCountAsync(int page, int pageSize)
{
    // Refresh cache every 5 minutes
    if (_cachedTotalCount == null || 
        _cacheTime == null || 
        DateTime.UtcNow - _cacheTime > TimeSpan.FromMinutes(5))
    {
        _cachedTotalCount = await Context.Set<Product>().CountAsync();
        _cacheTime = DateTime.UtcNow;
    }

    var items = await Context.Set<Product>()
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<Product>
    {
        Items = items,
        CurrentPage = page,
        PageSize = pageSize,
        TotalItems = _cachedTotalCount.Value,
        TotalPages = (int)Math.Ceiling(_cachedTotalCount.Value / (double)pageSize)
    };
}
```

## Cursor-Based Pagination

For better performance with large datasets:

```csharp
public async Task<CursorPagedResult<Product>> GetPagedByCursorAsync(
    Guid? cursor,
    int pageSize)
{
    var query = Context.Set<Product>().AsQueryable();

    if (cursor.HasValue)
    {
        // Get items after the cursor
        query = query.Where(p => p.Id.CompareTo(cursor.Value) > 0);
    }

    var items = await query
        .OrderBy(p => p.Id)
        .Take(pageSize + 1)
        .ToListAsync();

    var hasMore = items.Count > pageSize;
    if (hasMore)
        items = items.Take(pageSize).ToList();

    var nextCursor = hasMore ? items.Last().Id : (Guid?)null;

    return new CursorPagedResult<Product>
    {
        Items = items,
        NextCursor = nextCursor,
        HasMore = hasMore
    };
}

public class CursorPagedResult<T>
{
    public IList<T> Items { get; set; } = new List<T>();
    public Guid? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
```

## Best Practices

### Do's ?

- **Limit maximum page size** (e.g., 100 items)
- **Use 1-based page numbers** (more user-friendly)
- **Index columns** used for ordering
- **Validate page parameters** in controllers
- **Return metadata** (total count, page info)
- **Consider cursor pagination** for very large datasets
- **Cache counts** for expensive queries

### Don'ts ?

- **Don't allow unlimited page sizes**
- **Don't use Skip/Take without ordering**
- **Don't count on every request** if not needed
- **Don't forget to validate** page and pageSize
- **Don't load all data** then paginate in memory
- **Don't use 0-based pages** in public APIs

## Next Steps

- **[How-To: Custom Repository](custom-repository.md)** - Add custom query methods
- **[How-To: Optimize Queries](optimize-queries.md)** - Improve query performance
- **[Core Concepts: Repository Pattern](../core-concepts.md#repository-pattern)** - Understand the basics
- **[API Development](../api-development.md)** - Build paginated APIs

---

**Questions?** See [FAQ](../faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
