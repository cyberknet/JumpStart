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
using JumpStart.Api.Clients.Advanced;
using JumpStart.Api.DTOs;

namespace JumpStart.Api.Clients;

/// <summary>
/// Defines the contract for API clients that interact with DTO-based HTTP endpoints for entities with Guid identifiers.
/// This is the recommended API client interface for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TDto">The data transfer object type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.SimpleEntityDto"/>.</typeparam>
/// <typeparam name="TCreateDto">The data transfer object type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The data transfer object type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto{Guid}"/>.</typeparam>
/// <remarks>
/// <para>
/// This interface simplifies API client creation for the most common scenario: entities with Guid identifiers.
/// It inherits from <see cref="JumpStart.Api.Clients.Advanced.IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}"/> with TKey fixed to Guid.
/// </para>
/// <para>
/// Use Refit to generate the implementation automatically by decorating with Refit attributes.
/// All CRUD operations are inherited from the base interface and use Guid as the key type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define your API client interface
/// public interface IProductApiClient : ISimpleApiClient&lt;ProductDto, CreateProductDto, UpdateProductDto&gt;
/// {
///     // Custom operations specific to products
///     [Get("/api/products/featured")]
///     Task&lt;IEnumerable&lt;ProductDto&gt;&gt; GetFeaturedAsync();
/// }
/// 
/// // Register in DI
/// services.AddRefitClient&lt;IProductApiClient&gt;()
///     .ConfigureHttpClient(c =&gt; c.BaseAddress = new Uri("https://api.example.com/api/products"));
/// 
/// // Or use the extension method
/// services.AddSimpleApiClient&lt;IProductApiClient&gt;("https://api.example.com/api/products");
/// 
/// // Use in your code
/// var product = await productClient.GetByIdAsync(Guid.NewGuid());
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.Clients.Advanced.IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}"/>
public interface ISimpleApiClient<TDto, TCreateDto, TUpdateDto>
    : IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, Guid>
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
{
}
