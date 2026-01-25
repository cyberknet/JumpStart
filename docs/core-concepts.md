# Core Concepts

Understanding the fundamental concepts behind JumpStart will help you build applications more effectively and make the most of the framework's capabilities.

## Overview

JumpStart is built around several core concepts that work together to provide a productive development experience:

1. **Entity System** - Base classes for domain entities
2. **Repository Pattern** - Abstraction for data access
3. **User Context** - Tracking who performs operations
4. **Dependency Injection** - First-class DI support
5. **DTOs and Mapping** - Separation of concerns for APIs
6. **API Controllers** - RESTful endpoint generation

## Entity System


JumpStart provides a flexible entity system based on a single set of base classes with Guid keys by default, but extensible for custom key types.

### Entity (Guid-based)

**Purpose:** Quick and easy development with sensible defaults for most applications.

```csharp
public class Product : Entity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Provides:**
- `Id` property of type `Guid`
- Implements `IEntity`

### AuditableEntity

Adds automatic audit tracking:

```csharp
public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Provides:**
- Everything from `Entity`
- `CreatedById` - Who created this entity
- `CreatedOn` - When it was created
- `ModifiedById` - Who last modified it
- `ModifiedOn` - When it was last modified
- Implements `IAuditable`

### NamedEntity

For entities that have a name:

```csharp
public class Category : NamedEntity
{
    public string Description { get; set; } = string.Empty;
}
```

**Provides:**
- Everything from `Entity`
- `Name` property (required, max 100 chars)
- Implements `INamed`

### AuditableNamedEntity

Combines naming and auditing:

```csharp
public class Category : AuditableNamedEntity
{
    public string Description { get; set; } = string.Empty;
}
```

**Provides:**
- Everything from `AuditableEntity` and `NamedEntity`

## Repository Pattern

Repositories provide an abstraction layer between your domain/business logic and data access.

### Why Repositories?

1. **Separation of Concerns** - Business logic doesn't depend on EF Core
2. **Testability** - Easy to mock for unit tests
3. **Consistency** - Standard CRUD operations across entities
4. **Reusability** - Common queries in one place

### Simple Repositories

For Guid-based entities:

```csharp
public interface IProductRepository : IRepository<Product, Guid>
{
    // Add custom methods here
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(
        DbContext context,
        IUserContext? userContext = null)
        : base(context, userContext)
    {
    }
}
```

**Built-in Methods:**

```csharp
// Retrieval
Task<Product?> GetByIdAsync(Guid id);
Task<IList<Product>> GetAllAsync();
Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize);
Task<IList<Product>> QueryAsync(QueryOptions options);

// Creation
Task<Product> AddAsync(Product entity);

// Update
Task<Product> UpdateAsync(Product entity);

// Deletion
Task DeleteAsync(Guid id);
Task DeleteAsync(Product entity);

// Existence Check
Task<bool> ExistsAsync(Guid id);

// Count
Task<int> CountAsync();
```



### Adding Custom Methods

Extend repositories with domain-specific queries:

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IList<Product>> GetLowStockProductsAsync(int threshold);
    Task<IList<Product>> GetProductsByCategoryAsync(Guid categoryId);
    Task<Product?> GetBySkuAsync(string sku);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(
        DbContext context,
        IUserContext? userContext)
        : base(context, userContext)
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
}
```

### Pagination

Built-in pagination support prevents loading large datasets:

```csharp
var page1 = await repository.GetPagedAsync(page: 1, pageSize: 20);

Console.WriteLine($"Items: {page1.Items.Count}");
Console.WriteLine($"Total Items: {page1.TotalItems}");
Console.WriteLine($"Total Pages: {page1.TotalPages}");
Console.WriteLine($"Has Next: {page1.HasNextPage}");
Console.WriteLine($"Has Previous: {page1.HasPreviousPage}");

// Navigate pages
var page2 = await repository.GetPagedAsync(page: 2, pageSize: 20);
```

## User Context

User Context provides information about the current user, enabling automatic audit tracking.

### IUserContext (Guid Users)

For Simple entities:

```csharp
public interface IUserContext
{
    Task<Guid?> GetCurrentUserIdAsync();
}
```

### Implementations

#### Blazor Server (Cookie Authentication)

```csharp
public class BlazorUserContext : IUserContext
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public BlazorUserContext(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
            return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
```

#### Web API (JWT Bearer Tokens)

```csharp
public class ApiUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid?> GetCurrentUserIdAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return Task.FromResult<Guid?>(null);

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
            return Task.FromResult<Guid?>(userId);

        return Task.FromResult<Guid?>(null);
    }
}
```

#### Testing (Mock Context)

```csharp
public class MockUserContext : IUserContext
{
    private readonly Guid? _userId;

    public MockUserContext(Guid? userId = null)
    {
        _userId = userId ?? Guid.NewGuid();
    }

