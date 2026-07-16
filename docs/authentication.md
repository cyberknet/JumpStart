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
   - `TokenController`'s `POST /api/token/exchange` resolves and issues permission-bearing tokens
     (ADR-013)

2. **JumpStart** - Core library with authentication services
   - `IJwtTokenService` / `JwtTokenService` - Generates JWT tokens
   - `ITokenStore` / `TokenStore` - Stores tokens for the current user session
   - `JwtAuthenticationHandler` - HTTP handler that adds JWT tokens to API requests
   - `JwtExchangeHandler` - HTTP handler that ensures a real, permission-resolved token exists
     before a request goes out, auto-attached to auto-discovered API clients (ADR-014)

3. **JumpStart.DemoApp** - Blazor Server application
   - Uses cookie-based authentication for user login
   - `JwtExchangeHandler` obtains permission-resolved JWTs for API calls automatically

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

This token carries no `Permission` claims, so every JumpStart-generated endpoint will return `403`
(see [Entity Authorization](entity-authorization.md)) until you add them. If your Blazor app is
calling a separate JumpStart API project, don't hand-roll that step - see
[JwtExchangeHandler](#jwtexchangehandler-recommended-for-a-separate-api-project) below, which
resolves and attaches real permissions automatically.

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
public class ProductsController : ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>
{
    public ProductsController(
        IProductRepository repository,
        IMapper mapper,
        ILogger<ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>> logger,
        ICorrelationContextAccessor correlationContext)
        : base(repository, mapper, logger, correlationContext)
    {
    }
}
```

### 4. Accessing User Information in API

`IUserContext` only exposes `GetCurrentUserIdAsync()` (no `UserId`/`Username` properties) - that's the one piece of information the framework needs for audit tracking. If you need more (e.g. username), read it directly from `HttpContext.User` claims:

```csharp
public class CustomController : ControllerBase
{
    private readonly IUserContext _userContext;
    
    public CustomController(IUserContext userContext)
    {
        _userContext = userContext;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        var username = User.Identity?.Name;
        
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
builder.Services.AddScoped<IUserContext, ApiUserContext>();

// Enables the token-exchange endpoint (POST /api/token/exchange) and role/permission
// administration - see ADR-012/ADR-013 and JwtExchangeHandler below.
builder.Services.AddJumpStart(options =>
{
    options.RegisterAuthorizationController = true;
    options.RegisterTokenController = true;
});
```

### JumpStart.DemoApp (Program.cs)

```csharp
// JWT Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddTransient<JwtAuthenticationHandler>();
builder.Services.AddTransient<JwtExchangeHandler>();

// Register this BEFORE AddJumpStart so RegisterApiClients (via AutoDiscoverApiClients) can see
// it's already present and auto-attach JwtExchangeHandler/JwtAuthenticationHandler to every
// [ApiClientFor<...>]-decorated client - see ADR-014.
builder.Services.AddApiClient<ITokenExchangeApiClient>(apiBaseUrl);

builder.Services.AddJumpStart(options =>
{
    options.ApiBaseUrl = apiBaseUrl;
    options.AutoDiscoverApiClients = true; // auto-attaches the handlers below for you
});

// IProductApiClient predates [ApiClientFor<...>] and is registered manually, so it needs the
// chain wired up by hand - auto-discovered clients don't.
builder.Services.AddApiClient<IProductApiClient>($"{apiBaseUrl}/api/products")
    .AddHttpMessageHandler<JwtExchangeHandler>()
    .AddHttpMessageHandler<JwtAuthenticationHandler>();
```

#### JwtExchangeHandler (recommended for a separate API project)

When a Blazor Server app calls a separate JumpStart API, it has no direct database access to
resolve `Permission` claims itself - and it can't ask the API to resolve them either, since the
token it would use to authenticate that call is exactly what it's trying to produce.
`JwtExchangeHandler` solves this: it mints a short-lived, claim-free identity assertion JWT from
the current Blazor user, exchanges it via the API's `POST /api/token/exchange` endpoint for a
real, permission-resolved JWT, and stores it - all automatically, for every auto-discovered client.
See [ADR-013: JWT Token Exchange](architecture/adr/013-jwt-token-exchange.md) and
[ADR-014: Automatic JWT Exchange for Auto-Discovered API Clients](architecture/adr/014-automatic-jwt-exchange-for-api-clients.md)
for the full design, and [Role-Based Permission Management](architecture/adr/012-role-based-permission-management.md)
for how those permissions are actually assigned to users in the first place.

> **⚠️ Disable prerendering on any page/component that calls an auto-discovered API client.**
> `JwtExchangeHandler` can only identify the current user via a live Blazor Server circuit
> (`CircuitServicesAccessor`, see ADR-014's correction note) - during static prerendering, before
> the circuit exists, there is no way to resolve the user at all, and the handler throws
> `InvalidOperationException` rather than silently sending an unauthenticated request. Any
> `@page` component that calls an API client from `OnInitializedAsync` (or another lifecycle
> method) needs:
> ```razor
> @rendermode @(new InteractiveServerRenderMode(prerender: false))
> ```
> instead of the plain `@rendermode InteractiveServer` the default templates generate. This
> applies to every page in the demo app that talks to an API client - see ADR-014's correction
> note for the full list.

## Testing

The framework includes comprehensive tests for all authentication components:

- **JwtTokenServiceTests** - Token generation and validation
- **TokenStoreTests** - Token storage and retrieval
- **JwtAuthenticationHandlerTests** - HTTP handler functionality
- **JwtExchangeHandlerTests** - Assertion minting, exchange, and store behavior
- **TokenControllerTests** - Permission resolution and identity validation for the exchange endpoint
- **JwtExchangeAutoAttachmentTests** - Auto-attachment detection logic in `RegisterApiClients`

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
