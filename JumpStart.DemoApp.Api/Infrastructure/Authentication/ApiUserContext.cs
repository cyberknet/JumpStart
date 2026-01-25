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
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Http;

namespace JumpStart.DemoApp.Api.Infrastructure.Authentication;

/// <summary>
/// Provides access to the current authenticated user from JWT bearer token in Web API requests.
/// This implementation extracts the user ID from JWT claims for automatic audit tracking.
/// </summary>
/// <remarks>
/// <para>
/// This class is designed for Web API scenarios where authentication is performed using
/// JWT bearer tokens. It extracts the user's ID from the ClaimTypes.NameIdentifier claim
/// in the JWT token and makes it available to repositories for audit tracking.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is thread-safe as it uses IHttpContextAccessor which provides per-request isolation.
/// Register as scoped in dependency injection.
/// </para>
/// <para>
/// <strong>Claims Expected:</strong>
/// The JWT token must include a ClaimTypes.NameIdentifier claim with a valid Guid value
/// representing the user's ID. If the claim is missing or invalid, null is returned.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Registration in Program.cs
/// builder.Services.AddHttpContextAccessor();
/// builder.Services.AddScoped&lt;IUserContext, ApiUserContext&gt;();
/// 
/// // JWT token generation (in Blazor Server)
/// var claims = new[]
/// {
///     new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
///     new Claim(ClaimTypes.Name, username),
///     new Claim(ClaimTypes.Email, email)
/// };
/// 
/// // Repository automatically uses ApiUserContext
/// public class ProductRepository : Repository&lt;Product&gt;
/// {
///     public ProductRepository(
///         DbContext context,
///         IUserContext userContext) // ApiUserContext injected here
///         : base(context, userContext)
///     {
///     }
/// }
/// 
/// // When AddAsync is called, user ID is automatically populated
/// var product = new Product { Name = "Test" };
/// await repository.AddAsync(product);
/// // product.CreatedById is automatically set from JWT claims
/// </code>
/// </example>
public class ApiUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiUserContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// The HTTP context accessor for accessing the current request's user claims.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="httpContextAccessor"/> is null.
    /// </exception>
    public ApiUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Asynchronously retrieves the unique identifier of the currently authenticated user from JWT claims.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The user's Guid identifier if authenticated and claim is valid
    /// - null if not authenticated, claim is missing, or claim value is not a valid Guid
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method extracts the user ID from the ClaimTypes.NameIdentifier claim in the
    /// JWT bearer token. The extraction is performed synchronously but returns a Task for
    /// interface compatibility.
    /// </para>
    /// <para>
    /// <strong>Return Scenarios:</strong>
    /// - User authenticated with valid Guid claim ? Returns Guid value
    /// - User not authenticated ? Returns null
    /// - Authenticated but claim missing ? Returns null
    /// - Authenticated but claim not parseable as Guid ? Returns null
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// This method is lightweight and performs no I/O operations. Claims are already
    /// available in memory from the JWT token validation.
    /// </para>
    /// </remarks>
    public Task<Guid?> GetCurrentUserIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        // Check if user is authenticated
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<Guid?>(null);
        }

        // Extract user ID claim from JWT token
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Try to parse as Guid
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return Task.FromResult<Guid?>(userId);
        }

        // Claim missing or not a valid Guid
        return Task.FromResult<Guid?>(null);
    }
}
