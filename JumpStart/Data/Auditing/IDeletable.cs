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
/// Defines the contract for entities that track soft deletion audit information.
/// Enables the soft delete pattern where entities are marked as deleted rather than physically removed from the database.
/// </summary>
/// <remarks>
/// <para>
/// This interface supports the soft delete pattern, a best practice for maintaining data integrity and audit trails.
/// Instead of physically removing entities from the database (hard delete), entities implementing this interface
/// are marked as deleted by setting DeletedById and DeletedOn properties. This approach:
/// - Preserves data for audit trails and compliance reporting
/// - Maintains referential integrity by keeping foreign key relationships intact
/// - Allows for potential data recovery if deletion was accidental
/// - Enables historical analysis and reporting
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// - DeletedById (Guid?) - The identifier of the user who deleted the entity (nullable)
/// - DeletedOn (DateTime?) - The UTC timestamp when the entity was deleted (nullable)
/// Both properties are nullable, indicating the entity is active when null and deleted when populated.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during soft delete operations:
/// - DeletedById is populated from the current user context (ICurrentUserService)
/// - DeletedOn is set to DateTime.UtcNow
/// Application code should not set these values directly; use repository soft delete methods instead.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or use a base class that implements it) when:
/// - Data must be preserved for audit trails and compliance
/// - Referential integrity is important
/// - Data recovery capability is desired
/// - Historical reporting requires access to deleted entities
/// - Regulatory requirements mandate data retention
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other audit interfaces:
/// - <see cref="ICreatable"/> - Tracks who created the entity and when
/// - <see cref="IModifiable"/> - Tracks who last modified the entity and when
/// - <see cref="IAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="AuditableEntity"/> - Implements IAuditable (includes IDeletable)
/// - Custom base classes that implement IDeletable for specific scenarios
/// </para>
/// <para>
/// <strong>Querying and Filtering:</strong>
/// Repositories should automatically filter out soft-deleted entities in normal queries by checking
/// DeletedOn == null. Include soft-deleted entities only in specialized queries for audit reports,
/// data recovery tools, or administrative interfaces.
/// </para>
/// <para>
/// <strong>Hard Delete vs Soft Delete:</strong>
/// - Soft Delete (this interface): Sets DeletedById and DeletedOn, entity remains in database
/// - Hard Delete: Physically removes entity from database using DbContext.Remove()
/// Choose soft delete for user data and business entities; hard delete may be appropriate for
/// temporary data, cache entries, or entities with no audit requirements.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple entity implementing IDeletable
/// public class Document : JumpStart.Data.Entity, JumpStart.Data.Auditing.IDeletable
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     // IDeletable properties
///     public System.Guid? DeletedById { get; set; }
///     public System.DateTimeOffset? DeletedOn { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="ICreatable"/>
/// <seealso cref="IModifiable"/>
/// <seealso cref="IAuditable"/>
/// <seealso cref="AuditableEntity"/>
public interface IDeletable
{
    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity (soft delete).
    /// </summary>
    /// <value>
    /// The user identifier that deleted this entity, or null if the entity is active (not deleted).
    /// This value is automatically set by the repository layer during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during soft delete operations using the
    /// current user's identifier from the security context (typically ICurrentUserService).
    /// It should never be set manually in application code; use repository soft delete methods instead.
    /// </para>
    /// <para>
    /// A null value indicates the entity is active (not deleted). A non-null value indicates the
    /// entity has been soft-deleted and should be excluded from normal queries. Use specialized
    /// queries when you need to include or specifically target deleted entities.
    /// </para>
    /// <para>
    /// Soft deletion semantics:
    /// - null: Entity is active
    /// - non-null: Entity is soft-deleted
    /// This property should always have the same null status as <see cref="DeletedOn"/>.
    /// </para>
    /// </remarks>
    Guid? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was deleted (soft delete).
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was deleted, or null if the
    /// entity is active (not deleted). This value is automatically set by the repository layer
    /// during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during soft delete operations using
    /// DateTimeOffset.UtcNow. It should never be set manually in application code; use repository
    /// soft delete methods instead. Always stored in UTC to ensure consistency across time zones.
    /// </para>
    /// <para>
    /// A null value indicates the entity is active (not deleted). A non-null value indicates the
    /// entity has been soft-deleted and should be excluded from normal queries.
    /// </para>
    /// <para>
    /// Best practices:
    /// - Always use UTC for audit timestamps (DateTimeOffset.UtcNow)
    /// - Use for filtering active vs deleted entities
    /// - Filter with "DeletedOn == null" in normal queries
    /// - Include deleted entities only for audit reports or admin functions
    /// - Consider adding database index for query performance
    /// - Use EF Core global query filters to automatically exclude deleted entities
    /// </para>
    /// <para>
    /// Recovery/Restore:
    /// To restore a soft-deleted entity, set both DeletedById and DeletedOn back to null.
        /// This should be done through a repository method to ensure proper audit trail logging.
        /// </para>
        /// </remarks>
        DateTimeOffset? DeletedOn { get; set; }
    }
