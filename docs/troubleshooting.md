# Troubleshooting

Common issues and their solutions when using JumpStart.

## Installation and Setup

### Package Not Found

**Problem:** `dotnet add package JumpStart` fails with "Unable to find package"

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Try again
dotnet add package JumpStart
```

### Missing Dependencies

**Problem:** Build fails with missing assembly references

**Solution:**
```bash
# Restore all packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

## Entity Framework Core

### Migrations Not Creating

**Problem:** `dotnet ef migrations add` fails

**Solutions:**

1. **Install EF Core tools:**
```bash
dotnet tool install --global dotnet-ef
```

2. **Verify project setup:**
```bash
# Add design-time package
dotnet add package Microsoft.EntityFrameworkCore.Design
```

3. **Specify project and startup project:**
```bash
dotnet ef migrations add InitialCreate --project JumpStart.DemoApp --startup-project JumpStart.DemoApp
```

### Database Update Fails

**Problem:** `dotnet ef database update` fails

**Solutions:**

1. **Check connection string:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyDb;Trusted_Connection=True"
  }
}
```

2. **Verify SQL Server is running:**
```bash
# For LocalDB
sqllocaldb info
sqllocaldb start mssqllocaldb
```

3. **Check for pending migrations:**
```bash
dotnet ef migrations list
```

### Tables Not Created

**Problem:** Database exists but tables are missing

**Solution:**
```bash
# Apply migrations
dotnet ef database update

# Or drop and recreate
dotnet ef database drop
dotnet ef database update
```

## Audit Tracking

### Audit Fields Are Null

**Problem:** `CreatedById` and `CreatedOn` are null after saving

**Diagnosis:**
```csharp
// Check if user context is registered
var userContext = serviceProvider.GetService<IUserContext>();
if (userContext == null)
{
    Console.WriteLine("User context not registered!");
}

// Check if it returns a value
var userId = await userContext.GetCurrentUserIdAsync();
Console.WriteLine($"Current User ID: {userId ?? Guid.Empty}");
```

**Solutions:**

1. **Register user context:**
```csharp
builder.Services.AddScoped<IUserContext, BlazorUserContext>();
```

2. **Pass user context to repository:**
```csharp
// Constructor should receive it
public ProductRepository(
    DbContext context,
    IUserContext? userContext) // Don't forget this!
    : base(context, userContext)
{
}
```

3. **Ensure user is authenticated:**
```csharp
public class BlazorUserContext : IUserContext
{
    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        // Check if authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("User not authenticated");
            return null;
        }

        // Rest of implementation
    }
}
```

### Wrong User in Audit Fields

**Problem:** Audit fields show incorrect user

**Solutions:**

1. **Verify claim type:**
```csharp
// Check what claims are available
var claims = user.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
_logger.LogInformation("Claims: {Claims}", string.Join(", ", claims));

// Use correct claim type
var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

2. **Check claim value format:**
```csharp
if (Guid.TryParse(userIdClaim, out var userId))
{
    return userId;
}
else
{
    _logger.LogWarning("User ID claim '{Claim}' is not a valid Guid", userIdClaim);
    return null;
}
```

## Authentication

### 401 Unauthorized on All Requests

**Problem:** API returns 401 even with valid credentials

**Solutions:**

1. **Check authentication middleware order:**
```csharp
// Must be in this order
app.UseAuthentication();  // First
app.UseAuthorization();   // Second
app.MapControllers();     // Third
```

2. **Verify JWT configuration:**
```csharp
// Secret key must match between token generation and validation
var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);

// Issuer and Audience must match
ValidIssuer = configuration["JwtSettings:Issuer"],
ValidAudience = configuration["JwtSettings:Audience"],
```

3. **Check token format:**
```bash
# Token should start with "Bearer "
curl -H "Authorization: Bearer YOUR_TOKEN" https://localhost:7030/api/products
```

### Token Validation Failed

**Problem:** "IDX10223: Lifetime validation failed. The token is expired."

**Solutions:**

1. **Check token expiration:**
```csharp
var handler = new JwtSecurityTokenHandler();
var token = handler.ReadJwtToken(tokenString);
Console.WriteLine($"Expires: {token.ValidTo}");
Console.WriteLine($"Now: {DateTime.UtcNow}");
```

2. **Adjust ClockSkew if needed:**
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    // ... other settings
    ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes tolerance
};
```

3. **Use correct time zone:**
```csharp
// Always use UTC for token expiration
expires: DateTime.UtcNow.AddMinutes(expirationMinutes)
```

### CORS Errors

**Problem:** "CORS policy: No 'Access-Control-Allow-Origin' header"

**Solutions:**

1. **Add CORS before authentication:**
```csharp
app.UseCors("AllowSpecificOrigins"); // Before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
```

2. **Configure CORS policy correctly:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies/auth headers
    });
});
```

3. **Check origin matches exactly:**
```csharp
// These are different origins:
"https://localhost:7001"  // With https
"http://localhost:7001"   // With http
"https://localhost:7002"  // Different port
```

## AutoMapper

### Mapping Errors

**Problem:** "Missing type map configuration or unsupported mapping"

