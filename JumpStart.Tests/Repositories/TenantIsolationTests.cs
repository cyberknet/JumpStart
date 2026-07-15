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
using JumpStart.Data;
using JumpStart.Data.Auditing;
using JumpStart.Data.MultiTenant;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Tests for multi-tenant data isolation (ADR-010): the global EF Core query filter applied to
/// <see cref="ITenantScoped"/> entities, and <see cref="Repository{TEntity}.AddAsync"/>'s
/// automatic <c>TenantId</c> population.
/// </summary>
public class TenantIsolationTests
{
    public class TenantProduct : AuditableEntity, ITenantScoped
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext? tenantContext = null)
        : JumpStartDbContext(options, tenantContext)
    {
        public DbSet<TenantProduct> Products => Set<TenantProduct>();
    }

    private class FixedTenantContext(Guid? tenantId) : ITenantContext
    {
        public Task<Guid?> GetCurrentTenantIdAsync() => Task.FromResult(tenantId);
    }

    private class TenantProductRepository(DbContext context, IUserContext? userContext = null)
        : Repository<TenantProduct>(context, userContext)
    {
    }

    private readonly string _dbName = Guid.NewGuid().ToString();

    private TestDbContext CreateContext(Guid? tenantId) =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(_dbName).Options,
            new FixedTenantContext(tenantId));

    [Fact]
    public async Task AddAsync_PopulatesTenantId_FromCurrentTenant()
    {
        var tenantA = Guid.NewGuid();
        await using var context = CreateContext(tenantA);
        var repository = new TenantProductRepository(context);

        var created = await repository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "Widget" });

        Assert.Equal(tenantA, created.TenantId);
    }

    [Fact]
    public async Task AddAsync_LeavesTenantIdUnset_WhenNoTenantContext()
    {
        await using var context = CreateContext(null);
        var repository = new TenantProductRepository(context);

        var created = await repository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "Widget" });

        Assert.Equal(Guid.Empty, created.TenantId);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsCurrentTenantsEntities()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "A-Product" });
        }

        await using (var seedContext = CreateContext(tenantB))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "B-Product" });
        }

        await using var tenantAContext = CreateContext(tenantA);
        var tenantARepository = new TenantProductRepository(tenantAContext);
        var visibleToA = (await tenantARepository.GetAllAsync()).ToList();

        var product = Assert.Single(visibleToA);
        Assert.Equal("A-Product", product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForAnotherTenantsEntity()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid productId;

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            var created = await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "A-Product" });
            productId = created.Id;
        }

        await using var tenantBContext = CreateContext(tenantB);
        var tenantBRepository = new TenantProductRepository(tenantBContext);
        var result = await tenantBRepository.GetByIdAsync(productId, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Throws_ForAnotherTenantsEntity()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid productId;

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            var created = await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "A-Product" });
            productId = created.Id;
        }

        await using var tenantBContext = CreateContext(tenantB);
        var tenantBRepository = new TenantProductRepository(tenantBContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            tenantBRepository.UpdateAsync(new TenantProduct { Id = productId, Name = "Hijacked" }));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_ForAnotherTenantsEntity()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid productId;

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            var created = await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "A-Product" });
            productId = created.Id;
        }

        await using var tenantBContext = CreateContext(tenantB);
        var tenantBRepository = new TenantProductRepository(tenantBContext);

        var deleted = await tenantBRepository.DeleteAsync(productId);

        Assert.False(deleted);
    }

    [Fact]
    public async Task NullTenantContext_SeesEntitiesFromAllTenants()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seedContext = CreateContext(tenantA))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "A-Product" });
        }

        await using (var seedContext = CreateContext(tenantB))
        {
            var seedRepository = new TenantProductRepository(seedContext);
            await seedRepository.AddAsync(new TenantProduct { Id = Guid.NewGuid(), Name = "B-Product" });
        }

        await using var noTenantContext = CreateContext(null);
        var noTenantRepository = new TenantProductRepository(noTenantContext);
        var visibleToAll = await noTenantRepository.GetAllAsync();

        Assert.Equal(2, visibleToAll.Count());
    }
}
