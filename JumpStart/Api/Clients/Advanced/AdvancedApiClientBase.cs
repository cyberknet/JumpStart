using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Base implementation of API client for entities with custom key types.
/// </summary>
public abstract class AdvancedApiClientBase<TEntity, TKey> : IAdvancedApiClient<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    protected readonly HttpClient HttpClient;
    protected readonly string BaseEndpoint;

    protected AdvancedApiClientBase(HttpClient httpClient, string baseEndpoint)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        BaseEndpoint = baseEndpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseEndpoint));
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        var response = await HttpClient.GetAsync($"{BaseEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TEntity>();
    }

    public virtual async Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity>? options = null)
    {
        var queryString = BuildQueryString(options);
        var response = await HttpClient.GetAsync($"{BaseEndpoint}{queryString}");
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagedResult<TEntity>>()
            ?? new PagedResult<TEntity> { Items = new List<TEntity>(), TotalCount = 0 };
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        var response = await HttpClient.PostAsJsonAsync(BaseEndpoint, entity);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TEntity>()
            ?? throw new InvalidOperationException("Failed to deserialize created entity");
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var response = await HttpClient.PutAsJsonAsync($"{BaseEndpoint}/{entity.Id}", entity);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TEntity>()
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

    protected virtual string BuildQueryString(QueryOptions<TEntity>? options)
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
