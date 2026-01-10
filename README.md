# JumpStart

A comprehensive Blazor framework for rapid application development with built-in entity management, repository patterns, audit tracking, and API integration.

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-GPL--3.0-green)](LICENSE.txt)
[![GitHub](https://img.shields.io/github/stars/cyberknet/JumpStart?style=social)](https://github.com/cyberknet/JumpStart)

## ? Features

- **?? Entity Base Classes** - Simple and advanced entity types with built-in ID management
- **?? Audit Tracking** - Automatic tracking of created/modified/deleted information
- **??? Repository Pattern** - Generic repository with async support and pagination
- **?? API Controllers** - Base controllers for rapid RESTful API development
- **?? API Clients** - Type-safe Refit-based API clients with automatic configuration
- **?? AutoMapper Integration** - Simplified DTO mapping with built-in profiles
- **?? JWT Authentication** - Built-in JWT token generation and validation
- **?? Dependency Injection** - First-class DI support throughout the framework

## ?? Quick Start

### Installation

```bash
dotnet add package JumpStart
```

### Your First Entity

```csharp
public class Product : SimpleAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

### Your First Repository

```csharp
public interface IProductRepository : ISimpleRepository<Product, Guid> { }

public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, ISimpleUserContext? userContext)
        : base(context, userContext) { }
}
```

### Your First API

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<
    Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper) { }
}
```

That's it! You now have full CRUD endpoints with audit tracking. ??

## ?? Documentation

### Getting Started
- **[Quick Start Guide](docs/getting-started.md)** - Build your first application in minutes
- **[Core Concepts](docs/core-concepts.md)** - Understand the framework fundamentals
- **[Sample Applications](docs/samples.md)** - Learn from complete working examples

### Guides
- **[Audit Tracking](docs/audit-tracking.md)** - Automatic change tracking
- **[API Development](docs/api-development.md)** - Build RESTful APIs
- **[Authentication & Security](docs/authentication.md)** - JWT and cookie authentication

### How-To Guides
- **[Create Custom Repositories](docs/how-to/custom-repository.md)** - Add custom query methods
- **[Implement Pagination](docs/how-to/pagination.md)** - Handle large datasets efficiently
- **[Secure API Endpoints](docs/how-to/secure-endpoints.md)** - Add authentication and authorization
- **[More How-To Guides ?](docs/how-to/index.md)**

