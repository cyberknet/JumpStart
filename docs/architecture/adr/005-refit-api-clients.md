# ADR-005: Refit for API Clients

**Status:** Accepted (Decision section code samples corrected - see note)

**Date:** 2025-01-15

**Decision Makers:** JumpStart Core Team

> **⚠️ Correction (2026-01-25):** The choice of Refit itself, and the rationale in Context/
> Consequences/Alternatives, still holds. But sections 1-4 under Decision originally described a
> "Simple vs Advanced" dual client/controller hierarchy (`ISimpleApiClient`,
> `IAdvancedApiClient<TDto,TCreateDto,TUpdateDto,TKey>`, `SimpleApiControllerBase`,
> `SimpleApiClientExtensions.AddSimpleApiClient`) that was removed along with custom key-type
> support - see [ADR-009: Guid-Only Entities](009-guid-only-entities.md). The current system has
> a single `IApiClient<TDto,TCreateDto,TUpdateDto>` (Guid-only), a single
> `ApiControllerBase<TEntity,TDto,TCreateDto,TUpdateDto,TRepository>` (see
> [API Development](../../api-development.md)), and registration via
> `AddApiClient<TInterface>(baseAddress)` or attribute-based auto-discovery
> (`[ApiClientFor<TController,TEntity,TDto,TCreateDto,TUpdateDto,TRepository>]` +
> `AutoDiscoverApiClients`). The sections below have been updated to match.

## Context

Modern applications frequently need to consume HTTP APIs, particularly when:

- **Blazor Server** applications call separate Web APIs
- **Microservices** communicate with each other
- **Mobile apps** integrate with backend services
- **Desktop applications** connect to cloud services
- **Batch jobs** consume REST endpoints

Traditional approaches to HTTP API consumption present challenges:

- **Manual HttpClient Code** - Verbose, error-prone, repetitive
- **Boilerplate** - Serialization, deserialization, error handling duplicated
- **No Type Safety** - String-based URLs and dynamic types
- **Poor Discoverability** - No IntelliSense for API endpoints
- **Testing Difficulty** - Hard to mock HTTP calls
- **Maintenance Burden** - Changes to API require updating multiple places

We needed a solution that:
- Provides type-safe API client generation
- Reduces boilerplate code dramatically
- Integrates with ASP.NET Core dependency injection
- Supports authentication handlers (JWT, OAuth, etc.)
- Enables easy mocking for testing
- Follows REST conventions naturally
- Works seamlessly with JumpStart patterns

## Decision

We will use **Refit** as the primary HTTP client library with JumpStart-specific extension methods for streamlined registration.

### 1. Refit API Client Interfaces

Define strongly-typed interfaces decorated with Refit attributes:

```csharp
public interface IProductApiClient : IApiClient<ProductDto, CreateProductDto, UpdateProductDto>
{
    // Base CRUD operations inherited from IApiClient<TDto, TCreateDto, TUpdateDto>
    // - Task<ProductDto?> GetByIdAsync(Guid id)
    // - Task<PagedResult<ProductDto>> GetAllAsync(QueryOptions? options = null)
    // - Task<ProductDto> CreateAsync(CreateProductDto dto)
    // - Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto)
    // - Task<bool> DeleteAsync(Guid id)

    // Custom endpoints specific to products
    [Get("/featured")]
    Task<IEnumerable<ProductDto>> GetFeaturedAsync();

    [Get("/category/{categoryId}")]
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);

    [Post("/{id}/activate")]
    Task ActivateAsync(Guid id);
}
```

There is only one client base interface today - entities are Guid-only (see
[ADR-009](009-guid-only-entities.md)), so there is no separate "advanced/custom key type" client
interface.

### 2. Base API Client Interface

JumpStart provides a single base interface for standard CRUD operations
(`JumpStart.Api.Clients.IApiClient<TDto, TCreateDto, TUpdateDto>`):

```csharp
public interface IApiClient<TDto, TCreateDto, TUpdateDto>
    where TDto : EntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
{
    [Get("/{id}")]
    Task<TDto?> GetByIdAsync(Guid id);

    [Get("")]
    Task<PagedResult<TDto>> GetAllAsync([Query] QueryOptions? options = null);

    [Post("")]
    Task<TDto> CreateAsync([Body] TCreateDto createDto);

    [Put("/{id}")]
    Task<TDto> UpdateAsync(Guid id, [Body] TUpdateDto updateDto);

    [Delete("/{id}")]
    Task<bool> DeleteAsync(Guid id);
}
```

