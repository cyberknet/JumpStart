using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Client interface for interacting with DTO-based API endpoints for entities with custom key types.
/// </summary>
public interface IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, TKey>
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
    where TKey : struct
{
    Task<TDto?> GetByIdAsync(TKey id);
    Task<PagedResult<TDto>> GetAllAsync(QueryOptions? options = null);
    Task<TDto> CreateAsync(TCreateDto createDto);
    Task<TDto> UpdateAsync(TUpdateDto updateDto);
    Task<bool> DeleteAsync(TKey id);
}

/// <summary>
/// Base implementation of API client with DTOs for entities with custom key types.
/// </summary>
public abstract class AdvancedApiClientBase<TDto, TCreateDto, TUpdateDto, TKey> 
    : IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, TKey>
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
    where TKey : struct
{
    protected readonly HttpClient HttpClient;
    protected readonly string BaseEndpoint;

    protected AdvancedApiClientBase(HttpClient httpClient, string baseEndpoint)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        BaseEndpoint = baseEndpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseEndpoint));
    }

    public virtual async Task<TDto?> GetByIdAsync(TKey id)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TDto>();
    }

    public virtual async Task<PagedResult<TDto>> GetAllAsync(QueryOptions? options = null)
    {
        var queryString = BuildQueryString(options);
        var response = await HttpClient.GetAsync($"{BaseEndpoint}{queryString}");
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagedResult<TDto>>()
            ?? new PagedResult<TDto> { Items = new List<TDto>(), TotalCount = 0 };
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
    {
        var response = await HttpClient.PostAsJsonAsync(BaseEndpoint, createDto);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TDto>()
            ?? throw new InvalidOperationException("Failed to deserialize created entity");
    }

    public virtual async Task<TDto> UpdateAsync(TUpdateDto updateDto)
    {
        var response = await HttpClient.PutAsJsonAsync($"{BaseEndpoint}/{updateDto.Id}", updateDto);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TDto>()
            ?? throw new InvalidOperationException("Failed to deserialize updated entity");
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var response = await HttpClient.DeleteAsync($"{BaseEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }

    protected virtual string BuildQueryString(QueryOptions? options)
    {
        if (options == null)
            return string.Empty;

        var parameters = new List<string>();

        if (options.PageNumber.HasValue)
            parameters.Add($"pageNumber={options.PageNumber.Value}");

        if (options.PageSize.HasValue)
            parameters.Add($"pageSize={options.PageSize.Value}");

        if (options.SortDescending)
            parameters.Add("sortDescending=true");

        return parameters.Count > 0 ? "?" + string.Join("&", parameters) : string.Empty;
    }
}

/// <summary>
/// Helper class for query options without generic type parameter.
/// </summary>
public class QueryOptions
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public bool SortDescending { get; set; }
}
