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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Authorization.Repositories;
using JumpStart.Repositories;

namespace JumpStart.Authorization;

/// <summary>
/// Asks whether the current user holds a permission. See ADR-019.
/// </summary>
/// <remarks>
/// <para>
/// <strong>A seam, not a feature.</strong> Endpoint authorization still works exactly as ADR-011
/// describes - <c>EntityPermissionHandler</c> compares a flat claim, which is fast and unchanged.
/// This interface exists for code that needs to ask a permission question itself, and so that a
/// future model whose grants cannot fit in a token can be added behind it. Per-resource permissions
/// ("administrator on server A but not server B") are the obvious case: the grant set stops being
/// bounded, so it cannot live in a JWT, and the check has to become a lookup. Introducing the
/// interface costs nothing now and saves rewriting every call site then.
/// </para>
/// <para>
/// The default implementation resolves from the database rather than from claims, which makes it
/// correct for a grant made after the current token was issued - claims are a snapshot, and ADR-012
/// already records that the framework has no token-refresh mechanism.
/// </para>
/// </remarks>
public interface IPermissionEvaluator
{
    /// <summary>Whether the current user holds <paramref name="permission"/> in the current tenant.</summary>
    Task<bool> HasAsync(string permission, CancellationToken cancellationToken = default);

    /// <summary>Everything the current user holds in the current tenant.</summary>
    Task<IReadOnlyCollection<string>> CurrentAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves permissions from the database for whoever <see cref="IUserContext"/> reports, in
/// whichever tenant <see cref="ITenantContext"/> reports.
/// </summary>
/// <remarks>
/// Deliberately built on the framework's own context abstractions rather than on
/// <c>IHttpContextAccessor</c> or a <c>ClaimsPrincipal</c>: JumpStart keeps ASP.NET specifics out of
/// its user and tenant plumbing (see <see cref="IUserContext"/>), and doing the same here means this
/// works unchanged in a background job, a Blazor circuit, or a test.
/// </remarks>
public sealed class DatabasePermissionEvaluator(
    PermissionResolver resolver,
    IUserContext? userContext = null,
    ITenantContext? tenantContext = null) : IPermissionEvaluator
{
    /// <inheritdoc />
    public async Task<bool> HasAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var held = await CurrentAsync(cancellationToken);
        return held.Contains(permission);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> CurrentAsync(
        CancellationToken cancellationToken = default)
    {
        // No identified user means no permissions - not "all of them". The same fail-closed reading
        // ADR-018 applies to rows.
        var userId = userContext is null ? null : await userContext.GetCurrentUserIdAsync();
        if (userId is not { } id)
        {
            return [];
        }

        var tenantId = tenantContext is null ? null : await tenantContext.GetCurrentTenantIdAsync();

        return await resolver.ResolveAsync(id, tenantId, cancellationToken);
    }
}
