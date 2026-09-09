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
using JumpStart.Authorization;
using Xunit;

namespace JumpStart.Tests.Authorization;

/// <summary>
/// Tests for ADR-019's four grant rules: declared, correctly scoped, delegable, and held by the
/// grantor.
/// </summary>
public class PermissionGrantValidatorTests
{
    private const string TenantPermission = "Product.Delete";
    private const string NonDelegable = "Product.Purge";
    private const string PlatformPermission = "Platform.ManageTenants";

    private static readonly PermissionDescriptor[] Declared =
    [
        new(TenantPermission, PermissionScope.Tenant, "Products", DelegableByTenantAdmin: true),
        new(NonDelegable, PermissionScope.Tenant, "Products", DelegableByTenantAdmin: false),
        new(PlatformPermission, PermissionScope.Platform, "Platform")
    ];

    private sealed class StubEvaluator(params string[] held) : IPermissionEvaluator
    {
        public Task<bool> HasAsync(string permission, CancellationToken ct = default) =>
            Task.FromResult(held.Contains(permission));

        public Task<IReadOnlyCollection<string>> CurrentAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyCollection<string>>(held);
    }

    private sealed class RefusingPolicy : IRoleManagementPolicy
    {
        public Task<bool> CanManageRolesAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyCollection<string>> GrantablePermissionsAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyCollection<string>>([]);
    }

    private static PermissionGrantValidator Create(
        IPermissionEvaluator evaluator, IRoleManagementPolicy? policy = null)
    {
        var registry = new PermissionRegistry(Declared);
        return new PermissionGrantValidator(
            registry, policy ?? new PermissiveRoleManagementPolicy(registry), evaluator);
    }

    // Rule 1 - declared.

    [Fact]
    public async Task Refuses_APermissionThatWasNeverDeclared()
    {
        var validator = Create(new StubEvaluator("Anything.AtAll"));

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync("Invented.Permission", Guid.NewGuid()));

        Assert.Contains("not a declared permission", ex.Message);
    }

    [Fact]
    public async Task EmptyRegistry_RefusesEverything()
    {
        var registry = new EmptyPermissionRegistry();
        var validator = new PermissionGrantValidator(
            registry, new PermissiveRoleManagementPolicy(registry), new StubEvaluator(TenantPermission));

        await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(TenantPermission, Guid.NewGuid()));
    }

    // Rule 2 - scope.

    [Fact]
    public async Task Refuses_APlatformPermissionGrantedInsideATenant()
    {
        // The escalation this whole decision exists to stop: a tenant administrator handing
        // themselves the string an application uses to mark its own operators.
        var validator = Create(new StubEvaluator(PlatformPermission));

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(PlatformPermission, Guid.NewGuid()));

        Assert.Contains("platform-wide", ex.Message);
    }

    [Fact]
    public async Task Refuses_ATenantPermissionGrantedGlobally()
    {
        var validator = Create(new StubEvaluator(TenantPermission));

        await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(TenantPermission, tenantId: null));
    }

    // Rule 3 - delegable, and permitted by the application's policy.

    [Fact]
    public async Task Refuses_APermissionNotMarkedDelegable()
    {
        var validator = Create(new StubEvaluator(NonDelegable));

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(NonDelegable, Guid.NewGuid()));

        Assert.Contains("not delegable", ex.Message);
    }

    [Fact]
    public async Task Refuses_WhenTheApplicationsPolicySaysThisTenantMayNotManageRoles()
    {
        var validator = Create(new StubEvaluator(TenantPermission), new RefusingPolicy());

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(TenantPermission, Guid.NewGuid()));

        Assert.Contains("not permitted to administer", ex.Message);
    }

    // Rule 4 - the grantor holds it. The rule that holds even when the registry is misconfigured.

    [Fact]
    public async Task Refuses_WhenTheGrantorDoesNotHoldItThemselves()
    {
        var validator = Create(new StubEvaluator()); // holds nothing

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(TenantPermission, Guid.NewGuid()));

        Assert.Contains("do not hold it yourself", ex.Message);
    }

    [Fact]
    public async Task Allows_AValidGrant()
    {
        var validator = Create(new StubEvaluator(TenantPermission));

        await validator.ValidateAsync(TenantPermission, Guid.NewGuid());
    }

    [Fact]
    public async Task AsSystem_SkipsTheDelegationRules()
    {
        var validator = Create(new StubEvaluator()); // holds nothing

        // Platform scope + no tenant + no grantor: the seeding case that establishes the first
        // platform operator.
        await validator.ValidateAsync(PlatformPermission, tenantId: null, asSystem: true);

        // Still refused, because rule 1 is not waived by being the system.
        await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync("Invented.Permission", tenantId: null, asSystem: true));
    }

    /// <summary>
    /// Sign-up, and the reason rule 3 has to be waived along with rule 4.
    /// </summary>
    /// <remarks>
    /// A founder receives the built-in role inside a brand-new organization whose plan does not
    /// include role separation, so the policy correctly answers "this organization may not
    /// administer its own roles" - true, and beside the point, because nobody in the organization is
    /// administering anything. Refusing here made registration fail outright: no organization, no
    /// owner, no account.
    /// </remarks>
    [Fact]
    public async Task AsSystem_GrantsInsideATenantWhosePolicyRefusesDelegation()
    {
        var validator = Create(new StubEvaluator(), new RefusingPolicy());

        await validator.ValidateAsync(TenantPermission, Guid.NewGuid(), asSystem: true);

        // ...including one a tenant administrator could never hand out themselves, which is exactly
        // what a built-in owner role contains.
        await validator.ValidateAsync(NonDelegable, Guid.NewGuid(), asSystem: true);
    }

    /// <summary>
    /// The waiver is for the system alone - an ordinary caller still meets the full set.
    /// </summary>
    [Fact]
    public async Task APersonIsStillRefusedWhereTheSystemIsAllowed()
    {
        var validator = Create(new StubEvaluator(TenantPermission, NonDelegable), new RefusingPolicy());

        await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(TenantPermission, Guid.NewGuid()));
    }

    /// <summary>
    /// Scope survives the waiver: the system cannot put a platform permission inside a tenant, which
    /// is the shape an escalation would take.
    /// </summary>
    [Fact]
    public async Task AsSystem_StillCannotGrantAPlatformPermissionInsideATenant()
    {
        var validator = Create(new StubEvaluator());

        await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAsync(PlatformPermission, Guid.NewGuid(), asSystem: true));
    }

    // Assignment - a role hands over everything in it.

    [Fact]
    public async Task Refuses_AssigningARoleContainingSomethingTheGrantorLacks()
    {
        // The walk-around rule 4 would otherwise have: rather than granting the permission, hand
        // somebody an existing role that already contains it.
        var validator = Create(new StubEvaluator(TenantPermission));

        var ex = await Assert.ThrowsAsync<PermissionGrantException>(
            () => validator.ValidateAssignmentAsync(
                [TenantPermission, NonDelegable], Guid.NewGuid()));

        Assert.Contains(NonDelegable, ex.Message);
    }
}
