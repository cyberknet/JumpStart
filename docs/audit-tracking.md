# Audit Tracking

Learn how JumpStart automatically tracks who created, modified, and deleted entities, providing a complete audit trail for your application.

## Overview

Audit tracking answers critical questions:
- **Who** created this record?
- **When** was it created?
- **Who** last modified it?
- **When** was it last modified?
- **Who** deleted it (soft delete)?
- **When** was it deleted?

JumpStart handles this **automatically** - you don't need to manually set these fields!

## Why Audit Tracking?

### Compliance
Many industries require audit trails for regulatory compliance (HIPAA, SOX, GDPR, etc.).

### Troubleshooting
Track down who made changes and when, helping diagnose issues.

### Security
Detect unauthorized changes or suspicious activity.

### User Experience
Show "Created by John Doe on Jan 1, 2024" in your UI.

### Data Quality
Understand data lifecycle and aging.

## Quick Start

### 1. Use Auditable Entities

Change your entity to inherit from an auditable base class:

```csharp
// Before: No audit tracking
public class Product : Entity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// After: Automatic audit tracking
public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 2. Implement User Context

Create a user context to tell JumpStart who the current user is:

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

### 3. Register Services

Register your user context and repositories:

```csharp
// Register user context
builder.Services.AddScoped<IUserContext, BlazorUserContext>();

// Register repository (it will use the user context)
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Or use JumpStart helpers
builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        jumpStart.RegisterUserContext<BlazorUserContext>();
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });
```

### 4. That's It!

Now when you save entities, audit fields are populated automatically:

```csharp
var product = new Product
{
    Name = "Widget",
    Price = 19.99m
};

await repository.AddAsync(product);

// product.CreatedById is now set to current user's ID
// product.CreatedOn is now set to current UTC time
```

## Auditable Entity Types

### AuditableEntity

Basic audit tracking with Guid user IDs:

```csharp
public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
```

**Audit Fields:**
- `Guid CreatedById` - Who created it
- `DateTimeOffset CreatedOn` - When created (UTC)
- `Guid? ModifiedById` - Who last modified (null if never modified)
- `DateTimeOffset? ModifiedOn` - When last modified (null if never modified)
- `Guid? DeletedById` - Who soft-deleted it (null if not deleted)
- `DateTimeOffset? DeletedOn` - When soft-deleted (null if not deleted)

`AuditableEntity` implements `IAuditable` (`ICreatable` + `IModifiable` + `IDeletable`), so soft
delete support is included automatically - see [Soft Delete](#soft-delete) below, which is less
"advanced" than it sounds.

### AuditableNamedEntity

Auditing + named entity:

```csharp
public class Category : AuditableNamedEntity
{
    public string Description { get; set; } = string.Empty;
}
```

**Includes:** Everything from `AuditableEntity` plus `Name` property.

## How It Works

### Behind the Scenes

When you call repository methods:

#### AddAsync (Create)

```csharp
await repository.AddAsync(product);
```

1. Repository checks if entity is auditable
2. Calls `userContext.GetCurrentUserIdAsync()`
3. Sets `CreatedById` to current user
4. Sets `CreatedOn` to `DateTimeOffset.UtcNow`
5. Saves to database

#### UpdateAsync (Modify)

```csharp
await repository.UpdateAsync(product);
```

1. Repository checks if entity is auditable
2. Calls `userContext.GetCurrentUserIdAsync()`
3. Sets `ModifiedById` to current user
4. Sets `ModifiedOn` to `DateTimeOffset.UtcNow`
5. Saves to database

**Note:** `CreatedById` and `CreatedOn` are **not** changed during updates.

## User Context Implementations

### Blazor Server (Cookie Auth)

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

**Registration:**
```csharp
builder.Services.AddScoped<IUserContext, BlazorUserContext>();
```

### Web API (JWT Bearer)

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

**Registration:**
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, ApiUserContext>();
```

### Background Jobs (System User)

For background jobs where there's no authenticated user:

```csharp
public class SystemUserContext : IUserContext
{
    private readonly Guid _systemUserId;

    public SystemUserContext(IConfiguration configuration)
    {
        // Get system user ID from config
        var systemUserIdString = configuration["SystemUserId"] 
            ?? throw new InvalidOperationException("SystemUserId not configured");
        
        _systemUserId = Guid.Parse(systemUserIdString);
    }

    public Task<Guid?> GetCurrentUserIdAsync()
    {
        return Task.FromResult<Guid?>(_systemUserId);
    }
}
```

**appsettings.json:**
```json
{
  "SystemUserId": "00000000-0000-0000-0000-000000000001"
}
```

### Testing (Mock Context)

For unit tests:

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

**In Tests:**
```csharp
var testUserId = Guid.NewGuid();
var userContext = new MockUserContext(testUserId);
var repository = new ProductRepository(dbContext, userContext);

var product = new Product { Name = "Test" };
await repository.AddAsync(product);

Assert.Equal(testUserId, product.CreatedById);
```

## Soft Delete

Any entity inheriting from `AuditableEntity` (or implementing `IDeletable` directly) already gets
soft delete for free - there's no extra interface to add and no repository code to write:

- `Repository<TEntity>.DeleteAsync(id)` automatically detects `IDeletable` and sets
  `DeletedOn`/`DeletedById` instead of physically removing the row.
