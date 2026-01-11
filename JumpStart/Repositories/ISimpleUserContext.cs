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
using JumpStart.Repositories.Advanced;

namespace JumpStart.Repositories;

/// <summary>
/// Provides access to the current authenticated user's Guid identifier for audit tracking.
/// This is the recommended interface for most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a simplified API for user context operations by inheriting from 
/// <see cref="JumpStart.Repositories.Advanced.IUserContext{TKey}"/> with Guid as the fixed key type. This eliminates the need
/// for explicit generic key type parameters in most application code, making the API cleaner and easier to use.
/// </para>
/// <para>
/// <strong>Purpose:</strong>
/// User context implementations bridge the gap between the authentication system and the audit tracking
/// system. They retrieve the current user's ID from the authentication mechanism and make it available
/// to repositories for automatic audit field population (CreatedById, ModifiedById, DeletedById).
/// </para>
/// <para>
/// <strong>Why Use This Interface:</strong>
/// - Simplifies API by eliminating generic key type parameter
/// - Recommended for new applications (Guid is the preferred identifier type)
/// - Provides all functionality of IUserContext with cleaner syntax
/// - Works seamlessly with ISimpleEntity-based entities and SimpleRepository
/// - Reduces complexity in service layer and dependency injection
/// </para>
/// <para>
/// <strong>Common Implementation Sources:</strong>
/// Implementations typically retrieve the current user from:
/// - <strong>ASP.NET Core:</strong> HttpContext.User claims
/// - <strong>Blazor Server:</strong> AuthenticationStateProvider
/// - <strong>Blazor WebAssembly:</strong> AuthenticationStateProvider with JWT
/// - <strong>Background Services:</strong> Thread principal or scoped context
/// - <strong>Desktop Applications:</strong> Windows identity or custom authentication
/// - <strong>Test Scenarios:</strong> Mock implementations with fixed user IDs
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface when:
/// - Building new applications (recommended default)
/// - Using Guid as the user identifier type
/// - You want a simpler API without generic type parameters
/// - Working with entities that inherit from SimpleEntity or SimpleAuditableEntity
/// - Using SimpleRepository as your base repository
/// </para>
/// <para>
/// <strong>When to Use IUserContext Instead:</strong>
/// For applications requiring non-Guid user keys (int, long, custom structs), use 
/// <see cref="JumpStart.Repositories.Advanced.IUserContext{TKey}"/> directly which provides the same functionality
/// with explicit control over the key type.
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
/// Or manually:
/// <code>
/// services.AddScoped&lt;ISimpleUserContext, MyUserContext&gt;();
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
/// public class HttpUserContext : ISimpleUserContext
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
/// public class BlazorUserContext : ISimpleUserContext
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
///         if (user?.Identity?.IsAuthenticated != true)
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
/// // Example 3: Test implementation with fixed user ID
/// public class TestUserContext : ISimpleUserContext
/// {
///     private readonly Guid _userId;
///     
///     public TestUserContext(Guid? userId = null)
///     {
///         _userId = userId ?? Guid.NewGuid();
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         return Task.FromResult&lt;Guid?&gt;(_userId);
///     }
/// }
/// 
/// // Example 4: Background service implementation with AsyncLocal
/// public class BackgroundUserContext : ISimpleUserContext
/// {
///     private static readonly AsyncLocal&lt;Guid?&gt; _currentUserId = new();
///     
///     public static void SetCurrentUserId(Guid userId)
///     {
///         _currentUserId.Value = userId;
///     }
///     
///     public static void ClearCurrentUserId()
///     {
///         _currentUserId.Value = null;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         return Task.FromResult(_currentUserId.Value);
///     }
/// }
/// 
/// // Example 5: Using user context in a repository
/// public class ProductRepository : SimpleRepository&lt;Product&gt;
/// {
///     private readonly ISimpleUserContext _userContext;
///     
///     public ProductRepository(
///         DbContext context, 
///         ISimpleUserContext userContext) 
///         : base(context, userContext)
///     {
///         _userContext = userContext;
///     }
///     
///     public async Task&lt;Product&gt; CreateProductAsync(Product product)
///     {
///         // User context is automatically used by base repository
///         // to populate CreatedById field
///         return await AddAsync(product);
///     }
/// }
/// 
/// // Example 6: Registration in Startup/Program (ASP.NET Core)
/// public void ConfigureServices(IServiceCollection services)
/// {
///     // Register HttpContextAccessor (required for web apps)
///     services.AddHttpContextAccessor();
///     
///     // Register JumpStart with user context
///     services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
///         options => options.UseSqlServer(connectionString),
///         jumpStart => jumpStart.RegisterUserContext&lt;HttpUserContext&gt;());
/// }
/// 
/// // Example 7: Registration for Blazor Server
/// public void ConfigureServices(IServiceCollection services)
/// {
///     services.AddJumpStart(options =>
///     {
///         options.RegisterUserContext&lt;BlazorUserContext&gt;();
///         options.ScanAssembly(typeof(Program).Assembly);
///     });
/// }
/// 
/// // Example 8: Conditional user context (unauthenticated scenarios)
/// public class ConditionalUserContext : ISimpleUserContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private static readonly Guid SystemUserId = Guid.Empty; // System user
///     
///     public ConditionalUserContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         var httpContext = _httpContextAccessor.HttpContext;
///         
///         // Check if user is authenticated
///         if (httpContext?.User?.Identity?.IsAuthenticated != true)
///         {
///             // Return system user for unauthenticated operations
///             return Task.FromResult&lt;Guid?&gt;(SystemUserId);
///         }
///         
///         var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         if (Guid.TryParse(userIdClaim, out var userId))
///         {
///             return Task.FromResult&lt;Guid?&gt;(userId);
///         }
///         
///         return Task.FromResult&lt;Guid?&gt;(null);
///     }
/// }
/// 
/// // Example 9: Caching user context for performance
/// public class CachedUserContext : ISimpleUserContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private Guid? _cachedUserId;
///     private bool _hasRetrieved;
///     
///     public CachedUserContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
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
/// // Example 10: Using in a service layer
/// public class OrderService
/// {
///     private readonly IOrderRepository _orderRepository;
///     private readonly ISimpleUserContext _userContext;
///     
///     public OrderService(
///         IOrderRepository orderRepository,
///         ISimpleUserContext userContext)
///     {
///         _orderRepository = orderRepository;
///         _userContext = userContext;
///     }
///     
///     public async Task&lt;Order&gt; CreateOrderAsync(OrderDto orderDto)
///     {
///         var currentUserId = await _userContext.GetCurrentUserIdAsync();
///         
///         if (!currentUserId.HasValue)
///         {
///             throw new UnauthorizedAccessException("User must be authenticated");
///         }
///         
///         var order = new Order
///         {
///             CustomerId = currentUserId.Value,
///             TotalAmount = orderDto.TotalAmount
///             // CreatedById will be set automatically by repository
///         };
///         
///         return await _orderRepository.AddAsync(order);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.Advanced.IUserContext{TKey}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/>
/// <seealso cref="JumpStart.Repositories.SimpleRepository{TEntity}"/>
public interface ISimpleUserContext : IUserContext<Guid>
{
}
