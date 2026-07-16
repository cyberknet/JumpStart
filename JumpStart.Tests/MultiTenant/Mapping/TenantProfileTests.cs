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

using AutoMapper;
using JumpStart.Data;
using JumpStart.MultiTenant.DTOs;
using JumpStart.MultiTenant.Mapping;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JumpStart.Tests.MultiTenant.Mapping;

/// <summary>
/// Tests for <see cref="TenantProfile"/>. See ADR-015.
/// </summary>
public class TenantProfileTests
{
    private readonly IMapper _mapper;

    public TenantProfileTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TenantProfile>();
        }, loggerFactory);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Configuration_IsValid()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TenantProfile>();
        }, loggerFactory);

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void TenantToDto_MapsAllFields()
    {
        var tenant = new Tenant
        {
            Name = "Acme Corporation",
            IsActive = true,
            ContactEmail = "admin@acme.com"
        };

        var dto = _mapper.Map<TenantDto>(tenant);

        Assert.Equal("Acme Corporation", dto.Name);
        Assert.True(dto.IsActive);
        Assert.Equal("admin@acme.com", dto.ContactEmail);
    }

    [Fact]
    public void CreateTenantDtoToTenant_MapsAllFields()
    {
        var createDto = new CreateTenantDto
        {
            Name = "Acme Corporation",
            IsActive = false,
            ContactEmail = "admin@acme.com"
        };

        var tenant = _mapper.Map<Tenant>(createDto);

        Assert.Equal("Acme Corporation", tenant.Name);
        Assert.False(tenant.IsActive);
        Assert.Equal("admin@acme.com", tenant.ContactEmail);
    }

    [Fact]
    public void UpdateTenantDtoToTenant_MapsAllFields()
    {
        var updateDto = new UpdateTenantDto
        {
            Id = System.Guid.NewGuid(),
            Name = "Acme Corporation Renamed",
            IsActive = true,
            ContactEmail = "billing@acme.com"
        };

        var tenant = _mapper.Map<Tenant>(updateDto);

        // Id is deliberately ignored by EntityMappingProfile's update map - the repository handles
        // it separately, based on the route/existing entity, not the mapped value.
        Assert.Equal("Acme Corporation Renamed", tenant.Name);
        Assert.Equal("billing@acme.com", tenant.ContactEmail);
    }
}
