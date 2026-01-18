# API Development

Build production-ready RESTful APIs quickly with JumpStart's base controllers, DTOs, and API clients.

## Overview

JumpStart provides everything you need to create RESTful APIs:

- **Base Controllers** - Standard CRUD endpoints with minimal code
- **DTOs** - Separation between internal entities and external contracts
- **AutoMapper** - Automatic entity-DTO mapping
- **API Clients** - Type-safe Refit-based clients
- **Authentication** - Built-in JWT support
- **Swagger** - Automatic API documentation

## Quick Start

### 1. Create DTOs

Define data transfer objects for your API:

```csharp
// Read DTO (what API returns)
public class ProductDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

// Create DTO (what API accepts for POST)
public class CreateProductDto : ICreateDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

// Update DTO (what API accepts for PUT)
public class UpdateProductDto : IUpdateDto
{
    [StringLength(200)]
    public string? Name { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Range(0.01, 999999.99)]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }
}
```

### 2. Create AutoMapper Profile

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

### 3. Create API Controller

Use base controller for instant CRUD endpoints:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<
    Product,           // Entity
    ProductDto,        // Read DTO
    CreateProductDto,  // Create DTO
    UpdateProductDto,  // Update DTO
    Guid>              // Key type
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper)
    {
    }
}
```

### 4. Register Services

Configure services in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JumpStart
builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        jumpStart.RegisterUserContext<ApiUserContext>();
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });

// Add AutoMapper
builder.Services.AddJumpStartAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 5. Test Your API

That's it! You now have these endpoints:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

## Base Controller Features

### What You Get Automatically

#### GET All (with pagination)

```http
GET /api/products?page=1&pageSize=20
```

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Widget",
      "price": 19.99,
      "stockQuantity": 100
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

#### GET By ID

```http
GET /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Widget",
  "price": 19.99,
  "stockQuantity": 100
}
```

#### POST (Create)

```http
POST /api/products
Content-Type: application/json

{
  "name": "New Widget",
  "price": 29.99,
  "stockQuantity": 50
}
```

**Response:** `201 Created` with location header and created entity

#### PUT (Update)

```http
PUT /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "name": "Updated Widget",
  "price": 24.99
}
```

**Response:** `200 OK` with updated entity

#### DELETE

```http
DELETE /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:** `204 No Content`

### Standard Response Codes

- `200 OK` - Successful GET or PUT
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Validation errors
- `404 Not Found` - Entity not found
- `500 Internal Server Error` - Server error

## Custom Endpoints

Extend base controllers with custom actions:

### Simple Custom Endpoint

```csharp
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper)
    {
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search(
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        var query = Repository.GetQuery();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var products = await query.ToListAsync();
        
        return Ok(Mapper.Map<IEnumerable<ProductDto>>(products));
    }
}
```

### Using Custom Repository

```csharp
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
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

    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(Guid categoryId)
    {
        var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
        return Ok(Mapper.Map<IEnumerable<ProductDto>>(products));
    }

    [HttpPost("{id}/restock")]
    public async Task<ActionResult<ProductDto>> Restock(
        Guid id,
        [FromBody] RestockRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();

        product.StockQuantity += request.Quantity;
        await _productRepository.UpdateAsync(product);

        return Ok(Mapper.Map<ProductDto>(product));
    }
}

public class RestockRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
```

## DTOs in Depth

### Why DTOs?

1. **Security** - Don't expose internal structure
2. **Versioning** - API can evolve independently
3. **Validation** - API-specific rules
4. **Documentation** - Clear API contracts
5. **Performance** - Send only necessary data

### DTO Design Patterns

#### Flat DTOs

Simple, flat structure:

```csharp
public class ProductDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName, 
        opt => opt.MapFrom(src => src.Category!.Name));
```

#### Nested DTOs

Related entities as nested objects:

```csharp
public class ProductDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public CategoryDto Category { get; set; } = null!;
    public decimal Price { get; set; }
}

public class CategoryDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
}
```

```csharp
CreateMap<Product, ProductDto>();
CreateMap<Category, CategoryDto>();
```

#### Detailed vs Summary DTOs

Different views for different scenarios:

```csharp
// Summary (for lists)
public class ProductSummaryDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Detailed (for single item)
public class ProductDetailDto : SimpleEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public CategoryDto Category { get; set; } = null!;
    public List<ReviewDto> Reviews { get; set; } = new();
}
```

### DTO Validation

Use data annotations for validation:

```csharp
public class CreateProductDto : ICreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 3, 
        ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    [Url(ErrorMessage = "Must be a valid URL")]
    public string? ImageUrl { get; set; }
    
    [EmailAddress]
    public string? SupplierEmail { get; set; }
}
```

### Custom Validation

Implement `IValidatableObject`:

```csharp
public class CreateProductDto : ICreateDto, IValidatableObject
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalePrice.HasValue && SalePrice >= Price)
        {
            yield return new ValidationResult(
                "Sale price must be less than regular price",
                new[] { nameof(SalePrice) });
        }
    }
}
```

## API Clients

JumpStart uses Refit to create type-safe HTTP clients.

### Define API Client Interface

```csharp
public interface IProductApiClient : ISimpleApiClient<ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    // Custom endpoints
    [Get("/api/products/search")]
    Task<IEnumerable<ProductDto>> SearchAsync(
        [Query] string? name = null,
        [Query] decimal? minPrice = null,
        [Query] decimal? maxPrice = null);

    [Get("/api/products/low-stock")]
    Task<IEnumerable<ProductDto>> GetLowStockAsync([Query] int threshold = 10);

    [Post("/api/products/{id}/restock")]
    Task<ProductDto> RestockAsync(Guid id, [Body] RestockRequest request);
}
```

### Register API Client

