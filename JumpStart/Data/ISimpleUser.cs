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
/// Defines the contract for user entities with Guid identifiers in the system.
/// This is the recommended interface for user entities in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="JumpStart.Data.Advanced.IUser{T}"/> with Guid as the user identifier type, providing
/// a simplified API for the common case of Guid-based user identities. User entities represent
/// authenticated users who can perform auditable actions throughout the application.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="JumpStart.Data.Advanced.IUser{T}"/> which requires specifying a type parameter, this interface
/// uses Guid throughout. This simplifies the API and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// - Natural fit for modern authentication and authorization systems
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// Inherited from <see cref="IUser{Guid}"/> and <see cref="IEntity{Guid}"/>:
/// - Id (Guid) - The unique identifier for the user
/// This interface is a marker interface with no additional properties, relying on concrete
/// implementations to add user-specific properties like Username, Email, etc.
/// </para>
/// <para>
/// <strong>Purpose in Audit System:</strong>
/// User entities implementing this interface are fundamental to the audit tracking system. Audit
/// fields like CreatedById, ModifiedById, and DeletedById reference users implementing ISimpleUser.
/// This provides type-safe identification of who performed operations throughout the application:
/// - CreatedById references ISimpleUser (who created the entity)
/// - ModifiedById references ISimpleUser (who last modified the entity)
/// - DeletedById references ISimpleUser (who soft-deleted the entity)
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from a base class that implements it) when:
/// - Creating user/account entities for authentication and authorization
/// - Building systems that track who performs operations
/// - Integrating with ASP.NET Core Identity or similar Guid-based auth systems
/// - User entities need to be referenced in audit trails
/// - Simplified API without generic type parameters is preferred
/// </para>
/// <para>
/// <strong>Common Implementations:</strong>
/// Rather than implementing this interface directly, consider:
/// - Creating a custom User class with application-specific properties (Username, Email, etc.)
/// - Using SimpleUser base class if available in your application
/// - Extending SimpleEntity and implementing ISimpleUser with user-specific properties
/// </para>
/// <para>
/// <strong>Typical User Properties:</strong>
/// Concrete implementations typically include properties such as:
/// - Username or LoginName (unique identifier for login)
/// - Email (for communication and sometimes login)
/// - PasswordHash (for authentication)
/// - FirstName, LastName (for display)
/// - IsActive, IsLocked (for account management)
/// - Roles or Claims (for authorization)
/// - LastLoginDate, CreatedDate (for tracking)
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If your application uses custom key types (int, long, custom struct) instead of Guid for user IDs,
/// use the Advanced namespace generic interface <see cref="JumpStart.Data.Advanced.IUser{T}"/> directly.
/// </para>
/// <para>
/// <strong>Marker Interface Pattern:</strong>
/// This interface serves as a type alias/marker interface to distinguish user entities from other
/// entities in the system. This enables type-safe operations and clear separation between user
/// entities and data entities, which is essential for audit trails and security.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple user entity
/// public class ApplicationUser : ISimpleEntity, ISimpleUser
/// {
///     public Guid Id { get; set; }
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
///     
///     [StringLength(100)]
///     public string? FirstName { get; set; }
///     
///     [StringLength(100)]
///     public string? LastName { get; set; }
///     
///     public bool IsActive { get; set; } = true;
///     public DateTime CreatedDate { get; set; }
///     public DateTime? LastLoginDate { get; set; }
/// }
/// 
/// // Example 2: User with roles
/// public class User : ISimpleEntity, ISimpleUser
/// {
///     public Guid Id { get; set; }
///     public string Username { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public string PasswordHash { get; set; } = string.Empty;
///     
///     public List&lt;string&gt; Roles { get; set; } = new();
///     public List&lt;string&gt; Claims { get; set; } = new();
/// }
/// 
/// // Example 3: User context service
/// public interface ICurrentUserService
/// {
///     Guid GetUserId();
///     Task&lt;ISimpleUser?&gt; GetCurrentUserAsync();
///     bool IsAuthenticated();
///     string GetUsername();
/// }
/// 
/// public class CurrentUserService : ICurrentUserService
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private readonly DbContext _context;
///     
///     public Guid GetUserId()
///     {
///         var userId = _httpContextAccessor.HttpContext?.User
///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
///     }
///     
///     public async Task&lt;ISimpleUser?&gt; GetCurrentUserAsync()
///     {
///         var userId = GetUserId();
///         if (userId == Guid.Empty) return null;
///         
///         return await _context.Set&lt;ApplicationUser&gt;().FindAsync(userId);
///     }
///     
///     public bool IsAuthenticated()
///     {
///         return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
///     }
///     
///     public string GetUsername()
///     {
///         return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
///     }
/// }
/// 
/// // Example 4: Auditable entity with user references
/// public class Document : SimpleAuditableEntity
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     
///     // Navigation properties to user entities
///     public ApplicationUser? CreatedBy { get; set; }
///     public ApplicationUser? ModifiedBy { get; set; }
///     public ApplicationUser? DeletedBy { get; set; }
/// }
/// 
/// // Example 5: EF Core configuration with user navigation properties
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;ApplicationUser&gt; Users { get; set; }
///     public DbSet&lt;Document&gt; Documents { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Configure user entity
///         modelBuilder.Entity&lt;ApplicationUser&gt;()
///             .HasIndex(u => u.Username)
///             .IsUnique();
///         
///         modelBuilder.Entity&lt;ApplicationUser&gt;()
///             .HasIndex(u => u.Email)
///             .IsUnique();
///         
///         // Configure navigation properties to user
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
/// 
/// // Example 6: User repository
/// public class UserRepository
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;ApplicationUser?&gt; GetByUsernameAsync(string username)
///     {
///         return await _context.Set&lt;ApplicationUser&gt;()
///             .FirstOrDefaultAsync(u => u.Username == username &amp;&amp; u.IsActive);
///     }
///     
///     public async Task&lt;ApplicationUser?&gt; GetByEmailAsync(string email)
///     {
///         return await _context.Set&lt;ApplicationUser&gt;()
///             .FirstOrDefaultAsync(u => u.Email == email &amp;&amp; u.IsActive);
///     }
///     
///     public async Task&lt;bool&gt; UsernameExistsAsync(string username, Guid? excludeId = null)
///     {
///         var query = _context.Set&lt;ApplicationUser&gt;()
///             .Where(u => u.Username == username);
///         
///         if (excludeId.HasValue)
///         {
///             query = query.Where(u => u.Id != excludeId.Value);
///         }
///         
///         return await query.AnyAsync();
///     }
/// }
/// 
/// // Example 7: Authentication service
/// public class AuthenticationService
/// {
///     private readonly UserRepository _userRepository;
///     private readonly IPasswordHasher&lt;ApplicationUser&gt; _passwordHasher;
///     
///     public async Task&lt;ApplicationUser?&gt; AuthenticateAsync(string username, string password)
///     {
///         var user = await _userRepository.GetByUsernameAsync(username);
///         if (user == null || !user.IsActive)
///             return null;
///         
///         var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
///         if (result == PasswordVerificationResult.Success)
///         {
///             user.LastLoginDate = DateTime.UtcNow;
///             return user;
///         }
///         
///         return null;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.IUser{T}"/>
/// <seealso cref="JumpStart.Data.ISimpleEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
public interface ISimpleUser : IUser<Guid>
{
    // This interface intentionally contains no members beyond those inherited from IUser<Guid>.
    // It serves as a type alias/marker interface to simplify the API by removing the need for
    // generic type parameters when using the recommended Guid-based user identifiers.
    // Concrete implementations should add user-specific properties like Username, Email, etc.
}
