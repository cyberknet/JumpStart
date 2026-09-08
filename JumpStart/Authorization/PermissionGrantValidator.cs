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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JumpStart.Authorization;

/// <summary>
/// Thrown when a permission grant is refused. See <see cref="PermissionGrantValidator"/>.
/// </summary>
public class PermissionGrantException(string message) : InvalidOperationException(message);

/// <summary>
/// The four rules every permission grant must satisfy. See ADR-019.
/// </summary>
/// <remarks>
/// <para>
/// Used by the repositories rather than by a controller, deliberately: the paths that matter most -
/// seeders, background jobs, anything that never sees an HTTP request - do not go through a
/// controller, and a rule enforced only at the edge is a rule with a hole in it.
/// </para>
/// <para>
/// <strong>Rule 4 does most of the work.</strong> "You cannot grant what you do not hold" is a
/// property of the transaction rather than of the permission, so it holds even when the registry is
/// misconfigured: a grant can never increase the set of permissions in existence, only redistribute
/// it. The other three depend on somebody having curated the registry correctly.
/// </para>
/// </remarks>
public class PermissionGrantValidator(
    IPermissionRegistry registry,
    IRoleManagementPolicy policy,
    IPermissionEvaluator evaluator)
{
    /// <summary>
    /// Validates a grant of <paramref name="permission"/> made within <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="permission">The permission being granted.</param>
    /// <param name="tenantId">
    /// The tenant the grant is scoped to, or <c>null</c> for a platform-wide grant.
    /// </param>
    /// <param name="asSystem">
    /// When <c>true</c>, skips the "grantor already holds it" rule because there is no grantor - a
    /// startup seeder establishing the first platform operator, for example. The one sanctioned
    /// exception, and callers say so explicitly: ADR-019 requires it be expressed rather than
    /// arrived at by there happening to be no current user.
    /// </param>
    /// <exception cref="PermissionGrantException">Thrown when any rule is broken.</exception>
    public async Task ValidateAsync(
        string permission,
        Guid? tenantId,
        bool asSystem = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new PermissionGrantException("A permission name is required.");
        }

        // Rule 1: declared.
        if (!registry.TryGet(permission, out var descriptor))
        {
            throw new PermissionGrantException(
                $"'{permission}' is not a declared permission. Permissions must be registered with "
                + "IPermissionRegistry before they can be granted - see ADR-019. If this application "
                + "has not declared any, granting is refused entirely and deliberately.");
        }

        // Rule 2: scope matches where it is being granted. A platform permission handed out inside a
        // tenant is the exact shape of an escalation.
        if (descriptor.Scope == PermissionScope.Platform && tenantId is not null)
        {
            throw new PermissionGrantException(
                $"'{permission}' is a platform-wide permission and cannot be granted inside a tenant.");
        }

        if (descriptor.Scope == PermissionScope.Tenant && tenantId is null && !asSystem)
        {
            throw new PermissionGrantException(
                $"'{permission}' is a tenant permission and cannot be granted globally.");
        }

        if (tenantId is { } tenant)
        {
            // Rule 3: delegable, and permitted by the application's own policy. Intersected with the
            // registry rather than taken from the policy alone, so a policy cannot widen the set.
            if (!descriptor.DelegableByTenantAdmin)
            {
                throw new PermissionGrantException(
                    $"'{permission}' is not delegable by a tenant administrator.");
            }

            if (!await policy.CanManageRolesAsync(tenant, cancellationToken))
            {
                throw new PermissionGrantException(
                    "This organization is not permitted to administer its own roles.");
            }

            var grantable = await policy.GrantablePermissionsAsync(tenant, cancellationToken);
            if (!grantable.Contains(permission))
            {
                throw new PermissionGrantException(
                    $"'{permission}' is not among the permissions this organization may grant.");
            }
        }

        // Rule 4: the grantor holds it themselves.
        if (asSystem)
        {
            return;
        }

        if (!await evaluator.HasAsync(permission, cancellationToken))
        {
            throw new PermissionGrantException(
                $"You cannot grant '{permission}' because you do not hold it yourself.");
        }
    }

    /// <summary>
    /// Validates every permission a role grants - used when assigning that role to somebody.
    /// </summary>
    /// <remarks>
    /// Assigning a role is a grant of everything in it. Without this, rule 4 would guard
    /// <c>AddPermissionAsync</c> and be walked straight around by handing somebody an existing role
    /// that already contains the permission.
    /// </remarks>
    public async Task ValidateAssignmentAsync(
        IEnumerable<string> permissions,
        Guid? tenantId,
        bool asSystem = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        foreach (var permission in permissions)
        {
            await ValidateAsync(permission, tenantId, asSystem, cancellationToken);
        }
    }
}
