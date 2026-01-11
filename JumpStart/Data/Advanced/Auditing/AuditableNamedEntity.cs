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

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that have a name and require full audit tracking with custom key types.
/// This class must be inherited by concrete entity classes that need both naming and comprehensive audit trail functionality.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key and user identifier. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This class combines three essential patterns in a single base class:
/// - Entity identification through <see cref="JumpStart.Data.Advanced.Entity{T}"/> (provides Id property)
/// - Full audit tracking through <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> (creation, modification, soft deletion)
/// - Named entity pattern through <see cref="JumpStart.Data.INamed"/> (provides Name property)
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when your entities:
/// - Need a Name property as a key business attribute
/// - Require full audit trail (who created/modified/deleted and when)
/// - Use custom key types (int, long, Guid, or custom struct)
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
/// From <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>:
/// - Id (from Entity{T})
/// - CreatedById, CreatedOn
/// - ModifiedById, ModifiedOn
/// - DeletedById, DeletedOn
/// 
/// From <see cref="JumpStart.Data.INamed"/>:
/// - Name (this class adds this property)
/// </para>
/// <para>
/// <strong>Repository Management:</strong>
/// All audit fields are automatically populated by the repository layer. The Name property
/// should be set by application code, while Id and audit fields are system-managed.
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// - Use <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> for Guid identifiers (simpler, recommended for new apps)
/// - Use <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> if naming is not a core requirement
/// - Use <see cref="JumpStart.Data.Advanced.NamedEntity{T}"/> for named entities without audit tracking
/// - Use <see cref="JumpStart.Data.SimpleNamedEntity"/> for Guid-based named entities without audit
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Category entity with int identifier
/// public class Category : AuditableNamedEntity&lt;int&gt;
/// {
///     [StringLength(500)]
///     public string? Description { get; set; }
///     
///     public int DisplayOrder { get; set; }
///     
///     public bool IsActive { get; set; } = true;
/// }
/// 
/// // Example 2: ProductType entity with long identifier
/// public class ProductType : AuditableNamedEntity&lt;long&gt;
/// {
///     [StringLength(50)]
///     public string Code { get; set; } = string.Empty;
///     
///     public string? IconUrl { get; set; }
/// }
/// 
/// // Example 3: Department entity with Guid identifier
/// public class Department : AuditableNamedEntity&lt;Guid&gt;
/// {
///     public Guid? ParentDepartmentId { get; set; }
///     
///     public Department? ParentDepartment { get; set; }
///     
///     public List&lt;Department&gt; ChildDepartments { get; set; } = new();
///     
///     [StringLength(100)]
///     public string? ManagerName { get; set; }
/// }
/// 
/// // Example 4: Creating and using a named auditable entity
/// var category = new Category
/// {
///     Name = "Electronics",
///     Description = "Electronic devices and accessories",
///     DisplayOrder = 1,
///     IsActive = true
/// };
/// 
/// // Repository automatically sets audit fields on save
/// var saved = await repository.AddAsync(category);
/// // saved.Id is now assigned
/// // saved.CreatedById and CreatedOn are set from current user context
/// 
/// Console.WriteLine($"Category '{saved.Name}' created by user {saved.CreatedById} on {saved.CreatedOn}");
/// 
/// // Example 5: Querying named entities with audit filtering
/// var activeCategories = await dbContext.Categories
///     .Where(c => c.IsActive)
///     .Where(c => c.DeletedOn == null) // Exclude soft-deleted
///     .OrderBy(c => c.Name)
///     .ToListAsync();
/// 
/// // Example 6: Soft delete with audit trail
/// var categoryToDelete = await repository.GetByIdAsync(categoryId);
/// if (categoryToDelete != null)
/// {
///     await repository.SoftDeleteAsync(categoryToDelete);
///     // DeletedById and DeletedOn are automatically set
///     Console.WriteLine($"Category '{categoryToDelete.Name}' deleted by user {categoryToDelete.DeletedById}");
/// }
/// 
/// // Example 7: Audit history for a named entity
/// var department = await dbContext.Departments
///     .FirstOrDefaultAsync(d => d.Name == "Engineering");
/// 
/// if (department != null)
/// {
///     Console.WriteLine($"Department: {department.Name}");
///     Console.WriteLine($"Created by {department.CreatedById} on {department.CreatedOn:yyyy-MM-dd}");
///     
///     if (department.ModifiedOn.HasValue)
///     {
///         Console.WriteLine($"Last modified by {department.ModifiedById} on {department.ModifiedOn:yyyy-MM-dd}");
///     }
///     
///     if (department.DeletedOn.HasValue)
///     {
///         Console.WriteLine($"Deleted by {department.DeletedById} on {department.DeletedOn:yyyy-MM-dd}");
///     }
/// }
/// 
/// // Example 8: Search by name with audit constraints
/// var recentlyCreatedTypes = await dbContext.ProductTypes
///     .Where(pt => pt.Name.Contains(searchTerm))
///     .Where(pt => pt.CreatedOn &gt; DateTime.UtcNow.AddMonths(-1))
///     .Where(pt => pt.DeletedOn == null)
///     .OrderBy(pt => pt.Name)
///     .ToListAsync();
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
/// <seealso cref="JumpStart.Data.INamed"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
public abstract class AuditableNamedEntity<T> : AuditableEntity<T>, INamed
    where T : struct
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
    /// public class Category : AuditableNamedEntity&lt;int&gt;
    /// {
    ///     // Override to add validation
    ///     [Required]
    ///     [StringLength(100, MinimumLength = 2)]
    ///     public override string Name { get; set; } = null!;
    /// }
    /// </code>
    /// </example>
    public string Name { get; set; } = null!;
}
