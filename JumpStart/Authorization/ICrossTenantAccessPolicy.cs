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

namespace JumpStart.Authorization;

/// <summary>
/// What somebody may do inside a tenant they do not belong to.
/// </summary>
/// <param name="Permissions">
/// The permissions their token carries while acting there. Deliberately explicit rather than "all of
/// them": the point of acting as a tenant is to see what its own people see, and a token that
/// silently carried more than any member could hold would make support indistinguishable from
/// escalation.
/// </param>
public record CrossTenantAccess(IReadOnlyCollection<string> Permissions);

/// <summary>
/// Decides whether a user may act inside a tenant they are not a member of.
/// </summary>
/// <remarks>
/// <para>
/// Membership is the framework's normal answer to "may this person be in this tenant?" (ADR-015), and
/// <c>TokenController</c> refuses anything else. This is the one deliberate exception, and it exists
/// because every multi-tenant product eventually grows an administrator who has to look at a
/// customer's data to support them - and the alternative to a named seam is a second, parallel,
/// read-only copy of the entire application that drifts from the real one.
/// </para>
/// <para>
/// <strong>The framework refuses by default.</strong> <see cref="DenyCrossTenantAccessPolicy"/> is
/// registered unless an application replaces it, so nothing gains this capability by upgrading. Who
/// qualifies, and what they may do once inside, are questions only the application can answer - they
/// depend on its own permissions, which JumpStart does not know.
/// </para>
/// <para>
/// <strong>Implementations should log.</strong> A person reaching into a customer's tenant is the
/// event an audit asks about first, and the policy is the one place that sees every such decision.
/// </para>
/// </remarks>
public interface ICrossTenantAccessPolicy
{
    /// <summary>
    /// What <paramref name="userId"/> may do inside <paramref name="tenantId"/>, or <c>null</c> to
    /// refuse - which is what a caller who is simply not a member should get.
    /// </summary>
    Task<CrossTenantAccess?> EvaluateAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Refuses every request. The framework's default, and the right one for most applications.
/// </summary>
/// <remarks>
/// Fail-closed in the same spirit as ADR-018: an application that has not thought about cross-tenant
/// access does not get it by accident, and the absence of a policy means "no", not "unrestricted".
/// </remarks>
public sealed class DenyCrossTenantAccessPolicy : ICrossTenantAccessPolicy
{
    /// <inheritdoc />
    public Task<CrossTenantAccess?> EvaluateAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<CrossTenantAccess?>(null);
}
