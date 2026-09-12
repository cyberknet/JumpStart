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
using System.Threading.Tasks;
using JumpStart.MultiTenant.Clients;
using JumpStart.MultiTenant.DTOs;
using JumpStart.Services;
using JumpStart.Services.Authentication;
using Moq;
using Xunit;

namespace JumpStart.Tests.Services;

/// <summary>
/// Tests for <see cref="ApiTenantSelectionService"/>: API-client-based tenant selection. See ADR-015.
/// </summary>
public class ApiTenantSelectionServiceTests
{
    private readonly Mock<ITenantsApiClient> _mockTenantsClient;
    private readonly Mock<ITokenStore> _mockTokenStore;
    private readonly ApiTenantSelectionService _service;

    public ApiTenantSelectionServiceTests()
    {
        _mockTenantsClient = new Mock<ITenantsApiClient>();
        _mockTokenStore = new Mock<ITokenStore>();

        // No circuit: CircuitServicesAccessor.CircuitId is null and Services is null, which is the
        // documented "called outside any circuit activity" path - the service resolves directly
        // instead of going through the per-circuit cache, and finds no NavigationManager to read a
        // tenant from the URL. That is exactly the shape these tests want, since they are about
        // tenant selection itself rather than about circuit plumbing.
        _service = new ApiTenantSelectionService(
            _mockTenantsClient.Object,
            _mockTokenStore.Object,
            new CircuitServicesAccessor(),
            new CircuitTenantCache(),
            new TenantSelectionOptions());
    }

    private static TenantDto MakeTenantDto(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        IsActive = true
    };

    [Fact]
    public async Task GetAvailableTenantsAsync_ReturnsTenantsSortedByName()
    {
        var beta = MakeTenantDto("Beta");
        var alpha = MakeTenantDto("Alpha");
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { beta, alpha });

        var tenants = await _service.GetAvailableTenantsAsync();

