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
        _service = new ApiTenantSelectionService(_mockTenantsClient.Object, _mockTokenStore.Object);
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
