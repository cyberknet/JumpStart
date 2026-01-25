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
/// Defines the contract for entities that track complete audit information including creation, modification, and soft deletion.
/// This interface combines <see cref="ICreatable"/>, <see cref="IModifiable"/>, and <see cref="IDeletable"/>
/// to provide comprehensive audit trail capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the root contract for full audit tracking in the JumpStart framework. It combines
/// three specialized audit interfaces to track the complete lifecycle of an entity:
/// - <see cref="ICreatable"/> - Tracks who created the entity and when (CreatedById, CreatedOn)
/// - <see cref="IModifiable"/> - Tracks who last modified the entity and when (ModifiedById, ModifiedOn)
/// - <see cref="IDeletable"/> - Tracks who deleted the entity and when for soft deletes (DeletedById, DeletedOn)
/// </para>
/// <para>
/// <strong>Properties Defined by Inherited Interfaces:</strong>
/// From ICreatable:
/// - CreatedById (Guid) - User who created the entity
/// - CreatedOn (DateTimeOffset) - When the entity was created
/// 
/// From IModifiable:
/// - ModifiedById (Guid?) - User who last modified the entity (nullable)
/// - ModifiedOn (DateTimeOffset?) - When the entity was last modified (nullable)
/// 
/// From IDeletable:
/// - DeletedById (Guid?) - User who deleted the entity (nullable)
/// - DeletedOn (DateTimeOffset?) - When the entity was deleted (nullable)
/// </para>
/// <para>
/// <strong>Automatic Management:</strong>
/// All audit fields are automatically populated by the repository layer in the JumpStart framework:
/// - Creation fields are set during AddAsync operations
/// - Modification fields are updated during UpdateAsync operations
/// - Deletion fields are set during soft delete operations
/// These fields should not be manually set in application code.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from <see cref="AuditableEntity"/>) when:
/// - Compliance or regulatory requirements mandate tracking all changes
/// - You need to know who created, modified, or deleted each entity
/// - Soft delete functionality is required to preserve historical data
/// - Complete audit trail is essential for your business domain
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, use one of these base classes:
/// - <see cref="AuditableEntity"/> - Full audit tracking
/// - <see cref="AuditableNamedEntity"/> - Adds Name property to auditable entities
/// </para>
/// <para>
/// <strong>Soft Delete Pattern:</strong>
/// This interface supports the soft delete pattern where entities are marked as deleted (DeletedOn has a value)
/// rather than physically removed from the database. This preserves data for:
/// - Audit trails and compliance reporting
/// - Historical analysis and data recovery
/// - Maintaining referential integrity
/// Repositories can automatically filter out soft-deleted entities in queries.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Using IAuditable to check audit status
/// public bool IsModified(JumpStart.Data.Auditing.IAuditable entity)
/// {
///     return entity.ModifiedOn.HasValue;
/// }
/// 
/// public bool IsDeleted(JumpStart.Data.Auditing.IAuditable entity)
/// {
///     return entity.DeletedOn.HasValue;
/// }
/// 
/// // Example: Filtering and querying auditable entities
/// var recentlyModified = await dbContext.Products
///     .Where(p => p.ModifiedOn &gt; System.DateTime.UtcNow.AddDays(-7))
///     .Where(p => p.DeletedOn == null) // Exclude soft-deleted
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// </code>
/// </example>
/// <seealso cref="ICreatable"/>
/// <seealso cref="IModifiable"/>
/// <seealso cref="IDeletable"/>
/// <seealso cref="AuditableEntity"/>
/// <seealso cref="SimpleAuditableEntity"/>
public interface IAuditable
    : ICreatable, IModifiable, IDeletable
{
    // This interface intentionally contains no members.
    // It serves as a marker interface that combines ICreatable, IModifiable, and IDeletable.
}