        Assert.Equal(2, tenants.Count);
        Assert.Equal("Alpha", tenants[0].Name);
        Assert.Equal("Beta", tenants[1].Name);
    }

    [Fact]
    public async Task GetAvailableTenantsAsync_CachesResult_CallsApiOnlyOnce()
    {
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { MakeTenantDto("Acme") });

        await _service.GetAvailableTenantsAsync();
        await _service.GetAvailableTenantsAsync();

        _mockTenantsClient.Verify(c => c.GetMineAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableTenantsAsync_WithACircuit_CachesThroughCircuitTenantCache_NotOnThisInstance()
    {
        // The bug RefreshAvailableTenantsAsync exists to fix: an earlier version of this class cached
        // on the instance unconditionally, so a second, separately-scoped instance sharing the same
        // circuit (exactly what every render-mode island gets - see the class remarks) held its own,
        // independently stale copy forever. Routing through CircuitTenantCache instead means two
        // instances that agree on the circuit id share one cache entry, not two.
        var mockTenantsClient = new Mock<ITenantsApiClient>();
        mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { MakeTenantDto("Acme") });

        var accessor = new CircuitServicesAccessor { CircuitId = "circuit-1" };
        var sharedCache = new CircuitTenantCache();

        var islandA = new ApiTenantSelectionService(
            mockTenantsClient.Object, new Mock<ITokenStore>().Object, accessor, sharedCache,
            new TenantSelectionOptions());
        var islandB = new ApiTenantSelectionService(
            mockTenantsClient.Object, new Mock<ITokenStore>().Object, accessor, sharedCache,
            new TenantSelectionOptions());

        await islandA.GetAvailableTenantsAsync();
        await islandB.GetAvailableTenantsAsync();

        mockTenantsClient.Verify(c => c.GetMineAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAvailableTenantsAsync_InvalidatesTheCircuitCache_SoTheNextCallRefetches()
    {
        var mockTenantsClient = new Mock<ITenantsApiClient>();
        mockTenantsClient.SetupSequence(c => c.GetMineAsync())
            .ReturnsAsync(new List<TenantDto> { MakeTenantDto("Acme") })
            .ReturnsAsync(new List<TenantDto> { MakeTenantDto("Acme Holdings") });

        var accessor = new CircuitServicesAccessor { CircuitId = "circuit-1" };
        var service = new ApiTenantSelectionService(
            mockTenantsClient.Object, new Mock<ITokenStore>().Object, accessor, new CircuitTenantCache(),
            new TenantSelectionOptions());

        var before = await service.GetAvailableTenantsAsync();
        await service.RefreshAvailableTenantsAsync();
        var after = await service.GetAvailableTenantsAsync();

        Assert.Equal("Acme", before[0].Name);
        Assert.Equal("Acme Holdings", after[0].Name);
        mockTenantsClient.Verify(c => c.GetMineAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task RefreshAvailableTenantsAsync_RaisesAvailableTenantsChanged()
    {
        var raised = false;
        _service.AvailableTenantsChanged += () => raised = true;

        await _service.RefreshAvailableTenantsAsync();

        Assert.True(raised);
    }

    [Fact]
    public async Task GetCurrentTenantIdAsync_AutoSelectsFirstAvailableTenant()
    {
        var tenant = MakeTenantDto("Acme");
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { tenant });

        var tenantId = await _service.GetCurrentTenantIdAsync();

        Assert.Equal(tenant.Id, tenantId);
    }

    [Fact]
    public async Task GetCurrentTenantIdAsync_ReturnsNull_WhenNoTenantsAvailable()
    {
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto>());

        var tenantId = await _service.GetCurrentTenantIdAsync();

        Assert.Null(tenantId);
    }

    [Fact]
    public async Task SetCurrentTenantAsync_ReturnsTrue_ClearsTokenStore_AndRaisesEvent_ForAccessibleTenant()
    {
        var tenant = MakeTenantDto("Acme");
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { tenant });

        Guid? raisedTenantId = null;
        _service.TenantChanged += id => raisedTenantId = id;

        var result = await _service.SetCurrentTenantAsync(tenant.Id);

        Assert.True(result);
        Assert.Equal(tenant.Id, raisedTenantId);
        _mockTokenStore.Verify(s => s.ClearToken(), Times.Once);
        Assert.Equal(tenant.Id, await _service.GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task SetCurrentTenantAsync_ReturnsFalse_ForInaccessibleTenant()
    {
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { MakeTenantDto("Acme") });

        var result = await _service.SetCurrentTenantAsync(Guid.NewGuid());

        Assert.False(result);
        _mockTokenStore.Verify(s => s.ClearToken(), Times.Never);
    }

    [Fact]
    public async Task ClearCurrentTenantAsync_ClearsSelection_ClearsTokenStore_AndRaisesEvent()
    {
        var tenant = MakeTenantDto("Acme");
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { tenant });
        await _service.SetCurrentTenantAsync(tenant.Id);
        _mockTokenStore.Invocations.Clear();

        Guid? raisedTenantId = Guid.NewGuid();
        _service.TenantChanged += id => raisedTenantId = id;

        await _service.ClearCurrentTenantAsync();

        // GetCurrentTenantIdAsync auto-selects the first available tenant again on the very next
        // call - same behavior as BlazorTenantSelectionService, which has no way to distinguish
        // "user deliberately cleared" from "no selection made yet". The event firing with null and
        // the token store being cleared are what ClearCurrentTenantAsync actually guarantees.
        Assert.Null(raisedTenantId);
        _mockTokenStore.Verify(s => s.ClearToken(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HasAccessToTenantAsync_ReturnsTrueOnlyForTenantsInMineList()
    {
        var tenant = MakeTenantDto("Acme");
        _mockTenantsClient.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { tenant });

        Assert.True(await _service.HasAccessToTenantAsync(tenant.Id));
        Assert.False(await _service.HasAccessToTenantAsync(Guid.NewGuid()));
    }
}
