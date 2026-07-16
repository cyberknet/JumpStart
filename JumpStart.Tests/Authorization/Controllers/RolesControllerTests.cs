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
using AutoMapper;
using Correlate;
using JumpStart.Authorization;
using JumpStart.Authorization.Controllers;
using JumpStart.Authorization.DTOs;
using JumpStart.Authorization.Mapping;
using JumpStart.Authorization.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JumpStart.Tests.Authorization.Controllers;

/// <summary>
/// Tests for <see cref="RolesController"/>'s custom actions: permission grant/revoke and
/// user-role assign/unassign/list. See ADR-012.
/// </summary>
public class RolesControllerTests
{
    private readonly Mock<IRoleRepository> _mockRepository;
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _mockRepository = new Mock<IRoleRepository>();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
        }, loggerFactory);
        var mapper = config.CreateMapper();

        var mockLogger = new Mock<ILogger<RolesController>>();

        var correlationContext = new CorrelationContext { CorrelationId = "test-correlation-id" };
        var mockCorrelationContextAccessor = new Mock<ICorrelationContextAccessor>();
        mockCorrelationContextAccessor.SetupGet(a => a.CorrelationContext).Returns(correlationContext);

        _controller = new RolesController(_mockRepository.Object, mapper, mockLogger.Object, mockCorrelationContextAccessor.Object);
    }

    [Fact]
    public async Task GetPermissions_ReturnsPermissionsFromRepository()
    {
        var roleId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetPermissionsForRoleAsync(roleId))
            .ReturnsAsync(new List<string> { "Product.Get", "Product.List" });

        var result = await _controller.GetPermissions(roleId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var permissions = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(["Product.Get", "Product.List"], permissions);
    }

    [Fact]
    public async Task GrantPermission_ReturnsBadRequest_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("Permission", "Required");

        var result = await _controller.GrantPermission(Guid.NewGuid(), new GrantPermissionDto { Permission = "" });

        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GrantPermission_CallsRepositoryAndReturnsNoContent()
    {
        var roleId = Guid.NewGuid();
        _mockRepository.Setup(r => r.AddPermissionAsync(roleId, "Form.Get"))
            .ReturnsAsync(new RolePermission { RoleId = roleId, Permission = "Form.Get" });

        var result = await _controller.GrantPermission(roleId, new GrantPermissionDto { Permission = "Form.Get" });

        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.AddPermissionAsync(roleId, "Form.Get"), Times.Once);
    }

    [Fact]
    public async Task RevokePermission_ReturnsNoContent_WhenRemoved()
    {
        var roleId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RemovePermissionAsync(roleId, "Form.Get")).ReturnsAsync(true);

        var result = await _controller.RevokePermission(roleId, "Form.Get");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RevokePermission_ReturnsNotFound_WhenNotGranted()
    {
        var roleId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RemovePermissionAsync(roleId, "Form.Get")).ReturnsAsync(false);

        var result = await _controller.RevokePermission(roleId, "Form.Get");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetUsers_ReturnsUserIdsFromRepository()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetUsersForRoleAsync(roleId)).ReturnsAsync(new List<Guid> { userId });

        var result = await _controller.GetUsers(roleId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<Guid>>(okResult.Value);
        Assert.Equal([userId], users);
    }

    [Fact]
    public async Task AssignUser_CallsRepositoryWithTenantId_AndReturnsNoContent()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        _mockRepository.Setup(r => r.AssignUserToRoleAsync(userId, roleId, tenantId))
            .ReturnsAsync(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });

        var result = await _controller.AssignUser(roleId, userId, tenantId);

        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.AssignUserToRoleAsync(userId, roleId, tenantId), Times.Once);
    }

    [Fact]
    public async Task AssignUser_DefaultsTenantIdToNull_WhenOmitted()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.AssignUserToRoleAsync(userId, roleId, null))
            .ReturnsAsync(new UserRole { UserId = userId, RoleId = roleId, TenantId = null });

        var result = await _controller.AssignUser(roleId, userId);

        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.AssignUserToRoleAsync(userId, roleId, null), Times.Once);
    }

    [Fact]
    public async Task UnassignUser_ReturnsNotFound_WhenNotAssigned()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.UnassignUserFromRoleAsync(userId, roleId, null)).ReturnsAsync(false);

        var result = await _controller.UnassignUser(roleId, userId);

        Assert.IsType<NotFoundResult>(result);
    }
}
