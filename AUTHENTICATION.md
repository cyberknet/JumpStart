# JumpStart API Authentication Setup

This document describes the JWT authentication implementation for the JumpStart framework.

## Overview

The JumpStart framework now includes a separate Web API project (`JumpStart.DemoApp.Api`) that uses JWT Bearer authentication, while the Blazor Server application (`JumpStart.DemoApp`) uses cookie-based authentication. The Blazor app communicates with the API using JWT tokens.

## Architecture

### Projects

1. **JumpStart.DemoApp.Api** - Standalone Web API with JWT authentication
   - Exposes RESTful endpoints
   - Validates JWT tokens from requests
   - Uses `ApiUserContext` for audit tracking

2. **JumpStart** - Core library with authentication services
   - `IJwtTokenService` / `JwtTokenService` - Generates JWT tokens
   - `ITokenStore` / `TokenStore` - Stores tokens for the current user session
   - `JwtAuthenticationHandler` - HTTP handler that adds JWT tokens to API requests

3. **JumpStart.DemoApp** - Blazor Server application
   - Uses cookie-based authentication for user login
   - Generates JWT tokens for API calls
   - Includes authentication handler in API client pipeline

## Configuration

### JumpStart.DemoApp.Api (appsettings.json)

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

**Important Security Notes:**
- The `SecretKey` must be at least 32 characters long
- In production, store the secret key securely (Azure Key Vault, environment variables, etc.)
- Never commit production secrets to source control
- Update `BlazorServerUrl` to match your Blazor app's URL

### JumpStart.DemoApp (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForJwtTokenGeneration!",
    "Issuer": "JumpStartBlazorServer",
    "Audience": "JumpStartApi",
    "ExpirationMinutes": 60
  },
  "ApiBaseUrl": "https://localhost:7002"
}
```

**Note:** The `JwtSettings` should match between the Blazor app and API for token validation to work correctly.

## Usage

### 1. Generating JWT Tokens (in Blazor Server)

```csharp
public class AuthenticationService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenStore _tokenStore;
    
    public AuthenticationService(
        IJwtTokenService jwtTokenService, 
        ITokenStore tokenStore)
    {
        _jwtTokenService = jwtTokenService;
        _tokenStore = tokenStore;
    }
    
    public async Task<string> LoginAsync(string username, string password)
    {
        // 1. Validate credentials using Identity
        var user = await ValidateCredentialsAsync(username, password);
        
        // 2. Generate JWT token
        var token = _jwtTokenService.GenerateToken(user.Id, user.UserName);
        
        // 3. Store token for API calls
        _tokenStore.SetToken(token);
        
        return token;
    }
    
    public void Logout()
    {
        _tokenStore.ClearToken();
    }
}
```

### 2. Making Authenticated API Calls

Once the token is stored, all API calls automatically include the JWT token:

```csharp
public class ProductService
{
    private readonly IProductApiClient _apiClient;
    
    public ProductService(IProductApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    // The JwtAuthenticationHandler automatically adds the Bearer token
    public async Task<ProductDto> GetProductAsync(Guid id)
    {
        return await _apiClient.GetByIdAsync(id);
    }
}
```

### 3. Securing API Endpoints

In the API project, use the `[Authorize]` attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires valid JWT token
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

### 4. Accessing User Information in API

The `ApiUserContext` provides access to the authenticated user:

```csharp
public class CustomController : ControllerBase
{
    private readonly ISimpleUserContext _userContext;
    
    public CustomController(ISimpleUserContext userContext)
    {
        _userContext = userContext;
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = _userContext.UserId;
        var username = _userContext.Username;
        
        return Ok(new { userId, username });
    }
}
```

## Service Registration

### JumpStart.DemoApp.Api (Program.cs)

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

### JumpStart.DemoApp (Program.cs)

```csharp
// JWT Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddTransient<JwtAuthenticationHandler>();

// API Client with JWT Handler
builder.Services.AddSimpleApiClient<IProductApiClient>($"{apiBaseUrl}/api/products")
    .AddHttpMessageHandler<JwtAuthenticationHandler>();
```

## Testing

The framework includes comprehensive tests for all authentication components:

- **JwtTokenServiceTests** - Token generation and validation
- **TokenStoreTests** - Token storage and retrieval
- **JwtAuthenticationHandlerTests** - HTTP handler functionality

Run tests with:
```bash
dotnet test JumpStart.Tests/JumpStart.Tests.csproj
```

## Security Best Practices

1. **Secret Key Management**
   - Use at least 32 characters for the secret key
   - Store secrets in Azure Key Vault or secure environment variables
   - Rotate keys periodically

2. **Token Expiration**
   - Set appropriate expiration times (default: 60 minutes)
   - Implement refresh token logic for longer sessions
   - Clear tokens on logout

3. **CORS Configuration**
   - Restrict `AllowBlazorServer` to specific domains
   - Use `AllowCredentials` only when necessary
   - Validate origins in production

4. **HTTPS**
   - Always use HTTPS in production
   - Configure HSTS headers
   - Redirect HTTP to HTTPS

5. **Token Storage**
   - Current implementation uses in-memory storage (scoped per user)
   - Consider secure storage options for production
   - Implement token refresh logic

## Troubleshooting

### 401 Unauthorized

- Verify JWT settings match between Blazor app and API
- Check that the token is being stored after login
- Ensure the API endpoint has `[Authorize]` attribute
- Verify CORS policy allows the request

### Token Validation Failed

- Check that the secret key is identical in both configurations
- Verify issuer and audience match
- Ensure the token hasn't expired
- Check system clock synchronization

### Missing Claims

- Verify claims are added during token generation
- Check the `ClockSkew` setting (set to Zero for strict validation)
- Ensure proper claim types are used (e.g., `ClaimTypes.NameIdentifier`)

## Additional Resources

- [Microsoft JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Refit Documentation](https://github.com/reactiveui/refit)
- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
