# ADR-013: JWT Token Exchange for Permission Resolution

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

[ADR-012](012-role-based-permission-management.md) gives applications a real way to determine which
`Permission` claims a user should have (`IRoleRepository.GetPermissionClaimsForUserAsync`), but that
repository only exists where a `JumpStartDbContext`-derived context is registered - typically the API
project. A Blazor Server front end that authenticates users via ASP.NET Core Identity's cookie
(knowing the real, authenticated user) has no way to turn that into a `Permission`-bearing JWT for
calling the API, because:

- It cannot call `IRoleRepository` directly - no `JumpStartDbContext` is registered there.
- It cannot resolve permissions by calling the API over HTTP either, because the JWT it would use to
  authenticate that call is exactly the thing it's trying to produce - a circular dependency.

This is also where [ADR-011](011-entity-authorization.md) explicitly deferred a limitation:
`IJwtTokenService.GenerateToken(int userId, ...)` takes an `int` (inconsistent with
[ADR-009](009-guid-only-entities.md)'s Guid-only entities) and a flat `Dictionary<string, string>`
that cannot carry multiple `Permission` claims. Building a real token-issuance flow requires fixing
both.

## Decision

### 1. Reuse the Existing JWT Trust, No New Authentication Mechanism

The Blazor app and the API already share a JWT signing secret (`JwtSettings:SecretKey`) - that's how
`JwtAuthenticationHandler`-attached tokens get validated by the API's `AddJwtBearer` today. This
decision reuses that same trust relationship for a second purpose rather than introducing a new
mechanism (an API key, mTLS, a service secret) alongside it.

### 2. Two-Token Exchange

1. The Blazor app, having already authenticated the user via Identity's cookie, mints a **short-lived
   identity assertion token** - a normal JWT, signed with the same shared secret, carrying only the
   user's identity (no `Permission` claims), with a short expiration (~2 minutes). It is not stored
   anywhere; it exists only to make one call.
2. It calls `POST /api/token/exchange` on the API, passing that assertion token as its Bearer token.
3. The endpoint is protected by plain `[Authorize]` - not `[EntityAuthorize]`. Since the API already
   has `AddJwtBearer` configured, this "just works": any validly signed, non-expired JWT
   authenticates the call, whether or not it carries `Permission` claims. No new carve-out from
   entity-authorization is needed - `[EntityAuthorize]` is never applied here in the first place.
4. Inside the action, the endpoint extracts the caller's user ID from the assertion token's claims,
   calls `IRoleRepository.GetPermissionClaimsForUserAsync(userId)`, and mints the **real,
   long-lived, `Permission`-bearing JWT** using the now-fixed `IJwtTokenService`.
5. The Blazor app stores that real token in `ITokenStore`, where the existing
   `JwtAuthenticationHandler` attaches it to every subsequent API call automatically.

The chicken-and-egg problem is resolved by splitting "prove who I am" from "tell me what I can do"
into two tokens, exchanged in one round trip - the first token only needs to be *authenticated*, not
*authorized* for anything.

### 3. IJwtTokenService Signature Fix

```csharp
public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string username, IEnumerable<Claim>? additionalClaims = null, TimeSpan? expiration = null);
}
```

- `int userId` → `Guid userId`, consistent with [ADR-009](009-guid-only-entities.md).
- `Dictionary<string, string>?` → `IEnumerable<Claim>?`, so multiple `Permission` claims (or any
  other repeated claim type) can be added directly - the flat-dictionary limitation
  [ADR-011](011-entity-authorization.md) flagged is fixed here, not worked around again.
- New `TimeSpan? expiration` override lets the same method mint both the long-lived real token and
  the short-lived assertion token, rather than introducing a second, parallel token-minting method.

### 4. TokenController

A new framework-provided controller (`JumpStart/Services/Authentication/Controllers/TokenController.cs`),
registered opt-in via `JumpStartOptions.RegisterTokenController` (mirroring
`RegisterFormsController`/`RegisterAuthorizationController`):

```csharp
[ApiController]
[Route("api/token")]
public class TokenController : ControllerBase
{
    [HttpPost("exchange")]
    [Authorize]
    public async Task<ActionResult<TokenResponseDto>> Exchange() { ... }
}
```

This is deliberately a framework piece, not something every consuming application rebuilds -
JumpStart already owns `IJwtTokenService`, `JwtAuthenticationHandler`, and `ITokenStore`; token
*issuance* belongs alongside them.

### 5. Client Side

`ITokenExchangeApiClient` (Refit) passes the assertion token explicitly per call via
`[Header("Authorization")]`, rather than through `JwtAuthenticationHandler` (which would attach
whatever's currently in `ITokenStore` - nothing, the first time):

```csharp
[Post("/api/token/exchange")]
Task<TokenResponseDto> ExchangeAsync([Header("Authorization")] string bearerAssertionToken);
```

## Consequences

### Positive Consequences

- Closes a real gap: Blazor Server apps (or any client with its own authentication, not a
  `JumpStartDbContext`) can now obtain a real, permission-resolved JWT instead of hand-building one
  or granting a hardcoded blanket permission set.
- No new authentication mechanism - reuses the exact JWT signing/validation infrastructure already
  in place, keeping the framework's authentication story to one shape instead of two.
- Fixes `IJwtTokenService`'s Guid/multi-claim limitations for everyone, not just this flow -
  resolves the negative consequence [ADR-011](011-entity-authorization.md) explicitly flagged as
  deferred.

### Negative Consequences

- `IJwtTokenService.GenerateToken`'s signature change is breaking. Accepted deliberately - consistent
  with this framework's pre-1.0 status and the precedent set by [ADR-009](009-guid-only-entities.md).
- The assertion token is a second, distinct kind of JWT (identity-only, short-lived) alongside the
  real permission-bearing token. Anyone extending this flow needs to understand which one a given
  code path is holding - a plain, unclaimed JWT is not the same as an authorized one, even though
  both pass `[Authorize]`.
- `/api/token/exchange` trusts *any* validly signed JWT as proof of identity, including one minted
  moments ago purely for this exchange. This is intentional and safe within the trust boundary this
  framework already assumes (whoever holds `JwtSettings:SecretKey` is trusted), but is a boundary
  worth being deliberate about if that secret's distribution ever changes.

### Neutral Consequences

- `TokenController` requires `IRoleRepository` to be available, so `RegisterTokenController` also
  registers it (`TryAddScoped`) rather than requiring `RegisterAuthorizationController` as a
  prerequisite - an application can expose token exchange without exposing the Roles/UserPermissions
  CRUD API publicly.
- This flow assumes the caller already has *some* way to know the user's real identity (here,
  ASP.NET Core Identity's cookie in Blazor Server). It does not itself replace a login/credential
  -verification flow.

## Alternatives Considered

- **A dedicated service-to-service secret/API key for the exchange endpoint**: rejected - introduces
  a second trust mechanism alongside the JWT signing secret already shared between the two projects,
  for no real gain in this topology.
- **Calling `IRoleRepository` directly from the Blazor project**: rejected - would require registering
  a `JumpStartDbContext` (and therefore a full EF Core/SQL dependency) in a project that is otherwise
  a pure API-consuming client, undermining the "Why Separate API Project?" separation.
- **A single-token flow** (skip the assertion token, have the Blazor app call the exchange endpoint
  anonymously with just a raw user ID): rejected - trivially spoofable; anyone could request a token
  for any user ID with no proof of identity at all.

## References

- [ADR-011: Entity-Level Authorization](011-entity-authorization.md) - the `IJwtTokenService`
  limitation this decision fixes, and why the exchange endpoint needs no `[EntityAuthorize]` carve-out
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - the
  permission-resolution query this endpoint calls
- [ADR-004: JWT Authentication](004-jwt-authentication.md) - the original JWT authentication design;
  its `IJwtTokenService` code samples reflect the pre-this-decision signature (see the correction note
  added there)
- [ADR-009: Guid-Only Entities](009-guid-only-entities.md) - why `userId` is now `Guid`
