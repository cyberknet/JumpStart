# Sample Applications

Learn from complete, working examples of JumpStart applications.

## Overview

JumpStart includes two reference implementations that demonstrate best practices and common patterns:

1. **JumpStart.DemoApp** - Blazor Server application with ASP.NET Core Identity
2. **JumpStart.DemoApp.Api** - Standalone RESTful API with JWT authentication

## JumpStart.DemoApp

A full-featured Blazor Server application demonstrating:

### Features

- **ASP.NET Core Identity** - User registration, login, and account management
- **Blazor Server** - Interactive UI with server-side rendering
- **Entity Framework Core** - SQL Server database with migrations
- **Audit Tracking** - Automatic tracking of created/modified records
- **Repository Pattern** - Clean separation of data access
- **AutoMapper** - Entity-DTO mapping
- **Product Management** - Complete CRUD operations

### Project Structure

```
JumpStart.DemoApp/
??? Components/          # Blazor components and pages
?   ??? Account/        # Identity UI components
??? Controllers/        # API controllers (optional)
??? Data/              # Entities and DbContext
?   ??? Product.cs
?   ??? ApplicationUser.cs
?   ??? ApplicationDbContext.cs
??? DTOs/              # Data transfer objects
?   ??? ProductDto.cs
??? Mapping/           # AutoMapper profiles
?   ??? ProductMappingProfile.cs
??? Repositories/      # Custom repositories
?   ??? ProductRepository.cs
??? Services/          # Application services
?   ??? BlazorUserContext.cs
??? Program.cs         # Application configuration
```

### Key Files

#### Entity with Audit Tracking

**Data/Product.cs:**

```csharp
public class Product : SimpleAuditableNamedEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    
    // Navigation properties
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

#### Repository with Custom Methods

**Repositories/ProductRepository.cs:**

```csharp
public interface IProductRepository : ISimpleRepository<Product, Guid>
{
    Task<IList<Product>> GetLowStockProductsAsync(int threshold);
    Task<IList<Product>> GetProductsByCategoryAsync(Guid categoryId);
}

public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(
        ApplicationDbContext context,
        ISimpleUserContext? userContext)
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
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
```

#### User Context for Blazor

**Services/BlazorUserContext.cs:**

```csharp
public class BlazorUserContext : ISimpleUserContext
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

#### Configuration

**Program.cs:**

```csharp
using JumpStart;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Add JumpStart with DbContext
builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        jumpStart.RegisterUserContext<BlazorUserContext>();
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });

// Add AutoMapper
builder.Services.AddJumpStartAutoMapper(typeof(Program).Assembly);

// Add Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

### Running the Sample

```bash
# Navigate to project
cd JumpStart.DemoApp

# Update database
dotnet ef database update

# Run application
dotnet run
```

Navigate to `https://localhost:7001` (or configured port).

## JumpStart.DemoApp.Api

A standalone RESTful API demonstrating:

### Features

- **JWT Bearer Authentication** - Secure token-based authentication
- **RESTful Endpoints** - Standard CRUD operations
- **Swagger/OpenAPI** - Interactive API documentation
- **CORS Support** - Cross-origin resource sharing
- **Audit Tracking** - Automatic tracking via JWT claims
- **Validation** - Input validation and error handling

### Project Structure

```
JumpStart.DemoApp.Api/
??? Controllers/              # API controllers
?   ??? ExampleController.cs
??? Infrastructure/
?   ??? Authentication/      # JWT configuration
?       ??? JwtSettings.cs
?       ??? ApiUserContext.cs
??? Program.cs              # API configuration
??? appsettings.json        # Configuration
```

### Key Files

#### JWT Configuration

**Infrastructure/Authentication/JwtSettings.cs:**

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
```

#### User Context for API

**Infrastructure/Authentication/ApiUserContext.cs:**

```csharp
public class ApiUserContext : ISimpleUserContext
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

#### Configuration

**Program.cs:**

```csharp
// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

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
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorServer", policy =>
    {
        policy.WithOrigins(blazorServerUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// User Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISimpleUserContext, ApiUserContext>();
```

