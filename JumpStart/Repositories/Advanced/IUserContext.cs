using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JumpStart.Repositories.Advanced;

/// <summary>
/// Provides access to the current authenticated user's identifier for audit tracking with custom key types.
/// This interface should be implemented by application services that manage user context.
/// </summary>
/// <typeparam name="TKey">The type of the user's primary key. Must be a value type (int, Guid, long, etc.).</typeparam>
/// <remarks>
/// <para>
/// For most applications using Guid identifiers, use <see cref="ISimpleUserContext"/> instead for a simpler API.
/// </para>
/// <para>
/// Implementations typically retrieve the current user from:
/// - HttpContext (for web applications)
/// - Authentication state (for Blazor applications)
/// - Thread principal (for background services)
/// </para>
/// </remarks>
public interface IUserContext<TKey> where TKey : struct
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the user's identifier if authenticated; otherwise, null.
    /// </returns>
    Task<TKey?> GetCurrentUserIdAsync();
}
