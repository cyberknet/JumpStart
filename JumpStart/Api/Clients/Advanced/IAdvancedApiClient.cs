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
using System.Threading.Tasks;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Repositories;
using Refit;

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Defines the contract for API clients that interact with DTO-based HTTP endpoints for entities with custom key types.
/// This interface provides standard CRUD operations using DTOs for type-safe API communication via Refit.
/// </summary>
/// <typeparam name="TDto">The data transfer object type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.Advanced.EntityDto{TKey}"/>.</typeparam>
/// <typeparam name="TCreateDto">The data transfer object type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The data transfer object type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key. Must be a value type (struct).</typeparam>
/// <remarks>
/// <para>
/// This interface is part of the Advanced namespace and supports custom key types (int, long, custom structs).
/// For applications using Guid identifiers, use <see cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/> instead.
/// </para>
/// <para>
/// The interface uses Refit attributes for declarative HTTP API definitions:
/// - GET operations return DTOs (read-only data)
/// - POST operations accept CreateDTOs (no Id/audit fields)
/// - PUT operations accept UpdateDTOs (includes Id, excludes audit fields)
/// - DELETE operations perform soft delete when supported by the entity
/// </para>
/// <para>
/// All operations are asynchronous and handle HTTP status codes appropriately.
/// Refit automatically handles serialization, deserialization, and error responses.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define your API client interface
/// public interface IProductApiClient : IAdvancedApiClient&lt;ProductDto, CreateProductDto, UpdateProductDto, int&gt;
/// {
///     // Custom operations specific to products
///     [Get("/api/products/featured")]
///     Task&lt;IEnumerable&lt;ProductDto&gt;&gt; GetFeaturedAsync();
/// }
/// 
/// // Register in DI
/// services.AddRefitClient&lt;IProductApiClient&gt;()
///     .ConfigureHttpClient(c =&gt; c.BaseAddress = new Uri("https://api.example.com"));
/// 
/// // Use in your code
/// var product = await productClient.GetByIdAsync(123);
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/>
public interface IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, TKey>
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
    where TKey : struct
{
    /// <summary>
    /// Retrieves a single entity DTO by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the entity DTO if found, or null if not found (HTTP 404).
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown when the HTTP request fails. Check StatusCode property for details.
    /// Returns null for 404 Not Found responses.
    /// </exception>
    /// <remarks>
    /// This method performs an HTTP GET request to the API endpoint.
    /// Refit automatically handles 404 responses by returning null.
    /// Other error status codes will throw ApiException.
    /// </remarks>
    /// <example>
    /// <code>
    /// var product = await client.GetByIdAsync(123);
    /// if (product != null)
    /// {
    ///     Console.WriteLine($"Found: {product.Name}");
    /// }
    /// </code>
    /// </example>
    [Get("/{id}")]
    Task<TDto?> GetByIdAsync(TKey id);

    /// <summary>
    /// Retrieves a paged collection of entity DTOs with optional sorting.
    /// </summary>
    /// <param name="options">
    /// Optional query parameters for pagination and sorting.
    /// If null, all entities are returned without pagination.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="JumpStart.Repositories.PagedResult{T}"/> with the requested page of entities
    /// and pagination metadata (page number, page size, total count).
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown when the HTTP request fails. Check StatusCode property for details.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs an HTTP GET request with query string parameters.
    /// Refit automatically serializes the <paramref name="options"/> object to query string parameters.
    /// </para>
    /// <para>
    /// If <paramref name="options"/> is null or pagination is not specified,
    /// the API may return all entities (subject to server-side limits).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new QueryOptions 
    /// { 
    ///     PageNumber = 1, 
    ///     PageSize = 20,
    ///     SortDescending = false
    /// };
    /// var result = await client.GetAllAsync(options);
    /// Console.WriteLine($"Total: {result.TotalCount}, Page {result.PageNumber}");
    /// </code>
    /// </example>
    [Get("")]
    Task<PagedResult<TDto>> GetAllAsync([Query] QueryOptions? options = null);

    /// <summary>
    /// Creates a new entity by sending a create DTO to the API.
    /// </summary>
    /// <param name="createDto">
    /// The data transfer object containing the data for the new entity.
    /// Should not include Id or audit fields as these are generated by the server.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the created entity DTO with server-generated fields populated
    /// (Id, audit timestamps, etc.).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="createDto"/> is null.
    /// </exception>
    /// <exception cref="ApiException">
    /// Thrown when the HTTP request fails. Check StatusCode property for details.
    /// This includes validation errors (HTTP 400) and server errors (HTTP 500).
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs an HTTP POST request with the <paramref name="createDto"/> as JSON body.
    /// Refit automatically serializes the DTO and handles the response.
    /// The server validates the DTO, creates the entity, and returns the complete entity with
    /// all server-generated fields populated.
    /// </para>
    /// <para>
    /// Common HTTP status codes:
    /// - 201 Created: Success, returns created entity
    /// - 400 Bad Request: Validation failed
    /// - 401 Unauthorized: Authentication required
    /// - 403 Forbidden: Insufficient permissions
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var createDto = new CreateProductDto 
    /// { 
    ///     Name = "New Product",
    ///     Price = 99.99m
    /// };
    /// var created = await client.CreateAsync(createDto);
    /// Console.WriteLine($"Created product with Id: {created.Id}");
    /// </code>
    /// </example>
    [Post("")]
    Task<TDto> CreateAsync([Body] TCreateDto createDto);

    /// <summary>
    /// Updates an existing entity by sending an update DTO to the API.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the entity to update.
    /// Must match the Id property in the updateDto for validation.
    /// </param>
    /// <param name="updateDto">
    /// The data transfer object containing the updated data.
    /// Must include the entity Id to identify which entity to update.
    /// Should not include audit fields as these are managed by the server.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the updated entity DTO with all current field values
    /// including server-updated audit timestamps.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="updateDto"/> is null.
    /// </exception>
    /// <exception cref="ApiException">
    /// Thrown when the HTTP request fails. Check StatusCode property for details.
    /// This includes 404 Not Found if the entity doesn't exist.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs an HTTP PUT request with the <paramref name="updateDto"/> as JSON body.
    /// The entity Id from the DTO is used in the URL path.
    /// Refit automatically serializes the DTO and handles the response.
    /// The server validates the DTO, updates the entity, and returns the complete updated entity.
    /// </para>
    /// <para>
    /// Common HTTP status codes:
    /// - 200 OK: Success, returns updated entity
    /// - 400 Bad Request: Validation failed or Id mismatch
    /// - 404 Not Found: Entity doesn't exist
    /// - 409 Conflict: Concurrency conflict (if using optimistic concurrency)
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var updateDto = new UpdateProductDto 
    /// { 
    ///     Id = 123,
    ///     Name = "Updated Product",
    ///     Price = 149.99m
    /// };
    /// var updated = await client.UpdateAsync(updateDto);
    /// Console.WriteLine($"Updated at: {updated.ModifiedOn}");
    /// </code>
    /// </example>
        [Put("/{id}")]
        Task<TDto> UpdateAsync(TKey id, [Body] TUpdateDto updateDto);

        /// <summary>
        /// Deletes an entity by its unique identifier.
        /// Performs a soft delete if the entity supports it (implements <see cref="Data.Advanced.Auditing.IDeletable{T}"/>).
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if the entity was successfully deleted,
        /// or false if the entity was not found (HTTP 404).
        /// </returns>
        /// <exception cref="ApiException">
        /// Thrown when the HTTP request fails (except for 404 which returns false).
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method performs an HTTP DELETE request to the API endpoint.
        /// Returns false for 404 Not Found responses (entity already deleted or doesn't exist).
        /// Returns true for successful deletion (HTTP 204 No Content).
        /// </para>
        /// <para>
        /// If the entity implements IDeletable, the API performs a soft delete:
        /// - Sets DeletedOn to the current UTC timestamp
        /// - Sets DeletedById to the current user's identifier
        /// - The entity remains in the database but is excluded from standard queries
        /// </para>
        /// <para>
        /// If the entity does not implement IDeletable, a hard delete is performed
        /// and the entity is permanently removed from the database.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool deleted = await client.DeleteAsync(123);
        /// if (deleted)
        /// {
        ///     Console.WriteLine("Product deleted successfully");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Product not found");
        /// }
        /// </code>
        /// </example>
        [Delete("/{id}")]
        Task<bool> DeleteAsync(TKey id);
    }