**appsettings.json:**

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForJwtTokenGeneration!",
    "Issuer": "JumpStartBlazorServer",
    "Audience": "JumpStartApi",
    "ExpirationMinutes": 60
  },
  "CorsSettings": {
    "BlazorServerUrl": "https://localhost:7001"
  }
}
```

#### Secured Controller

**Controllers/ExampleController.cs:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT authentication
public class ExampleController : ControllerBase
{
    private readonly ISimpleUserContext _userContext;

    public ExampleController(ISimpleUserContext userContext)
    {
        _userContext = userContext;
    }

    [HttpGet("current-user")]
    public async Task<ActionResult> GetCurrentUser()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        
        if (userId == null)
            return Unauthorized("User not authenticated");

        return Ok(new
        {
            UserId = userId,
            Message = "User authenticated successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("public")]
    [AllowAnonymous] // No authentication required
    public IActionResult GetPublic()
    {
        return Ok(new
        {
            Message = "This is a public endpoint",
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### Running the Sample

```bash
# Navigate to project
cd JumpStart.DemoApp.Api

# Run API
dotnet run
```

Navigate to `https://localhost:7030/swagger` (or configured port) for API documentation.

## Testing the Samples

### Generate JWT Token

Use the Blazor app to authenticate, then extract the token:

```csharp
// In Blazor app
public class AuthenticationService
{
    private readonly IJwtTokenService _tokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public async Task<string> LoginAsync(string username, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(
            username, password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _signInManager.UserManager.FindByNameAsync(username);
            return _tokenService.GenerateToken((int)user.Id, user.UserName!);
        }

        throw new InvalidOperationException("Login failed");
    }
}
```

### Call API with Token

```bash
# Using curl
curl -X GET "https://localhost:7030/api/example/current-user" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"

# Using PowerShell
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7030/api/example/current-user" -Headers $headers
```

## Common Patterns Demonstrated

### 1. Audit Tracking Across Applications

**Blazor App:**
- Uses `BlazorUserContext` with cookie authentication
- User ID extracted from `AuthenticationStateProvider`

**API:**
- Uses `ApiUserContext` with JWT authentication
- User ID extracted from JWT claims

Both store the same audit information in the database!

### 2. Separation of Concerns

- **Entities** - Domain models (no UI/API logic)
- **Repositories** - Data access only
- **Services** - Business logic
- **Controllers** - HTTP request handling
- **Components** - UI presentation

### 3. Dependency Injection

All dependencies injected via constructor:

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
}
```

### 4. Configuration Management

Typed configuration classes:

```csharp
public class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public int MaxUploadSize { get; set; }
    public EmailSettings Email { get; set; } = new();
}

// Registration
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Usage
public class MyService
{
    private readonly AppSettings _settings;

    public MyService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }
}
```

## Learning Path

### For Beginners

1. Start with **JumpStart.DemoApp**
2. Explore the `Product` entity and `ProductRepository`
3. Look at how audit tracking works automatically
4. Understand the Blazor components

### For Intermediate

1. Study **JumpStart.DemoApp.Api**
2. Understand JWT authentication setup
3. Explore CORS configuration
4. See how to secure endpoints

### For Advanced

1. Compare `BlazorUserContext` vs `ApiUserContext`
2. Study the AutoMapper profiles
3. Examine custom repository methods
4. Understand the service registration patterns

## Cloning and Running

```bash
# Clone repository
git clone https://github.com/cyberknet/JumpStart.git
cd JumpStart

# Restore packages
dotnet restore

# Run Blazor app
cd JumpStart.DemoApp
dotnet ef database update
dotnet run

# In another terminal, run API
cd ../JumpStart.DemoApp.Api
dotnet run
```

## Next Steps

- **[Getting Started](getting-started.md)** - Build your first app
- **[Core Concepts](core-concepts.md)** - Understand the framework
- **[API Development](api-development.md)** - Create RESTful APIs
- **[Authentication Guide](authentication.md)** - Implement security

---

**Questions?** See [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
