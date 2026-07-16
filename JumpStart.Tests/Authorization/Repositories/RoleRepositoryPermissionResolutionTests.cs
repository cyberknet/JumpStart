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
using JumpStart.Authorization.Repositories;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Authorization.Repositories;

/// <summary>
/// Tests for <see cref="RoleRepository.GetPermissionClaimsForUserAsync"/>: union of role-derived
/// and direct <see cref="UserPermission"/> grants, tenant isolation via
/// <see cref="Data.MultiTenant.ITenantScopedOptional"/>, and global (tenant-independent) grants.
/// See ADR-012.
/// </summary>
public class RoleRepositoryPermissionResolutionTests
{
    private class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext? tenantContext = null)
        : JumpStartDbContext(options, tenantContext)
    {
    }

    private class FixedTenantContext(Guid? tenantId) : ITenantContext
    {
        public Task<Guid?> GetCurrentTenantIdAsync() => Task.FromResult(tenantId);
    }

    private readonly string _dbName = Guid.NewGuid().ToString();

    private TestDbContext CreateContext(Guid? tenantId) =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(_dbName).Options,
            new FixedTenantContext(tenantId));

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_ReturnsEmpty_ForUserWithNoRolesOrGrants()
    {
        await using var context = CreateContext(Guid.NewGuid());
        var repository = new RoleRepository(context, null);

        var permissions = await repository.GetPermissionClaimsForUserAsync(Guid.NewGuid());

        Assert.Empty(permissions);
    }

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_ReturnsRoleDerivedPermissions()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(tenantId);
        var repository = new RoleRepository(context, null);

        var role = await repository.AddAsync(new Role { Name = "Editor", TenantId = tenantId });
        await repository.AddPermissionAsync(role.Id, "Form.Get");
        await repository.AddPermissionAsync(role.Id, "Form.Update");
        await repository.AssignUserToRoleAsync(userId, role.Id, tenantId);

        var permissions = await repository.GetPermissionClaimsForUserAsync(userId);

        Assert.Equal(2, permissions.Count);
        Assert.Contains("Form.Get", permissions);
        Assert.Contains("Form.Update", permissions);
    }

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_ReturnsDirectGrants()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(tenantId);
        var repository = new RoleRepository(context, null);
        context.UserPermissions.Add(new UserPermission { UserId = userId, Permission = "Product.Delete", TenantId = tenantId });
        await context.SaveChangesAsync();

        var permissions = await repository.GetPermissionClaimsForUserAsync(userId);

        var permission = Assert.Single(permissions);
        Assert.Equal("Product.Delete", permission);
    }

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_UnionsRoleAndDirectGrants_Distinct()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(tenantId);
        var repository = new RoleRepository(context, null);

        var role = await repository.AddAsync(new Role { Name = "Viewer", TenantId = tenantId });
        await repository.AddPermissionAsync(role.Id, "Product.Get");
        await repository.AssignUserToRoleAsync(userId, role.Id, tenantId);

        // Same permission granted directly too - should not be duplicated
        context.UserPermissions.Add(new UserPermission { UserId = userId, Permission = "Product.Get", TenantId = tenantId });
        context.UserPermissions.Add(new UserPermission { UserId = userId, Permission = "Product.List", TenantId = tenantId });
        await context.SaveChangesAsync();

        var permissions = await repository.GetPermissionClaimsForUserAsync(userId);

        Assert.Equal(2, permissions.Count);
        Assert.Contains("Product.Get", permissions);
        Assert.Contains("Product.List", permissions);
    }

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_ExcludesAnotherTenantsRole()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new RoleRepository(seedContext, null);
            var role = await seedRepository.AddAsync(new Role { Name = "TenantA Admin", TenantId = tenantA });
            await seedRepository.AddPermissionAsync(role.Id, "Form.Delete");
            await seedRepository.AssignUserToRoleAsync(userId, role.Id, tenantA);
        }

        await using var tenantBContext = CreateContext(tenantB);
        var tenantBRepository = new RoleRepository(tenantBContext, null);

        var permissions = await tenantBRepository.GetPermissionClaimsForUserAsync(userId);

        Assert.Empty(permissions);
    }

    [Fact]
    public async Task GetPermissionClaimsForUserAsync_IncludesGlobalRole_RegardlessOfCurrentTenant()
    {
        var userId = Guid.NewGuid();

        await using (var seedContext = CreateContext(null))
        {
            var seedRepository = new RoleRepository(seedContext, null);
            var globalRole = await seedRepository.AddAsync(new Role { Name = "System Administrator", TenantId = null });
            await seedRepository.AddPermissionAsync(globalRole.Id, "Product.Delete");
            await seedRepository.AssignUserToRoleAsync(userId, globalRole.Id, tenantId: null);
        }

        // The global role/assignment should still be visible from within any specific tenant's context
        await using var tenantContext = CreateContext(Guid.NewGuid());
        var tenantRepository = new RoleRepository(tenantContext, null);

        var permissions = await tenantRepository.GetPermissionClaimsForUserAsync(userId);

        var permission = Assert.Single(permissions);
        Assert.Equal("Product.Delete", permission);
    }

    [Fact]
    public async Task AddPermissionAsync_IsIdempotent()
    {
        await using var context = CreateContext(Guid.NewGuid());
        var repository = new RoleRepository(context, null);
        var role = await repository.AddAsync(new Role { Name = "Editor", TenantId = null });

        var first = await repository.AddPermissionAsync(role.Id, "Form.Get");
        var second = await repository.AddPermissionAsync(role.Id, "Form.Get");

        Assert.Equal(first.Id, second.Id);
        var allPermissions = await repository.GetPermissionsForRoleAsync(role.Id);
        var permission = Assert.Single(allPermissions);
        Assert.Equal("Form.Get", permission);
    }

    [Fact]
    public async Task AssignUserToRoleAsync_IsIdempotent()
    {
        await using var context = CreateContext(Guid.NewGuid());
        var repository = new RoleRepository(context, null);
        var role = await repository.AddAsync(new Role { Name = "Editor", TenantId = null });
        var userId = Guid.NewGuid();

        var first = await repository.AssignUserToRoleAsync(userId, role.Id, null);
        var second = await repository.AssignUserToRoleAsync(userId, role.Id, null);

        Assert.Equal(first.Id, second.Id);
        var users = await repository.GetUsersForRoleAsync(role.Id);
        var assignedUserId = Assert.Single(users);
        Assert.Equal(userId, assignedUserId);
    }

    [Fact]
    public async Task RemovePermissionAsync_ReturnsFalse_WhenNotGranted()
    {
        await using var context = CreateContext(Guid.NewGuid());
        var repository = new RoleRepository(context, null);
        var role = await repository.AddAsync(new Role { Name = "Editor", TenantId = null });

        var removed = await repository.RemovePermissionAsync(role.Id, "Form.Get");

        Assert.False(removed);
    }

    [Fact]
    public async Task UnassignUserFromRoleAsync_ReturnsFalse_WhenNotAssigned()
    {
        await using var context = CreateContext(Guid.NewGuid());
        var repository = new RoleRepository(context, null);
        var role = await repository.AddAsync(new Role { Name = "Editor", TenantId = null });

        var removed = await repository.UnassignUserFromRoleAsync(Guid.NewGuid(), role.Id, null);

        Assert.False(removed);
    }
}
