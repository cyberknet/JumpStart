# How-To: Secure API Endpoints

Learn how to protect your JumpStart API endpoints using authentication and authorization.

## Overview

API security involves:
- **Authentication** - Who is the user?
- **Authorization** - What can the user do?
- **Validation** - Is the request valid?
- **Rate Limiting** - Prevent abuse

## Quick Start: JWT Authentication

### 1. Configure JWT in API Project

**appsettings.json:**

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForJwtTokenGeneration!",
    "Issuer": "YourAppName",
    "Audience": "YourApiName",
    "ExpirationMinutes": 60
  }
}
```

**Program.cs:**

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

// Add JWT authentication
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
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Create JWT Settings Class

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
```

### 3. Secure Your Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper)
        : base(repository, mapper)
    {
    }
    
    // GET /api/products - Requires authentication
    // POST /api/products - Requires authentication
    // PUT /api/products/{id} - Requires authentication
    // DELETE /api/products/{id} - Requires authentication
}
```

### 4. Allow Anonymous Access to Specific Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    [AllowAnonymous] // Anyone can view products
    [HttpGet]
    public override Task<ActionResult<PagedResult<ProductDto>>> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return base.GetAllAsync(page, pageSize);
    }

    [AllowAnonymous] // Anyone can view a specific product
    [HttpGet("{id}")]
    public override Task<ActionResult<ProductDto>> GetByIdAsync(Guid id)
    {
        return base.GetByIdAsync(id);
    }

    // POST, PUT, DELETE still require authentication
}
```

## Role-Based Authorization

### 1. Configure Roles in JWT Token

When generating tokens, include role claims:

```csharp
public class JwtTokenService
{
    public string GenerateToken(Guid userId, string username, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 2. Require Specific Roles

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    [AllowAnonymous]
    [HttpGet]
    public override Task<ActionResult<PagedResult<ProductDto>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return base.GetAllAsync(page, pageSize);
    }

    [Authorize(Roles = "Admin,Manager")] // Only Admin or Manager can create
    [HttpPost]
    public override Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductDto dto)
    {
        return base.CreateAsync(dto);
    }

    [Authorize(Roles = "Admin,Manager")] // Only Admin or Manager can update
    [HttpPut("{id}")]
    public override Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductDto dto)
    {
        return base.UpdateAsync(id, dto);
    }

    [Authorize(Roles = "Admin")] // Only Admin can delete
    [HttpDelete("{id}")]
    public override Task<IActionResult> DeleteAsync(Guid id)
    {
        return base.DeleteAsync(id);
    }
}
```

## Policy-Based Authorization

### 1. Define Policies

**Program.cs:**

```csharp
builder.Services.AddAuthorization(options =>
{
    // Require Admin or Manager role
    options.AddPolicy("RequireManagement", policy =>
        policy.RequireRole("Admin", "Manager"));

    // Require Admin role only
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    // Require specific claim
    options.AddPolicy("RequireProductEdit", policy =>
        policy.RequireClaim("Permission", "ProductEdit"));

    // Custom policy with requirements
    options.AddPolicy("RequireMinimumAge", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});
```

### 2. Apply Policies to Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    [AllowAnonymous]
    [HttpGet]
    public override Task<ActionResult<PagedResult<ProductDto>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return base.GetAllAsync(page, pageSize);
    }

    [Authorize(Policy = "RequireManagement")]
    [HttpPost]
    public override Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductDto dto)
    {
        return base.CreateAsync(dto);
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpDelete("{id}")]
    public override Task<IActionResult> DeleteAsync(Guid id)
    {
        return base.DeleteAsync(id);
    }
}
```

### 3. Custom Authorization Requirements

```csharp
// Requirement
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}

