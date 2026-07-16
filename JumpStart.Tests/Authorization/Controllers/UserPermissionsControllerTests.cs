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
using JumpStart.Authorization.Controllers;
using JumpStart.Authorization.Mapping;
using JumpStart.Authorization.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JumpStart.Tests.Authorization.Controllers;

/// <summary>
/// Tests for <see cref="UserPermissionsController"/>'s custom <c>GetForUser</c> action.
/// See ADR-012.
/// </summary>
public class UserPermissionsControllerTests
{
    private readonly Mock<IUserPermissionRepository> _mockRepository;
    private readonly UserPermissionsController _controller;

    public UserPermissionsControllerTests()
    {
        _mockRepository = new Mock<IUserPermissionRepository>();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserPermissionProfile>();
        }, loggerFactory);
        var mapper = config.CreateMapper();

        var mockLogger = new Mock<ILogger<UserPermissionsController>>();

        var correlationContext = new CorrelationContext { CorrelationId = "test-correlation-id" };
        var mockCorrelationContextAccessor = new Mock<ICorrelationContextAccessor>();
        mockCorrelationContextAccessor.SetupGet(a => a.CorrelationContext).Returns(correlationContext);

        _controller = new UserPermissionsController(_mockRepository.Object, mapper, mockLogger.Object, mockCorrelationContextAccessor.Object);
    }

    [Fact]
    public async Task GetForUser_ReturnsDirectlyGrantedPermissions()
    {
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetPermissionsForUserAsync(userId))
            .ReturnsAsync(new List<string> { "Invoice.Get" });

        var result = await _controller.GetForUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var permissions = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(["Invoice.Get"], permissions);
    }

    [Fact]
    public async Task GetForUser_ReturnsEmpty_WhenUserHasNoDirectGrants()
    {
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetPermissionsForUserAsync(userId))
            .ReturnsAsync(new List<string>());

        var result = await _controller.GetForUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var permissions = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Empty(permissions);
    }
}
