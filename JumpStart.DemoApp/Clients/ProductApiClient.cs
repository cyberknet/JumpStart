using JumpStart.Api.Clients;
using JumpStart.DemoApp.DTOs;
using System.Net.Http.Json;

namespace JumpStart.DemoApp.Clients;

/// <summary>
/// API client interface for product operations using DTOs.
/// </summary>
public interface IProductApiClient : ISimpleApiClient<ProductDto, CreateProductDto, UpdateProductDto>
{
    Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold = 10);
    Task<IEnumerable<ProductDto>> GetActiveAsync();
}

/// <summary>
/// API client for product operations using DTOs.
/// Calls the ProductsController API endpoints.
/// </summary>
public class ProductApiClient : SimpleApiClientBase<ProductDto, CreateProductDto, UpdateProductDto>, 
                                 IProductApiClient
{
    public ProductApiClient(HttpClient httpClient) 
        : base(httpClient, "api/products")
    {
    }

    public async Task<IEnumerable<ProductDto>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/by-price-range?minPrice={minPrice}&maxPrice={maxPrice}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>()
            ?? Array.Empty<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold = 10)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/low-stock?threshold={threshold}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>()
            ?? Array.Empty<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetActiveAsync()
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/active");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>()
            ?? Array.Empty<ProductDto>();
    }
}
