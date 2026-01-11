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
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Data.Advanced;

/// <summary>
/// Provides a base implementation of the <see cref="JumpStart.Data.Advanced.IEntity{T}"/> interface for entities with a unique identifier.
/// This abstract class serves as the foundation for all entities in the JumpStart framework that require custom key types.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Must be a value type (struct) such as int, long, Guid, or custom struct.</typeparam>
/// <remarks>
/// <para>
/// This class provides the core identity functionality for entities in the JumpStart framework. It implements
/// the <see cref="JumpStart.Data.Advanced.IEntity{T}"/> interface and provides a strongly-typed Id property decorated with the
/// <see cref="KeyAttribute"/> for automatic recognition by Entity Framework Core and other ORMs.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - You need custom key types (int, long, Guid, or custom structs)
/// - You want explicit control over the identifier type
/// - You're working with legacy databases with specific key types
/// - You need different key types for different entities in the same application
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// For simpler scenarios, consider these alternatives:
/// - <see cref="JumpStart.Data.SimpleEntity"/> - Uses Guid identifiers (recommended for new applications)
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> - Adds full audit tracking (creation, modification, deletion)
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - Guid identifiers with full audit tracking
/// - <see cref="JumpStart.Data.Advanced.NamedEntity{T}"/> - Adds Name property for entities that need human-readable names
/// </para>
/// <para>
/// <strong>Key Type Considerations:</strong>
/// - int: Traditional auto-increment keys, simple and widely supported
/// - long: Large-scale systems with billions of records
/// - Guid: Distributed systems, no central coordination needed, globally unique
/// - Custom struct: Domain-specific identifiers (e.g., composite keys wrapped in a struct)
/// </para>
/// <para>
/// <strong>Entity Framework Core Integration:</strong>
/// The <see cref="KeyAttribute"/> on the Id property ensures automatic recognition as the primary key.
/// EF Core will automatically configure the appropriate database column type and constraints based on
/// the generic type parameter T.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Entity{T} is the base for several specialized entity types in the framework:
/// - AuditableEntity{T} extends this with audit fields
/// - NamedEntity{T} extends this with a Name property
/// - AuditableNamedEntity{T} combines both naming and auditing
/// All inherit the strongly-typed Id property and IEntity{T} implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity with int identifier
/// public class Product : Entity&lt;int&gt;
/// {
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     public string? Description { get; set; }
/// }
/// 
/// // Example 2: Entity with long identifier for large-scale systems
/// public class Transaction : Entity&lt;long&gt;
/// {
///     public DateTime TransactionDate { get; set; }
///     public decimal Amount { get; set; }
///     public string Type { get; set; } = string.Empty;
/// }
/// 
/// // Example 3: Entity with Guid identifier
/// public class Customer : Entity&lt;Guid&gt;
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public DateTime RegisteredDate { get; set; }
/// }
/// 
/// // Example 4: Using entities with repositories
/// public class ProductRepository
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;Product&gt; GetByIdAsync(int id)
///     {
///         return await _context.Set&lt;Product&gt;().FindAsync(id);
///     }
///     
///     public async Task&lt;Product&gt; AddAsync(Product product)
///     {
///         _context.Set&lt;Product&gt;().Add(product);
///         await _context.SaveChangesAsync();
///         // Id is automatically set by database (for int with auto-increment)
///         return product;
///     }
/// }
/// 
/// // Example 5: Generic repository using IEntity
/// public class GenericRepository&lt;TEntity, TKey&gt; 
///     where TEntity : Entity&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     
///     public GenericRepository(DbContext context)
///     {
///         _context = context;
///     }
///     
///     public async Task&lt;TEntity?&gt; GetByIdAsync(TKey id)
///     {
///         return await _context.Set&lt;TEntity&gt;().FindAsync(id);
///     }
///     
///     public async Task&lt;IEnumerable&lt;TEntity&gt;&gt; GetAllAsync()
///     {
///         return await _context.Set&lt;TEntity&gt;().ToListAsync();
///     }
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;TEntity&gt; UpdateAsync(TEntity entity)
///     {
///         _context.Set&lt;TEntity&gt;().Update(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task DeleteAsync(TKey id)
///     {
///         var entity = await GetByIdAsync(id);
///         if (entity != null)
///         {
///             _context.Set&lt;TEntity&gt;().Remove(entity);
///             await _context.SaveChangesAsync();
///         }
///     }
/// }
/// 
/// // Example 6: EF Core DbContext configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
///     public DbSet&lt;Customer&gt; Customers { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Entity&lt;int&gt; - auto-increment primary key
///         modelBuilder.Entity&lt;Product&gt;()
///             .Property(p => p.Id)
///             .ValueGeneratedOnAdd(); // Database generates value
///         
///         // Entity&lt;Guid&gt; - application-generated or database default
///         modelBuilder.Entity&lt;Customer&gt;()
///             .Property(c => c.Id)
///             .HasDefaultValueSql("NEWID()"); // SQL Server example
///     }
/// }
/// 
/// // Example 7: Polymorphic usage with IEntity
/// public class EntityService
/// {
///     public bool HasValidId&lt;TKey&gt;(IEntity&lt;TKey&gt; entity) where TKey : struct
///     {
///         return !EqualityComparer&lt;TKey&gt;.Default.Equals(entity.Id, default);
///     }
///     
///     public void DisplayEntityInfo&lt;TKey&gt;(IEntity&lt;TKey&gt; entity) where TKey : struct
///     {
///         Console.WriteLine($"Entity ID: {entity.Id}");
///         Console.WriteLine($"Entity Type: {entity.GetType().Name}");
///     }
/// }
/// 
/// // Example 8: Custom struct as key type
/// public struct OrderNumber
/// {
///     public int Year { get; set; }
///     public int Sequence { get; set; }
///     
///     public override string ToString() => $"{Year}-{Sequence:D6}";
/// }
/// 
/// public class Order : Entity&lt;OrderNumber&gt;
/// {
///     public DateTime OrderDate { get; set; }
///     public decimal TotalAmount { get; set; }
///     
///     // EF Core requires configuration for custom struct keys
///     // Configure in OnModelCreating with owned type or value converter
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.IEntity{T}"/>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.NamedEntity{T}"/>
public abstract class Entity<T> : IEntity<T> where T : struct
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// This property is marked with the <see cref="KeyAttribute"/> to indicate it is the primary key.
    /// </summary>
    /// <value>
    /// The unique identifier of type <typeparamref name="T"/>. The value is typically set by the
    /// database for auto-increment keys (int, long) or by the application for Guid keys.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="KeyAttribute"/> decoration ensures this property is recognized as the primary key
    /// by Entity Framework Core and other ORMs without requiring additional configuration in most cases.
    /// </para>
    /// <para>
    /// <strong>Key Generation Strategies by Type:</strong>
    /// - int/long: Typically database-generated with auto-increment/identity
    /// - Guid: Can be application-generated (Guid.NewGuid()) or database-generated (NEWSEQUENTIALID())
    /// - Custom struct: Requires explicit configuration and value generation logic
    /// </para>
    /// <para>
    /// <strong>Best Practices:</strong>
    /// - For int/long: Let database generate values (configure ValueGeneratedOnAdd in EF Core)
    /// - For Guid: Generate in application before saving (ensures value exists for navigation properties)
    /// - Never manually set for auto-increment keys
    /// - Always check for default value to determine if entity is new or existing
    /// </para>
    /// <para>
    /// <strong>Usage in Repositories:</strong>
    /// Use the Id property to determine if an entity is new (Id == default) or existing (Id != default).
    /// This is particularly important for insert vs update logic in generic repositories.
    /// </para>
    /// </remarks>
    [Key]
    public T Id { get; set; }
}
