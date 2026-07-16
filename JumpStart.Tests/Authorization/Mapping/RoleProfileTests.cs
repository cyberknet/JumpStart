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
using AutoMapper;
using JumpStart.Authorization;
using JumpStart.Authorization.DTOs;
using JumpStart.Authorization.Mapping;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JumpStart.Tests.Authorization.Mapping;

/// <summary>
/// Tests for <see cref="RoleProfile"/> and <see cref="UserPermissionProfile"/>. See ADR-012.
/// </summary>
public class RoleProfileTests
{
    private readonly IMapper _mapper;

    public RoleProfileTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
            cfg.AddProfile<UserPermissionProfile>();
        }, loggerFactory);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Configuration_IsValid()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
            cfg.AddProfile<UserPermissionProfile>();
        }, loggerFactory);

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void RoleToDto_MapsPermissionsCollection_ToStringList()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Editor",
            Description = "Can edit content",
            TenantId = Guid.NewGuid(),
            Permissions =
            [
                new RolePermission { Permission = "Form.Get" },
                new RolePermission { Permission = "Form.Update" }
            ]
        };

        var dto = _mapper.Map<RoleDto>(role);

        Assert.Equal("Editor", dto.Name);
        Assert.Equal(role.TenantId, dto.TenantId);
        Assert.Equal(2, dto.Permissions.Count);
        Assert.Contains("Form.Get", dto.Permissions);
        Assert.Contains("Form.Update", dto.Permissions);
    }

    [Fact]
    public void CreateRoleDtoToRole_PreservesNullTenantId_ForGlobalRole()
    {
        var createDto = new CreateRoleDto { Name = "System Administrator", TenantId = null };

        var role = _mapper.Map<Role>(createDto);

        Assert.Equal("System Administrator", role.Name);
        Assert.Null(role.TenantId);
    }

    [Fact]
    public void CreateRoleDtoToRole_PreservesTenantId_ForTenantOwnedRole()
    {
        var tenantId = Guid.NewGuid();
        var createDto = new CreateRoleDto { Name = "Billing Manager", TenantId = tenantId };

        var role = _mapper.Map<Role>(createDto);

        Assert.Equal(tenantId, role.TenantId);
    }

    [Fact]
    public void CreateUserPermissionDtoToUserPermission_MapsAllFields()
    {
        var userId = Guid.NewGuid();
        var createDto = new CreateUserPermissionDto { UserId = userId, Permission = "Invoice.Get", TenantId = null };

        var grant = _mapper.Map<UserPermission>(createDto);

        Assert.Equal(userId, grant.UserId);
        Assert.Equal("Invoice.Get", grant.Permission);
        Assert.Null(grant.TenantId);
    }
}