Pagination and sorting are folded into the single `GetAllAsync` call via
`JumpStart.Api.Clients.QueryOptions` (`PageNumber`, `PageSize`, `SortBy`, `SortDescending`), rather
than a separate `GetPagedAsync` endpoint.

### 3. JumpStart Extension Methods

Two registration paths exist today:

- **Manual:** `AddApiClient<TInterface>(baseAddress)` - explicit base URL per client.
- **Automatic:** decorate the client interface with
  `[ApiClientFor<TController,TEntity,TDto,TCreateDto,TUpdateDto,TRepository>]` and enable
  `AutoDiscoverApiClients` in `JumpStartOptions` - the framework derives the route from the
  controller's `[Route]` attribute and registers the Refit client for you. All six type arguments
  are required (`ApiClientForAttribute` has no shorter overload); they must match the target
  controller's own generic arguments exactly.

```csharp
// Manual registration
builder.Services.AddApiClient<IProductApiClient>("https://api.example.com/api/products")
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

// Automatic registration - attribute goes on the client interface itself
[ApiClientFor<ProductsController, Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>()]
public interface IProductApiClient : IApiClient<ProductDto, CreateProductDto, UpdateProductDto>
{
}

builder.Services.AddJumpStart(options =>
{
    options.AutoDiscoverApiClients = true;
    options.ApiBaseUrl = "https://api.example.com";
    options.ScanAssembly(typeof(IProductApiClient).Assembly);
});
```

### 4. API Controller Base

Refit-compatible endpoints are exposed by the framework's single
`ApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository>` (in
`JumpStart.Api.Controllers`) - there is no separate `SimpleApiControllerBase`. It already provides
`GetById`, `GetAll` (paged/sorted), `Create`, `Update`, and `Delete` with AutoMapper, structured
logging, and correlation IDs built in. See [API Development](../../api-development.md) for the
full constructor signature and examples; there's no need to hand-roll it as shown in earlier
drafts of this ADR.

### 5. Complete Example

**API Controller:**
```csharp
[Authorize]
[Route("api/[controller]")]
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

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeatured()
    {
        // Custom endpoint implementation
        var products = await _repository.GetFeaturedAsync();
        return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
    }
}
```

**Blazor Component:**
```csharp
@page "/products"
@inject IProductApiClient ProductApi

<h3>Products</h3>

@if (products == null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var product in products)
    {
        <div>@product.Name - @product.Price.ToString("C")</div>
    }
}

@code {
    private IEnumerable<ProductDto>? products;

    protected override async Task OnInitializedAsync()
    {
        // Type-safe, IntelliSense-supported API call
        products = await ProductApi.GetAllAsync();
    }
}
```

## Consequences

### Positive Consequences

- **Type Safety** - Compile-time checking of endpoints, parameters, return types
- **IntelliSense Support** - Full IDE support for API discovery
- **Minimal Boilerplate** - Interface definition is all you need
- **Automatic Serialization** - JSON handling built-in
- **DI Integration** - First-class ASP.NET Core DI support
- **HttpClient Best Practices** - Uses IHttpClientFactory under the hood
- **Easy Mocking** - Interface-based design perfect for testing
- **Authentication Integration** - Works seamlessly with DelegatingHandler
- **Error Handling** - ApiException with status codes and response content
- **Developer Productivity** - Write less code, focus on business logic
- **Consistency** - Uniform API client pattern across application
- **Maintainability** - Changes to API reflected in interface

### Negative Consequences

- **Library Dependency** - Another third-party dependency to manage
- **Learning Curve** - Developers must learn Refit attributes and conventions
- **Limited Flexibility** - Some advanced HttpClient scenarios harder to implement
- **Magic** - Code generation can be confusing for debugging
- **Breaking Changes** - Refit updates may introduce breaking changes
- **Attribute Overhead** - Must learn and apply correct Refit attributes

### Neutral Consequences

- **Interface Required** - Must define interface even for simple clients
- **Runtime Generation** - Client implementation generated at runtime via dynamic proxy
- **JSON Only** - Primarily designed for JSON APIs (can be extended for other formats)

## Alternatives Considered

### 1. Manual HttpClient

Write HttpClient code manually for each endpoint.

**Pros:**
- No additional dependencies
- Full control over HTTP behavior
- No "magic" code generation

**Cons:**
- Massive boilerplate
- Error-prone
- No type safety
- Poor maintainability
- Hard to test

**Why Rejected:** Too much boilerplate for minimal benefit.

### 2. HttpClientFactory with Extension Methods

