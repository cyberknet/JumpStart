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
using System.Threading.Tasks;

namespace JumpStart.Repositories.Advanced;

/// <summary>
/// Provides access to the current authenticated user's identifier for audit tracking with custom key types.
/// This interface defines the contract for services that retrieve the currently authenticated user's ID.
/// </summary>
/// <typeparam name="TKey">
/// The type of the user's primary key. Must be a value type (struct) such as int, long, Guid, or custom structs.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is part of the Advanced namespace for applications requiring custom user key types
/// beyond Guid. It enables automatic population of audit fields (CreatedById, ModifiedById, DeletedById)
/// in repository operations by providing access to the current user's identifier.
/// </para>
/// <para>
/// <strong>Purpose:</strong>
/// User context implementations bridge the gap between the authentication system and the audit tracking
/// system. They retrieve the current user's ID from the authentication mechanism and make it available
/// to repositories for automatic audit field population.
/// </para>
/// <para>
/// <strong>For Guid Keys:</strong>
/// For most applications using Guid identifiers (recommended), use <see cref="JumpStart.Repositories.ISimpleUserContext"/> 
/// instead, which provides the same functionality without generic type parameters for a simpler API.
/// </para>
/// <para>
/// <strong>Common Implementation Sources:</strong>
/// Implementations typically retrieve the current user from:
/// - <strong>Web Applications:</strong> HttpContext.User claims (ASP.NET Core)
/// - <strong>Blazor Server:</strong> AuthenticationStateProvider
/// - <strong>Blazor WebAssembly:</strong> AuthenticationStateProvider with JWT
/// - <strong>Background Services:</strong> Thread principal or scoped context
/// - <strong>Desktop Applications:</strong> Windows identity or custom authentication
/// - <strong>Test Scenarios:</strong> Mock implementations with fixed user IDs
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface when:
/// - Your application uses int, long, or custom struct keys for user identifiers
/// - You need audit tracking with non-Guid user IDs
/// - You're working with existing databases using non-Guid user keys
/// - You want explicit control over generic type parameters
/// </para>
/// <para>
/// <strong>Registration:</strong>
/// Register your implementation during dependency injection setup:
/// <code>
/// services.AddJumpStart(options =>
/// {
///     options.RegisterUserContext&lt;MyUserContext&gt;();
/// });
/// </code>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// Implementations should be thread-safe as they may be called concurrently in web applications.
/// Consider using AsyncLocal or scoped services to maintain per-request user context.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: ASP.NET Core implementation with int keys
/// public class HttpUserContext : IUserContext&lt;int&gt;
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     
///     public HttpUserContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;int?&gt; GetCurrentUserIdAsync()
///     {
///         var userIdClaim = _httpContextAccessor.HttpContext?.User
///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         
///         if (int.TryParse(userIdClaim, out var userId))
///         {
///             return Task.FromResult&lt;int?&gt;(userId);
///         }
///         
///         return Task.FromResult&lt;int?&gt;(null);
///     }
/// }
/// 
/// // Example 2: Blazor Server implementation with long keys
/// public class BlazorUserContext : IUserContext&lt;long&gt;
/// {
///     private readonly AuthenticationStateProvider _authStateProvider;
///     
///     public BlazorUserContext(AuthenticationStateProvider authStateProvider)
///     {
///         _authStateProvider = authStateProvider;
///     }
///     
///     public async Task&lt;long?&gt; GetCurrentUserIdAsync()
///     {
///         var authState = await _authStateProvider.GetAuthenticationStateAsync();
///         var user = authState.User;
///         
///         if (!user.Identity?.IsAuthenticated ?? false)
///         {
///             return null;
///         }
///         
///         var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         if (long.TryParse(userIdClaim, out var userId))
///         {
///             return userId;
///         }
///         
///         return null;
///     }
/// }
/// 
/// // Example 3: Test implementation with fixed user ID
/// public class TestUserContext : IUserContext&lt;int&gt;
/// {
///     private readonly int _userId;
///     
///     public TestUserContext(int userId = 1)
///     {
///         _userId = userId;
///     }
///     
///     public Task&lt;int?&gt; GetCurrentUserIdAsync()
///     {
///         return Task.FromResult&lt;int?&gt;(_userId);
///     }
/// }
/// 
/// // Example 4: Background service implementation
/// public class BackgroundUserContext : IUserContext&lt;int&gt;
/// {
///     private static readonly AsyncLocal&lt;int?&gt; _currentUserId = new();
///     
///     public static void SetCurrentUserId(int userId)
///     {
///         _currentUserId.Value = userId;
///     }
///     
///     public Task&lt;int?&gt; GetCurrentUserIdAsync()
///     {
///         return Task.FromResult(_currentUserId.Value);
///     }
/// }
/// 
/// // Example 5: Using user context in a repository
/// public class AuditableRepository&lt;TEntity, TKey&gt; 
///     where TEntity : class, IEntity&lt;TKey&gt;, ICreatable&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     private readonly IUserContext&lt;TKey&gt; _userContext;
///     
///     public AuditableRepository(DbContext context, IUserContext&lt;TKey&gt; userContext)
///     {
///         _context = context;
///         _userContext = userContext;
///     }
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         // Automatically populate audit fields
///         entity.CreatedOn = DateTime.UtcNow;
///         entity.CreatedById = await _userContext.GetCurrentUserIdAsync();
///         
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
/// }
/// 
/// // Example 6: Registration in Startup/Program
/// public void ConfigureServices(IServiceCollection services)
/// {
///     // Register HttpContextAccessor (required for web apps)
///     services.AddHttpContextAccessor();
///     
///     // Register JumpStart with user context
///     services.AddJumpStart(options =>
///     {
///         options.RegisterUserContext&lt;HttpUserContext&gt;();
///         options.ScanAssembly(typeof(Program).Assembly);
///     });
///     
///     // Or manually register user context
///     services.AddScoped&lt;IUserContext&lt;int&gt;, HttpUserContext&gt;();
/// }
/// 
/// // Example 7: Conditional user context (unauthenticated scenarios)
/// public class ConditionalUserContext : IUserContext&lt;int&gt;
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private const int SystemUserId = 0; // Default for system operations
///     
///     public ConditionalUserContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;int?&gt; GetCurrentUserIdAsync()
///     {
///         var httpContext = _httpContextAccessor.HttpContext;
///         
///         // Check if user is authenticated
///         if (httpContext?.User?.Identity?.IsAuthenticated != true)
///         {
///             // Return system user for unauthenticated operations
///             return Task.FromResult&lt;int?&gt;(SystemUserId);
///         }
///         
///         var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         if (int.TryParse(userIdClaim, out var userId))
///         {
///             return Task.FromResult&lt;int?&gt;(userId);
///         }
///         
///         return Task.FromResult&lt;int?&gt;(null);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
public interface IUserContext<TKey> where TKey : struct
{
    /// <summary>
    /// Asynchronously retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The user's identifier (<typeparamref name="TKey"/>) if a user is authenticated
    /// - <c>null</c> if no user is authenticated or the user ID cannot be determined
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called by repositories during create, update, and delete operations to
    /// automatically populate audit fields. Implementations should:
    /// - Be lightweight and fast (avoid database calls if possible)
    /// - Return null for unauthenticated requests
    /// - Cache the user ID per request/scope when possible
    /// - Handle exceptions gracefully (return null rather than throw)
    /// </para>
    /// <para>
    /// <strong>Return Value Guidelines:</strong>
    /// - Return the user's ID if authenticated and ID is available
    /// - Return null if not authenticated (anonymous requests)
    /// - Return null if authentication state cannot be determined
    /// - Some implementations may return a "system user" ID (e.g., 0) for background operations
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// This method may be called multiple times per request. Consider caching the result
    /// within the current scope to avoid redundant authentication state lookups.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// Implementations must be thread-safe as this method may be called concurrently.
    /// Use AsyncLocal, scoped services, or HttpContext which are inherently per-request.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example implementation with caching
    /// public class CachedUserContext : IUserContext&lt;int&gt;
    /// {
    ///     private readonly IHttpContextAccessor _httpContextAccessor;
    ///     private int? _cachedUserId;
    ///     private bool _hasRetrieved;
    ///     
    ///     public Task&lt;int?&gt; GetCurrentUserIdAsync()
    ///     {
    ///         if (_hasRetrieved)
    ///         {
    ///             return Task.FromResult(_cachedUserId);
    ///         }
    ///         
    ///         var userIdClaim = _httpContextAccessor.HttpContext?.User
    ///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
    ///         
    ///         _cachedUserId = int.TryParse(userIdClaim, out var userId) 
    ///             ? userId 
    ///             : null;
    ///         _hasRetrieved = true;
    ///         
    ///         return Task.FromResult(_cachedUserId);
    ///     }
    /// }
    /// 
    /// // Example usage in repository
    /// var userId = await _userContext.GetCurrentUserIdAsync();
    /// if (userId.HasValue)
    /// {
    ///     entity.CreatedById = userId.Value;
    /// }
    /// </code>
    /// </example>
    Task<TKey?> GetCurrentUserIdAsync();
}
