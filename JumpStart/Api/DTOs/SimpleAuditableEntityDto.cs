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

namespace JumpStart.Api.DTOs;

/// <summary>
/// Base DTO for auditable entities with Guid identifiers.
/// Includes read-only audit information (creation and modification tracking) populated by the system.
/// This is the recommended base DTO for most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class combines the simplicity of <see cref="SimpleEntityDto"/> (Guid-based identifiers)
/// with comprehensive audit tracking capabilities. It's the most common base DTO for applications
/// that need to track who created and modified entities, and when those actions occurred.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// - Inherits from <see cref="SimpleEntityDto"/> (provides Guid Id property)
/// - Implements <see cref="IDto"/> (through SimpleEntityDto)
/// - Adds four audit properties for creation and modification tracking
/// </para>
/// <para>
/// <strong>Audit Fields (Read-Only - System Managed):</strong>
/// All audit properties are populated automatically by the repository layer and should be
/// treated as read-only in DTOs. They are included in read operations but excluded from
/// create and update DTOs:
/// - <see cref="CreatedById"/> - User who created the entity (Guid)
/// - <see cref="CreatedOn"/> - When the entity was created (DateTime UTC)
/// - <see cref="ModifiedById"/> - User who last modified the entity (Guid?, null if never modified)
/// - <see cref="ModifiedOn"/> - When the entity was last modified (DateTime?, null if never modified)
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - Entities use Guid identifiers (most modern applications)
/// - Audit trail is required (who created/modified and when)
/// - Full compatibility with JumpStart's audit infrastructure is needed
/// - Simplified API with fixed Guid type is preferred over generic key types
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// - Use <see cref="SimpleEntityDto"/> if audit tracking is not needed
/// - Use <see cref="Advanced.AuditableEntityDto{TKey}"/> for custom key types (int, long, etc.)
/// - Use <see cref="Advanced.EntityDto{TKey}"/> for custom keys without audit tracking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple product DTO with audit tracking
/// public class ProductDto : SimpleAuditableEntityDto
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
/// }
/// 
/// // Example 2: API response with audit information
/// // GET /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
/// // {
/// //   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
/// //   "name": "Laptop",
/// //   "price": 999.99,
/// //   "description": "High-performance laptop",
/// //   "createdById": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
/// //   "createdOn": "2026-01-15T10:30:00Z",
/// //   "modifiedById": "9f8e7d6c-5b4a-3210-fedc-ba9876543210",
/// //   "modifiedOn": "2026-01-20T14:45:00Z"
/// // }
/// 
/// // Example 3: Customer DTO with related entities
/// public class CustomerDto : SimpleAuditableEntityDto
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public List&lt;OrderDto&gt; Orders { get; set; } = new();
/// }
/// 
/// // Example 4: Usage showing audit trail
/// var product = await repository.GetByIdAsync(productId);
/// var dto = mapper.Map&lt;ProductDto&gt;(product);
/// 
/// // Audit information is automatically included
/// Console.WriteLine($"Created by user {dto.CreatedById} on {dto.CreatedOn}");
/// if (dto.ModifiedOn.HasValue)
/// {
///     Console.WriteLine($"Last modified by user {dto.ModifiedById} on {dto.ModifiedOn}");
/// }
/// </code>
/// </example>
/// <seealso cref="SimpleEntityDto"/>
/// <seealso cref="Advanced.AuditableEntityDto{TKey}"/>
/// <seealso cref="IDto"/>
public abstract class SimpleAuditableEntityDto : SimpleEntityDto
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the user who created this entity.
    /// This value is set by the system during entity creation and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is created.
    /// It corresponds to the CreatedById field in entities implementing <see cref="Data.Advanced.Auditing.ICreatable{T}"/>.
    /// The Guid typically references a user in an identity management system.
    /// </remarks>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was created.
    /// This value is set by the system during entity creation and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is created.
    /// It corresponds to the CreatedOn field in entities implementing <see cref="Data.Advanced.Auditing.ICreatable{T}"/>.
    /// The value is stored in UTC to ensure consistency across time zones.
    /// </remarks>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the user who last modified this entity,
    /// or null if the entity has never been modified.
    /// This value is set by the system during entity updates and should not be modified by clients.
    /// </value>
    /// <remarks>
    /// This field is automatically populated by the repository when the entity is updated.
    /// It corresponds to the ModifiedById field in entities implementing <see cref="Data.Advanced.Auditing.IModifiable{T}"/>.
    /// A null value indicates the entity has been created but never modified.
    /// The Guid typically references a user in an identity management system.
    /// </remarks>
    public Guid? ModifiedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time (in UTC) when this entity was last modified.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTimeOffset"/> in UTC representing when the entity was last modified,
        /// or null if never modified.
        /// This value is set by the system during entity updates and should not be modified by clients.
        /// </value>
        /// <remarks>
        /// This field is automatically populated by the repository when the entity is updated.
        /// It corresponds to the ModifiedOn field in entities implementing <see cref="Data.Advanced.Auditing.IModifiable{T}"/>.
        /// A null value indicates the entity has been created but never modified.
        /// The value is stored in UTC to ensure consistency across time zones.
        /// </remarks>
        public DateTimeOffset? ModifiedOn { get; set; }
    }