```csharp
builder.Services.AddSimpleApiClient<IProductApiClient>("https://api.example.com/api/products");
```

### Use API Client

```csharp
public class ProductService
{
    private readonly IProductApiClient _apiClient;

    public ProductService(IProductApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ProductDto?> GetProductAsync(Guid id)
    {
        try
        {
            return await _apiClient.GetByIdAsync(id);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string name)
    {
        return await _apiClient.SearchAsync(name: name);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        return await _apiClient.CreateAsync(dto);
    }
}
```

## Authentication

### JWT Authentication

See the comprehensive [Authentication & Security](authentication.md) guide.

#### Quick Setup

**API Project (Program.cs):**

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });
```

**Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    // ... endpoints require valid JWT token
}
```

### Authorization

#### Role-Based

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public override Task<IActionResult> DeleteAsync(Guid id)
{
    return base.DeleteAsync(id);
}
```

#### Policy-Based

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProductManager", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// Controller
[Authorize(Policy = "ProductManager")]
[HttpPost]
public override Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductDto dto)
{
    return base.CreateAsync(dto);
}
```

## Error Handling

### Standard Error Responses

JumpStart controllers return standard problem details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Name is required"
    ],
    "Price": [
      "Price must be between 0.01 and 999,999.99"
    ]
  }
}
```

### Custom Error Handling

```csharp
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    [HttpPost("{id}/restock")]
    public async Task<ActionResult<ProductDto>> Restock(Guid id, [FromBody] RestockRequest request)
    {
        try
        {
            var product = await Repository.GetByIdAsync(id);
            
            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            if (product.StockQuantity + request.Quantity > 10000)
            {
                return BadRequest(new 
                { 
                    message = "Stock quantity cannot exceed 10,000",
                    currentStock = product.StockQuantity,
                    requestedAddition = request.Quantity
                });
            }

            product.StockQuantity += request.Quantity;
            await Repository.UpdateAsync(product);

            return Ok(Mapper.Map<ProductDto>(product));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }
}
```

## Swagger/OpenAPI

### Configure Swagger

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "Product management API",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });

    // Add JWT authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

### XML Documentation

Add XML comments to controllers and DTOs:

```csharp
/// <summary>
/// Restocks a product with additional inventory.
/// </summary>
/// <param name="id">The product ID</param>
/// <param name="request">The restock request containing quantity</param>
/// <returns>The updated product</returns>
/// <response code="200">Product successfully restocked</response>
/// <response code="400">Invalid request or stock limit exceeded</response>
/// <response code="404">Product not found</response>
[HttpPost("{id}/restock")]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ProductDto>> Restock(
    Guid id, 
    [FromBody] RestockRequest request)
{
    // Implementation
}
```

## Testing APIs

### Integration Tests

```csharp
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessAndProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content);
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 19.99m,
            StockQuantity = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
    }
}
```

## Best Practices

### Do's ?

- **Use DTOs** instead of exposing entities directly
- **Validate input** using data annotations
- **Return proper HTTP status codes**
- **Document APIs** with XML comments
- **Version APIs** for breaking changes
- **Use async/await** consistently
- **Implement pagination** for large datasets
- **Add authentication** to sensitive endpoints

### Don'ts ?

- **Don't expose entities** directly in APIs
- **Don't return IQueryable** from APIs
- **Don't forget validation**
- **Don't use 200 OK** for everything
- **Don't expose internal errors** to clients
- **Don't skip authorization checks**
- **Don't forget rate limiting** in production

## Next Steps

- **[Authentication Guide](authentication.md)** - Implement JWT authentication
- **[How-To: Secure Endpoints](how-to/secure-endpoints.md)** - Add authorization
- **[How-To: Custom Controllers](how-to/custom-controllers.md)** - Advanced controller patterns
- **[Core Concepts](core-concepts.md)** - Understand the fundamentals

---

**Need Help?** Check the [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).

## Project Structure

The demo application is split into three projects:

### JumpStart.DemoApp (Blazor UI)
- **Port:** https://localhost:7099
- **Responsibilities:**
  - Blazor Server components and pages
  - ASP.NET Core Identity (user authentication)
  - API client registration (Refit)
  - JWT token management for API calls
- **Database:** ApplicationDbContext (Identity tables only)

### JumpStart.DemoApp.Api (Web API)
- **Port:** https://localhost:7030
- **Responsibilities:**
  - RESTful API controllers
  - Business logic and repositories
  - Entity Framework DbContext (business entities)
  - AutoMapper configuration
  - JWT authentication validation
- **Database:** ApiDbContext (Product tables)

### JumpStart.DemoApp.Shared (Contracts)
- **Responsibilities:**
  - DTOs shared between UI and API
  - Refit API client interfaces
  - No runtime dependencies

## Running the Demo

You must run **both projects simultaneously**:

1. **Start the API project first:**

cd JumpStart.DemoApp.Api dotnet run

API will be available at https://localhost:7030

2. **Start the UI project:**

cd JumpStart.DemoApp dotnet run

UI will be available at https://localhost:7099

### Visual Studio
- Right-click solution →**Set Startup Projects**
- Select **Multiple startup projects**
- Set both `JumpStart.DemoApp` and `JumpStart.DemoApp.Api` to **Start**

## Configuration

### API Project (appsettings.json)
{ "ConnectionStrings": { "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=JumpStartDemo;..." }, "JwtSettings": { "SecretKey": "YourSecretKey...", "Issuer": "JumpStartDemoApi", "Audience": "JumpStartDemoApp" }, "CorsSettings": { "BlazorServerUrl": "https://localhost:7099" } }
### UI Project (appsettings.json)
{ "ConnectionStrings": { "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=JumpStartDemo;..." }, "ApiBaseUrl": "https://localhost:7030" }
