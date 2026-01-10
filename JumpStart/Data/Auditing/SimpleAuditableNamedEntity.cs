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
/// Provides an abstract base implementation for entities that have a human-readable name and require full audit tracking with Guid identifiers.
/// This is the recommended base class for named, auditable entities in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class combines three key aspects of entity management:
/// - Entity identification with Guid (from <see cref="SimpleEntity"/>)
/// - Human-readable naming (implements <see cref="INamed"/>)
/// - Complete audit trail (from <see cref="SimpleAuditableEntity"/>)
/// </para>
/// <para>
/// It is ideal for master data, lookup tables, categories, and reference data that need both
/// user-friendly names and comprehensive audit tracking.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Inherits from <see cref="SimpleAuditableEntity"/> (which inherits from <see cref="SimpleEntity"/>)
/// and implements <see cref="INamed"/>. This provides:
/// - Id property (Guid) from SimpleEntity
/// - Six audit properties from SimpleAuditableEntity (CreatedById, CreatedOn, ModifiedById, ModifiedOn, DeletedById, DeletedOn)
/// - Name property from INamed
/// Total of 8 inherited/implemented properties.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// This class uses Guid for both entity IDs and user identifiers, simplifying the API compared to the
/// generic <see cref="Advanced.Auditing.AuditableNamedEntity{T}"/>. This is the recommended approach for new
/// applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - Simplified API with no generic type parameters
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// All audit fields are automatically populated by repository implementations when configured with
/// an <see cref="Repositories.ISimpleUserContext"/>. The Name property must be set by application code.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities need:
/// - Guid identifier (Id)
/// - Human-readable name for display and search
/// - Full audit trail (creation, modification, soft deletion tracking)
/// - Soft delete functionality
/// - Simplified API without generic type parameters
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// This class is ideal for:
/// - Categories with audit tracking (Product Categories, Document Categories)
/// - Types and Statuses with history (Order Types, Task Statuses, Priority Levels)
/// - Master data with compliance requirements (Departments, Locations, Regions)
/// - Lookup tables requiring audit trails (Countries, Industries, Job Titles)
/// - Reference data with regulatory tracking
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// Consider these alternatives based on your requirements:
/// - <see cref="SimpleNamedEntity"/> - Named entity without audit tracking
/// - <see cref="SimpleAuditableEntity"/> - Full audit tracking without naming
/// - <see cref="SimpleEntity"/> - Basic entity with just Guid Id
/// - <see cref="Advanced.Auditing.AuditableNamedEntity{T}"/> - For custom key types (int, long, custom struct)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Category with audit tracking
/// public class ProductCategory : SimpleAuditableNamedEntity
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
///     // - CreatedById, CreatedOn
///     // - ModifiedById, ModifiedOn
///     // - DeletedById, DeletedOn
/// }
/// 
/// // Example 2: Status type with audit trail
/// public class OrderStatus : SimpleAuditableNamedEntity
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
/// // Example 3: Repository usage with automatic audit
/// public class CategoryService
/// {
///     private readonly SimpleRepository&lt;ProductCategory&gt; _repository;
///     
///     public async Task&lt;ProductCategory&gt; CreateCategoryAsync(string name, string description)
///     {
///         var category = new ProductCategory
///         {
///             Name = name,
///             Description = description,
///             DisplayOrder = 0
///             // Audit fields automatically set by repository
///         };
///         
///         return await _repository.AddAsync(category);
///     }
///     
///     public async Task&lt;ProductCategory&gt; UpdateCategoryAsync(Guid id, string name)
///     {
///         var category = await _repository.GetByIdAsync(id);
///         if (category != null)
///         {
///             category.Name = name;
///             // ModifiedById and ModifiedOn automatically set
///             return await _repository.UpdateAsync(category);
///         }
///         throw new NotFoundException($"Category {id} not found");
///     }
/// }
/// 
/// // Example 4: Querying by name with audit filtering
/// public class CategoryQueryService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;ProductCategory&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Categories
///             .Where(c => c.DeletedOn == null) // Active only
///             .Where(c => c.Name.Contains(searchTerm))
///             .OrderBy(c => c.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;ProductCategory?&gt; GetByNameAsync(string name)
///     {
///         return await _context.Categories
///             .Where(c => c.DeletedOn == null)
///             .FirstOrDefaultAsync(c => c.Name == name);
///     }
///     
///     public async Task&lt;bool&gt; NameExistsAsync(string name, Guid? excludeId = null)
///     {
///         var query = _context.Categories
///             .Where(c => c.DeletedOn == null)
///             .Where(c => c.Name == name);
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
/// // Example 5: Dropdown list generation
/// public class CategoryDropdownService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;SelectListItem&gt;&gt; GetCategorySelectListAsync()
///     {
///         return await _context.Categories
///             .Where(c => c.DeletedOn == null &amp;&amp; c.IsActive)
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
///         // Add unique constraint on Name
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .HasIndex(c => c.Name)
///             .IsUnique()
///             .HasFilter("[DeletedOn] IS NULL"); // Only for active records
///         
///         // Global query filter for soft deletes
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .HasQueryFilter(c => c.DeletedOn == null);
///         
///         // Navigation to user who created
///         modelBuilder.Entity&lt;ProductCategory&gt;()
///             .HasOne&lt;SimpleUser&gt;()
///             .WithMany()
///             .HasForeignKey(c => c.CreatedById)
///             .OnDelete(DeleteBehavior.Restrict);
///     }
/// }
/// 
/// // Example 7: Audit report with names
/// public class AuditReportService
/// {
///     public string GenerateNamedEntityAuditTrail(SimpleAuditableNamedEntity entity)
///     {
///         var trail = $"Entity: {entity.Name} (ID: {entity.Id})";
///         trail += $"\nCreated by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         
///         if (entity.ModifiedOn.HasValue)
///         {
///             trail += $"\nLast modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         
///         if (entity.DeletedOn.HasValue)
///         {
///             trail += $"\nDeleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         
///         return trail;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Data.SimpleNamedEntity"/>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.INamed"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableNamedEntity{T}"/>
/// <seealso cref="JumpStart.Repositories.SimpleRepository{TEntity}"/>
public abstract class SimpleAuditableNamedEntity : SimpleAuditableEntity, INamed
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
    /// - Add unique constraints if names must be unique within active entities
    /// - Index the column for search performance
    /// - Use case-insensitive collation for user-friendly searches
    /// - Validate for special characters if needed
    /// </para>
    /// <para>
    /// <strong>Validation Example:</strong>
    /// In concrete implementations, add validation attributes:
    /// </para>
    /// <code>
    /// [Required(ErrorMessage = "Name is required")]
    /// [StringLength(200, MinimumLength = 1)]
    /// public override string Name { get; set; } = null!;
    /// </code>
    /// </remarks>
    public string Name { get; set; } = null!;
}
