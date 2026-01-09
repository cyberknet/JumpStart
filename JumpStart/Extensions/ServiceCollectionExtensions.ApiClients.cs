using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering JumpStart API clients.
/// </summary>
public static class JumpStartApiClientExtensions
{
    /// <summary>
    /// Adds HttpClient services configured for JumpStart API clients.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureHttpClient">Action to configure the HttpClient (e.g., set base address).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJumpStartApiClients(
        this IServiceCollection services,
        Action<HttpClient>? configureHttpClient = null)
    {
        // Register named HttpClient for JumpStart API calls
        var builder = services.AddHttpClient("JumpStartApi");
        
        if (configureHttpClient != null)
        {
            builder.ConfigureHttpClient(configureHttpClient);
        }

        return services;
    }

    /// <summary>
    /// Registers a Simple API client with the service collection.
    /// </summary>
    /// <typeparam name="TClient">The API client type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientName">The name of the HttpClient to use (default: "JumpStartApi").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSimpleApiClient<TClient>(
        this IServiceCollection services,
        string httpClientName = "JumpStartApi")
        where TClient : class
    {
        services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(httpClientName);
            return (TClient)Activator.CreateInstance(typeof(TClient), httpClient)!;
        });

        return services;
    }
}
