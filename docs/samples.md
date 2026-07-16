# Sample Applications

Learn from complete, working examples of JumpStart applications.

## Overview

JumpStart includes two reference implementations that demonstrate best practices and common patterns:

1. **JumpStart.DemoApp** - Blazor Server application with ASP.NET Core Identity
2. **JumpStart.DemoApp.Api** - Standalone RESTful API with JWT authentication

Together they showcase JumpStart's intended two-project shape: the Blazor app owns **only** its
ASP.NET Core Identity database (for login/registration) and reaches everything else JumpStart
provides - Products, Forms, Roles/Permissions - through generated API clients talking to the
separate API project. There is no `JumpStartDbContext`, no repository, and no direct entity access
anywhere in the Blazor project.

> There's no way today to route ASP.NET Core Identity itself through an API client - that would be
> needed to remove the Blazor project's database dependency entirely. Out of scope for now.

## JumpStart.DemoApp

A Blazor Server application demonstrating:

### Features

- **ASP.NET Core Identity** - User registration, login, and account management (its own database -
  the *only* database this project touches directly)
- **Blazor Server** - Interactive UI with server-side rendering
- **Everything else via API clients** - Products, Forms, Roles, and UserPermissions are all
  consumed through Refit-based API clients calling `JumpStart.DemoApp.Api`
- **Automatic JWT exchange** - `JwtExchangeHandler` obtains a real, permission-resolved token for
  every API call with no manual login-to-token step (see
  [ADR-013](architecture/adr/013-jwt-token-exchange.md)/[ADR-014](architecture/adr/014-automatic-jwt-exchange-for-api-clients.md))

### Project Structure

```
JumpStart.DemoApp/
├── Components/              # Blazor components and pages
│   ├── Account/             # Identity UI components
│   ├── Layout/
│   └── Pages/
│       ├── Products/
│       ├── Forms/
│       ├── QuestionTypes/
│       └── Roles/           # Role/permission administration UI (ADR-012)
├── Data/                    # Identity only - no JumpStart entities here
│   ├── ApplicationUser.cs
│   └── ApplicationDbContext.cs
├── Clients/                 # Refit API client interfaces
│   ├── ProductApiClient.cs  # IProductApiClient (manually registered)
│   └── IDemoBootstrapApiClient.cs
├── Services/
│   └── DemoNewUserBootstrapper.cs  # Demo-only bootstrap, not a framework concept
└── Program.cs
```

`IFormsApiClient`, `IRolesApiClient`, and `IUserPermissionsApiClient` aren't listed above because
they live in JumpStart core (`JumpStart.Forms.Clients`/`JumpStart.Authorization.Clients`) and are
discovered automatically - there's no need to redeclare them in the consuming app.

### Key Files

#### Identity-Only DbContext

**Data/ApplicationDbContext.cs:**

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

This is deliberately a plain `IdentityDbContext`, **not** a `JumpStartDbContext` - the Blazor
project has no JumpStart entities of its own, so there's nothing for it to seed or configure.

#### API Client

**Clients/ProductApiClient.cs:**

```csharp
public interface IProductApiClient : IApiClient<ProductDto, CreateProductDto, UpdateProductDto>
{
    [Get("/by-price-range")]
    Task<IEnumerable<ProductDto>> GetByPriceRangeAsync([Query] decimal minPrice, [Query] decimal maxPrice);

    [Get("/low-stock")]
    Task<IEnumerable<ProductDto>> GetLowStockAsync([Query] int threshold = 10);

    [Get("/active")]
    Task<IEnumerable<ProductDto>> GetActiveAsync();
}
```

Blazor components inject this directly (`@inject IProductApiClient ProductClient`) - there's no
local repository or service layer wrapping it, because there's nothing local to wrap.

#### Configuration

**Program.cs:**

```csharp
// 1. Identity's own database - the only DbContext in this project
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity services (cookie auth, registration, login)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// 3. JWT services used to call the separate API
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddTransient<JwtAuthenticationHandler>();
builder.Services.AddTransient<JwtExchangeHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";

// Registered BEFORE AddJumpStart so its auto-attachment check (ADR-014) sees it already present
builder.Services.AddApiClient<ITokenExchangeApiClient>(apiBaseUrl);

// 4. Everything else JumpStart provides - discovered automatically
builder.Services.AddJumpStart(options =>
{
    options.ApiBaseUrl = apiBaseUrl;
    options.AutoDiscoverApiClients = true;   // discovers IFormsApiClient, IRolesApiClient, ...
    options.AutoDiscoverRepositories = false; // no local repositories - this project has none
});

// Demo-only bootstrap client + service - grants a first-time user a role so the demo isn't empty.
// Called directly from Register.razor/ExternalLogin.razor right after account creation, not via a
// message handler - see ADR-014's correction note for why.
builder.Services.AddApiClient<IDemoBootstrapApiClient>(apiBaseUrl);
builder.Services.AddScoped<DemoNewUserBootstrapper>();

// IProductApiClient predates [ApiClientFor<...>] and is registered manually, so its handler
// chain is wired by hand - auto-discovered clients get this automatically.
builder.Services.AddApiClient<IProductApiClient>($"{apiBaseUrl}/api/products")
    .AddHttpMessageHandler<JwtExchangeHandler>()
    .AddHttpMessageHandler<JwtAuthenticationHandler>();
```

