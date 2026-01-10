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
using JumpStart.Api.DTOs.Advanced;

namespace JumpStart.Api.DTOs;

/// <summary>
/// Base DTO for entities with Guid identifiers.
/// Provides a simplified interface for the most common identifier type in modern applications.
/// This is the recommended base DTO for most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class simplifies DTO creation by fixing the key type to Guid, eliminating the need
/// for generic type parameters in the most common scenario. It inherits from
/// <see cref="EntityDto{TKey}"/> with TKey set to Guid, providing all the functionality
/// of the advanced DTO system with a cleaner, simpler syntax.
/// </para>
/// <para>
/// <strong>Why Use Guid Identifiers:</strong>
/// - Globally unique across distributed systems
/// - Can be generated client-side without database round-trips
/// - No sequential ID disclosure (better security)
/// - Ideal for microservices and distributed architectures
/// - Natural fit for modern cloud-based applications
/// - Eliminates ID collisions when merging data from multiple sources
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// - Inherits from <see cref="EntityDto{TKey}"/> with TKey = Guid
/// - Implements <see cref="IDto"/> (through EntityDto)
/// - Provides Id property of type Guid
/// - Can be extended by concrete DTOs or auditable DTOs
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use SimpleEntityDto when:
/// - Entities use Guid identifiers (recommended for new applications)
/// - Simplified API without generic type parameters is preferred
/// - No audit tracking is needed (use <see cref="SimpleAuditableEntityDto"/> for audit)
/// - Standard CRUD operations are sufficient
/// </para>
/// <para>
/// <strong>When to Use Alternatives:</strong>
/// - Use <see cref="SimpleAuditableEntityDto"/> if audit tracking is required (most common)
/// - Use <see cref="EntityDto{TKey}"/> for custom key types (int, long, custom structs)
/// - Use <see cref="Advanced.AuditableEntityDto{TKey}"/> for custom keys with audit tracking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple product DTO
/// public class ProductDto : SimpleEntityDto
/// {
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     [StringLength(1000)]
///     public string? Description { get; set; }
///     
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// // Example 2: Customer DTO with related entities
/// public class CustomerDto : SimpleEntityDto
/// {
///     public string Name { get; set; } = string.Empty;
///     
///     [EmailAddress]
///     public string Email { get; set; } = string.Empty;
///     
///     public string? Phone { get; set; }
///     
///     public List&lt;OrderDto&gt; Orders { get; set; } = new();
/// }
/// 
/// // Example 3: API response with Guid identifier
/// // GET /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
/// // Response:
/// // {
/// //   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
/// //   "name": "Laptop",
/// //   "price": 999.99,
/// //   "description": "High-performance laptop",
/// //   "category": "Electronics"
/// // }
/// 
/// // Example 4: Usage in controller
/// [HttpGet("{id}")]
/// public async Task&lt;ActionResult&lt;ProductDto&gt;&gt; GetById(Guid id)
/// {
///     var entity = await _repository.GetByIdAsync(id);
///     if (entity == null)
///         return NotFound();
///     
///     var dto = _mapper.Map&lt;ProductDto&gt;(entity);
///     return Ok(dto);
/// }
/// 
/// // Example 5: Creating instances
/// var product = new ProductDto
/// {
///     Id = Guid.NewGuid(),
///     Name = "New Product",
///     Price = 49.99m,
///     Description = "A great product",
///     Category = "Electronics"
/// };
/// </code>
/// </example>
/// <seealso cref="EntityDto{TKey}"/>
/// <seealso cref="SimpleAuditableEntityDto"/>
/// <seealso cref="IDto"/>
public abstract class SimpleEntityDto : EntityDto<Guid>
{
}
