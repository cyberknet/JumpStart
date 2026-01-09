using System.Security.Claims;
using JumpStart.Repository;
using Microsoft.AspNetCore.Components.Authorization;

namespace JumpStart.DemoApp.Services;

/// <summary>
/// Provides the current user's Guid identifier for JumpStart audit tracking in Blazor applications.
/// </summary>
public class BlazorUserContext : ISimpleUserContext
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorUserContext(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try to get the user ID from the NameIdentifier claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }

        return null;
    }
}