# Frequently Asked Questions (FAQ)

Common questions about using JumpStart.

## General Questions

### What is JumpStart?

JumpStart is a Blazor framework that provides entity base classes, repository pattern implementation, audit tracking, and API client integration for rapid application development. It reduces boilerplate code and enforces best practices.

### What version of .NET does JumpStart support?

JumpStart currently targets **.NET 10**. It uses the latest C# language features and ASP.NET Core capabilities.

### Is JumpStart open source?

Yes! JumpStart is open source and licensed under the **GNU General Public License v3.0**. See [LICENSE](../LICENSE.txt) for details.

### Can I use JumpStart in commercial projects?

Yes, JumpStart can be used in commercial projects under the GPL v3.0 license. Please review the license terms to ensure compliance.

## Getting Started

### How do I install JumpStart?

```bash
dotnet add package JumpStart
```

See the [Getting Started Guide](getting-started.md) for a complete tutorial.

### Do I need to use both Simple and Advanced entities?

No! Choose one approach based on your needs:

- **Simple Entities** - Use Guid for IDs, quick and easy
- **Advanced Entities** - Custom key types, maximum flexibility

Most applications can use Simple entities exclusively.

### Can I mix Simple and Advanced entities in the same project?

Yes, but it's not recommended. Stick with one approach for consistency unless you have a specific need.

## Entities and Repositories

### When should I use SimpleEntity vs SimpleAuditableEntity?

Use `SimpleAuditableEntity` when you need to track:
- Who created the record
- When it was created
- Who last modified it
- When it was last modified

Use `SimpleEntity` for simple lookup tables or when audit tracking isn't needed.

### How do I add custom properties to entities?

Just add them like any C# class:

```csharp
public class Product : SimpleAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    
    // Add custom properties
    public string Sku { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? DiscontinuedDate { get; set; }
}
```

### Do I need to create a repository for every entity?

No! Use the base repository directly if you don't need custom methods:

```csharp
// No custom repository needed
builder.Services.AddScoped<ISimpleRepository<Category, Guid>, SimpleRepository<Category>>();
```

Create custom repositories only when you need specialized query methods.

### Can repositories access multiple entity types?

Repositories should generally focus on one entity type. For operations involving multiple entities, create a service layer:

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    
    public async Task CreateOrderAsync(CreateOrderDto dto)
    {
        // Orchestrate multiple repositories
    }
}
```

## Audit Tracking

### Why are my audit fields null?

Common causes:
1. **User Context not registered** - Make sure you register your ISimpleUserContext implementation
2. **User not authenticated** - The user context returns null for unauthenticated users
3. **Not using repository** - Direct DbContext operations bypass audit tracking

### Can I customize the audit fields?

Yes! Create your own base class:

```csharp
public abstract class CustomAuditableEntity : SimpleEntity, ISimpleAuditable
{
    public Guid CreatedById { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    
    // Custom fields
    public string? CreatedByName { get; set; }
    public string? ModifiedByName { get; set; }
    public string? IpAddress { get; set; }
}
```

### Does audit tracking work in background jobs?

Yes! Create a SystemUserContext that returns a configured system user ID:

```csharp
public class SystemUserContext : ISimpleUserContext
{
    private readonly Guid _systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Task<Guid?> GetCurrentUserIdAsync()
    {
        return Task.FromResult<Guid?>(_systemUserId);
    }
}
```

## API Development

### Do I have to use DTOs?

It's strongly recommended! DTOs provide:
- Security (don't expose internal structure)
- Versioning (API can change independently)
- Validation (API-specific rules)
- Flexibility (different views of data)

### Can I use the same DTO for Create and Update?

You can, but separate DTOs are better:

```csharp
// Create - all fields required
public class CreateProductDto : ICreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
}

