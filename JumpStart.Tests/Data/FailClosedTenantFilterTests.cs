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
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Tests for ADR-018: a null <see cref="JumpStartDbContext.CurrentTenantId"/> denies rather than
/// admitting every row.
/// </summary>
/// <remarks>
/// The filter used to begin <c>CurrentTenantId == null || ...</c>, which made tenant isolation
/// conditional on a claim being present and failed in the permissive direction - a request that
/// reached a tenant-scoped entity without a tenant read across every tenant, silently and
/// successfully. <see cref="UserRole"/> stands in for a tenant-scoped entity here because the
/// framework owns it; the filter is applied by reflection to every
/// <see cref="Data.MultiTenant.ITenantScopedOptional"/> and
/// <see cref="Data.MultiTenant.ITenantScoped"/> entity alike.
/// </remarks>
public class FailClosedTenantFilterTests
{
    private class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext? tenantContext = null)
        : JumpStartDbContext(options, tenantContext)
    {
    }

    private class FixedTenantContext(Guid? tenantId, bool singleTenant = false) : ITenantContext
    {
        public bool SingleTenantMode => singleTenant;

        public Task<Guid?> GetCurrentTenantIdAsync() => Task.FromResult(tenantId);
    }

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();

    private TestDbContext CreateContext(Guid? tenantId, bool singleTenant = false) =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(_dbName).Options,
            new FixedTenantContext(tenantId, singleTenant));

    private TestDbContext CreateContextWithNoTenantContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(_dbName).Options);

    private async Task SeedAsync()
    {
        await using var context = CreateContext(null, singleTenant: true);

        context.Set<UserRole>().AddRange(
            new UserRole { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), TenantId = _tenantA },
            new UserRole { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), TenantId = _tenantB },
            new UserRole { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), TenantId = null });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task NoCurrentTenant_ReturnsOnlyGlobalRows_NotEveryTenants()
    {
        await SeedAsync();

        await using var context = CreateContext(null);

        var rows = await context.Set<UserRole>().ToListAsync();

        // Before ADR-018 this returned all three. The global row stays - it belongs to no tenant,
        // which is the whole reason ITenantScopedOptional exists.
        var row = Assert.Single(rows);
        Assert.Null(row.TenantId);
    }

    [Fact]
    public async Task CurrentTenant_ReturnsThatTenantsRowsAndGlobalOnes()
    {
        await SeedAsync();

        await using var context = CreateContext(_tenantA);

        var rows = await context.Set<UserRole>().ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.TenantId == _tenantA);
        Assert.Contains(rows, r => r.TenantId == null);
        Assert.DoesNotContain(rows, r => r.TenantId == _tenantB);
    }

    [Fact]
    public async Task AcrossAllTenants_StillCrossesTheBoundaryDeliberately()
    {
        await SeedAsync();

        await using var context = CreateContext(null);

        var rows = await context.Set<UserRole>().AcrossAllTenants().ToListAsync();

        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public async Task SingleTenantMode_DisablesTheFilterEntirely()
    {
        await SeedAsync();

        await using var context = CreateContext(null, singleTenant: true);

        var rows = await context.Set<UserRole>().ToListAsync();

        Assert.Equal(3, rows.Count);
    }

    /// <summary>
    /// Supplying no <see cref="ITenantContext"/> at all means the application is not multi-tenant -
    /// absence of the mechanism is a design statement, and those applications keep exactly the
    /// behaviour they had before ADR-018. A tenant context that is present but yields no tenant is
    /// the dangerous case, and that one denies (see the first test).
    /// </summary>
    [Fact]
    public async Task NoTenantContextAtAll_BehavesAsSingleTenant()
    {
        await SeedAsync();

        await using var context = CreateContextWithNoTenantContext();

        var rows = await context.Set<UserRole>().ToListAsync();

        Assert.Equal(3, rows.Count);
    }
}