**Solutions:**

1. **Register all profiles:**
```csharp
builder.Services.AddJumpStartAutoMapper(typeof(Program).Assembly);
```

2. **Create mapping profile:**
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

3. **Validate configurations:**
```csharp
// In development
var mapper = serviceProvider.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid();
```

### Null Reference Exceptions

**Problem:** Mapping fails with null reference

**Solutions:**

1. **Handle null navigation properties:**
```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName,
        opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
```

2. **Use null-conditional operator:**
```csharp
.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category?.Name ?? "Uncategorized"));
```

## API Controllers

### 404 Not Found

**Problem:** API endpoint returns 404

**Solutions:**

1. **Check route configuration:**
```csharp
[ApiController]
[Route("api/[controller]")] // Will be "api/Products" for ProductsController
public class ProductsController : ControllerBase
```

2. **Verify controller registration:**
```csharp
builder.Services.AddControllers();
// ...
app.MapControllers(); // Don't forget this!
```

3. **Check HTTP method:**
```csharp
[HttpGet("{id}")] // GET /api/products/123
[HttpPost] // POST /api/products
[HttpPut("{id}")] // PUT /api/products/123
[HttpDelete("{id}")] // DELETE /api/products/123
```

### 400 Bad Request

**Problem:** POST/PUT returns 400 with validation errors

**Solutions:**

1. **Check model state:**
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState.Values
        .SelectMany(v => v.Errors)
        .Select(e => e.ErrorMessage);
    _logger.LogWarning("Validation errors: {Errors}", string.Join(", ", errors));
}
```

2. **Verify DTO properties:**
```csharp
public class CreateProductDto : ICreateDto
{
    [Required] // Check required fields
    [StringLength(200)] // Check length limits
    public string Name { get; set; } = string.Empty;
}
```

3. **Check Content-Type header:**
```bash
# Must be application/json
curl -X POST https://localhost:7030/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","price":19.99}'
```

## Swagger/OpenAPI

### Swagger UI Not Loading

**Problem:** /swagger returns 404

**Solutions:**

1. **Check environment:**
```csharp
if (app.Environment.IsDevelopment()) // Only in Development by default
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

2. **Enable in all environments:**
```csharp
app.UseSwagger();
app.UseSwaggerUI();
```

3. **Check correct URL:**
```
https://localhost:7030/swagger       // Swagger UI
https://localhost:7030/swagger/v1/swagger.json  // Swagger JSON
```

### Missing XML Documentation

**Problem:** Swagger doesn't show XML comments

**Solutions:**

1. **Enable XML documentation:**
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

2. **Include XML file:**
```csharp
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

## Performance Issues

### Slow Queries

**Problem:** API endpoints are slow

**Diagnosis:**
```csharp
// Add logging to see queries
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information);
});
```

**Solutions:**

1. **Add indexes:**
```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.HasIndex(e => e.Name);
    entity.HasIndex(e => e.CategoryId);
});
```

2. **Use pagination:**
```csharp
var result = await repository.GetPagedAsync(page: 1, pageSize: 20);
```

3. **Optimize includes:**
```csharp
// Instead of
var products = await context.Products
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .ToListAsync();

// Use split queries
var products = await context.Products
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .AsSplitQuery()
    .ToListAsync();
```

### Memory Issues

**Problem:** Application uses too much memory

**Solutions:**

1. **Use pagination:**
```csharp
// Don't do this
var allProducts = await repository.GetAllAsync(); // Loads everything

// Do this
var pagedProducts = await repository.GetPagedAsync(1, 20);
```

2. **Project to DTOs:**
```csharp
var products = await context.Products
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToListAsync();
```

3. **Dispose resources:**
```csharp
await using var context = new ApplicationDbContext(options);
// Use context
// Automatically disposed
```

## Common Error Messages

### "A connection was successfully established... existing connection was forcibly closed"

**Solution:** Check SQL Server is accessible and not behind a firewall.

### "Cannot insert explicit value for identity column"

**Solution:** Don't set `Id` for new entities - it's auto-generated:
```csharp
var product = new Product
{
    // Id = Guid.NewGuid(), // DON'T DO THIS
    Name = "Test Product"
};
```

### "The instance of entity type... cannot be tracked"

**Solution:** Don't track the same entity twice:
```csharp
// Use AsNoTracking for read-only queries
var products = await context.Products
    .AsNoTracking()
    .ToListAsync();
```

## Getting Help

If your issue isn't covered here:

1. **Check documentation:**
   - [Core Concepts](core-concepts.md)
   - [API Development](api-development.md)
   - [FAQ](faq.md)

2. **Search existing issues:**
   - [GitHub Issues](https://github.com/cyberknet/JumpStart/issues)

3. **Ask the community:**
   - [GitHub Discussions](https://github.com/cyberknet/JumpStart/discussions)

4. **Report a bug:**
   - [Create an Issue](https://github.com/cyberknet/JumpStart/issues/new)

---

**Still stuck?** [Open a discussion](https://github.com/cyberknet/JumpStart/discussions) with:
- Description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, etc.)
- Any error messages or stack traces
