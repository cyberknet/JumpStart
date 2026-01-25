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

namespace JumpStart.Repositories;

/// <summary>
/// Provides access to the current authenticated user's identifier for audit tracking with custom key types.
/// This interface defines the contract for services that retrieve the currently authenticated user's ID.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong>
/// User context implementations bridge the gap between the authentication system and the audit tracking
/// system. They retrieve the current user's ID from the authentication mechanism and make it available
/// to repositories for automatic audit field population.
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
/// // Example 1: ASP.NET Core implementation
/// public class HttpUserContext : IUserContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     
///     public HttpUserContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         var userIdClaim = _httpContextAccessor.HttpContext?.User
///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         
///         if (Guid.TryParse(userIdClaim, out var userId))
///         {
///             return Task.FromResult&lt;Guid?&gt;(userId);
///         }
///         
///         return Task.FromResult&lt;Guid?&gt;(null);
///     }
/// }
/// 
/// // Example 2: Blazor Server implementation
/// public class BlazorUserContext : IUserContext
/// {
///     private readonly AuthenticationStateProvider _authStateProvider;
///     
///     public BlazorUserContext(AuthenticationStateProvider authStateProvider)
///     {
///         _authStateProvider = authStateProvider;
///     }
///     
///     public async Task&lt;Guid?&gt; GetCurrentUserIdAsync()
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
///         if (Guid.TryParse(userIdClaim, out var userId))
///         {
///             return userId;
///         }
///         
///         return null;
///     }
/// }
/// 
/// // Example 3: Background service implementation
/// public class BackgroundUserContext : IUserContext
/// {
///     private static readonly AsyncLocal&lt;Guid?&gt; _currentUserId = new();
///     
///     public static void SetCurrentUserId(Guid userId)
///     {
///         _currentUserId.Value = userId;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         return Task.FromResult(_currentUserId.Value);
///     }
/// }
/// 
/// // Example 4: Registration in Startup/Program
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
///     services.AddScoped&lt;IUserContext, HttpUserContext&gt;();
/// }
/// 
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.IUserContext"/>
public interface IUserContext
{
    /// <summary>
    /// Asynchronously retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The user's identifier (<typeref name="Guid"/>) if a user is authenticated
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
    /// public class CachedUserContext : IUserContext&lt;Guid&gt;
    /// {
    ///     private readonly IHttpContextAccessor _httpContextAccessor;
    ///     private Guid? _cachedUserId;
    ///     private bool _hasRetrieved;
    ///     
    ///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
    ///     {
    ///         if (_hasRetrieved)
    ///         {
    ///             return Task.FromResult(_cachedUserId);
    ///         }
    ///         
    ///         var userIdClaim = _httpContextAccessor.HttpContext?.User
    ///             .FindFirst(ClaimTypes.NameIdentifier)?.Value;
    ///         
    ///         _cachedUserId = Guid.TryParse(userIdClaim, out var userId) 
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
    Task<Guid?> GetCurrentUserIdAsync();
}
