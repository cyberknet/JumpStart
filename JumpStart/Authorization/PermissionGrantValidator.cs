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
    /// <para>
    /// When <c>true</c>, the platform itself is making the grant rather than a person: a startup
    /// seeder establishing the first operator, or sign-up giving a founder the built-in role that
    /// makes their new organization usable. The one sanctioned exception, and callers say so
    /// explicitly - ADR-019 requires it be expressed rather than arrived at by there happening to be
    /// no current user.
    /// </para>
    /// <para>
    /// It skips rules 3 and 4, which are both questions about <em>a tenant administrator delegating
    /// a permission</em>: whether the grantor holds it, whether it may be delegated at all, and
    /// whether this organization's plan lets it administer roles. None of those has a meaningful
    /// answer when nobody is delegating - a founder receiving Owner on a plan with no role
    /// separation is not that organization administering its own roles, it is the platform giving
    /// them the standing every organization's founder gets.
    /// </para>
    /// <para>
    /// Rules 1 and 2 still apply, which is what keeps system grants honest: the platform cannot
    /// store a permission nobody declared, and cannot put a platform-wide permission inside a
    /// tenant.
    /// </para>
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

        // Rule 3 is about a tenant administrator delegating, so it is skipped when the platform
        // itself is granting - see the asSystem parameter. Without that exclusion the founder of a
        // brand-new organization cannot be given the built-in Owner role, because sign-up starts
        // them on a plan with no role separation and the policy check below correctly answers "this
        // organization may not administer its own roles" - true, and beside the point, since nobody
        // in the organization is administering anything.
        if (tenantId is { } tenant && !asSystem)
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

        // Rule 4: the grantor holds it themselves. Skipped for a system grant for the same reason as
        // rule 3 above - there is no grantor to measure.
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
