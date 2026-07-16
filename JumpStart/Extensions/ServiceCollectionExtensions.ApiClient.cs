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

using JumpStart;
using JumpStart.Api.Clients;
using JumpStart.Api.Controllers;
using JumpStart.Api.DTOs;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing API client registration extension methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Discovers and registers Refit API client interfaces from specified assemblies.
    /// Scans for interfaces derived from <see cref="JumpStart.Api.Clients.IApiClient{TDto, TCreateDto, TUpdateDto}"/>
    /// that are decorated with <see cref="JumpStart.Api.Controllers.ApiClientForAttribute{TController, TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/>,
    /// derives each one's base address from its target controller's <c>[Route]</c>, and registers a
    /// Refit client for it.
    /// </summary>
    /// <param name="services">The service collection to add API clients to.</param>
    /// <param name="options">The JumpStart options containing assembly list and lifetime settings.</param>
    /// <remarks>
    /// Also attaches <see cref="Services.Authentication.JwtExchangeHandler"/> and
    /// <see cref="Services.Authentication.JwtAuthenticationHandler"/> to each registered client
    /// when <see cref="CanAttachJwtExchangeHandlers"/> determines their prerequisites are already
    /// registered - see ADR-013/ADR-014.
    /// </remarks>
    private static void RegisterApiClients(IServiceCollection services, JumpStartOptions options)
    {
        var assemblies = options.Assemblies.Any()
            ? options.Assemblies.ToArray()
            : new[] { Assembly.GetCallingAssembly() };

        var apiClientInterfaces = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsInterface && IsCustomApiClientInterface(t))
            .ToList();

        foreach (var apiClientInterface in apiClientInterfaces)
        {
            var apiClientForAttr = apiClientInterface.GetCustomAttributes(false)
                .FirstOrDefault(a => a.GetType().IsGenericType && a.GetType().GetGenericTypeDefinition() == typeof(ApiClientForAttribute<,,,,,>));

            if (apiClientForAttr == null)
            {
                throw new InvalidOperationException(
                    $"API client interface '{apiClientInterface.FullName}' must be decorated with " +
                    "[ApiClientFor<TController,TEntity,TDto,TCreateDto,TUpdateDto,TRepository>], matching the " +
                    "target controller's own generic arguments. This is required for automatic route discovery and registration.");
            }

            var controllerType = apiClientForAttr.GetType().GetGenericArguments()[0];
            var routeAttr = GetRouteAttribute<RouteAttribute>(controllerType);
            if (routeAttr == null)
            {
                throw new InvalidOperationException(
                    $"Controller '{controllerType.FullName}' must be decorated with [Route] for API client registration.");
            }

            var controllerName = controllerType.Name;
            if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            var controllerRoute = routeAttr.Template.Replace("[controller]", controllerName.ToLowerInvariant());
            var baseUrl = options.ApiBaseUrl?.TrimEnd('/') ?? "";
            var fullBaseAddress = baseUrl + "/" + controllerRoute.TrimStart('/');

            // register the refit client if not already registered
            if (!services.Any(sd => sd.ServiceType == apiClientInterface))
            {
                var builder = services.AddRefitClient(apiClientInterface)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(fullBaseAddress));

                // Auto-attach JumpStart's prescribed JWT exchange flow (see ADR-013/ADR-014) when
                // its prerequisites are already registered - no separate opt-in flag, the presence
                // of these four services is the signal.
                if (CanAttachJwtExchangeHandlers(services))
                {
                    EnsureCircuitServicesAccessorRegistered(services);
                    builder.AddHttpMessageHandler<JwtExchangeHandler>()
                        .AddHttpMessageHandler<JwtAuthenticationHandler>();
                }
            }
        }
    }

    /// <summary>
    /// Determines whether <see cref="JwtExchangeHandler"/> and <see cref="JwtAuthenticationHandler"/>
    /// can be attached to an auto-discovered API client - i.e. whether all of
    /// <see cref="JwtExchangeHandler"/>'s dependencies are already registered. See ADR-014.
    /// </summary>
    private static bool CanAttachJwtExchangeHandlers(IServiceCollection services) =>
        services.Any(sd => sd.ServiceType == typeof(AuthenticationStateProvider)) &&
        services.Any(sd => sd.ServiceType == typeof(ITokenStore)) &&
        services.Any(sd => sd.ServiceType == typeof(IJwtTokenService)) &&
        services.Any(sd => sd.ServiceType == typeof(ITokenExchangeApiClient));

    /// <summary>
    /// Registers <see cref="CircuitServicesAccessor"/> and <see cref="ServicesAccessorCircuitHandler"/>
    /// if not already present - required by <see cref="JwtExchangeHandler"/> to reach the current
    /// circuit's <see cref="AuthenticationStateProvider"/> from a message handler that
    /// <see cref="System.Net.Http.IHttpClientFactory"/> constructs in a separate DI scope. See
    /// ADR-013's "Correction" note.
    /// </summary>
    private static void EnsureCircuitServicesAccessorRegistered(IServiceCollection services)
    {
        services.TryAddScoped<CircuitServicesAccessor>();

        if (!services.Any(sd => sd.ServiceType == typeof(CircuitHandler) && sd.ImplementationType == typeof(ServicesAccessorCircuitHandler)))
        {
            services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>();
        }
    }

    private static TAttribute? GetRouteAttribute<TAttribute>(Type type) where TAttribute : Attribute
    {
        var check = type;
        TAttribute? attribute = null;
        while (attribute == null && check != null)
        {
            attribute = check.GetCustomAttribute<TAttribute>(false);
            if (attribute == null && check.BaseType != null)
                check = check.BaseType;
        }

        return attribute;
    }

    /// <summary>
    /// Determines if a type is a recognized JumpStart API client interface.
    /// Checks for <see cref="JumpStart.Api.Clients.IApiClient{TDto, TCreateDto, TUpdateDto}"/> - the
    /// framework's only API client base interface (entities are Guid-only).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is an API client interface; otherwise, <c>false</c>.</returns>
    private static bool IsApiClientInterface(Type type) =>
        IsBaseInterface(type, typeof(IApiClient<,,>));

    /// <summary>
    /// Determines if a type is a custom API client interface that inherits from a JumpStart API client interface.
    /// This catches interfaces like IProductApiClient that extend IApiClient{ProductDto, CreateProductDto, UpdateProductDto}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a custom API client interface; otherwise, <c>false</c>.</returns>
    private static bool IsCustomApiClientInterface(Type type) =>
        IsCustomInterface(type, IsApiClientInterface);

    /// <summary>
    /// Registers a Refit-based API client for a Guid-keyed entity.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The API client interface type that inherits from <see cref="JumpStart.Api.Clients.IApiClient{TDto, TCreateDto, TUpdateDto}"/>.
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
    /// Chain with <c>.AddHttpMessageHandler&lt;TAttribute&gt;()</c> to add authentication handlers that
    /// inject JWT tokens or other credentials into requests.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example 1: Basic registration in Blazor Program.cs
    /// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";
    /// builder.Services.AddApiClient&lt;JumpStart.Api.Clients.IProductApiClient&gt;(
    ///     $"{apiBaseUrl}/api/products");
    ///
    /// // Example 2: With JWT authentication handler
    /// builder.Services.AddApiClient&lt;JumpStart.Api.Clients.IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
    ///
    /// // Example 3: With retry policy using Polly
    /// builder.Services.AddApiClient&lt;JumpStart.Api.Clients.IProductApiClient&gt;(
    ///     "https://api.example.com/api/products")
    ///     .AddTransientHttpErrorPolicy(p =&gt; 
    ///         p.WaitAndRetryAsync(3, retryAttempt =&gt; 
    ///             TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    ///
    /// // Example 4: Use in Blazor component
    /// // @inject JumpStart.Api.Clients.IProductApiClient ProductClient
    /// // var products = await ProductClient.GetAllAsync();
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddApiClient<TInterface>(
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
}