// Update - fields nullable for partial updates
public class UpdateProductDto : IUpdateDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
}
```

### How do I add custom endpoints to base controllers?

Just add new methods:

```csharp
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStock(
        [FromQuery] int threshold = 10)
    {
        // Custom implementation
    }
}
```

### Can I override base controller methods?

Yes! Mark the method as `override`:

```csharp
[HttpPost]
public override async Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductDto dto)
{
    // Custom validation
    if (dto.Price <= 0)
        return BadRequest("Price must be positive");

    // Call base implementation
    return await base.CreateAsync(dto);
}
```

## Authentication and Security

### What authentication methods does JumpStart support?

JumpStart provides infrastructure for:
- **JWT Bearer** tokens (for APIs)
- **Cookie** authentication (for Blazor Server)
- **API Keys** (service-to-service)

You need to configure the authentication in your application.

### How do I implement refresh tokens?

JumpStart doesn't provide refresh tokens out of the box. You'll need to implement them yourself:

```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
}
```

### Can I use different authentication for different endpoints?

Yes! Use multiple authentication schemes:

```csharp
[Authorize(AuthenticationSchemes = "Bearer")] // JWT only
public class ApiController : ControllerBase { }

[Authorize(AuthenticationSchemes = "Cookies")] // Cookies only
public class WebController : Controller { }
```

## Performance

### Does JumpStart impact performance?

JumpStart adds minimal overhead:
- Repository pattern: Negligible abstraction cost
- Audit tracking: One extra query per user context (usually cached)
- AutoMapper: Microseconds per mapping

The benefits far outweigh any minimal performance impact.

### How do I optimize queries?

1. **Use pagination** - Don't load entire tables
2. **Add indexes** - Index frequently queried columns
3. **Use Select** - Project to DTOs in queries
4. **Enable query splitting** - For multiple includes
5. **Avoid N+1** - Use Include for related entities

### Can I use compiled queries with JumpStart?

Yes! Compiled queries work with repositories:

```csharp
private static readonly Func<ApplicationDbContext, Guid, Task<Product?>> GetProductByIdCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, Guid id) =>
        context.Set<Product>().FirstOrDefault(p => p.Id == id));

public async Task<Product?> GetByIdAsync(Guid id)
{
    return await GetProductByIdCompiled(Context, id);
}
```

## Testing

### How do I test repositories?

Use an in-memory database:

```csharp
[Fact]
public async Task GetByIdAsync_ReturnsProduct()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    using var context = new ApplicationDbContext(options);
    var repository = new ProductRepository(context, null);

    var product = new Product { Name = "Test" };
    await repository.AddAsync(product);

    // Act
    var result = await repository.GetByIdAsync(product.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}
```

### How do I mock user context?

Create a mock implementation:

```csharp
public class MockUserContext : ISimpleUserContext
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

## Deployment

### Can I deploy JumpStart apps to Azure?

Yes! JumpStart apps are standard ASP.NET Core applications and can be deployed to:
- Azure App Service
- Azure Container Instances
- Azure Kubernetes Service
- Any hosting provider supporting .NET 10

### Do I need separate databases for Blazor app and API?

No, they can share the same database. The API and Blazor app are just different ways to access the same data.

### How do I handle connection strings in production?

Use Azure Key Vault or environment variables:

```csharp
// appsettings.json (local development)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;..."
  }
}

// Azure App Service (Application Settings)
ConnectionStrings__DefaultConnection = "Server=production;..."
```

## Troubleshooting

### Build errors about missing dependencies

Run:
```bash
dotnet restore
```

### EF Core migrations not working

Ensure you have EF Core tools installed:
```bash
dotnet tool install --global dotnet-ef
```

### Swagger not showing up

Check that you're in Development environment:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Contributing

### How can I contribute to JumpStart?

See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines on:
- Reporting bugs
- Suggesting features
- Submitting pull requests
- Writing documentation

### Where do I report bugs?

[Open an issue](https://github.com/cyberknet/JumpStart/issues) on GitHub with:
- Description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, etc.)

## More Questions?

- **[Troubleshooting Guide](troubleshooting.md)** - Common issues and solutions
- **[GitHub Discussions](https://github.com/cyberknet/JumpStart/discussions)** - Ask the community
- **[GitHub Issues](https://github.com/cyberknet/JumpStart/issues)** - Report bugs

---

**Can't find your answer?** [Open a discussion](https://github.com/cyberknet/JumpStart/discussions) or [create an issue](https://github.com/cyberknet/JumpStart/issues).
