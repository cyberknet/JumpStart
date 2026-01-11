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
/// Defines the contract for entities with Guid identifiers.
/// This is the recommended interface for most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="JumpStart.Data.Advanced.IEntity{T}"/> with Guid as the identifier type, providing
/// a simplified API for the common case of Guid-based entity identification. It serves as a marker
/// interface with no additional members beyond the Id property inherited from IEntity{Guid}.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="JumpStart.Data.Advanced.IEntity{T}"/> which requires specifying a type parameter, this interface
/// uses Guid throughout. This simplifies the API and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern ORM tools and databases handle Guid efficiently
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// - Natural fit for modern cloud-native architectures
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// Inherited from IEntity{Guid}:
/// - Id (Guid) - The unique identifier for the entity
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface (or a base class that implements it) when:
/// - Building new applications with modern architecture
/// - Guid identifiers are suitable for your domain
/// - You want simplified API without generic type parameters
/// - Working with distributed systems or microservices
/// - Client-side ID generation is beneficial
/// - Database-agnostic design is desired
/// </para>
/// <para>
/// <strong>Common Implementations:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="JumpStart.Data.SimpleEntity"/> - Basic entity with Guid identifier
/// - <see cref="JumpStart.Data.SimpleNamedEntity"/> - Adds Name property
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - Adds full audit tracking
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> - Combines naming and audit tracking
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If your application requires custom key types (int, long, custom struct) instead of Guid, use the
/// Advanced namespace generic interface <see cref="JumpStart.Data.Advanced.IEntity{T}"/> directly.
/// </para>
/// <para>
/// <strong>Marker Interface Pattern:</strong>
/// This interface serves as a type alias/marker interface, simplifying generic constraints and method
/// signatures. It enables code to work specifically with Guid-identified entities without the verbosity
/// of IEntity{Guid}.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing ISimpleEntity
/// public class Product : ISimpleEntity
/// {
///     public Guid Id { get; set; }
///     
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class Order : SimpleEntity
/// {
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
/// 
/// // Example 3: Generic repository for simple entities
/// public class SimpleRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TEntity?&gt; GetByIdAsync(Guid id)
///     {
///         return await _context.Set&lt;TEntity&gt;().FindAsync(id);
///     }
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         // Generate new Guid if not set
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
///     public async Task&lt;bool&gt; ExistsAsync(Guid id)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .AnyAsync(e => e.Id == id);
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
/// 
/// // Example 4: Service layer using ISimpleEntity
/// public class EntityService&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity
/// {
///     private readonly SimpleRepository&lt;TEntity&gt; _repository;
///     
///     public async Task&lt;TEntity?&gt; GetAsync(Guid id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
///     
///     public async Task&lt;TEntity&gt; CreateAsync(TEntity entity)
///     {
///         return await _repository.AddAsync(entity);
///     }
/// }
/// 
/// // Example 5: Polymorphic collections
/// public class EntityManager
/// {
///     public bool IsNew(ISimpleEntity entity)
///     {
///         return entity.Id == Guid.Empty;
///     }
///     
///     public void DisplayEntityInfo(ISimpleEntity entity)
///     {
///         Console.WriteLine($"Entity Type: {entity.GetType().Name}");
///         Console.WriteLine($"Entity ID: {entity.Id}");
///     }
///     
///     public List&lt;Guid&gt; GetAllIds&lt;TEntity&gt;(IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : ISimpleEntity
///     {
///         return entities.Select(e => e.Id).ToList();
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
/// // Example 7: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure Guid key with default value generation
///         modelBuilder.Entity&lt;Product&gt;()
///             .Property(p => p.Id)
///             .HasDefaultValueSql("NEWID()"); // SQL Server
///             // or .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.IEntity{T}"/>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.SimpleNamedEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
public interface ISimpleEntity : IEntity<Guid>
{
    // This interface intentionally contains no members beyond those inherited from IEntity<Guid>.
    // It serves as a type alias/marker interface to simplify the API by removing the need for
    // generic type parameters when using the recommended Guid-based entity identifiers.
}
