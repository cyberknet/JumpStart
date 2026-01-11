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

namespace JumpStart.Api.DTOs.Advanced;

/// <summary>
/// Base DTO for entities with custom key types.
/// Provides the Id property for read operations and serves as the foundation for all advanced DTOs.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This is the base class for all advanced DTOs in the JumpStart framework that use custom key types.
/// It implements <see cref="JumpStart.Api.DTOs.IDto"/> and provides the essential Id property required for entity identification.
/// </para>
/// <para>
/// The generic TKey parameter allows flexibility in choosing the identifier type:
/// - Use <c>int</c> for auto-incrementing integer IDs (most common in SQL databases)
/// - Use <c>long</c> for large-scale systems requiring more ID space
/// - Use <c>Guid</c> for distributed systems or when globally unique identifiers are needed
/// - Use custom struct types for composite or specialized keys
/// </para>
/// <para>
/// For entities with Guid identifiers (the most common scenario in modern applications),
/// use <see cref="JumpStart.Api.DTOs.SimpleEntityDto"/> instead, which provides a simpler interface without
/// the generic key parameter.
/// </para>
/// <para>
/// Derived classes should add additional properties specific to their entity type.
/// For entities requiring audit information, inherit from <see cref="JumpStart.Api.DTOs.Advanced.AuditableEntityDto{TKey}"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Creating a ProductDto with int identifier
/// public class ProductDto : EntityDto&lt;int&gt;
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// // Example 2: Creating an OrderDto with long identifier
/// public class OrderDto : EntityDto&lt;long&gt;
/// {
///     public DateTime OrderDate { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; } = string.Empty;
/// }
/// 
/// // Example 3: Using in API response
/// // GET /api/products/123
/// // Response:
/// // {
/// //   "id": 123,
/// //   "name": "Laptop",
/// //   "price": 999.99,
/// //   "category": "Electronics"
/// // }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.DTOs.IDto"/>
/// <seealso cref="JumpStart.Api.DTOs.SimpleEntityDto"/>
/// <seealso cref="JumpStart.Api.DTOs.Advanced.AuditableEntityDto{TKey}"/>
public abstract class EntityDto<TKey> : IDto where TKey : struct
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    /// <value>
    /// A value of type <typeparamref name="TKey"/> that uniquely identifies this entity.
    /// This corresponds to the primary key of the underlying entity in the data store.
    /// </value>
    /// <remarks>
    /// <para>
    /// The Id is populated when reading entities from the data store and should be included
    /// in update operations to identify which entity to modify. It is typically excluded from
    /// create DTOs as the ID is usually assigned by the database or repository.
    /// </para>
    /// <para>
    /// The type of the Id matches the TKey generic parameter and must be a value type (struct).
    /// Common types include int, long, and Guid.
    /// </para>
    /// </remarks>
    public TKey Id { get; set; }
}