`options.AutoDiscoverRepositories = false` is the load-bearing line here - it's what keeps this
project from silently growing a direct-database-access path as new JumpStart modules are added.
Every JumpStart feature (Forms, Roles, UserPermissions) reaches this app only as a Refit client.

### Running the Sample

```bash
# Navigate to project
cd JumpStart.DemoApp

# Update the Identity database
dotnet ef database update

# Run application
dotnet run
```

Navigate to `https://localhost:7099` (or your configured port). `JumpStart.DemoApp.Api` must also
be running for anything beyond registration/login to work.

## JumpStart.DemoApp.Api

A standalone RESTful API demonstrating:

### Features

- **JWT Bearer Authentication** - Validates the tokens `JwtExchangeHandler`/`TokenController`
  produce
- **Entity Authorization** - Every `ApiControllerBase` action requires a `Permission` claim by
  default (ADR-011) - no opt-out
- **RESTful Endpoints** - Products, Forms, Roles, and UserPermissions, all via
  `ApiControllerBase<TEntity, ...>`
- **Token Exchange Endpoint** - `POST /api/token/exchange` resolves and issues real,
  permission-bearing JWTs (ADR-013)
- **Swagger/OpenAPI** - Interactive API documentation
- **CORS Support** - Restricted to the Blazor app's origin
- **Audit Tracking** - Automatic tracking via JWT claims (`ApiUserContext`)

### Project Structure

```
JumpStart.DemoApp.Api/
├── Controllers/
│   ├── ProductsController.cs      # ApiControllerBase<Product, ...>
│   ├── DemoBootstrapController.cs # demo-only bootstrap endpoint
│   └── ExampleController.cs       # plain [Authorize] example, not entity-based
├── Data/
│   ├── Product.cs
│   └── ApiDbContext.cs            # : JumpStartDbContext
├── Repositories/
│   ├── IProductRepository.cs
│   └── ProductRepository.cs
├── Mapping/
│   └── ProductMappingProfile.cs
├── Infrastructure/
│   └── Authentication/
│       ├── JwtSettings.cs
│       └── ApiUserContext.cs
└── Program.cs
```

`RolesController`, `UserPermissionsController`, `FormsController`, `QuestionTypesController`, and
`TokenController` aren't listed above because they're framework-provided (`JumpStart.Authorization.Controllers`/
`JumpStart.Forms.Controllers`/`JumpStart.Services.Authentication.Controllers`) - registered by
`AddJumpStart`, not written by this project.

### Key Files

#### DbContext

**Data/ApiDbContext.cs:**

```csharp
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : JumpStartDbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required - seeds framework data (QuestionTypes, etc.)
    }
}
```

#### User Context for the API

**Infrastructure/Authentication/ApiUserContext.cs:**

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

        return Task.FromResult(Guid.TryParse(userIdClaim, out var userId) ? (Guid?)userId : null);
    }
}
```

#### Product Controller

**Controllers/ProductsController.cs:**

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductsController
    : ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>
{
    public ProductsController(IProductRepository repository, IMapper mapper,
        ILogger<ProductsController> logger, ICorrelationContextAccessor correlationAccessor)
        : base(repository, mapper, logger, correlationAccessor)
    {
    }

    // Inherits GET/GET-all/POST/PUT/DELETE, each already protected by [EntityAuthorize] (ADR-011)

    [HttpGet("by-price-range")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByPriceRange(
        [FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        var products = await _repository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        return Ok(_mapper.Map<List<ProductDto>>(products));
    }
}
```

#### Configuration

**Program.cs:**

```csharp
// 1. Database
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. JumpStart framework services
builder.Services.AddJumpStart(options =>
{
    options.RegisterUserContext<ApiUserContext>();
    options.RegisterTenantContext<JwtTenantContext>(); // reads the tenant_id JWT claim (ADR-015)
    options.AutoDiscoverRepositories = true;   // required for EnsureDbContextResolution
    options.ScanAssembly(typeof(Program).Assembly);
    options.RegisterFormsController = true;
    options.RegisterAuthorizationController = true; // Roles/UserPermissions CRUD (ADR-012)
    options.RegisterTokenController = true;         // POST /api/token/exchange (ADR-013)
    options.RegisterTenantsController = true;       // Tenant CRUD + membership (ADR-015)
});

// 3. AutoMapper
builder.Services.AddJumpStartAutoMapper(
    typeof(Program).Assembly,
    typeof(JumpStart.Forms.Form).Assembly);

// 4. JWT bearer authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 5. CORS - restrict to the Blazor app's origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorServer", policy =>
        policy.WithOrigins(blazorServerUrl).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

// 6. HTTP context accessor (for IUserContext)
builder.Services.AddHttpContextAccessor();

// 7. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 8. Controllers
builder.Services.AddControllers();
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
    "BlazorServerUrl": "https://localhost:7099"
  }
}
```

