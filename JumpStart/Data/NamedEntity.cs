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

namespace JumpStart.Data;

/// <summary>
/// Provides an abstract base implementation for entities that have both a unique identifier and a name.
/// This class combines entity identification with human-readable naming, making it ideal for lookup tables,
/// reference data, and master data entities.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="JumpStart.Data.Entity"/> to add a Name property through the <see cref="JumpStart.Data.INamed"/> interface.
/// It provides the foundation for entities that need both system-generated unique identifiers and
/// human-readable names. This pattern is common in master data, lookup tables, categories, types,
/// and other reference data where users need to identify entities by name.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities need:
/// - A unique identifier (Id property from Entity)
/// - A human-readable name for display and identification
/// - No audit tracking (creation/modification/deletion timestamps)
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// This is ideal for:
/// - Categories, Tags, Labels
/// - Types, Statuses, Priorities
/// - Countries, Regions, Cities
/// - Departments, Teams
/// - Product Types, Document Types
/// - Any lookup/reference data that needs a name
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// Consider these alternatives based on your requirements:
/// - <see cref="Data.Auditing.AuditableNamedEntity"/> - Adds full audit tracking to named entities
/// - <see cref="JumpStart.Data.Entity"/> - If naming is not required
/// </para>
/// <para>
/// <strong>Properties Provided:</strong>
/// - Id (from Entity) - The unique identifier
/// - Name (from INamed) - The human-readable name
/// Both properties are exposed for use in queries, sorting, filtering, and display.
/// </para>
/// <para>
/// <strong>Validation Considerations:</strong>
/// The Name property is marked as non-nullable (null!). In concrete implementations, consider adding
/// validation attributes such as [Required], [StringLength], or [RegularExpression] to enforce
/// business rules for the Name property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple category entity with Guid identifier
/// public class Category : JumpStart.Data.NamedEntity
/// {
///     [System.ComponentModel.DataAnnotations.StringLength(500)]
///     public string? Description { get; set; }
///     
///     public int DisplayOrder { get; set; }
///     
///     public bool IsActive { get; set; } = true;
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Entity"/>
/// <seealso cref="JumpStart.Data.INamed"/>
/// <seealso cref="Data.Auditing.AuditableNamedEntity"/>
public abstract class NamedEntity : Entity, INamed
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// This property provides human-readable identification for the entity.
    /// </summary>
    /// <value>
    /// A string representing the entity's name. This should be set by application code
    /// and is typically required (non-null).
    /// </value>
    /// <remarks>
    /// <para>
    /// The Name property serves as the primary human-readable identifier for the entity,
    /// distinguishing it from the system-generated Id. This property is ideal for:
    /// - Displaying in user interfaces (dropdowns, lists, grids)
    /// - Searching and filtering operations
    /// - Sorting entities alphabetically
    /// - User-friendly references in logs and reports
    /// </para>
    /// <para>
    /// <strong>Best Practices:</strong>
    /// - Consider adding [Required] attribute in concrete classes
    /// - Use [StringLength] to limit maximum length
    /// - Add unique constraints if names must be unique
    /// - Index the column for search performance
    /// - Use case-insensitive collation for user-friendly searches
    /// - Validate for special characters if needed
    /// </para>
    /// <para>
    /// <strong>Validation Example:</strong>
    /// In concrete implementations, override this property to add validation:
    /// </para>
    /// <code>
    /// [Required]
    /// [StringLength(200, MinimumLength = 1)]
    /// public override string Name { get; set; } = null!;
    /// </code>
    /// </remarks>
    public string Name { get; set; } = null!;
}
