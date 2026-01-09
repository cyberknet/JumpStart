using System;
using JumpStart.Api.Clients.Advanced;
using JumpStart.Api.DTOs;

namespace JumpStart.Api.Clients;

/// <summary>
/// Client interface for interacting with DTO-based API endpoints for entities with Guid identifiers.
/// This is the recommended API client interface for most applications using the JumpStart framework.
/// </summary>
public interface ISimpleApiClient<TDto, TCreateDto, TUpdateDto>
    : IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, Guid>
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
{
}

/// <summary>
/// Base implementation of API client with DTOs for entities with Guid identifiers.
/// This is the recommended base API client for most applications using the JumpStart framework.
/// </summary>
public abstract class SimpleApiClientBase<TDto, TCreateDto, TUpdateDto> 
    : AdvancedApiClientBase<TDto, TCreateDto, TUpdateDto, Guid>,
      ISimpleApiClient<TDto, TCreateDto, TUpdateDto>
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
{
    protected SimpleApiClientBase(HttpClient httpClient, string baseEndpoint) 
        : base(httpClient, baseEndpoint)
    {
    }
}
