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
/// Application-specific rules about which tenants may administer roles, and with what. See ADR-019.
/// </summary>
/// <remarks>
/// <para>
/// The seam between framework mechanism and product policy. Whether a given tenant may define roles
/// at all - and which of the delegable permissions it may use - is a decision about a subscription
/// tier, a feature flag, or a contract term. The framework asks the question and enforces the
/// answer; it never learns why the answer is what it is, and it must never gain a vocabulary for
/// plans, tiers or entitlements.
/// </para>
/// <para>
/// Applications with no such rule need not implement this: the default permits everything the
/// registry already marks delegable, so the registry alone remains the whole story.
/// </para>
/// </remarks>
public interface IRoleManagementPolicy
{
    /// <summary>Whether this tenant may define and administer roles of its own at all.</summary>
    Task<bool> CanManageRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The permissions this tenant may put into its own roles.
    /// </summary>
    /// <remarks>
    /// A narrowing of the registry's <see cref="PermissionDescriptor.DelegableByTenantAdmin"/> set,
    /// never a widening: the framework intersects this with that set, so an application cannot use
    /// this to make a non-delegable permission delegable.
    /// </remarks>
    Task<IReadOnlyCollection<string>> GrantablePermissionsAsync(
        Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// The default policy: every tenant may administer roles, using everything the registry marks
/// delegable.
/// </summary>
/// <remarks>
/// Permissive by design, because the restriction this seam exists for is a product decision and the
/// framework has no basis for guessing one. It is not a security default - the security default is
/// the registry, which refuses everything until an application declares something.
/// </remarks>
public sealed class PermissiveRoleManagementPolicy(IPermissionRegistry registry) : IRoleManagementPolicy
{
    /// <inheritdoc />
    public Task<bool> CanManageRolesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<string>> GrantablePermissionsAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> grantable = registry.All
            .Where(p => p.DelegableByTenantAdmin)
            .Select(p => p.Name)
            .ToList();

        return Task.FromResult(grantable);
    }
}
