using System;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking with Guid identifiers.
/// This class must be inherited by concrete entity classes that need comprehensive audit trail functionality.
/// </summary>
/// <remarks>
/// <para>
/// This is the recommended base class for auditable entities in most applications using the JumpStart framework.
/// It provides automatic tracking of:
/// - Entity creation (who and when)
/// - Entity modification (who and when)
/// - Entity soft deletion (who and when)
/// </para>
/// <para>
/// Inherits from <see cref="SimpleEntity"/> and implements <see cref="ISimpleAuditable"/>.
/// Uses Guid for both entity ID and audit user identifiers.
/// </para>
/// <para>
/// Audit fields are automatically populated by the repository layer when:
/// - An <see cref="Repositories.ISimpleUserContext"/> is registered
/// - Repository methods (AddAsync, UpdateAsync, DeleteAsync) are called
/// </para>
/// <para>
/// For applications requiring custom key types (int, long, etc.), use <see cref="Advanced.Auditing.AuditableEntity{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Product : SimpleAuditableEntity
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     
///     // Audit properties inherited from SimpleAuditableEntity:
///     // - Id (from SimpleEntity)
///     // - CreatedById, CreatedOn
///     // - ModifiedById, ModifiedOn
///     // - DeletedById, DeletedOn
/// }
/// </code>
/// </example>
/// <seealso cref="SimpleEntity"/>
/// <seealso cref="ISimpleAuditable"/>
/// <seealso cref="SimpleAuditableNamedEntity"/>
/// <seealso cref="Repositories.SimpleRepository{TEntity}"/>
public abstract class SimpleAuditableEntity : SimpleEntity, ISimpleAuditable
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the user ID.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during create operations.
    /// </value>
    /// <remarks>
    /// This property is required and is automatically populated by the repository
    /// when an <see cref="Repositories.ISimpleUserContext"/> is available.
    /// </remarks>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format.
    /// Automatically set to <see cref="DateTime.UtcNow"/> during create operations.
    /// </value>
    /// <remarks>
    /// Always stored in UTC to ensure consistency across time zones.
    /// Automatically populated by the repository during AddAsync operations.
    /// </remarks>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the user ID, or null if never modified.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This property is null when the entity has never been modified after creation.
    /// Automatically populated by the repository when an <see cref="Repositories.ISimpleUserContext"/> is available.
    /// </remarks>
    public Guid? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this entity was last modified.
    /// </summary>
    /// <value>
    /// A nullable <see cref="DateTime"/> in UTC format, or null if never modified.
    /// Automatically set to <see cref="DateTime.UtcNow"/> during update operations.
    /// </value>
    /// <remarks>
    /// This property is null when the entity has never been modified after creation.
    /// Always stored in UTC to ensure consistency across time zones.
    /// Automatically populated by the repository during UpdateAsync operations.
    /// </remarks>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who soft-deleted this entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the user ID, or null if not deleted.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is null for active (non-deleted) entities.
    /// When populated, the entity is considered "soft deleted" and will be excluded
    /// from standard queries by the repository's soft delete filter.
    /// </para>
    /// <para>
    /// Soft deletion allows entities to be recovered and maintains referential integrity.
    /// Automatically populated by the repository when an <see cref="Repositories.ISimpleUserContext"/> is available.
    /// </para>
    /// </remarks>
    public Guid? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this entity was soft-deleted.
    /// </summary>
    /// <value>
    /// A nullable <see cref="DateTime"/> in UTC format, or null if not deleted.
    /// Automatically set to <see cref="DateTime.UtcNow"/> during delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is null for active (non-deleted) entities.
    /// When populated, the entity is considered "soft deleted" and will be excluded
    /// from standard queries by the repository's soft delete filter.
    /// </para>
    /// <para>
    /// Soft deletion allows entities to be recovered and maintains audit history.
    /// Always stored in UTC to ensure consistency across time zones.
    /// Automatically populated by the repository during DeleteAsync operations.
    /// </para>
    /// </remarks>
    public DateTime? DeletedOn { get; set; }
}
