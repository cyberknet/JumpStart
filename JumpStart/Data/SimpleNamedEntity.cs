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
/// Provides an abstract base implementation for entities that have both a Guid identifier and a human-readable name.
/// This is the recommended base class for named entities in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class combines two key aspects of entity management:
/// - Entity identification with Guid (from <see cref="SimpleEntity"/>)
/// - Human-readable naming (implements <see cref="INamed"/>)
/// </para>
/// <para>
/// It is ideal for master data, lookup tables, categories, and reference data that need both
/// system-generated Guid identifiers and user-friendly names.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Inherits from <see cref="SimpleEntity"/> (which inherits from <see cref="Advanced.Entity{T}"/> with Guid)
/// and implements <see cref="INamed"/>. This provides:
/// - Id property (Guid) from SimpleEntity
/// - Name property (string) from INamed
/// Total of 2 properties.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// This class uses Guid for entity IDs, simplifying the API compared to the generic
/// <see cref="Advanced.NamedEntity{T}"/>. This is the recommended approach for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - Simplified API with no generic type parameters
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities need:
/// - Guid identifier (Id)
/// - Human-readable name for display and search
/// - No audit tracking (for audit tracking, use <see cref="Auditing.SimpleAuditableNamedEntity"/>)
/// - Simplified API without generic type parameters
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// This class is ideal for:
/// - Categories (Product Categories, Document Categories)
/// - Types and Statuses (Order Types, Task Statuses, Priority Levels)
/// - Master data (Departments, Locations, Regions)
/// - Lookup tables (Countries, Industries, Job Titles)
/// - Reference data without audit requirements
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// Consider these alternatives based on your requirements:
/// - <see cref="SimpleEntity"/> - Entity with Guid Id without naming
/// - <see cref="Auditing.SimpleAuditableEntity"/> - Entity with full audit tracking but no naming
/// - <see cref="Auditing.SimpleAuditableNamedEntity"/> - Adds full audit tracking to named entities
/// - <see cref="Advanced.NamedEntity{T}"/> - For custom key types (int, long, custom struct)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Category entity
/// public class ProductCategory : SimpleNamedEntity
/// {
///     [StringLength(500)]
///     public string? Description { get; set; }
///     
///     public int DisplayOrder { get; set; }
///     
///     public bool IsActive { get; set; } = true;
///     
///     // Navigation properties
///     public List&lt;Product&gt; Products { get; set; } = new();
///     
///     // Inherited properties:
///     // - Id (Guid)
///     // - Name (string)
/// }
/// 
/// // Example 2: Status entity
/// public class OrderStatus : SimpleNamedEntity
/// {
///     [Required]
///     [StringLength(50)]
///     public string Code { get; set; } = string.Empty;
///     
///     [StringLength(20)]
///     public string? ColorCode { get; set; }
///     
///     public bool IsFinal { get; set; }
///     
///     public int Sequence { get; set; }
/// }
/// 
/// // Example 3: Repository with name-based operations
/// public class CategoryRepository
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;ProductCategory&gt; AddAsync(ProductCategory category)
///     {
///         if (category.Id == Guid.Empty)
///         {
///             category.Id = Guid.NewGuid();
///         }
///         
///         _context.Categories.Add(category);
///         await _context.SaveChangesAsync();
///         return category;
///     }
///     
///     public async Task&lt;ProductCategory?&gt; GetByNameAsync(string name)
///     {
///         return await _context.Categories
///             .FirstOrDefaultAsync(c => c.Name == name &amp;&amp; c.IsActive);
///     }
///     
///     public async Task&lt;List&lt;ProductCategory&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Categories
///             .Where(c => c.Name.Contains(searchTerm) &amp;&amp; c.IsActive)
///             .OrderBy(c => c.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;bool&gt; NameExistsAsync(string name, Guid? excludeId = null)
///     {
///         var query = _context.Categories
///             .Where(c => c.Name == name &amp;&amp; c.IsActive);
///         
///         if (excludeId.HasValue)
///         {
///             query = query.Where(c => c.Id != excludeId.Value);
///         }
///         
///         return await query.AnyAsync();
///     }
/// }
/// 
/// // Example 4: Service layer
/// public class CategoryService
/// {
///     private readonly CategoryRepository _repository;
///     
///     public async Task&lt;ProductCategory&gt; CreateCategoryAsync(string name, string? description)
///     {
///         // Validate unique name
///         if (await _repository.NameExistsAsync(name))
///         {
///             throw new InvalidOperationException($"Category '{name}' already exists");
///         }
///         
///         var category = new ProductCategory
///         {
///             Name = name,
///             Description = description,
///             IsActive = true
///         };
///         
///         return await _repository.AddAsync(category);
///     }
///     
///     public async Task&lt;List&lt;ProductCategory&gt;&gt; SearchCategoriesAsync(string searchTerm)
///     {
///         return await _repository.SearchByNameAsync(searchTerm);
///     }
/// }
/// 
/// // Example 5: Dropdown/Select list generation
/// public class SelectListService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;SelectListItem&gt;&gt; GetCategorySelectListAsync()
///     {
///         return await _context.Categories
///             .Where(c => c.IsActive)
///             .OrderBy(c => c.DisplayOrder)
///             .ThenBy(c => c.Name)
///             .Select(c => new SelectListItem
///             {
///                 Value = c.Id.ToString(),
///                 Text = c.Name
///             })
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 6: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;ProductCategory&gt; Categories { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure Name property
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .Property(c => c.Name)
///             .IsRequired()
///             .HasMaxLength(200);
///         
///         // Add unique constraint on Name for active categories
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .HasIndex(c => c.Name)
///             .IsUnique()
///             .HasFilter("[IsActive] = 1"); // SQL Server syntax
///         
///         // Add index for search performance
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .HasIndex(c => c.Name);
///     }
/// }
/// 
/// // Example 7: Generic repository for named entities
/// public class SimpleNamedRepository&lt;TEntity&gt;
///     where TEntity : SimpleNamedEntity
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         if (entity.Id == Guid.Empty)
///         {
///             entity.Id = Guid.NewGuid();
///         }
///         
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
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
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.INamed"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.NamedEntity{T}"/>
public abstract class SimpleNamedEntity : SimpleEntity, INamed
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
    /// - Add [Required] attribute in concrete classes if name is mandatory
    /// - Use [StringLength] to limit maximum length (e.g., 200 characters)
    /// - Add unique constraints if names must be unique
    /// - Index the column for search performance
    /// - Use case-insensitive collation for user-friendly searches
    /// - Validate for special characters if needed
    /// </para>
    /// </remarks>
    public string Name { get; set; } = null!;
}
