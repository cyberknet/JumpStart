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
/// Defines the contract for entities that have a human-readable name property.
/// This interface enables naming capabilities across different entity types in the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a standardized way to add naming capabilities to entities. It establishes 
/// a contract that entities must have a Name property, which serves as a human-readable identifier
/// distinguishing the entity from system-generated identifiers like Id.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from a base class that implements it) when entities need:
/// - Human-readable identification in user interfaces
/// - Text-based searching and filtering capabilities
/// - Alphabetical sorting in lists and dropdowns
/// - Display names in reports and logs
/// - User-friendly references distinct from numeric/Guid IDs
/// </para>
/// <para>
/// <strong>Common Implementations:</strong>
/// Rather than implementing this interface directly, use one of these base classes:
/// - <see cref="JumpStart.Data.NamedEntity"/> - Named entity with Guid key
/// - <see cref="Auditing.AuditableNamedEntity"/> - Named entity with audit tracking
/// </para>
/// <para>
/// <strong>Validation Considerations:</strong>
/// While this interface includes data annotation attributes for documentation purposes, validation
/// should be enforced in concrete implementations. Add [Required] and [StringLength] attributes to 
/// the Name property in concrete classes to ensure proper validation by frameworks like ASP.NET 
/// Core, Blazor, Entity Framework Core, and validation libraries.
/// </para>
/// <para>
/// <strong>Best Practices:</strong>
/// - Set maximum length appropriate for your use case (often 200-500 characters)
/// - Consider adding unique constraints in database for active entities
/// - Use case-insensitive collation for user-friendly searches
/// - Index the Name column for query performance
/// - Validate for special characters based on business requirements
/// - Trim whitespace before saving
/// - Consider internationalization and localization needs
/// </para>
/// <para>
/// <strong>Polymorphic Usage:</strong>
/// This interface enables polymorphic operations across different entity types. Methods can accept
/// INamed parameters to work with any named entity, and collections of INamed can include mixed
/// entity types, all unified by having a Name property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple entity implementing INamed
/// public class Category : JumpStart.Data.Entity, JumpStart.Data.INamed
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.StringLength(200, MinimumLength = 1)]
///     public string Name { get; set; } = string.Empty;
///     public string? Description { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.NamedEntity"/>
/// <seealso cref="Auditing.AuditableNamedEntity"/>
public interface INamed
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    /// <value>
    /// A string representing the human-readable name of the entity.
    /// Should be between 1 and 255 characters for most use cases.
    /// </value>
    /// <remarks>
    /// <para>
    /// The Name property serves as the primary human-readable identifier for entities implementing
    /// this interface. Unlike system-generated identifiers (Id properties), the Name is intended
    /// for display in user interfaces, reports, and logs.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong>
    /// While data annotation attributes are shown here for documentation, validation must be
    /// enforced in concrete implementations. Override this property in concrete classes and
    /// add appropriate validation attributes:
    /// </para>
    /// <code>
    /// [Required]
    /// [StringLength(200, MinimumLength = 1)]
    /// public override string Name { get; set; } = string.Empty;
    /// </code>
    /// <para>
    /// <strong>Common Operations:</strong>
    /// - Searching: Use Contains, StartsWith, EndsWith for partial matching
    /// - Sorting: Order entities alphabetically by Name
    /// - Filtering: Find entities matching specific name patterns
    /// - Uniqueness: Validate name uniqueness within a scope
    /// - Display: Show Name in dropdowns, lists, and reports
    /// </para>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// - Index the Name column in the database for efficient queries
    /// - Consider full-text search for complex name queries
    /// - Use case-insensitive collation for user-friendly searches
    /// - Cache frequently accessed named entities
    /// </para>
    /// <para>
    /// <strong>Internationalization:</strong>
    /// - Store names in the user's language or a neutral format
    /// - Consider separate properties for localized names if needed
    /// - Use Unicode-aware string comparisons
    /// - Normalize names consistently (e.g., trim whitespace)
    /// </para>
    /// </remarks>
    string Name { get; set; }
}