- `JumpStartDbContext` applies a global EF Core query filter
  (`WHERE DeletedOn IS NULL`) to every `IDeletable` entity automatically, in `OnModelCreating` -
  you don't need to add your own `HasQueryFilter` call.

```csharp
public class Product : AuditableEntity // already includes DeletedById/DeletedOn via IDeletable
{
    public string Name { get; set; } = string.Empty;
}

// Repository.DeleteAsync(id) soft-deletes automatically - no override needed:
await repository.DeleteAsync(product.Id);
```

### Querying Including Deleted

This is the one gap: `IRepository<TEntity>` has no built-in way to include soft-deleted rows, so a
custom repository method is the way to reach them (bypassing the base class's automatic filter,
not the global EF Core query filter, which requires `IgnoreQueryFilters()`):

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IList<Product>> GetDeletedProductsAsync();
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, IUserContext? userContext = null)
        : base(context, userContext) { }

    public async Task<IList<Product>> GetDeletedProductsAsync()
    {
        return await _context.Set<Product>()
            .IgnoreQueryFilters() // bypass the global soft-delete filter
            .Where(p => p.DeletedOn != null)
            .ToListAsync();
    }
}
```

## Displaying Audit Information

### In Razor Pages

```html
<div class="audit-info">
    <p>
        Created by: @Model.Product.CreatedById 
        on @Model.Product.CreatedOn.ToString("g")
    </p>
    @if (Model.Product.ModifiedOn.HasValue)
    {
        <p>
            Last modified by: @Model.Product.ModifiedById 
            on @Model.Product.ModifiedOn.Value.ToString("g")
        </p>
    }
</div>
```

### In APIs (DTOs)

Include audit info in DTOs:

```csharp
public class ProductDto : AuditableEntityDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Inherited from AuditableEntityDto:
    // public Guid CreatedById { get; set; }
    // public DateTimeOffset CreatedOn { get; set; }
    // public Guid? ModifiedById { get; set; }
    // public DateTimeOffset? ModifiedOn { get; set; }
}
```

### With User Names

Join with user table for display names:

```csharp
public class ProductWithUserDto : AuditableEntityDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public string CreatedByName { get; set; } = string.Empty;
    public string? ModifiedByName { get; set; }
}
```

```csharp
public async Task<ProductWithUserDto?> GetProductWithUserInfoAsync(Guid id)
{
    return await _context.Set<Product>()
        .Where(p => p.Id == id)
        .Select(p => new ProductWithUserDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CreatedById = p.CreatedById,
            CreatedOn = p.CreatedOn,
            CreatedByName = p.CreatedBy!.UserName!, // Navigation property
            ModifiedById = p.ModifiedById,
            ModifiedOn = p.ModifiedOn,
            ModifiedByName = p.ModifiedBy != null ? p.ModifiedBy.UserName : null
        })
        .FirstOrDefaultAsync();
}
```

## Custom Audit Fields

Add custom audit fields by extending base classes:

```csharp
public class AuditableProductEntity : AuditableEntity
{
    // Custom audit fields
    public string? ChangeReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class Product : AuditableProductEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Set in Repository:**
```csharp
public override async Task<Product> UpdateAsync(Product entity)
{
    // Set custom audit fields
    entity.IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    entity.UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
    
    return await base.UpdateAsync(entity);
}
```

## Best Practices

### Do's ?

- **Use UTC times** for all audit timestamps (done automatically)
- **Store user IDs** not usernames (they can change)
- **Index audit fields** if you query by them frequently
- **Include audit info in logs** for correlation
- **Test audit tracking** in unit tests

### Don'ts ?

- **Don't manually set audit fields** - let JumpStart handle it
- **Don't use nullable CreatedBy** - entities should always have a creator
- **Don't expose sensitive audit info** in public APIs
- **Don't modify CreatedBy** after creation
- **Don't forget to register UserContext** in DI

## Troubleshooting

### Audit Fields Are Null

**Problem:** `CreatedById` is null after saving.

**Solutions:**
1. Verify user context is registered:
   ```csharp
   builder.Services.AddScoped<IUserContext, BlazorUserContext>();
   ```

2. Check user context returns valid ID:
   ```csharp
   var userId = await userContext.GetCurrentUserIdAsync();
   Console.WriteLine($"Current User: {userId}");
   ```

3. Ensure user is authenticated before saving

### Wrong User ID

**Problem:** Audit fields show wrong user.

**Solutions:**
1. Verify claim type matches:
   ```csharp
   ClaimTypes.NameIdentifier // Standard claim type
   ```

2. Check authentication is working:
   ```csharp
   var user = httpContext.User;
   Console.WriteLine($"Authenticated: {user.Identity?.IsAuthenticated}");
   ```

3. Debug user context implementation

### Audit Fields Not Updating

**Problem:** `ModifiedBy` not changing on updates.

**Solutions:**
1. Verify you're using repository's `UpdateAsync` method
2. Ensure user context is registered
3. Check entity inherits from auditable base class

## Next Steps

- **[Core Concepts](core-concepts.md)** - Understand entity and repository patterns
- **[How-To: Soft Delete](how-to/soft-delete.md)** - Implement soft delete
- **[How-To: Custom Audit Fields](how-to/custom-audit-fields.md)** - Extend audit tracking
- **[API Development](api-development.md)** - Expose audit info in APIs

---

**Questions?** See [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
