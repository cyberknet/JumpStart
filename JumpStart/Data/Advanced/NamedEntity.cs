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

namespace JumpStart.Data.Advanced;

/// <summary>
/// Provides an abstract base implementation for entities that have both a unique identifier and a name.
/// This class combines entity identification with human-readable naming, making it ideal for lookup tables,
/// reference data, and master data entities.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="Entity{T}"/> to add a Name property through the <see cref="INamed"/> interface.
/// It provides the foundation for entities that need both system-generated unique identifiers and
/// human-readable names. This pattern is common in master data, lookup tables, categories, types,
/// and other reference data where users need to identify entities by name.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities need:
/// - A unique identifier (Id property from Entity{T})
/// - A human-readable name for display and identification
/// - Custom key types (int, long, Guid, or custom struct)
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
/// - <see cref="SimpleNamedEntity"/> - Uses Guid identifiers (simpler, recommended for new apps)
/// - <see cref="Auditing.AuditableNamedEntity{T}"/> - Adds full audit tracking to named entities
/// - <see cref="SimpleAuditableNamedEntity"/> - Guid identifiers with naming and full audit tracking
/// - <see cref="Entity{T}"/> - If naming is not required
/// </para>
/// <para>
/// <strong>Properties Provided:</strong>
/// - Id (from Entity{T}) - The unique identifier
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
/// // Example 1: Simple category entity with int identifier
/// public class Category : NamedEntity&lt;int&gt;
/// {
///     [StringLength(500)]
///     public string? Description { get; set; }
///     
///     public int DisplayOrder { get; set; }
///     
///     public bool IsActive { get; set; } = true;
/// }
/// 
/// // Example 2: Status type with long identifier
/// public class Status : NamedEntity&lt;long&gt;
/// {
///     [StringLength(50)]
///     public string Code { get; set; } = string.Empty;
///     
///     [StringLength(20)]
///     public string? Color { get; set; }
///     
///     public bool IsFinal { get; set; }
/// }
/// 
/// // Example 3: Country entity with Guid identifier
/// public class Country : NamedEntity&lt;Guid&gt;
/// {
///     [StringLength(2)]
///     public string Iso2Code { get; set; } = string.Empty;
///     
///     [StringLength(3)]
///     public string Iso3Code { get; set; } = string.Empty;
///     
///     public string? Region { get; set; }
/// }
/// 
/// // Example 4: Using named entities in repositories
/// public class CategoryRepository
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;Category&gt;&gt; GetAllAsync()
///     {
///         return await _context.Set&lt;Category&gt;()
///             .OrderBy(c => c.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;Category?&gt; GetByNameAsync(string name)
///     {
///         return await _context.Set&lt;Category&gt;()
///             .FirstOrDefaultAsync(c => c.Name == name);
///     }
///     
///     public async Task&lt;List&lt;Category&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Set&lt;Category&gt;()
///             .Where(c => c.Name.Contains(searchTerm))
///             .OrderBy(c => c.Name)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 5: Dropdown list/select list usage
/// public class CategoryService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;SelectListItem&gt;&gt; GetCategorySelectListAsync()
///     {
///         return await _context.Set&lt;Category&gt;()
///             .Where(c => c.IsActive)
///             .OrderBy(c => c.Name)
///             .Select(c => new SelectListItem
///             {
///                 Value = c.Id.ToString(),
///                 Text = c.Name
///             })
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 6: Generic service for any named entity
/// public class NamedEntityService&lt;TEntity, TKey&gt;
///     where TEntity : NamedEntity&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TEntity?&gt; GetByIdAsync(TKey id)
///     {
///         return await _context.Set&lt;TEntity&gt;().FindAsync(id);
///     }
///     
///     public async Task&lt;TEntity?&gt; GetByNameAsync(string name)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .FirstOrDefaultAsync(e => e.Name == name);
///     }
///     
///     public async Task&lt;List&lt;TEntity&gt;&gt; GetAllOrderedByNameAsync()
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .OrderBy(e => e.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;bool&gt; NameExistsAsync(string name, TKey? excludeId = null)
///     {
///         var query = _context.Set&lt;TEntity&gt;().Where(e => e.Name == name);
///         
///         if (excludeId.HasValue)
///         {
///             query = query.Where(e => !e.Id.Equals(excludeId.Value));
///         }
///         
///         return await query.AnyAsync();
///     }
/// }
/// 
/// // Example 7: Validation with data annotations
/// public class ProductType : NamedEntity&lt;int&gt;
/// {
///     [Required(ErrorMessage = "Name is required")]
///     [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
///     public override string Name { get; set; } = null!;
///     
///     [StringLength(500)]
///     public string? Description { get; set; }
/// }
/// 
/// // Example 8: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;Category&gt; Categories { get; set; }
///     public DbSet&lt;Status&gt; Statuses { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure Name property
///         modelBuilder.Entity&lt;Category&gt;()
///             .Property(c => c.Name)
///             .IsRequired()
///             .HasMaxLength(200);
///         
///         // Add unique constraint on Name
///         modelBuilder.Entity&lt;Category&gt;()
///             .HasIndex(c => c.Name)
///             .IsUnique();
///         
///         // Configure for case-insensitive searches (SQL Server example)
///         modelBuilder.Entity&lt;Category&gt;()
///             .Property(c => c.Name)
///             .UseCollation("SQL_Latin1_General_CP1_CI_AS");
///     }
/// }
/// 
/// // Example 9: Polymorphic queries using INamed
/// public class NamedEntityManager
/// {
///     public List&lt;string&gt; GetAllNames&lt;TEntity, TKey&gt;(IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : NamedEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         return entities.Select(e => e.Name).OrderBy(n => n).ToList();
///     }
///     
///     public TEntity? FindByName&lt;TEntity, TKey&gt;(
///         IEnumerable&lt;TEntity&gt; entities,
///         string name,
///         StringComparison comparison = StringComparison.OrdinalIgnoreCase)
///         where TEntity : NamedEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         return entities.FirstOrDefault(e => 
///             e.Name.Equals(name, comparison));
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Entity{T}"/>
/// <seealso cref="JumpStart.Data.INamed"/>
/// <seealso cref="JumpStart.Data.SimpleNamedEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableNamedEntity{T}"/>
public abstract class NamedEntity<T> : Entity<T>, INamed
    where T : struct
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
