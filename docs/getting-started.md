# Getting Started with JumpStart

Welcome! This guide will help you get up and running with JumpStart in minutes. By the end, you'll have created your first entity, repository, and API endpoint.

## Prerequisites

Before you begin, ensure you have:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- An IDE (Visual Studio 2022 17.12+, Visual Studio Code, or Rider)
- Basic knowledge of C# and ASP.NET Core
- SQL Server (LocalDB, Express, or full version) for Entity Framework Core

## Installation

### Install via NuGet

```bash
dotnet add package JumpStart
```

Or using Package Manager Console in Visual Studio:

```powershell
Install-Package JumpStart
```

### Clone the Sample

Alternatively, clone the repository to see working examples:

```bash
git clone https://github.com/cyberknet/JumpStart.git
cd JumpStart
```

## Quick Start Tutorial

Let's build a simple product catalog application step-by-step.

### Step 1: Create a New Project

```bash
dotnet new webapp -n MyProductCatalog
cd MyProductCatalog
dotnet add package JumpStart
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Step 2: Create Your First Entity

Create a `Product` entity using JumpStart's base classes:

```csharp
using JumpStart.Data;

namespace MyProductCatalog.Data;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product : SimpleEntity
{
    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity in stock.
    /// </summary>
    public int StockQuantity { get; set; }
}
```

**What You Get:**
- `SimpleEntity` provides an `Id` property (Guid) automatically
- Inheriting from `SimpleAuditableEntity` would add audit tracking (CreatedBy, CreatedOn, etc.)

### Step 3: Create a DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using MyProductCatalog.Data;

namespace MyProductCatalog.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
        });
    }
}
```

### Step 4: Create a Repository

JumpStart repositories handle all CRUD operations for you:

```csharp
using JumpStart.Repositories;
using MyProductCatalog.Data;

namespace MyProductCatalog.Repositories;

/// <summary>
/// Repository for Product entities.
/// </summary>
public interface IProductRepository : ISimpleRepository<Product, Guid>
{
    // Add custom methods here if needed
}

public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context)
        : base(context, null) // null for no user context (no audit tracking)
    {
    }
}
```

**What You Get:**
- `GetByIdAsync(Guid id)`
- `GetAllAsync()`
- `GetPagedAsync(int page, int pageSize)`
- `AddAsync(Product entity)`
- `UpdateAsync(Product entity)`
- `DeleteAsync(Guid id)`
- And more!

### Step 5: Register Services

Configure JumpStart in your `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MyProductCatalog.Data;
using MyProductCatalog.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories using JumpStart
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
```

### Step 6: Add Connection String

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyProductCatalog;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 7: Create and Apply Migrations

```bash
# Install EF Core tools if you haven't already
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### Step 8: Use Your Repository

Create a Razor Page to display products:

**Pages/Products/Index.cshtml.cs:**

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyProductCatalog.Data;
using MyProductCatalog.Repositories;

namespace MyProductCatalog.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductRepository _repository;

    public IndexModel(IProductRepository repository)
    {
        _repository = repository;
    }

    public IList<Product> Products { get; set; } = new List<Product>();

    public async Task OnGetAsync()
    {
        Products = await _repository.GetAllAsync();
    }
}
```

**Pages/Products/Index.cshtml:**

```html
@page
@model MyProductCatalog.Pages.Products.IndexModel
@{
    ViewData["Title"] = "Products";
}

<h1>Products</h1>

<table class="table">
    <thead>
        <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Price</th>
            <th>Stock</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var product in Model.Products)
        {
            <tr>
                <td>@product.Name</td>
                <td>@product.Description</td>
                <td>@product.Price.ToString("C")</td>
                <td>@product.StockQuantity</td>
            </tr>
        }
    </tbody>
</table>
```

### Step 9: Run Your Application

```bash
dotnet run
```

Navigate to `https://localhost:5001/Products` (or your configured port) to see your product list!

## What's Next?

Congratulations! You've created your first JumpStart application. Here's what to explore next:

### Add Audit Tracking
Change your entity to inherit from `SimpleAuditableEntity` to automatically track who created and modified records:

```csharp
public class Product : SimpleAuditableEntity
{
    // ... properties
}
```

See [Audit Tracking Guide](audit-tracking.md) for details.

### Create an API
Add API controllers to expose your data as RESTful endpoints:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper)
    {
    }
}
```

See [API Development Guide](api-development.md) for details.

### Use Pagination
Handle large datasets efficiently:

```csharp
public async Task OnGetAsync(int page = 1)
{
    var result = await _repository.GetPagedAsync(page, pageSize: 20);
    Products = result.Items;
    TotalPages = result.TotalPages;
}
```

See [How-To: Pagination](how-to/pagination.md) for details.

### Add Custom Repository Methods
Extend repositories with custom queries:

```csharp
public interface IProductRepository : ISimpleRepository<Product, Guid>
{
    Task<IList<Product>> GetLowStockProductsAsync(int threshold);
}
```

See [How-To: Custom Repository](how-to/custom-repository.md) for details.

## Learning Resources

- **[Core Concepts](core-concepts.md)** - Deep dive into JumpStart's architecture
- **[API Development](api-development.md)** - Build RESTful APIs
- **[How-To Guides](how-to/index.md)** - Task-oriented guides
- **[Sample Applications](samples.md)** - Complete working examples
- **[API Reference](api/index.html)** - Complete API documentation

## Getting Help

- **[FAQ](faq.md)** - Frequently asked questions
- **[Troubleshooting](troubleshooting.md)** - Common issues and solutions
- **[GitHub Issues](https://github.com/cyberknet/JumpStart/issues)** - Report bugs or request features
- **[Discussions](https://github.com/cyberknet/JumpStart/discussions)** - Ask questions and share ideas

## Next Steps

Ready to dive deeper? Continue with:

1. **[Core Concepts](core-concepts.md)** - Understand the framework fundamentals
2. **[Audit Tracking](audit-tracking.md)** - Add automatic change tracking
3. **[API Development](api-development.md)** - Build RESTful APIs

Happy coding! ??
