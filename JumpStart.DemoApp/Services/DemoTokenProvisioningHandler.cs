using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JumpStart.Services.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace JumpStart.DemoApp.Services;

/// <summary>
/// Ensures a signed-in user's <see cref="ITokenStore"/> holds a JWT with the demo's
/// <c>Permission</c> claims before any request reaches <see cref="JwtAuthenticationHandler"/>.
/// </summary>
/// <remarks>
/// <para>
/// JumpStart's <c>ApiControllerBase</c> actions all require a <c>Permission</c> claim of the form
/// <c>"{EntityName}.{Action}"</c> (see ADR-011) - without one, every API call the Blazor app makes
/// returns 403. <see cref="IJwtTokenService.GenerateToken"/>'s <c>additionalClaims</c> parameter is
/// a flat <c>Dictionary&lt;string, string&gt;</c> (one value per key), so it can't add the several
/// distinct <c>Permission</c> claims this demo needs on its own - this handler builds the token
/// directly instead, using the same signing configuration <see cref="JwtTokenService"/> reads from
/// <c>JwtSettings</c>.
/// </para>
/// <para>
/// Registered as the outermost handler in the Refit client pipeline (before
/// <see cref="JwtAuthenticationHandler"/>), so the token is guaranteed to exist before that handler
/// tries to attach it - this avoids relying on Blazor component lifecycle ordering.
/// </para>
/// </remarks>
public class DemoTokenProvisioningHandler(
    AuthenticationStateProvider authStateProvider,
    ITokenStore tokenStore,
    IConfiguration configuration) : DelegatingHandler
{
    private static readonly string[] DemoEntities = ["Product", "Form", "QuestionType"];
    private static readonly string[] DemoActions = ["Get", "List", "Create", "Update", "Delete"];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetToken() == null)
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = user.Identity.Name ?? userIdClaim ?? "demo-user";

                tokenStore.SetToken(GenerateDemoToken(userIdClaim, username));
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Builds a JWT carrying the current user's real Guid identifier plus a full set of demo
    /// <c>Permission</c> claims (every action, for every entity this demo app exposes).
    /// </summary>
    private string GenerateDemoToken(string? userIdClaim, string username)
    {
        var secretKey = configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var audience = configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");
        var expirationMinutes = int.Parse(configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userIdClaim ?? Guid.Empty.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var entity in DemoEntities)
        {
            foreach (var action in DemoActions)
            {
                claims.Add(new Claim("Permission", $"{entity}.{action}"));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