**Important Security Notes:**
- The `SecretKey` must be at least 32 characters long and identical in both projects' configuration
- In production, store the secret key securely (Azure Key Vault, environment variables, etc.)
- Never commit production secrets to source control

### Running the Sample

```bash
cd JumpStart.DemoApp.Api
dotnet ef database update
dotnet run
```

Navigate to `https://localhost:7030/swagger` (or your configured port) for API documentation.

## Testing the Samples

Register and log in through the Blazor app, then visit any page that calls the API (Products,
Forms, Roles). There is no manual "generate a token" step - `JwtExchangeHandler` mints a short-lived
identity assertion from the logged-in user, exchanges it for a real token via
`POST /api/token/exchange`, and stores it automatically on the first API call of the session (see
[ADR-013](architecture/adr/013-jwt-token-exchange.md)). A brand-new user is bootstrapped into a
"Demo Administrator" role the first time this happens, so the demo isn't empty on first login (demo-only, see [ADR-012](architecture/adr/012-role-based-permission-management.md)'s bootstrapping note).

To call the API directly without going through Blazor (e.g. for debugging), you still need a real
token. Get one from `TokenController`, using any validly-signed JWT as the identity assertion:

```bash
# Exchange an assertion token for a real, permission-resolved one
curl -X POST "https://localhost:7030/api/token/exchange" \
  -H "Authorization: Bearer YOUR_ASSERTION_TOKEN_HERE"

# Then call the API with the real token
curl -X GET "https://localhost:7030/api/products" \
  -H "Authorization: Bearer YOUR_REAL_TOKEN_HERE"
```

## Common Patterns Demonstrated

### 1. Audit Tracking Only Where There's Something to Audit

`ApiUserContext` (API project) extracts the user ID from JWT claims for `Repository<TEntity>`'s
automatic `CreatedById`/`ModifiedById` population. The Blazor project has no `IUserContext`
implementation at all - it has no repositories and no entities of its own, so there's nothing for
one to serve.

### 2. Separation of Concerns

- **Entities** - Domain models (API project only)
- **Repositories** - Data access (API project only)
- **Controllers** - HTTP request handling, protected by entity authorization by default
- **API Clients** - Strongly-typed Refit interfaces (Blazor project's only way to reach JumpStart data)
- **Components** - UI presentation (Blazor project)

### 3. Dependency Injection

All dependencies injected via constructor, including generated API clients:

```csharp
@inject IProductApiClient ProductClient

@code {
    private IEnumerable<ProductDto>? products;

    protected override async Task OnInitializedAsync()
    {
        var result = await ProductClient.GetAllAsync();
        products = result.Items;
    }
}
```

## Learning Path

### For Beginners

1. Start with **JumpStart.DemoApp.Api** - it's where the actual entities, repositories, and
   controllers live
2. Explore `Product`, `IProductRepository`, and `ProductsController`
3. See how `[EntityAuthorize]` protects every CRUD action automatically

### For Intermediate

1. Study **JumpStart.DemoApp** - notice it has no entities, repositories, or DbContext for
   JumpStart data at all, only `IProductApiClient`/`IFormsApiClient`/etc.
2. Trace a request: Blazor component → API client → `JwtExchangeHandler` → API → `[EntityAuthorize]`
3. Explore CORS and JWT bearer configuration on the API side

### For Advanced

1. Compare `ApiUserContext` (JWT-claim-based) with a cookie-based `IUserContext` you might write for
   a single-project app (see [Authentication & Security](authentication.md))
2. Study `JwtExchangeHandler`/`TokenController` (ADR-013/014) and how `RegisterApiClients`
   auto-attaches the handler chain
3. Study the AutoMapper profiles and custom repository methods
4. Read [Role-Based Permission Management](architecture/adr/012-role-based-permission-management.md)
   and try granting/revoking permissions through the Roles admin UI

## Cloning and Running

```bash
# Clone repository
git clone https://github.com/cyberknet/JumpStart.git
cd JumpStart

# Restore packages
dotnet restore

# Run the API first
cd JumpStart.DemoApp.Api
dotnet ef database update
dotnet run

# In another terminal, run the Blazor app
cd ../JumpStart.DemoApp
dotnet ef database update
dotnet run
```

## Next Steps

- **[Getting Started](getting-started.md)** - Build your first app
- **[Core Concepts](core-concepts.md)** - Understand the framework
- **[API Development](api-development.md)** - Create RESTful APIs
- **[Authentication Guide](authentication.md)** - Implement security
- **[Role-Based Permission Management](architecture/adr/012-role-based-permission-management.md)** -
  How Permission claims are administered

---

**Questions?** See [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
