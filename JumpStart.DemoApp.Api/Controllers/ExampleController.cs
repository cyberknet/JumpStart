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
using System.Threading.Tasks;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.DemoApp.Api.Controllers;

/// <summary>
/// Example API controller demonstrating JWT authentication and user context integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT authentication
public class ExampleController : ControllerBase
{
    private readonly IUserContext _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleController"/> class.
    /// </summary>
    /// <param name="userContext">The user context for retrieving the current user's ID.</param>
    public ExampleController(IUserContext userContext)
    {
        _userContext = userContext;
    }

    /// <summary>
    /// Gets the current authenticated user's ID from the JWT token.
    /// </summary>
    /// <returns>The current user's GUID, or null if not authenticated.</returns>
    /// <response code="200">Returns the current user's ID</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("current-user")]
    [ProducesResponseType(typeof(Guid?), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid?>> GetCurrentUser()
    {
        var userId = await _userContext.GetCurrentUserIdAsync();
        
        if (userId == null)
        {
            return Unauthorized("User not authenticated or user ID not found in token");
        }
        
        return Ok(new
        {
            UserId = userId,
            Message = "User authenticated successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Example endpoint that can be called without authentication.
    /// </summary>
    /// <returns>A welcome message.</returns>
    [HttpGet("public")]
    [AllowAnonymous] // Override the [Authorize] attribute
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetPublic()
    {
        return Ok(new
        {
            Message = "This is a public endpoint - no authentication required",
            Timestamp = DateTime.UtcNow
        });
    }
}
