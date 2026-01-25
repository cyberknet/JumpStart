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
/// Defines the contract for entities that track modification audit information.
/// Enables tracking of who last modified an entity and when the modification occurred.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends the audit tracking system beyond creation to include modification history.
/// It defines the properties needed to track the most recent modification: who made the change and
/// when it occurred. These fields are automatically populated by the repository layer during update
/// operations and should remain unchanged after each update until the next modification.
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// - ModifiedById (Guid?) - The identifier of the user who last modified the entity (nullable)
/// - ModifiedOn (DateTime?) - The UTC timestamp when the entity was last modified (nullable)
/// Both properties are nullable, indicating the entity has never been modified when null.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during UpdateAsync operations:
/// - ModifiedById is populated from the current user context (ICurrentUserService)
/// - ModifiedOn is set to DateTime.UtcNow
/// These values are updated on every modification. Application code should not set these manually.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or use a base class that implements it) when:
/// - You need to track who last changed each entity
/// - Modification timestamp is required for audit trails
/// - Conflict detection or optimistic concurrency is needed
/// - Compliance requires tracking entity changes
/// - You want to display "last updated" information to users
/// </para>
/// <para>
/// <strong>Modification vs Creation:</strong>
/// A null ModifiedOn value typically indicates the entity has been created but never modified since creation.
/// This is normal and expected. Only after the first update operation will these fields be populated.
/// The creation audit is tracked separately by <see cref="ICreatable"/>.
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other audit interfaces:
/// - <see cref="ICreatable"/> - Tracks who created the entity and when
/// - <see cref="IDeletable"/> - Tracks soft deletion (who/when deleted)
/// - <see cref="IAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="AuditableEntity"/> - Implements IAuditable (includes IModifiable)
/// - Custom base classes that implement IModifiable for specific scenarios
/// </para>
/// <para>
/// <strong>Optimistic Concurrency:</strong>
/// The ModifiedOn property can be used as a concurrency token in database configurations to detect
/// and prevent conflicting updates. If two users try to update the same entity simultaneously,
/// the second update can be rejected based on the ModifiedOn timestamp mismatch.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple entity implementing IModifiable
/// public class BlogPost : JumpStart.Data.Entity, JumpStart.Data.Auditing.ICreatable, JumpStart.Data.Auditing.IModifiable
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     // ICreatable properties
///     public System.Guid CreatedById { get; set; }
///     public System.DateTimeOffset CreatedOn { get; set; }
///     // IModifiable properties
///     public System.Guid? ModifiedById { get; set; }
///     public System.DateTimeOffset? ModifiedOn { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="ICreatable"/>
/// <seealso cref="IDeletable"/>
/// <seealso cref="IAuditable"/>
/// <seealso cref="AuditableEntity"/>
public interface IModifiable
{
    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// The user identifier that last modified this entity, or null if the entity has never been
    /// modified since creation. This value is automatically set by the repository layer during
    /// update operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during UpdateAsync operations using the
    /// current user's identifier from the security context (typically ICurrentUserService).
    /// It should never be set manually in application code; use repository update methods instead.
    /// </para>
    /// <para>
    /// A null value indicates the entity has been created but never modified. This is normal and
    /// expected for newly created entities. The value is updated with each modification operation.
    /// </para>
    /// <para>
    /// Nullability semantics:
    /// - null: Entity created but never modified
    /// - non-null: Entity has been modified at least once
    /// This property should always have the same null status as <see cref="ModifiedOn"/>.
    /// </para>
    /// <para>
    /// Use cases:
    /// - Displaying "last modified by" information in UIs
    /// - Filtering entities by modifier
    /// - Audit trail reporting
    /// - Tracking user activity and contributions
    /// </para>
    /// </remarks>
    Guid? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was last modified.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was last modified, or null if
    /// the entity has never been modified since creation. This value is automatically set by the
    /// repository layer during update operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during UpdateAsync operations using
    /// DateTimeOffset.UtcNow. It should never be set manually in application code; use repository
    /// update methods instead. Always stored in UTC to ensure consistency across time zones.
    /// </para>
    /// <para>
    /// A null value indicates the entity has been created but never modified. This is normal and
    /// expected for newly created entities. The value is updated with each modification operation.
    /// </para>
    /// <para>
    /// Best practices:
    /// - Always use UTC for audit timestamps (DateTimeOffset.UtcNow)
    /// - Convert to local time in presentation layer if needed
    /// - Use for sorting by recency
    /// - Use for filtering recently modified entities
    /// - Consider as concurrency token for optimistic locking
    /// - Index this column in database for query performance
    /// </para>
    /// <para>
    /// Optimistic Concurrency:
    /// This property is ideal for use as a concurrency token in Entity Framework:
    /// - Configure as .IsConcurrencyToken() in EF Core
    /// - Prevents lost updates from concurrent modifications
    /// - Automatically throws DbUpdateConcurrencyException on conflict
    /// </para>
    /// <para>
    /// Common queries:
    /// - Recently modified: ModifiedOn > cutoffDate
    /// - Never modified: ModifiedOn == null
    /// - Modified by user: ModifiedById == userId
        /// - Stale entities: ModifiedOn &lt; oldDate
        /// </para>
        /// </remarks>
        DateTimeOffset? ModifiedOn { get; set; }
    }
