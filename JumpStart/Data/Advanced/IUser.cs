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
/// Defines the base contract for all user entities in the system with custom key types.
/// User entities represent authenticated users who can perform auditable actions.
/// This marker interface distinguishes user entities from other entities for audit tracking and user context operations.
/// </summary>
/// <typeparam name="TKey">The type of the user's primary key. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IEntity{T}"/> to provide type-safe identification of user entities
/// throughout the JumpStart framework. It serves as a marker interface with no additional members,
/// allowing the framework to distinguish user entities from other entities in generic code, particularly
/// for audit tracking where user information is recorded.
/// </para>
/// <para>
/// <strong>Purpose and Design:</strong>
/// The primary purpose of this interface is to enable type-safe user identification in audit tracking systems.
/// When audit fields like CreatedById, ModifiedById, or DeletedById are populated, they reference entities
/// that implement IUser{TKey}. This provides compile-time safety and enables navigation properties to user entities.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from a base class that implements it) when:
/// - Creating user/account entities that can perform auditable actions
/// - Building authentication and authorization systems
/// - Tracking who performed specific operations (audit trails)
/// - Custom key types are required (int, long, Guid, custom struct)
/// - You need explicit control over the user identifier type
/// </para>
/// <para>
/// <strong>Alternative Options:</strong>
/// For simpler scenarios, consider:
/// - <see cref="Data.IUser"/> - User interface with Guid key (recommended for new applications)
/// - <see cref="SimpleUser"/> - Concrete user entity with Guid key and common properties
/// - Custom user base classes that implement IUser{TKey} for specific requirements
/// </para>
/// <para>
/// <strong>Audit Tracking Integration:</strong>
/// This interface is fundamental to the audit tracking system. Audit interfaces like ICreatable{T},
/// IModifiable{T}, and IDeletable{T} use the same TKey type parameter, which typically corresponds
/// to the user's identifier type. This ensures type safety when recording who performed operations:
/// - CreatedById references a user implementing IUser{TKey}
/// - ModifiedById references a user implementing IUser{TKey}
/// - DeletedById references a user implementing IUser{TKey}
/// </para>
/// <para>
/// <strong>Generic Constraints:</strong>
/// The struct constraint (where TKey : struct) ensures the key type is a value type, providing:
/// - No null reference exceptions for the Id
/// - Better performance through stack allocation
/// - Clear semantics (default value vs populated)
/// - Type safety in generic repository patterns
/// </para>
/// <para>
/// <strong>Typical Implementation Pattern:</strong>
/// User entities typically include additional properties such as Username, Email, PasswordHash,
/// FirstName, LastName, IsActive, Roles, etc. The interface itself is minimal by design,
/// allowing flexibility in how user entities are structured while maintaining type safety for identification.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple user entity with int identifier
/// public class User : IUser&lt;int&gt;
/// {
///     public int Id { get; set; }
///     
///     [Required]
///     [StringLength(50)]
///     public string Username { get; set; } = string.Empty;
///     
///     [Required]
///     [EmailAddress]
///     public string Email { get; set; } = string.Empty;
///     
///     public string PasswordHash { get; set; } = string.Empty;
///     public bool IsActive { get; set; } = true;
///     public DateTime CreatedDate { get; set; }
/// }
/// 
/// // Example 2: User entity with Guid identifier
/// public class ApplicationUser : IUser&lt;Guid&gt;
/// {
///     public Guid Id { get; set; }
///     public string Username { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public string FirstName { get; set; } = string.Empty;
///     public string LastName { get; set; } = string.Empty;
///     public List&lt;string&gt; Roles { get; set; } = new();
/// }
/// 
/// // Example 3: Auditable entity with user references
/// public class Document : IEntity&lt;int&gt;, ICreatable&lt;int&gt;, IModifiable&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     
///     // Audit fields referencing User entity
///     public int CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     public int? ModifiedById { get; set; }
///     public DateTime? ModifiedOn { get; set; }
///     
///     // Navigation properties to User
///     public User? CreatedBy { get; set; }
///     public User? ModifiedBy { get; set; }
/// }
/// 
/// // Example 4: User service with generic user type
/// public class UserService&lt;TUser, TKey&gt; 
///     where TUser : class, IUser&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TUser?&gt; GetByIdAsync(TKey id)
///     {
///         return await _context.Set&lt;TUser&gt;().FindAsync(id);
///     }
///     
///     public async Task&lt;TUser?&gt; GetByUsernameAsync(string username)
///     {
///         // Assumes user entity has Username property
///         return await _context.Set&lt;TUser&gt;()
///             .FirstOrDefaultAsync(u => 
///                 EF.Property&lt;string&gt;(u, "Username") == username);
///     }
///     
///     public async Task&lt;bool&gt; IsUserActiveAsync(TKey userId)
///     {
///         var user = await GetByIdAsync(userId);
///         return user != null &amp;&amp; 
///                EF.Property&lt;bool&gt;(user, "IsActive");
///     }
/// }
/// 
/// // Example 5: Current user service
/// public interface ICurrentUserService&lt;TKey&gt; where TKey : struct
/// {
///     TKey GetUserId();
///     Task&lt;IUser&lt;TKey&gt;?&gt; GetCurrentUserAsync();
///     bool IsAuthenticated();
/// }
/// 
/// public class CurrentUserService : ICurrentUserService&lt;int&gt;
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private readonly DbContext _context;
///     
///     public int GetUserId()
///     {
///         var userId = _httpContextAccessor.HttpContext?.User
///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         return int.TryParse(userId, out var id) ? id : 0;
///     }
///     
///     public async Task&lt;IUser&lt;int&gt;?&gt; GetCurrentUserAsync()
///     {
///         var userId = GetUserId();
///         if (userId == 0) return null;
///         
///         return await _context.Set&lt;User&gt;().FindAsync(userId);
///     }
///     
///     public bool IsAuthenticated()
///     {
///         return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
///     }
/// }
/// 
/// // Example 6: Repository with automatic audit population
/// public class AuditableRepository&lt;TEntity, TKey&gt; 
///     where TEntity : class, IEntity&lt;TKey&gt;, IAuditable&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     private readonly ICurrentUserService&lt;TKey&gt; _currentUserService;
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         // Automatically set audit fields from current user
///         entity.CreatedById = _currentUserService.GetUserId();
///         entity.CreatedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;TEntity&gt; UpdateAsync(TEntity entity)
///     {
///         // Automatically update modification audit fields
///         entity.ModifiedById = _currentUserService.GetUserId();
///         entity.ModifiedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Update(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
/// }
/// 
/// // Example 7: EF Core navigation property configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;User&gt; Users { get; set; }
///     public DbSet&lt;Document&gt; Documents { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure navigation properties to User
///         modelBuilder.Entity&lt;Document&gt;()
///             .HasOne(d => d.CreatedBy)
///             .WithMany()
///             .HasForeignKey(d => d.CreatedById)
///             .OnDelete(DeleteBehavior.Restrict);
///         
///         modelBuilder.Entity&lt;Document&gt;()
///             .HasOne(d => d.ModifiedBy)
///             .WithMany()
///             .HasForeignKey(d => d.ModifiedById)
///             .OnDelete(DeleteBehavior.Restrict);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.IEntity{T}"/>
/// <seealso cref="JumpStart.Data.ISimpleUser"/>
/// <seealso cref="JumpStart.Data.ISimpleUser"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IModifiable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/>
public interface IUser<TKey> : IEntity<TKey> where TKey : struct
{
    // This interface intentionally contains no members beyond those inherited from IEntity<TKey>.
    // It serves as a marker interface to distinguish user entities from other entities,
    // enabling type-safe user identification in audit tracking and user context operations.
}
