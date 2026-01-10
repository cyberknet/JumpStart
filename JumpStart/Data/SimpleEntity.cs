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
using JumpStart.Data.Advanced;

namespace JumpStart.Data;

/// <summary>
/// Provides an abstract base implementation for entities with Guid identifiers.
/// This is the recommended base class for most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a simplified base for entity implementations by using Guid as the identifier type,
/// removing the need for generic type parameters. It inherits from <see cref="Entity{T}"/> with Guid
/// and implements <see cref="ISimpleEntity"/> to provide the standard entity contract.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="Entity{T}"/> which requires specifying a type parameter, this class
/// uses Guid throughout. This simplifies inheritance and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern ORM tools and databases handle Guid efficiently
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// - Natural fit for modern cloud-native architectures
/// - Simplified API without generic type parameters
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Inherits from Entity{Guid} (which implements IEntity{Guid}) and implements ISimpleEntity.
/// This provides the Id property of type Guid and all standard entity operations.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - Building new applications with modern architecture
/// - Guid identifiers are suitable for your domain
/// - You want simplified inheritance without generic type parameters
/// - No audit tracking is required (for audit tracking, use <see cref="Auditing.SimpleAuditableEntity"/>)
/// - No naming is required (for naming, use <see cref="SimpleNamedEntity"/>)
/// - Working with distributed systems or microservices
/// - Database-agnostic design is desired
/// </para>
/// <para>
/// <strong>Enhanced Base Classes:</strong>
/// Consider these alternatives based on your requirements:
/// - <see cref="SimpleNamedEntity"/> - Adds Name property
/// - <see cref="Auditing.SimpleAuditableEntity"/> - Adds full audit tracking (creation, modification, deletion)
/// - <see cref="Auditing.SimpleAuditableNamedEntity"/> - Combines naming and full audit tracking
/// - <see cref="Entity{T}"/> - For custom key types (int, long, custom struct)
/// </para>
/// <para>
/// <strong>Properties Provided:</strong>
/// - Id (Guid) - The unique identifier inherited from Entity{Guid}
/// The Id property has a default value of Guid.Empty. Set it to a new Guid value before
/// saving to the database, either manually or let the repository/ORM generate it.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple product entity
/// public class Product : SimpleEntity
/// {
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     [StringLength(1000)]
///     public string? Description { get; set; }
///     
///     public bool IsActive { get; set; } = true;
/// }
/// 
/// // Example 2: Order entity with relationships
/// public class Order : SimpleEntity
/// {
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     public string Status { get; set; } = "Pending";
///     
///     public Guid CustomerId { get; set; }
///     public Customer? Customer { get; set; }
///     
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
/// 
/// // Example 3: Repository usage with automatic Guid generation
/// public class ProductRepository
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;Product&gt; AddAsync(Product product)
///     {
///         // Generate new Guid if not already set
///         if (product.Id == Guid.Empty)
///         {
///             product.Id = Guid.NewGuid();
///         }
///         
///         _context.Products.Add(product);
///         await _context.SaveChangesAsync();
///         return product;
///     }
///     
///     public async Task&lt;Product?&gt; GetByIdAsync(Guid id)
///     {
///         return await _context.Products.FindAsync(id);
///     }
///     
///     public async Task&lt;List&lt;Product&gt;&gt; GetAllAsync()
///     {
///         return await _context.Products
///             .Where(p => p.IsActive)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 4: Service layer
/// public class ProductService
/// {
///     private readonly ProductRepository _repository;
///     
///     public async Task&lt;Product&gt; CreateProductAsync(string name, decimal price)
///     {
///         var product = new Product
///         {
///             // Id will be auto-generated by repository
///             Name = name,
///             Price = price,
///             IsActive = true
///         };
///         
///         return await _repository.AddAsync(product);
///     }
///     
///     public async Task&lt;Product?&gt; GetProductAsync(Guid id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
/// }
/// 
/// // Example 5: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure Guid key with default value generation
///         modelBuilder.Entity&lt;Product&gt;()
///             .Property(p => p.Id)
///             .HasDefaultValueSql("NEWID()"); // SQL Server
///             // or .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL
///             // or .HasDefaultValueSql("uuid_generate_v4()"); // PostgreSQL with uuid-ossp
///         
///         // Add indexes
///         modelBuilder.Entity&lt;Product&gt;()
///             .HasIndex(p => p.Name);
///     }
/// }
/// 
/// // Example 6: Client-side ID generation
/// var product = new Product
/// {
///     Id = Guid.NewGuid(), // Generate before saving
///     Name = "Laptop",
///     Price = 999.99m
/// };
/// await repository.AddAsync(product);
/// 
/// // Example 7: Generic repository for SimpleEntity
/// public class SimpleRepository&lt;TEntity&gt;
///     where TEntity : SimpleEntity
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
///     public async Task&lt;TEntity?&gt; GetByIdAsync(Guid id)
///     {
///         return await _context.Set&lt;TEntity&gt;().FindAsync(id);
///     }
///     
///     public async Task UpdateAsync(TEntity entity)
///     {
///         _context.Set&lt;TEntity&gt;().Update(entity);
///         await _context.SaveChangesAsync();
///     }
///     
///     public async Task DeleteAsync(Guid id)
///     {
///         var entity = await GetByIdAsync(id);
///         if (entity != null)
///         {
///             _context.Set&lt;TEntity&gt;().Remove(entity);
///             await _context.SaveChangesAsync();
///         }
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="Entity{T}"/>
/// <seealso cref="ISimpleEntity"/>
/// <seealso cref="SimpleNamedEntity"/>
/// <seealso cref="Auditing.SimpleAuditableEntity"/>
/// <seealso cref="Auditing.SimpleAuditableNamedEntity"/>
public abstract class SimpleEntity : Entity<Guid>, ISimpleEntity
{
    // This class intentionally contains no members beyond those inherited from Entity<Guid>.
    // It serves as a simplified base class that removes the need for generic type parameters
    // when using the recommended Guid-based entity identifiers.
}