// Handler
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var birthDateClaim = context.User.FindFirst(c => c.Type == "BirthDate");
        
        if (birthDateClaim == null)
        {
            return Task.CompletedTask;
        }

        if (DateTime.TryParse(birthDateClaim.Value, out var birthDate))
        {
            var age = DateTime.Today.Year - birthDate.Year;
            
            if (birthDate.Date > DateTime.Today.AddYears(-age))
                age--;

            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

// Registration
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
```

## Resource-Based Authorization

Authorize based on the resource being accessed:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    private readonly IAuthorizationService _authorizationService;

    public ProductsController(
        ISimpleRepository<Product, Guid> repository,
        IMapper mapper,
        IAuthorizationService authorizationService)
        : base(repository, mapper)
    {
        _authorizationService = authorizationService;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await Repository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();

        // Check if user can edit this specific product
        var authResult = await _authorizationService.AuthorizeAsync(
            User, product, "ProductEditPolicy");

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return await base.UpdateAsync(id, dto);
    }
}

// Authorization handler
public class ProductEditHandler : AuthorizationHandler<OperationAuthorizationRequirement, Product>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Product resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Admin can edit any product
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Users can only edit products they created
        if (Guid.TryParse(userId, out var userGuid) && 
            resource.CreatedById == userGuid)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

## API Key Authentication

For service-to-service authentication:

### 1. Create API Key Middleware

```csharp
public class ApiKeyMiddleware
{
    private const string API_KEY_HEADER = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for certain paths
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        var apiKey = _configuration["ApiKey"];
        
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}

// Registration
app.UseMiddleware<ApiKeyMiddleware>();
```

### 2. API Key Attribute

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string API_KEY_HEADER = "X-API-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key missing");
            return;
        }

        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        
        var apiKey = configuration["ApiKey"];

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
        }
    }
}

// Usage
[ApiController]
[Route("api/[controller]")]
[ApiKey] // Require API key for all endpoints
public class ProductsController : SimpleApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, Guid>
{
    // ...
}
```

## CORS Configuration

Allow specific origins to access your API:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "https://www.example.com",
                "https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    // Development policy
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowDevelopment");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}
```

## Rate Limiting

Prevent API abuse with rate limiting:

```csharp
using AspNetCoreRateLimit;

// Program.cs
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60
        },
        new RateLimitRule
        {
            Endpoint = "*/api/products",
            Period = "1m",
            Limit = 100
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();

app.UseIpRateLimiting();
```

## Input Validation

Always validate input to prevent injection attacks:

```csharp
public class CreateProductDto : ICreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9\s-]+$", 
        ErrorMessage = "Name can only contain letters, numbers, spaces, and hyphens")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}
```

## Swagger with Authentication

Configure Swagger to support JWT authentication:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
});
```

## Best Practices

### Do's ?

- **Use HTTPS** in production
- **Store secrets securely** (Azure Key Vault, environment variables)
- **Rotate keys regularly**
- **Use strong secret keys** (minimum 32 characters)
- **Validate all input**
- **Implement rate limiting**
- **Log authentication failures**
- **Use least privilege** (minimum required permissions)
- **Set appropriate token expiration**
- **Implement refresh tokens** for long sessions

### Don'ts ?

- **Don't store secrets** in source control
- **Don't use weak secret keys**
- **Don't trust client input**
- **Don't expose internal errors** to clients
- **Don't allow unlimited API calls**
- **Don't skip HTTPS** in production
- **Don't use the same key** across environments
- **Don't forget to validate** on server side

## Testing Secured Endpoints

```csharp
public class SecuredProductsControllerTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _token = string.Empty;

    public SecuredProductsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var loginDto = new { Username = "testuser", Password = "password123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        _token = result!.Token;
    }

    [Fact]
    public async Task GetProducts_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithValidToken_Returns200()
    {
        // Arrange
        await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteProduct_AsNonAdmin_Returns403()
    {
        // Arrange
        await AuthenticateAsync(); // Non-admin user
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
```

## Next Steps

- **[Authentication & Security Guide](authentication.md)** - Complete authentication setup
- **[How-To: Custom Controllers](custom-controllers.md)** - Advanced controller patterns
- **[API Development](../api-development.md)** - Build RESTful APIs
- **[Core Concepts](../core-concepts.md)** - Understand the fundamentals

---

**Questions?** See [FAQ](../faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
