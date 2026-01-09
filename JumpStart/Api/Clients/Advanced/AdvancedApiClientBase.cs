using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Repositories;

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Provides an abstract base implementation for API clients that communicate with DTO-based HTTP endpoints
/// for entities with custom key types.
/// This class implements the <see cref="IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}"/> interface
/// and handles common HTTP operations using DTOs.
/// </summary>
/// <typeparam name="TDto">The data transfer object type for read operations. Must inherit from <see cref="EntityDto{TKey}"/>.</typeparam>
/// <typeparam name="TCreateDto">The data transfer object type for create operations. Must implement <see cref="ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The data transfer object type for update operations. Must implement <see cref="IUpdateDto{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key. Must be a value type (struct).</typeparam>
/// <remarks>
/// <para>
/// This abstract base class provides a complete implementation of standard CRUD operations
/// via HTTP/REST APIs. Derived classes need only provide the HttpClient and base endpoint URL
/// in the constructor and can add custom operations as needed.
/// </para>
/// <para>
/// Key features:
/// - Automatic JSON serialization/deserialization using System.Text.Json
/// - Consistent error handling across all operations
/// - Query string building for pagination and sorting
/// - Null-safe responses with appropriate default values
/// - HTTP status code interpretation (404 returns null, others throw)
/// </para>
/// <para>
/// HTTP Method Mapping:
/// - GET /{id} ? GetByIdAsync
/// - GET / ? GetAllAsync (with optional query parameters)
/// - POST / ? CreateAsync
/// - PUT /{id} ? UpdateAsync
/// - DELETE /{id} ? DeleteAsync
/// </para>
/// <para>
/// This class is part of the Advanced namespace and supports custom key types.
/// For Guid-based applications, use <see cref="Clients.SimpleApiClientBase{TDto, TCreateDto, TUpdateDto}"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic Usage with Integer Keys</strong></para>
/// <code>
/// public class ProductApiClient : AdvancedApiClientBase&lt;ProductDto, CreateProductDto, UpdateProductDto, int&gt;
/// {
///     public ProductApiClient(HttpClient httpClient) 
///         : base(httpClient, "api/products")
///     {
///     }
///     
///     // Inherited methods available:
///     // - GetByIdAsync(int id)
///     // - GetAllAsync(QueryOptions options)
///     // - CreateAsync(CreateProductDto dto)
///     // - UpdateAsync(UpdateProductDto dto)
///     // - DeleteAsync(int id)
/// }
/// </code>
/// <para><strong>Example 2: Adding Custom Operations</strong></para>
/// <code>
/// public class ProductApiClient : AdvancedApiClientBase&lt;ProductDto, CreateProductDto, UpdateProductDto, int&gt;
/// {
///     public ProductApiClient(HttpClient httpClient) 
///         : base(httpClient, "api/products") { }
///     
///     // Custom operation for business-specific queries
///     public async Task&lt;IEnumerable&lt;ProductDto&gt;&gt; GetByPriceRangeAsync(decimal min, decimal max)
///     {
///         var response = await HttpClient.GetAsync(
///             $"{BaseEndpoint}/by-price-range?min={min}&amp;max={max}");
///         response.EnsureSuccessStatusCode();
///         return await response.Content.ReadFromJsonAsync&lt;IEnumerable&lt;ProductDto&gt;&gt;()
///             ?? Array.Empty&lt;ProductDto&gt;();
///     }
/// }
/// </code>
/// <para><strong>Example 3: Using in a Blazor Component</strong></para>
/// <code>
/// @inject IProductApiClient ProductClient
/// 
/// @code {
///     private ProductDto? product;
///     
///     protected override async Task OnInitializedAsync()
///     {
///         // Get a single product
///         product = await ProductClient.GetByIdAsync(123);
///         
///         // Get paginated list
///         var options = new QueryOptions { PageNumber = 1, PageSize = 20 };
///         var page = await ProductClient.GetAllAsync(options);
///         
///         // Create new product
///         var createDto = new CreateProductDto { Name = "New Product" };
///         var created = await ProductClient.CreateAsync(createDto);
///         
///         // Update product
///         var updateDto = new UpdateProductDto { Id = 123, Name = "Updated" };
///         var updated = await ProductClient.UpdateAsync(updateDto);
///         
///         // Delete product
///         await ProductClient.DeleteAsync(123);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}"/>
/// <seealso cref="Clients.SimpleApiClientBase{TDto, TCreateDto, TUpdateDto}"/>
/// <seealso cref="Controllers.Advanced.AdvancedApiControllerBase{TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository}"/>
public abstract class AdvancedApiClientBase<TDto, TCreateDto, TUpdateDto, TKey> 
    : IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, TKey>
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
    where TKey : struct
{
    /// <summary>
    /// The HTTP client used for making API requests.
    /// This client is configured with base address and default headers.
    /// </summary>
    /// <remarks>
    /// The HttpClient should be obtained from IHttpClientFactory and configured
    /// with appropriate base address, timeout, and authentication handlers.
    /// </remarks>
    protected readonly HttpClient HttpClient;

    /// <summary>
    /// The base endpoint path for this API client (e.g., "api/products").
    /// All HTTP requests are made relative to this endpoint.
    /// </summary>
    /// <value>
    /// A string representing the base API endpoint path without leading or trailing slashes.
    /// </value>
    /// <remarks>
    /// Trailing slashes are automatically removed during construction to ensure consistent URL formatting.
    /// </remarks>
    protected readonly string BaseEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedApiClientBase{TDto, TCreateDto, TUpdateDto, TKey}"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client to use for API requests. Should be obtained from IHttpClientFactory.
    /// Must not be null.
    /// </param>
    /// <param name="baseEndpoint">
    /// The base endpoint path for API requests (e.g., "api/products").
    /// Trailing slashes are automatically removed.
    /// Must not be null or empty.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpClient"/> or <paramref name="baseEndpoint"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The HttpClient should be configured with:
    /// - Base address (if not using absolute URLs)
    /// - Authentication headers (if required)
    /// - Timeout settings appropriate for the API
    /// - Custom message handlers (logging, retry policies, etc.)
    /// </para>
    /// <para>
    /// The baseEndpoint parameter defines the root path for all CRUD operations.
    /// Individual operation paths are appended to this base.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ProductApiClient : AdvancedApiClientBase&lt;ProductDto, CreateProductDto, UpdateProductDto, int&gt;
    /// {
    ///     public ProductApiClient(HttpClient httpClient) 
    ///         : base(httpClient, "api/products")
    ///     {
    ///         // HttpClient is configured via dependency injection
    ///         // BaseEndpoint is now "api/products" (trailing slash removed)
    ///     }
    /// }
    /// </code>
    /// </example>
    protected AdvancedApiClientBase(HttpClient httpClient, string baseEndpoint)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        BaseEndpoint = baseEndpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseEndpoint));
    }

    /// <inheritdoc />
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request fails with a non-success status code (except 404).
    /// </exception>
    public virtual async Task<TDto?> GetByIdAsync(TKey id)
    {
        // Construct the full URL by combining the base endpoint with the entity ID
        // Example: "api/products/123" for a product with ID 123
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/{id}");

        // Handle 404 Not Found as a special case - return null to indicate the entity doesn't exist
        // This allows calling code to distinguish between "not found" and "error"
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        // For all other non-success status codes (400, 401, 403, 500, etc.), throw an exception
        // This includes validation errors, authorization failures, and server errors
        response.EnsureSuccessStatusCode();

        // Deserialize the JSON response body into the DTO type
        // Returns null if the response body is empty or cannot be deserialized
        return await response.Content.ReadFromJsonAsync<TDto>();
    }

    /// <inheritdoc />
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request fails with a non-success status code.
    /// </exception>
    public virtual async Task<PagedResult<TDto>> GetAllAsync(QueryOptions? options = null)
    {
        // Build query string from pagination and sorting options
        // Example results: "?pageNumber=1&pageSize=20" or "" if no options
        var queryString = BuildQueryString(options);

        // Construct the full URL by appending the query string to the base endpoint
        // Example: "api/products?pageNumber=1&pageSize=20"
        var response = await HttpClient.GetAsync($"{BaseEndpoint}{queryString}");

        // Throw for non-success status codes (400, 401, 403, 500, etc.)
        // This ensures any API errors are propagated to the caller
        response.EnsureSuccessStatusCode();

        // Deserialize the JSON response into a PagedResult object
        // If deserialization fails or returns null, create an empty result to prevent null reference exceptions
        // This ensures calling code always receives a valid PagedResult object
        return await response.Content.ReadFromJsonAsync<PagedResult<TDto>>()
            ?? new PagedResult<TDto> { Items = new List<TDto>(), TotalCount = 0 };
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="createDto"/> is null.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request fails with a non-success status code.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server response cannot be deserialized to <typeparamref name="TDto"/>.
    /// </exception>
    public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
    {
        // Send HTTP POST request to the base endpoint with the createDto serialized as JSON in the request body
        // The HttpClient automatically serializes the object to JSON with appropriate content-type headers
        var response = await HttpClient.PostAsJsonAsync(BaseEndpoint, createDto);

        // Validate the response status code
        // Throws HttpRequestException for non-success codes like:
        // - 400 Bad Request (validation errors)
        // - 401 Unauthorized (authentication required)
        // - 403 Forbidden (insufficient permissions)
        // - 500 Internal Server Error (server-side failures)
        response.EnsureSuccessStatusCode();

        // Deserialize the JSON response body into the full DTO type
        // The response includes server-generated fields (Id, audit timestamps, etc.)
        // Throw InvalidOperationException if deserialization fails or returns null
        // This indicates a protocol violation where the server didn't return the expected data
        return await response.Content.ReadFromJsonAsync<TDto>()
            ?? throw new InvalidOperationException("Failed to deserialize created entity");
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="updateDto"/> is null.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request fails with a non-success status code.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server response cannot be deserialized to <typeparamref name="TDto"/>.
    /// </exception>
    public virtual async Task<TDto> UpdateAsync(TUpdateDto updateDto)
    {
        // Send HTTP PUT request to the entity-specific endpoint with the updateDto serialized as JSON
        // The URL includes the entity ID to identify which resource to update
        // Example: PUT api/products/123 with the product data in the request body
        var response = await HttpClient.PutAsJsonAsync($"{BaseEndpoint}/{updateDto.Id}", updateDto);

        // Validate the response status code
        // Throws HttpRequestException for non-success codes like:
        // - 400 Bad Request (validation errors or ID mismatch)
        // - 404 Not Found (entity doesn't exist)
        // - 409 Conflict (concurrency conflict with optimistic locking)
        // - 500 Internal Server Error (server-side failures)
        response.EnsureSuccessStatusCode();

        // Deserialize the JSON response body into the full DTO type
        // The response includes all current field values including server-updated audit timestamps
        // Throw InvalidOperationException if deserialization fails or returns null
        // This indicates a protocol violation where the server didn't return the expected data
        return await response.Content.ReadFromJsonAsync<TDto>()
            ?? throw new InvalidOperationException("Failed to deserialize updated entity");
    }

    /// <inheritdoc />
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request fails with a non-success status code (except 404).
    /// </exception>
    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        // Send HTTP DELETE request to the entity-specific endpoint
        // Example: DELETE api/products/123
        // This may perform either a soft delete (sets DeletedOn/DeletedById) or hard delete
        // depending on whether the entity implements IDeletable
        var response = await HttpClient.DeleteAsync($"{BaseEndpoint}/{id}");

        // Handle 404 Not Found as a special case - return false to indicate the entity doesn't exist
        // This could mean the entity was already deleted or never existed
        // Calling code can distinguish between "not found" and "error" by checking the return value
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        // For all other non-success status codes (400, 401, 403, 500, etc.), throw an exception
        // This includes authorization failures, validation errors, and server errors
        response.EnsureSuccessStatusCode();

        // Return true to indicate successful deletion
        // The API typically returns 204 No Content for successful DELETE operations
        return true;
    }

    /// <summary>
    /// Builds a query string from the provided query options.
    /// Constructs URL parameters for pagination and sorting.
    /// </summary>
    /// <param name="options">
    /// The query options containing pagination and sorting parameters.
    /// Can be null to return no query string.
    /// </param>
    /// <returns>
    /// A query string starting with "?" if parameters exist, or an empty string if no parameters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method constructs query strings in the format: ?pageNumber=1&amp;pageSize=20&amp;sortDescending=true
    /// </para>
    /// <para>
    /// Parameters are only included if they have values:
    /// - pageNumber: Included if PageNumber.HasValue is true
    /// - pageSize: Included if PageSize.HasValue is true
    /// - sortDescending: Only included if true (false is omitted as it's the default)
    /// </para>
    /// <para>
    /// The method is virtual to allow derived classes to add custom query parameters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Full options
    /// var options = new QueryOptions { PageNumber = 2, PageSize = 50, SortDescending = true };
    /// var query = BuildQueryString(options);
    /// // Result: "?pageNumber=2&amp;pageSize=50&amp;sortDescending=true"
    /// 
    /// // Example 2: Partial options
    /// var options = new QueryOptions { PageNumber = 1 };
    /// var query = BuildQueryString(options);
    /// // Result: "?pageNumber=1"
    /// 
    /// // Example 3: Null options
    /// var query = BuildQueryString(null);
    /// // Result: ""
    /// </code>
    /// </example>
    protected virtual string BuildQueryString(QueryOptions? options)
    {
        // Return empty string if no options provided - this will result in a request without query parameters
        // The API will use its default pagination/sorting behavior
        if (options == null)
            return string.Empty;

        // Build a list of URL-encoded query parameters
        // Each parameter will be in the format: name=value
        var parameters = new List<string>();

        // Add pageNumber parameter if it has a value
        // Example: "pageNumber=2" for the second page
        // Nullable allows distinguishing between "not specified" and "page 0"
        if (options.PageNumber.HasValue)
            parameters.Add($"pageNumber={options.PageNumber.Value}");

        // Add pageSize parameter if it has a value
        // Example: "pageSize=50" to request 50 items per page
        // Nullable allows the API to use its default page size when not specified
        if (options.PageSize.HasValue)
            parameters.Add($"pageSize={options.PageSize.Value}");

        // Add sortDescending parameter only when true
        // We omit the parameter when false since ascending sort is the default
        // This keeps URLs shorter and cleaner for the common case
        if (options.SortDescending)
            parameters.Add("sortDescending=true");

                // Combine all parameters with & separators and prefix with ?
                // Example result: "?pageNumber=2&pageSize=50&sortDescending=true"
                // Return empty string if no parameters were added
                return parameters.Count > 0 ? "?" + string.Join("&", parameters) : string.Empty;
            }
        }
