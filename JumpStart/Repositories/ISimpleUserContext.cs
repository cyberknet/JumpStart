using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JumpStart.Repositories.Advanced;

namespace JumpStart.Repositories;

/// <summary>
/// Provides access to the current authenticated user's Guid identifier for audit tracking.
/// This is the recommended interface for most applications using the JumpStart framework.
/// This interface should be implemented by application services that manage user context.
/// </summary>
/// <remarks>
/// <para>
/// This interface inherits from <see cref="IUserContext{TKey}"/> with Guid as the key type,
/// providing a simplified API without explicit key type parameters.
/// For applications requiring custom key types (int, string, etc.), use <see cref="IUserContext{TKey}"/> directly.
/// </para>
/// <para>
/// Implementations typically retrieve the current user from:
/// - HttpContext (for web applications)
/// - Authentication state (for Blazor applications)
/// - Thread principal (for background services)
/// </para>
/// <para>
/// Example implementation for Blazor Server:
/// <code>
/// public class UserContext : IUserContext
/// {
///     private readonly AuthenticationStateProvider _authStateProvider;
///     
///     public UserContext(AuthenticationStateProvider authStateProvider)
///     {
///         _authStateProvider = authStateProvider;
///     }
///     
///     public async Task&lt;Guid?&gt; GetCurrentUserIdAsync()
///     {
///         var authState = await _authStateProvider.GetAuthenticationStateAsync();
///         var user = authState.User;
///         
///         if (user?.Identity?.IsAuthenticated == true)
///         {
///             var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
///             if (userIdClaim != null &amp;&amp; Guid.TryParse(userIdClaim.Value, out var userId))
///             {
///                 return userId;
///             }
///         }
///         
///         return null;
///     }
/// }
/// </code>
/// </para>
/// <para>
/// Register as scoped in dependency injection:
/// <code>
/// services.AddScoped&lt;IUserContext, UserContext&gt;();
/// </code>
/// </para>
/// </remarks>
public interface ISimpleUserContext : IUserContext<Guid>
{
}
