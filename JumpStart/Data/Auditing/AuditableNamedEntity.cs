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
/// Provides an abstract base implementation for entities that have a name and require full audit tracking with custom key types.
/// This class must be inherited by concrete entity classes that need both naming and comprehensive audit trail functionality.
/// </summary>
/// <remarks>
/// <para>
/// This class combines three essential patterns in a single base class:
/// - Entity identification through <see cref="Entity"/> (provides Id property)
/// - Full audit tracking through <see cref="AuditableEntity"/> (creation, modification, soft deletion)
/// - Named entity pattern through <see cref="INamed"/> (provides Name property)
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities:
/// - Need a Name property as a key business attribute
/// - Require full audit trail (who created/modified/deleted and when)
/// - Need soft delete functionality with audit information
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// This is ideal for master data entities such as:
/// - Categories, Tags, Classifications
/// - Product Types, Document Types
/// - Departments, Teams, Roles
/// - Status Types, Priority Levels
/// - Any lookup/reference data that needs naming and audit tracking
/// </para>
/// <para>
/// <strong>Inherited Properties:</strong>
/// From <see cref="AuditableEntity"/>:
/// - Id (from Entity)
/// - CreatedById, CreatedOn
/// - ModifiedById, ModifiedOn
/// - DeletedById, DeletedOn
/// 
/// From <see cref="INamed"/>:
/// - Name (this class adds this property)
/// </para>
/// <para>
/// <strong>Repository Management:</strong>
/// All audit fields are automatically populated by the repository layer. The Name property
/// should be set by application code, while Id and audit fields are system-managed.
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// - Use <see cref="AuditableEntity"/> if naming is not a core requirement
/// - Use <see cref="NamedEntity"/> for named entities without audit tracking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Category entity
/// public class Category : JumpStart.Data.Auditing.AuditableNamedEntity
/// {
///     [System.ComponentModel.DataAnnotations.StringLength(500)]
///     public string? Description { get; set; }
///     public int DisplayOrder { get; set; }
///     public bool IsActive { get; set; } = true;
/// }
/// </code>
/// </example>
/// <seealso cref="AuditableEntity"/>
/// <seealso cref="INamed"/>
public abstract class AuditableNamedEntity : AuditableEntity, INamed
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// This is a required property that identifies the entity in a human-readable way.
    /// </summary>
    /// <value>
    /// A string representing the entity's name. This should be set by application code
    /// and is typically required (non-null).
    /// </value>
    /// <remarks>
    /// <para>
    /// The Name property serves as the primary human-readable identifier for the entity.
    /// Unlike the Id which is system-generated, the Name is typically user-provided and
    /// represents the entity in user interfaces and reports.
    /// </para>
    /// <para>
    /// Best practices for Name property:
    /// - Should be unique within the entity's context when appropriate (e.g., category names)
    /// - Consider adding validation attributes in concrete classes ([Required], [StringLength])
    /// - Use for sorting and display purposes
    /// - Can be used in search and filter operations
    /// </para>
    /// <para>
    /// Database considerations:
    /// - Typically indexed for performance in search operations
    /// - Consider adding unique constraints if business rules require uniqueness
    /// - May need case-insensitive collation for user-friendly searches
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Category : JumpStart.Data.Auditing.AuditableNamedEntity
    /// {
    ///     // Override to add validation
    ///     [System.ComponentModel.DataAnnotations.Required]
    ///     [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
    ///     public override string Name { get; set; } = null!;
    /// }
    /// </code>
    /// </example>
    public string Name { get; set; } = null!;
}