    public Task<Guid?> GetCurrentUserIdAsync()
    {
        return Task.FromResult(_userId);
    }
}
```

### How Audit Tracking Works

When you save an auditable entity:

1. Repository calls `userContext.GetCurrentUserIdAsync()`
2. For new entities, sets `CreatedById` and `CreatedOn`
3. For updated entities, sets `ModifiedById` and `ModifiedOn`
4. Changes are saved to the database

**All automatic** - you don't need to set these properties manually!

## Dependency Injection

JumpStart is designed for ASP.NET Core's dependency injection container.

### Basic Registration

```csharp
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// User Context
builder.Services.AddScoped<IUserContext, BlazorUserContext>();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### JumpStart Helpers

Use convenience methods for common scenarios:

#### Register DbContext and Repositories

```csharp
using JumpStart;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        // Register user context
        jumpStart.RegisterUserContext<BlazorUserContext>();

        // Scan assembly for repositories
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });
```

This automatically:
- Registers the DbContext
- Registers your user context
- Discovers and registers all repositories in the assembly

#### Register AutoMapper

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddJumpStartAutoMapper(typeof(Program).Assembly);
```

Discovers and registers all AutoMapper profiles in your assembly.

## DTOs and Mapping

DTOs (Data Transfer Objects) separate your internal entities from external API contracts.

### Why DTOs?

1. **Security** - Don't expose internal entity structure
2. **Versioning** - API contracts can evolve independently
3. **Flexibility** - Different views of the same data
4. **Validation** - API-specific validation rules

### DTO Base Classes

JumpStart provides base DTOs that correspond to entity types:

```csharp
// Read DTO
public class ProductDto : EntityDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Create DTO
public class CreateProductDto : ICreateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Update DTO
public class UpdateProductDto : IUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; } // Nullable for partial updates
}
```

### AutoMapper Profiles

Map between entities and DTOs:

```csharp
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
```

JumpStart base profiles handle common scenarios automatically.

## API Controllers

Base controllers provide standard RESTful endpoints with minimal code.

### Simple API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ApiControllerBase<
    Product,           // Entity type
    ProductDto,        // Read DTO
    CreateProductDto,  // Create DTO
    UpdateProductDto>  // Update DTO
{
    public ProductsController(
        IRepository<Product> repository,
        IMapper mapper)
        : base(repository, mapper)
    {
    }
}
```

**Provides these endpoints automatically:**

- `GET /api/products` - Get all products (paginated)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Adding Custom Endpoints

Extend base controllers with custom actions:

```csharp
public class ProductsController : ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto>
{
    private readonly IProductRepository _productRepository;

    public ProductsController(
        IProductRepository repository,
        IMapper mapper)
        : base(repository, mapper)
    {
        _productRepository = repository;
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStock(
        [FromQuery] int threshold = 10)
    {
        var products = await _productRepository.GetLowStockProductsAsync(threshold);
        return Ok(Mapper.Map<IEnumerable<ProductDto>>(products));
    }

    [HttpGet("by-sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetBySku(string sku)
    {
        var product = await _productRepository.GetBySkuAsync(sku);
        
        if (product == null)
            return NotFound();

        return Ok(Mapper.Map<ProductDto>(product));
    }
}
```

## Putting It All Together

Here's how all the concepts work together in a typical application:

```
User Request
    ?
API Controller (ApiControllerBase)
    ?
AutoMapper (DTO ? Entity)
    ?
Repository (Repository)
    ?
User Context (Gets current user)
    ?
Entity (Audit fields populated)
    ?
DbContext (EF Core)
    ?
Database
```

### Example Flow: Creating a Product

1. **User sends request:** `POST /api/products` with `CreateProductDto`
2. **Controller receives:** Base controller validates and maps DTO to entity
3. **Repository called:** `repository.AddAsync(product)`
4. **User context queried:** Gets current user ID from authentication
5. **Audit fields set:** `CreatedById` and `CreatedOn` populated automatically
6. **Entity saved:** Changes persisted to database via EF Core
7. **Response mapped:** Entity mapped back to `ProductDto` and returned

**You only write:** The entity class, DTOs, and mapping profile. Everything else is handled by JumpStart!

## Best Practices

### Entity Design

? **Do:**
- Keep entities focused on domain logic
- Use appropriate base classes (Simple vs Advanced)
- Add navigation properties for relationships
- Override `ToString()` for debugging

? **Don't:**
- Add UI-specific properties
- Include API-specific validation
- Expose sensitive data in DTOs

### Repository Usage

? **Do:**
- Inject repositories, not DbContext
- Add custom methods for complex queries
- Use async methods
- Return interfaces from services

? **Don't:**
- Access DbContext.Set<T>() directly in controllers
- Mix repository and direct EF Core queries
- Forget to register repositories in DI

### User Context

? **Do:**
- Register appropriate implementation per project type
- Handle null user IDs gracefully
- Use scoped lifetime for web applications

? **Don't:**
- Hard-code user IDs
- Assume user is always authenticated
- Use singleton lifetime

## Next Steps

Now that you understand the core concepts:

- **[Audit Tracking Guide](audit-tracking.md)** - Deep dive into audit functionality
- **[API Development Guide](api-development.md)** - Build comprehensive APIs
- **[How-To Guides](how-to/index.md)** - Solve specific tasks
- **[Architecture](architecture/index.md)** - Understand design decisions

---

**Questions?** Check the [FAQ](faq.md) or [open a discussion](https://github.com/cyberknet/JumpStart/discussions).
