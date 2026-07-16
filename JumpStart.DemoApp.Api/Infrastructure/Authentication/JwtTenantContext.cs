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
using JumpStart.Repositories;
using Microsoft.AspNetCore.Http;

namespace JumpStart.DemoApp.Api.Infrastructure.Authentication;

/// <summary>
/// Provides access to the current tenant from the <c>tenant_id</c> JWT claim in Web API requests.
/// See ADR-015.
/// </summary>
/// <remarks>
/// <para>
/// The <c>tenant_id</c> claim is stamped onto the real token only after
/// <c>TokenController.Exchange</c> independently verifies tenant membership server-side (see
/// <c>IUserTenantRepository.HasAccessAsync</c>) - by the time this class reads it, it has already
/// been validated once. It is not re-validated here; this class is purely a read of an already-
/// trusted claim, mirroring <see cref="ApiUserContext"/>'s relationship to <c>ClaimTypes.NameIdentifier</c>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> thread-safe via <see cref="IHttpContextAccessor"/>, which provides
/// per-request isolation. Register as scoped in dependency injection.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Registration in Program.cs
/// builder.Services.AddJumpStart(options =&gt;
/// {
///     options.RegisterTenantContext&lt;JwtTenantContext&gt;();
/// });
/// </code>
/// </example>
public class JwtTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTenantContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// The HTTP context accessor for accessing the current request's claims.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpContextAccessor"/> is null.
    /// </exception>
    public JwtTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        var tenantClaim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;

        return Task.FromResult(Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : (Guid?)null);
    }
}
