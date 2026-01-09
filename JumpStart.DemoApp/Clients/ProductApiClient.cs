using JumpStart.Api.Clients;
using JumpStart.DemoApp.Data;
using System.Net.Http.Json;

namespace JumpStart.DemoApp.Clients;

/// <summary>
/// API client for product operations.
/// Calls the ProductsController API endpoints.
/// </summary>
public interface IProductApiClient : ISimpleApiClient<Product>
{
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10);
    Task<IEnumerable<Product>> GetActiveAsync();
}

public class ProductApiClient : SimpleApiClientBase<Product>, IProductApiClient
{
    public ProductApiClient(HttpClient httpClient) 
        : base(httpClient, "api/products")
    {
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/by-price-range?minPrice={minPrice}&maxPrice={maxPrice}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<Product>>()
            ?? Array.Empty<Product>();
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/low-stock?threshold={threshold}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<Product>>()
            ?? Array.Empty<Product>();
    }

    public async Task<IEnumerable<Product>> GetActiveAsync()
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/active");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<Product>>()
            ?? Array.Empty<Product>();
    }
}
