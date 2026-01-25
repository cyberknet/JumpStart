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


namespace JumpStart.Data.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking including creation, modification, and soft deletion.
/// Inherit from this class in your entity models to enable comprehensive audit trail functionality in JumpStart/.NET 10 projects.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="Entity"/> and implements <see cref="IAuditable"/> to provide
/// complete audit tracking capabilities. It automatically tracks:
/// - Who created the entity and when (CreatedById, CreatedOn)
/// - Who last modified the entity and when (ModifiedById, ModifiedOn)
/// - Who deleted the entity and when (DeletedById, DeletedOn) for soft delete scenarios
/// </para>
/// <para>
/// <strong>Audit Fields Managed by Repository:</strong>
/// All audit fields are automatically populated by the repository layer. You should not set these
/// fields manually in your application code:
/// - CreatedById, CreatedOn: Set during AddAsync operations based on current user context
/// - ModifiedById, ModifiedOn: Set during UpdateAsync operations based on current user context
/// - DeletedById, DeletedOn: Set during soft delete operations based on current user context
/// </para>
/// <para>
/// <strong>Soft Delete Support:</strong>
/// This entity supports soft deletion, meaning entities are marked as deleted rather than physically
/// removed from the database. This preserves data for audit purposes and allows for potential recovery.
/// Repositories can filter out soft-deleted entities automatically in queries.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - You need complete audit trail (creation, modification, deletion tracking)
/// - Compliance or regulatory requirements mandate tracking changes
/// - You want soft delete functionality to preserve historical data
/// - Custom key types (int, long, Guid) are needed
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// - Use <see cref="Entity"/> if no audit tracking is needed
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Product entity with full audit tracking
/// public class Product : JumpStart.Data.Auditing.AuditableEntity
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     [System.ComponentModel.DataAnnotations.StringLength(1000)]
///     public string? Description { get; set; }
///     public string Category { get; set; } = string.Empty;
/// }
/// </code>
/// </example>
/// <seealso cref="Entity"/>
/// <seealso cref="IAuditable"/>
public abstract class AuditableEntity : Entity, IAuditable
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// The user identifier that created this entity. This is automatically set by the repository
    /// during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during AddAsync operations using the current
    /// user's identifier from the security context. It should never be set manually in application code.
    /// </remarks>
    public Guid CreatedById { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was created. This is automatically
    /// set by the repository during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during AddAsync operations using DateTimeOffset.UtcNow.
    /// Always stored in UTC to ensure consistency across different time zones.
    /// </remarks>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// The user identifier that last modified this entity, or null if the entity has never been modified
    /// after creation. This is automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during UpdateAsync operations using the current
    /// user's identifier. A null value indicates the entity has been created but never modified.
    /// </remarks>
    public Guid? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was last modified.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was last modified, or null if never
    /// modified after creation. This is automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during UpdateAsync operations using DateTimeOffset.UtcNow.
    /// A null value indicates the entity has been created but never modified. Always stored in UTC.
    /// </remarks>
    public DateTimeOffset? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity (soft delete).
    /// </summary>
    /// <value>
    /// The user identifier that deleted this entity, or null if the entity has not been deleted.
    /// This is automatically set by the repository during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This field is populated by the repository layer during soft delete operations using the current
    /// user's identifier. A null value indicates the entity has not been deleted.
    /// </para>
    /// <para>
    /// Soft delete means the entity is marked as deleted but not physically removed from the database,
    /// preserving the data for audit purposes and potential recovery.
    /// </para>
    /// </remarks>
    public Guid? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was deleted (soft delete).
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was deleted, or null if not deleted.
    /// This is automatically set by the repository during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This field is populated by the repository layer during soft delete operations using DateTimeOffset.UtcNow.
    /// A null value indicates the entity has not been deleted. Always stored in UTC.
    /// </para>
    /// <para>
    /// Entities with a non-null DeletedOn value should typically be filtered out of normal queries,
    /// but can be included for audit reporting or recovery scenarios.
    /// </para>
    /// </remarks>
    public DateTimeOffset? DeletedOn { get; set; }
}
