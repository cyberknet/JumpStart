// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>. 
 */

using System;
using JumpStart;
using JumpStart.Api.Clients;
using Refit;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing API client registration extension methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Discovers and registers API client implementations from specified assemblies.
    /// Scans for classes implementing ISimpleApiClient or IAdvancedApiClient interfaces.
    /// Registers the concrete class and all API client-related interfaces it implements.
    /// </summary>
    /// <param name="services">The service collection to add API clients to.</param>
    /// <param name="options">The JumpStart options containing assembly list and lifetime settings.</param>
    private static void RegisterApiClients(IServiceCollection services, JumpStartOptions options)
    {
        RegisterServicesByInterface(
            services,
            options,
            IsApiClientInterface,
            IsCustomApiClientInterface,
            options.ApiClientLifetime);
    }

    /// <summary>
    /// Determines if a type is a recognized JumpStart API client interface.
    /// Checks for ISimpleApiClient{TDto, TCreateDto, TUpdateDto} or IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is an API client interface; otherwise, <c>false</c>.</returns>
    private static bool IsApiClientInterface(Type type) =>
        IsBaseInterface(type, typeof(ISimpleApiClient<,,>), typeof(JumpStart.Api.Clients.Advanced.IAdvancedApiClient<,,,>));

    /// <summary>
    /// Determines if a type is a custom API client interface that inherits from a JumpStart API client interface.
    /// This catches interfaces like IProductApiClient that extend ISimpleApiClient{ProductDto, CreateProductDto, UpdateProductDto}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a custom API client interface; otherwise, <c>false</c>.</returns>
    private static bool IsCustomApiClientInterface(Type type) =>
        IsCustomInterface(type, IsApiClientInterface);

    /// <summary>
    /// Registers a Refit-based API client for a Simple entity with Guid identifier.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The API client interface type that inherits from <see cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
    /// Must be an interface decorated with Refit attributes.
    /// </typeparam>
    /// <param name="services">The service collection to register the client in.</param>
    /// <param name="baseAddress">
    /// The base address of the API endpoint (e.g., "https://api.example.com/api/products").
    /// All API calls will be relative to this address.
    /// </param>
    /// <returns>
    /// An <see cref="IHttpClientBuilder"/> that can be used to configure the underlying HttpClient,
    /// add policies, handlers, or additional configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="baseAddress"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="baseAddress"/> is not a valid URI.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method configures Refit with the following default settings:
    /// - System.Text.Json for serialization
    /// - Camel case property names
    /// - Ignores null values in JSON
    /// - Uses IHttpClientFactory for proper client lifecycle management
    /// </para>
    /// <para>
    /// <strong>Typical Usage in Blazor Server:</strong>
    /// Register API clients in Program.cs to call a separate API project. The client is registered
    /// with Scoped lifetime, making it suitable for use in Blazor components with @inject.
    /// </para>
    /// <para>
    /// <strong>Authentication:</strong>
    /// Chain with <c>.AddHttpMessageHandler&lt;T&gt;()</c> to add authentication handlers that
    /// inject JWT tokens or other credentials into requests.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Basic registration in Blazor Program.cs
    /// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";
    /// builder.Services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     $"{apiBaseUrl}/api/products");
    /// 
    /// // Example 2: With JWT authentication handler
    /// builder.Services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
    /// 
    /// // Example 3: With retry policy using Polly
    /// builder.Services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddTransientHttpErrorPolicy(p => 
    ///         p.WaitAndRetryAsync(3, retryAttempt => 
    ///             TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    /// 
    /// // Example 4: Use in Blazor component
    /// // @inject IProductApiClient ProductClient
    /// // var products = await ProductClient.GetAllAsync();
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSimpleApiClient<TInterface>(
        this IServiceCollection services,
        string baseAddress)
        where TInterface : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentNullException(nameof(baseAddress));

        // Validate that the baseAddress is a valid URI
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseUri))
            throw new ArgumentException($"Invalid base address: {baseAddress}", nameof(baseAddress));

        // Register the Refit client with System.Text.Json serialization
        return services.AddRefitClient<TInterface>()
            .ConfigureHttpClient(c => c.BaseAddress = baseUri);
    }

    /// <summary>
    /// Registers a Refit-based API client with additional HTTP client configuration.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The API client interface type that inherits from <see cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="services">The service collection to register the client in.</param>
    /// <param name="baseAddress">The base address of the API endpoint.</param>
    /// <param name="configureClient">
    /// An optional action to configure the HttpClient with additional settings
    /// such as default headers, timeout, max response buffer size, etc.
    /// </param>
    /// <returns>
    /// An <see cref="IHttpClientBuilder"/> for further configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="baseAddress"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload allows configuring the HttpClient during registration,
    /// which is useful for setting default headers, timeouts, or other client-level settings.
    /// </para>
    /// <para>
    /// <strong>Common Configurations:</strong>
    /// - Timeout: Set request timeout different from default (100 seconds)
    /// - Default headers: Add API keys, version headers, user agent
    /// - Max response buffer: Control memory usage for large responses
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Configure timeout and headers
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     client =>
    ///     {
    ///         client.Timeout = TimeSpan.FromSeconds(30);
    ///         client.DefaultRequestHeaders.Add("X-Api-Version", "2.0");
    ///         client.DefaultRequestHeaders.Add("User-Agent", "JumpStart/1.0");
    ///     });
    /// 
    /// // Example 2: Add API key header
    /// var apiKey = builder.Configuration["ApiKey"];
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     client =>
    ///     {
    ///         client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    ///     });
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSimpleApiClient<TInterface>(
        this IServiceCollection services,
        string baseAddress,
        Action<HttpClient>? configureClient)
        where TInterface : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentNullException(nameof(baseAddress));

        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseUri))
            throw new ArgumentException($"Invalid base address: {baseAddress}", nameof(baseAddress));

        return services.AddRefitClient<TInterface>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = baseUri;
                configureClient?.Invoke(c);
            });
    }

    /// <summary>
    /// Registers a Refit-based API client with full HTTP client builder configuration.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The API client interface type that inherits from <see cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="services">The service collection to register the client in.</param>
    /// <param name="baseAddress">The base address of the API endpoint.</param>
    /// <param name="configureBuilder">
    /// An action to configure the IHttpClientBuilder for advanced scenarios
    /// such as adding policies, handlers, or configuring named clients.
    /// </param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/>, <paramref name="baseAddress"/>, 
    /// or <paramref name="configureBuilder"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload provides the most flexibility by exposing the IHttpClientBuilder
    /// for comprehensive configuration including:
    /// - Resilience patterns (retry, circuit breaker, timeout) via Polly
    /// - Custom message handlers (authentication, logging, caching)
    /// - Request/response pipeline customization
    /// - Named client configuration
    /// </para>
    /// <para>
    /// <strong>Resilience Patterns:</strong>
    /// Use Polly policies to handle transient failures, implement circuit breakers,
    /// and add timeouts for robust API communication.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Complete resilience configuration
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     builder =>
    ///     {
    ///         // Retry policy with exponential backoff
    ///         builder.AddTransientHttpErrorPolicy(p => 
    ///             p.WaitAndRetryAsync(3, retryAttempt => 
    ///                 TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    ///         
    ///         // Circuit breaker after 5 failures for 30 seconds
    ///         builder.AddTransientHttpErrorPolicy(p =>
    ///             p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
    ///         
    ///         // Timeout policy
    ///         builder.AddPolicyHandler(Policy.TimeoutAsync&lt;HttpResponseMessage&gt;(
    ///             TimeSpan.FromSeconds(10)));
    ///     });
    /// 
    /// // Example 2: Add multiple handlers
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     builder =>
    ///     {
    ///         builder.AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
    ///         builder.AddHttpMessageHandler&lt;LoggingHandler&gt;();
    ///         builder.AddHttpMessageHandler&lt;CachingHandler&gt;();
    ///     });
    /// 
    /// // Example 3: Real-world Blazor Server configuration
    /// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";
    /// builder.Services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     $"{apiBaseUrl}/api/products",
    ///     clientBuilder =>
    ///     {
    ///         // Add JWT authentication
    ///         clientBuilder.AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
    ///         
    ///         // Add retry for transient failures
    ///         clientBuilder.AddTransientHttpErrorPolicy(p => 
    ///             p.WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1)));
    ///     });
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSimpleApiClient<TInterface>(
        this IServiceCollection services,
        string baseAddress,
        Action<IHttpClientBuilder> configureBuilder)
        where TInterface : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentNullException(nameof(baseAddress));
        
        if (configureBuilder == null)
            throw new ArgumentNullException(nameof(configureBuilder));

        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseUri))
            throw new ArgumentException($"Invalid base address: {baseAddress}", nameof(baseAddress));

        var builder = services.AddRefitClient<TInterface>()
            .ConfigureHttpClient(c => c.BaseAddress = baseUri);

        configureBuilder(builder);

        return builder;
    }
}