### Architecture
- **[Design Philosophy](docs/architecture/index.md)** - Understand the framework's design
- **[Architecture Decision Records](docs/architecture/adr/index.md)** - Historical design decisions
- **[Extension Points](docs/architecture/index.md#extension-points)** - Customize the framework

### Reference
- **[API Reference](docs/api/index.html)** - Complete API documentation
- **[FAQ](docs/faq.md)** - Frequently asked questions
- **[Troubleshooting](docs/troubleshooting.md)** - Common issues and solutions

## ?? Key Concepts

### Entity System

Choose between **Simple** (Guid-based) or **Advanced** (custom key types) entities:

```csharp
// Simple - Quick and easy with Guids
public class Category : SimpleAuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

// Advanced - Custom key types with maximum flexibility
public class Order : AuditableEntity<int, Guid>
{
    public decimal Total { get; set; }
}
```

### Automatic Audit Tracking

Inheriting from auditable entities automatically tracks:
- **CreatedById** - Who created the record
- **CreatedOn** - When it was created
- **ModifiedById** - Who last modified it
- **ModifiedOn** - When it was last modified

No manual tracking needed! Just use the repository methods.

### Repository Pattern

Built-in methods include:
- `GetByIdAsync(id)` - Get single entity
- `GetAllAsync()` - Get all entities
- `GetPagedAsync(page, pageSize)` - Paginated results
- `AddAsync(entity)` - Create new
- `UpdateAsync(entity)` - Update existing
- `DeleteAsync(id)` - Remove entity
- `CountAsync()` - Count entities
- `ExistsAsync(id)` - Check existence

Extend with custom methods when needed.

### API Development

Base controllers provide complete REST APIs automatically:

- `GET /api/products` - List all (with pagination)
- `GET /api/products/{id}` - Get single
- `POST /api/products` - Create
- `PUT /api/products/{id}` - Update
- `DELETE /api/products/{id}` - Delete

Add custom endpoints as needed.

## ??? Sample Projects

The repository includes two complete reference implementations:

### JumpStart.DemoApp
Blazor Server application demonstrating:
- ASP.NET Core Identity integration
- Audit tracking with user context
- Repository pattern usage
- AutoMapper configuration

### JumpStart.DemoApp.Api
Standalone RESTful API demonstrating:
- JWT Bearer authentication
- Swagger/OpenAPI documentation
- CORS configuration
- Secured endpoints

See the **[Samples Documentation](docs/samples.md)** for detailed walkthroughs.

## ??? Technology Stack

- **.NET 10** - Latest platform features
- **Entity Framework Core 10** - ORM and data access
- **ASP.NET Core 10** - Web framework
- **Refit 8** - Type-safe REST API client
- **AutoMapper 16** - Object-to-object mapping
- **xUnit** - Testing framework

## ?? Package Information

```xml
<PackageReference Include="JumpStart" Version="1.0.0" />
```

**Requirements:**
- .NET 10 or later
- Entity Framework Core 10 (for database access)
- AutoMapper (included as dependency)

## ?? Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for:
- Reporting bugs
- Suggesting features
- Submitting pull requests
- Writing documentation

## ?? Building Documentation

JumpStart uses [DocFX](https://dotnet.github.io/docfx/) to generate API reference documentation from XML comments.

### Prerequisites

Install DocFX globally:
```bash
dotnet tool install -g docfx
# Or update if already installed
dotnet tool update -g docfx
```

### Build Documentation

**Windows:**
```cmd
build-docs.cmd
```

**Linux/macOS:**
```bash
chmod +x build-docs.sh
./build-docs.sh
```

**Manual build:**
```bash
# Generate metadata and build site
docfx docfx.json

# Serve locally
docfx serve _site
```

The generated documentation will be in the `_site` folder. Open `_site/index.html` in a browser or serve it locally at `http://localhost:8080`.

### Documentation Structure

- `docs/` - Conceptual documentation (Markdown)
- `api/` - Generated API reference (auto-generated from XML comments)
- `docfx.json` - DocFX configuration
- `build-docs.cmd` / `build-docs.sh` - Build scripts

### Writing XML Comments

The framework uses comprehensive XML documentation. When contributing code, please include:

```csharp
/// <summary>
/// Brief description of the class/method.
/// </summary>
/// <remarks>
/// Detailed explanation with examples if needed.
/// </remarks>
/// <example>
/// <code>
/// // Usage example
/// var entity = new MyEntity();
/// </code>
/// </example>
public class MyEntity { }
```

## ?? License

JumpStart is licensed under the **GNU General Public License v3.0**. See [LICENSE.txt](LICENSE.txt) for details.

This means you can:
- ? Use commercially
- ? Modify the code
- ? Distribute
- ? Use privately

With the requirement that:
- ?? Disclose source
- ?? License and copyright notice
- ?? Same license
- ?? State changes

## ?? Links

- **Documentation:** [docs/index.md](docs/index.md)
- **GitHub:** https://github.com/cyberknet/JumpStart
- **Issues:** https://github.com/cyberknet/JumpStart/issues
- **Discussions:** https://github.com/cyberknet/JumpStart/discussions

## ?? Examples

### Complete CRUD with Audit Tracking

```csharp
// 1. Define entity
public class Product : SimpleAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// 2. Create repository
public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, ISimpleUserContext userContext)
        : base(context, userContext) { }
}

// 3. Register services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISimpleUserContext, BlazorUserContext>();

// 4. Use in your code
public class ProductService
{
    private readonly IProductRepository _repository;

    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        return await _repository.AddAsync(product);
        // CreatedById and CreatedOn automatically set!
    }
}
```

### Build a Complete API

```csharp
// 1. Define DTOs
public class ProductDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreateProductDto : ICreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
}

// 2. Create controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<
    Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper) { }

    // All CRUD endpoints automatically available!
}
```

## ?? Learning Resources

**New to JumpStart?** Follow this learning path:

1. **[Getting Started](docs/getting-started.md)** - Build your first app (15 minutes)
2. **[Core Concepts](docs/core-concepts.md)** - Understand the fundamentals (30 minutes)
3. **[Sample Applications](docs/samples.md)** - Study working examples (1 hour)
4. **[How-To Guides](docs/how-to/index.md)** - Solve specific tasks (as needed)

**Building APIs?**
- [API Development Guide](docs/api-development.md)
- [Authentication & Security](docs/authentication.md)
- [How-To: Secure Endpoints](docs/how-to/secure-endpoints.md)

**Need Help?**
- [FAQ](docs/faq.md) - Common questions
- [Troubleshooting](docs/troubleshooting.md) - Common issues
- [GitHub Discussions](https://github.com/cyberknet/JumpStart/discussions) - Ask the community

## ? Star History

If you find JumpStart useful, please consider giving it a star! ?

## ?? Acknowledgments

Built with ?? by [Scott Blomfield](https://github.com/cyberknet) and contributors.

Special thanks to:
- The ASP.NET Core team for an amazing framework
- The Entity Framework Core team for excellent ORM capabilities
- The open-source community for inspiration and feedback

---

**Ready to jump start your development?** [Get Started ?](docs/getting-started.md)