Use IHttpClientFactory with custom extension methods for common patterns.

**Pros:**
- ASP.NET Core native
- More control than Refit
- No magic

**Cons:**
- Still requires significant boilerplate
- No type-safe contract
- Custom extensions to maintain

**Why Rejected:** Refit provides better developer experience.

### 3. NSwag Client Generation

Generate clients from OpenAPI/Swagger specification.

**Pros:**
- Auto-generated from API spec
- Includes models and clients
- Great for consuming external APIs

**Cons:**
- Build-time code generation
- Generated code in source control or build step
- Less flexible for modifications
- Requires OpenAPI specification

**Why Rejected:** Runtime generation (Refit) is more flexible. NSwag can still be used for external APIs.

### 4. RestSharp

Popular REST client library.

**Pros:**
- Mature and battle-tested
- Flexible
- Large community

**Cons:**
- More verbose than Refit
- No interface-based clients
- Manual request building
- Not as DI-friendly

**Why Rejected:** Refit provides better developer experience and type safety.

### 5. gRPC

Use gRPC instead of REST.

**Pros:**
- Better performance
- Strong typing
- Bidirectional streaming

**Cons:**
- Not REST (different paradigm)
- Requires Protocol Buffers
- Browser support limited
- More complex infrastructure

**Why Rejected:** REST is more universal and easier for web applications. gRPC can be adopted later if needed.

## Implementation Best Practices

### Error Handling

```csharp
public async Task<IEnumerable<ProductDto>> LoadProductsAsync()
{
    try
    {
        return await _productApi.GetAllAsync();
    }
    catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
    {
        // Handle unauthorized (redirect to login, refresh token, etc.)
        await HandleUnauthorizedAsync();
        return Enumerable.Empty<ProductDto>();
    }
    catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        // Handle not found
        return Enumerable.Empty<ProductDto>();
    }
    catch (ApiException ex)
    {
        // Handle other API errors
        _logger.LogError(ex, "API call failed: {StatusCode}", ex.StatusCode);
        throw;
    }
}
```

### Retry Policies with Polly

```csharp
builder.Services.AddApiClient<IProductApiClient>("https://api.example.com/api/products")
    .AddHttpMessageHandler<JwtAuthenticationHandler>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### Request/Response Logging

```csharp
public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request: {Method} {Uri}", request.Method, request.RequestUri);
        
        var response = await base.SendAsync(request, cancellationToken);
        
        _logger.LogInformation("Response: {StatusCode}", response.StatusCode);
        
        return response;
    }
}

// Register
builder.Services.AddApiClient<IProductApiClient>(baseUrl)
    .AddHttpMessageHandler<LoggingHandler>()
    .AddHttpMessageHandler<JwtAuthenticationHandler>();
```

### Testing with Mock

```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        // Arrange
        var mockApi = new Mock<IProductApiClient>();
        mockApi.Setup(x => x.GetAllAsync(It.IsAny<QueryOptions?>()))
            .ReturnsAsync(new PagedResult<ProductDto>
            {
                Items = new[] { new ProductDto { Id = Guid.NewGuid(), Name = "Product 1" } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 1
            });

        var service = new ProductService(mockApi.Object);

        // Act
        var result = await service.GetAllProductsAsync();

        // Assert
        Assert.Single(result.Items);
        mockApi.Verify(x => x.GetAllAsync(It.IsAny<QueryOptions?>()), Times.Once);
    }
}
```

## Refit Features Used

### HTTP Methods
- `[Get]` - GET requests
- `[Post]` - POST requests
- `[Put]` - PUT requests
- `[Delete]` - DELETE requests
- `[Patch]` - PATCH requests

### Parameters
- `[Body]` - Request body (JSON serialized)
- `[Query]` - Query string parameter
- `[Header]` - Request header
- Route parameters - `{id}` in path

### Return Types
- `Task<T>` - Deserialized response
- `Task<IApiResponse<T>>` - Full response with headers/status
- `Task` - No response body expected
- `Task<HttpResponseMessage>` - Raw response

## References

- [Refit GitHub Repository](https://github.com/reactiveui/refit)
- [Refit Documentation](https://github.com/reactiveui/refit#readme)
- [IHttpClientFactory - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [ADR-004: JWT Authentication](004-jwt-authentication.md)

## Related Documentation

- [How-To: Create API Clients](../../how-to/api-clients.md)
- [How-To: Test API Clients](../../how-to/test-api-clients.md)
- [API Reference: IApiClient](../../api/clients/iapiclient.md)
