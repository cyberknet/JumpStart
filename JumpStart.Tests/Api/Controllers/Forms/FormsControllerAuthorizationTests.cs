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

using JumpStart.Authorization;
using JumpStart.Forms.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace JumpStart.Tests.Api.Controllers.Forms;

/// <summary>
/// Verifies that <see cref="FormsController"/>'s hand-written custom actions - which, per ADR-011,
/// do not inherit <c>[EntityAuthorize]</c> automatically the way the base CRUD actions do - each
/// carry an explicit <see cref="EntityAuthorizeAttribute"/> and are actually enforced by
/// <see cref="EntityPermissionHandler"/>: a request without the matching "Form.{Action}" permission
/// claim must not be authorized.
/// </summary>
public class FormsControllerAuthorizationTests
{
    [Theory]
    [InlineData(nameof(FormsController.GetActiveForms), "Form.List")]
    [InlineData(nameof(FormsController.GetFormStatistics), "Form.Get")]
    [InlineData(nameof(FormsController.SubmitFormResponse), "Form.SubmitResponse")]
    [InlineData(nameof(FormsController.GetFormResponseById), "Form.GetResponse")]
    [InlineData(nameof(FormsController.DeleteAllFormResponses), "Form.DeleteResponses")]
    public async Task CustomAction_RequiresMatchingPermissionClaim(string methodName, string requiredPermission)
    {
        var handler = new EntityPermissionHandler();
        var userWithoutClaim = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));

        var contextWithoutClaim = BuildAuthorizationContext(methodName, userWithoutClaim);
        await handler.HandleAsync(contextWithoutClaim);
        Assert.False(contextWithoutClaim.HasSucceeded);

        var identity = new ClaimsIdentity([new Claim("Permission", requiredPermission)], "TestAuth");
        var authorizedUser = new ClaimsPrincipal(identity);

        var contextWithClaim = BuildAuthorizationContext(methodName, authorizedUser);
        await handler.HandleAsync(contextWithClaim);
        Assert.True(contextWithClaim.HasSucceeded);
    }

    [Theory]
    [InlineData(nameof(FormsController.GetActiveForms), "Form.Get")] // wrong action for this endpoint
    [InlineData(nameof(FormsController.SubmitFormResponse), "Form.Create")] // base CRUD permission must not leak into response submission
    [InlineData(nameof(FormsController.DeleteAllFormResponses), "Form.Delete")] // base CRUD permission must not leak into bulk response deletion
    public async Task CustomAction_IsRejected_WhenUserHasUnrelatedPermissionClaim(string methodName, string unrelatedPermission)
    {
        var handler = new EntityPermissionHandler();
        var identity = new ClaimsIdentity([new Claim("Permission", unrelatedPermission)], "TestAuth");
        var userWithWrongClaim = new ClaimsPrincipal(identity);

        var context = BuildAuthorizationContext(methodName, userWithWrongClaim);
        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext BuildAuthorizationContext(string methodName, ClaimsPrincipal user)
    {
        var actionDescriptor = new ControllerActionDescriptor
        {
            MethodInfo = typeof(FormsController).GetMethod(methodName)!,
            ControllerTypeInfo = typeof(FormsController).GetTypeInfo(),
        };

        var endpoint = new Endpoint(
            requestDelegate: null,
            metadata: new EndpointMetadataCollection(actionDescriptor),
            displayName: methodName);

        var httpContext = new DefaultHttpContext();
        httpContext.SetEndpoint(endpoint);

        var requirement = new EntityPermissionRequirement();
        return new AuthorizationHandlerContext([requirement], user, httpContext);
    }
}
