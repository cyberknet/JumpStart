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
/// Rather than implementing this interface directly, consider using these base classes:
/// - <see cref="SimpleNamedEntity"/> - Basic named entity with Guid identifier
/// - <see cref="Advanced.NamedEntity{T}"/> - Named entity with custom key type
/// - <see cref="Auditing.SimpleAuditableNamedEntity"/> - Named entity with full audit tracking
/// - <see cref="Advanced.Auditing.AuditableNamedEntity{T}"/> - Named entity with custom key type and audit
/// </para>
/// <para>
/// <strong>Validation Considerations:</strong>
/// While this interface includes data annotation attributes for documentation purposes, validation
/// should be enforced in concrete implementations. Add [Required] and [StringLength] attributes
/// to the Name property in concrete classes to ensure proper validation by frameworks like
/// ASP.NET Core MVC, Entity Framework Core, and validation libraries.
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
/// // Example 1: Simple entity implementing INamed
/// public class Category : IEntity&lt;int&gt;, INamed
/// {
///     public int Id { get; set; }
///     
///     [Required]
///     [StringLength(200, MinimumLength = 1)]
///     public string Name { get; set; } = string.Empty;
///     
///     public string? Description { get; set; }
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class Department : SimpleNamedEntity
/// {
///     [StringLength(500)]
///     public string? Description { get; set; }
///     
///     public string? Location { get; set; }
///     public int EmployeeCount { get; set; }
/// }
/// 
/// // Example 3: Generic service working with any named entity
/// public class NamedEntityService
/// {
///     public List&lt;string&gt; GetAllNames&lt;TEntity&gt;(IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : INamed
///     {
///         return entities.Select(e => e.Name).OrderBy(n => n).ToList();
///     }
///     
///     public TEntity? FindByName&lt;TEntity&gt;(
///         IEnumerable&lt;TEntity&gt; entities,
///         string name,
///         StringComparison comparison = StringComparison.OrdinalIgnoreCase)
///         where TEntity : INamed
///     {
///         return entities.FirstOrDefault(e => e.Name.Equals(name, comparison));
///     }
///     
///     public Dictionary&lt;string, TEntity&gt; ToDictionary&lt;TEntity&gt;(IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : INamed
///     {
///         return entities.ToDictionary(e => e.Name);
///     }
/// }
/// 
/// // Example 4: Repository methods for named entities
/// public class NamedEntityRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity, INamed
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TEntity?&gt; GetByNameAsync(string name)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .FirstOrDefaultAsync(e => e.Name == name);
///     }
///     
///     public async Task&lt;List&lt;TEntity&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .Where(e => e.Name.Contains(searchTerm))
///             .OrderBy(e => e.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;bool&gt; NameExistsAsync(string name, Guid? excludeId = null)
///     {
///         var query = _context.Set&lt;TEntity&gt;().Where(e => e.Name == name);
///         
///         if (excludeId.HasValue)
///         {
///             query = query.Where(e => e.Id != excludeId.Value);
///         }
///         
///         return await query.AnyAsync();
///     }
/// }
/// 
/// // Example 5: Dropdown/Select list generation
/// public class SelectListService
/// {
///     public List&lt;SelectListItem&gt; CreateSelectList&lt;TEntity&gt;(
///         IEnumerable&lt;TEntity&gt; entities,
///         Func&lt;TEntity, string&gt; valueSelector)
///         where TEntity : INamed
///     {
///         return entities
///             .OrderBy(e => e.Name)
///             .Select(e => new SelectListItem
///             {
///                 Value = valueSelector(e),
///                 Text = e.Name
///             })
///             .ToList();
///     }
/// }
/// 
/// // Example 6: Polymorphic collection usage
/// public class EntityDisplayService
/// {
///     public void DisplayEntities(IEnumerable&lt;INamed&gt; entities)
///     {
///         foreach (var entity in entities.OrderBy(e => e.Name))
///         {
///             Console.WriteLine($"- {entity.Name}");
///         }
///     }
///     
///     public List&lt;INamed&gt; FilterByNamePattern(
///         IEnumerable&lt;INamed&gt; entities,
///         string pattern)
///     {
///         var regex = new Regex(pattern, RegexOptions.IgnoreCase);
///         return entities.Where(e => regex.IsMatch(e.Name)).ToList();
///     }
/// }
/// 
/// // Example 7: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Generic configuration for all INamed entities
///         foreach (var entityType in modelBuilder.Model.GetEntityTypes())
///         {
///             if (typeof(INamed).IsAssignableFrom(entityType.ClrType))
///             {
///                 // Require Name property
///                 modelBuilder.Entity(entityType.ClrType)
///                     .Property(nameof(INamed.Name))
///                     .IsRequired()
///                     .HasMaxLength(255);
///                 
///                 // Add index for search performance
///                 modelBuilder.Entity(entityType.ClrType)
///                     .HasIndex(nameof(INamed.Name));
///                 
///                 // Optional: Case-insensitive collation (SQL Server example)
///                 modelBuilder.Entity(entityType.ClrType)
///                     .Property(nameof(INamed.Name))
///                     .UseCollation("SQL_Latin1_General_CP1_CI_AS");
///             }
///         }
///     }
/// }
/// 
/// // Example 8: Validation in concrete class
/// public class ProductType : SimpleNamedEntity
/// {
///     [Required(ErrorMessage = "Product type name is required")]
///     [StringLength(200, MinimumLength = 2, 
///         ErrorMessage = "Name must be between 2 and 200 characters")]
///     [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", 
///         ErrorMessage = "Name can only contain letters, numbers, spaces, and hyphens")]
///     public override string Name { get; set; } = string.Empty;
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.SimpleNamedEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.NamedEntity{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableNamedEntity{T}"/>
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
