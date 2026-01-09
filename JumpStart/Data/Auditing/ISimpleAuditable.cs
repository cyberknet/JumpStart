using System;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Defines the contract for entities that track complete audit information with Guid identifiers.
/// This interface combines <see cref="ISimpleCreatable"/>, <see cref="ISimpleModifiable"/>, and <see cref="ISimpleDeletable"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is the recommended auditing interface for most applications using the JumpStart framework.
/// It provides automatic tracking of:
/// - Who created the entity and when (<see cref="ISimpleCreatable"/>)
/// - Who last modified the entity and when (<see cref="ISimpleModifiable"/>)
/// - Who soft-deleted the entity and when (<see cref="ISimpleDeletable"/>)
/// </para>
/// <para>
/// Audit fields are automatically populated by the repository layer when configured with
/// an <see cref="Repositories.ISimpleUserContext"/> implementation.
/// </para>
/// <para>
/// For applications requiring custom key types (int, long, etc.), use the Advanced namespace classes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Product : ISimpleAuditable
/// {
///     public Guid Id { get; set; }
///     public string Name { get; set; }
///     
///     // Audit properties from interfaces
///     public Guid CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     public Guid? ModifiedById { get; set; }
///     public DateTime? ModifiedOn { get; set; }
///     public Guid? DeletedById { get; set; }
///     public DateTime? DeletedOn { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="SimpleAuditableEntity"/>
/// <seealso cref="SimpleAuditableNamedEntity"/>
/// <seealso cref="Repositories.ISimpleUserContext"/>
public interface ISimpleAuditable : ISimpleCreatable, ISimpleModifiable, ISimpleDeletable
{
}
