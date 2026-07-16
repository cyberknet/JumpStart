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
using JumpStart.DemoApp.Clients;
using JumpStart.Services.Authentication;
using Microsoft.Extensions.Logging;

namespace JumpStart.DemoApp.Services;

/// <summary>
/// Demo-only: grants a brand-new user the "Demo Administrator" role immediately after their
/// account is created, so the demo is usable right away without every new user 403-ing on their
/// first request. See ADR-012's bootstrapping note and ADR-013/ADR-014.
/// </summary>
/// <remarks>
/// <para>
/// Called directly from the account-creation code paths (<c>Register.razor</c>,
/// <c>ExternalLogin.razor</c>) - both already know the new user's real ID and username at the
/// moment their account is created, so this mints the identity assertion and calls the bootstrap
/// endpoint right there. This replaced an earlier design (<c>DemoBootstrapHandler</c>, a
/// <see cref="System.Net.Http.DelegatingHandler"/> that inspected the *first* API response's token
/// for missing permission claims) that only worked if a new user's first API call happened to go
/// through the one client it was wired into - any other client would 403 immediately instead. Since
/// this class is called from Razor components (a genuinely circuit-scoped context), it needs none
/// of <c>JwtExchangeHandler</c>'s <c>CircuitServicesAccessor</c> machinery.
/// </para>
/// <para>
/// Not a framework concept - a real application decides its own onboarding/bootstrap policy. This
/// is demo convenience only, and deliberately best-effort: a failure here does not block
/// registration, since the alternative (a user who can't create an account at all because a
/// convenience endpoint is unreachable) is worse than "the account exists but isn't bootstrapped yet."
/// </para>
/// </remarks>
public class DemoNewUserBootstrapper(
    IJwtTokenService jwtTokenService,
    IDemoBootstrapApiClient demoBootstrapClient,
    ILogger<DemoNewUserBootstrapper> logger)
{
    private static readonly TimeSpan AssertionTokenLifetime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Grants the "Demo Administrator" role to a newly created user. Best-effort - logs and
    /// swallows failures rather than propagating them, since this must never block registration.
    /// </summary>
    /// <param name="userId">The newly created user's ID.</param>
    /// <param name="username">The newly created user's username (for the identity assertion).</param>
    public async Task EnsureAdminAsync(Guid userId, string username)
    {
        try
        {
            var assertionToken = jwtTokenService.GenerateToken(userId, username, expiration: AssertionTokenLifetime);
            await demoBootstrapClient.EnsureAdminAsync($"Bearer {assertionToken}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to bootstrap demo admin role for new user {UserId}", userId);
        }
    }
}
