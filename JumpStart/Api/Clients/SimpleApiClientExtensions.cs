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
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace JumpStart.Api.Clients;

/// <summary>
/// Extension methods for registering Refit-based Simple API clients (Guid-based entities) in the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide simplified registration for the common case of Guid-based entities.
/// They wrap the advanced API client registration with Guid as the key type.
/// </para>
/// <para>
/// For entities with custom key types (int, long, custom structs), use the Advanced namespace extensions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic registration
/// services.AddSimpleApiClient&lt;IProductApiClient&gt;("https://api.example.com/api/products");
/// 
/// // With custom configuration
/// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
///     "https://api.example.com/api/products",
///     client => client.Timeout = TimeSpan.FromSeconds(30));
/// </code>
/// </example>
public static class SimpleApiClientExtensions
{
    /// <summary>
    /// Registers a Refit-based API client for a Simple entity with Guid identifier.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The API client interface type that inherits from <see cref="ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
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
    /// This method is a convenience wrapper around AddAdvancedApiClient for Guid-based entities.
    /// It configures Refit with the same default settings:
    /// - System.Text.Json for serialization
    /// - Camel case property names
    /// - Ignores null values in JSON
    /// - Uses IHttpClientFactory for proper client lifecycle
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Basic registration
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products");
    /// 
    /// // Example 2: With retry policy
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddTransientHttpErrorPolicy(p => 
    ///         p.WaitAndRetryAsync(3, retryAttempt => 
    ///             TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    /// 
    /// // Example 3: With authentication
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddHttpMessageHandler&lt;AuthenticationHandler&gt;();
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
    /// The API client interface type that inherits from <see cref="ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
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
    /// This overload allows configuring the HttpClient during registration,
    /// which is useful for setting default headers, timeouts, or other client-level settings.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     client =>
    ///     {
    ///         client.Timeout = TimeSpan.FromSeconds(30);
    ///         client.DefaultRequestHeaders.Add("X-Api-Version", "2.0");
    ///         client.DefaultRequestHeaders.Add("User-Agent", "JumpStart/1.0");
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
    /// The API client interface type that inherits from <see cref="ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>.
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
    /// This overload provides the most flexibility by exposing the IHttpClientBuilder
    /// for comprehensive configuration including resilience patterns, authentication,
    /// logging, and custom message handlers.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddSimpleApiClient&lt;IProductApiClient&gt;(
    ///     "https://api.example.com/api/products",
    ///     builder =>
    ///     {
    ///         // Add retry policy
    ///         builder.AddTransientHttpErrorPolicy(p => 
    ///             p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));
    ///         
    ///         // Add circuit breaker
    ///         builder.AddTransientHttpErrorPolicy(p =>
    ///             p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
    ///         
    ///         // Add custom handler
    ///         builder.AddHttpMessageHandler&lt;LoggingHandler&gt;();
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
