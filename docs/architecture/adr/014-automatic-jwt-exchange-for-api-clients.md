# ADR-014: Automatic JWT Exchange for Auto-Discovered API Clients

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

`RegisterApiClients` (invoked when `AutoDiscoverApiClients` is enabled) registers a bare Refit
client for every `[ApiClientFor<...>]`-decorated interface it finds - no message handlers are
attached. This means every auto-discovered client (`IFormsApiClient`, and now
`IRolesApiClient`/`IUserPermissionsApiClient` from [ADR-012](012-role-based-permission-management.md))
sends no `Authorization` header at all, while manually-registered clients (`IProductApiClient` in
the demo app) have always needed their handler chain wired up by hand.

The natural fix - an `ITokenProvisioner` interface that consuming apps implement, with a generic
handler that calls it - was considered and rejected. JumpStart is deliberately opinionated: it
tells consuming applications what to do and how, rather than maximizing configurability. By the
time this question came up, JumpStart had already prescribed the exact token-acquisition flow for
a Blazor Server client calling a separate JumpStart API
([ADR-013](013-jwt-token-exchange.md)) - and "get the current user via
`AuthenticationStateProvider`" isn't an app-specific choice requiring an interface; it is the
standard Blazor Server idiom every such app already uses. There was nothing left to abstract.

## Decision

### 1. JwtExchangeHandler - A Concrete, Framework-Provided Handler

`JumpStart.Services.Authentication.JwtExchangeHandler` (a `DelegatingHandler`, alongside
`JwtAuthenticationHandler`) implements ADR-013's exchange flow directly - no interface, no
per-app implementation required:

```csharp
public class JwtExchangeHandler(
    AuthenticationStateProvider authStateProvider,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetToken() == null)
        {
            // Get the real, already-authenticated user from AuthenticationStateProvider
            // (the standard Blazor Server idiom - not an app-specific choice), mint a
            // short-lived identity assertion token, exchange it for a real one, store it.
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
```

This is the same logic `DemoTokenProvisioningHandler` hand-rolled earlier this session, now
promoted to a framework class because it isn't actually demo-specific - it's *the* way a Blazor
Server app obtains a token from a separate JumpStart API.

### 2. Automatic Attachment in RegisterApiClients

`RegisterApiClients` attaches `JwtExchangeHandler` then `JwtAuthenticationHandler` (in that order -
the first `.AddHttpMessageHandler<T>()` call is outermost) to every auto-discovered client, but
only when all four of `JwtExchangeHandler`'s dependencies are already registered in the service
collection:

```csharp
private static bool CanAttachJwtExchangeHandlers(IServiceCollection services) =>
    services.Any(sd => sd.ServiceType == typeof(AuthenticationStateProvider)) &&
    services.Any(sd => sd.ServiceType == typeof(ITokenStore)) &&
    services.Any(sd => sd.ServiceType == typeof(IJwtTokenService)) &&
    services.Any(sd => sd.ServiceType == typeof(ITokenExchangeApiClient));
```

If any are missing (an API-only project with no Blazor front end, or an app not using this flow
at all), nothing is attached - identical to today's behavior. There is no flag to opt out or in;
the presence of the four services *is* the signal, consistent with `RegisterApiClients` already
deriving everything else (route, base address) from what's already declared rather than requiring
separate configuration.

### 3. Demo-Only Bootstrap Stays Separate

The demo app's "grant a brand-new user the Demo Administrator role" convenience
(`DemoBootstrapController`/`IDemoBootstrapApiClient` from ADR-013) is not part of
`JwtExchangeHandler` - it is not something JumpStart prescribes for every application. It becomes
its own thin `DemoBootstrapHandler`, manually slotted between `JwtExchangeHandler` and
`JwtAuthenticationHandler` in the demo app's own manually-registered client
(`IProductApiClient`) - auto-discovered clients registered via `RegisterApiClients` do not get it,
since the framework has no way to know about (or opt into) demo-specific bootstrap logic.

## Consequences

### Positive Consequences

- Closes the gap flagged earlier this session ("auto-discovered clients get no message handlers")
  for any Blazor Server application using JumpStart's prescribed authentication flow - zero extra
  wiring required, the same way route/base-address discovery already requires none.
- `DemoTokenProvisioningHandler` (100+ lines of hand-rolled, demo-specific-looking code) is deleted
  entirely; what was genuinely reusable is now a framework class, and what was genuinely
  demo-specific (bootstrap) is now clearly, separately marked as such.
- No new abstraction surface - consistent with JumpStart's opinionated design (see
  [ADR-012](012-role-based-permission-management.md) and this decision's own Context section).

### Negative Consequences

- The attachment is implicit: an application that happens to register all four prerequisite
  services for unrelated reasons, but does not want this exchange flow, gets it anyway with no way
  to opt out short of not registering one of the four services. Accepted as consistent with
  JumpStart's "tell the application what to do" philosophy, but worth documenting clearly.
- Only auto-discovered clients benefit. `IProductApiClient` (manually registered, since it predates
  and does not use `[ApiClientFor<...>]`) still requires the full handler chain to be wired by hand
  - `RegisterApiClients`'s detection logic has no reach into manual `AddApiClient<T>()` call sites.

### Neutral Consequences

- `JwtExchangeHandler` depends on `AuthenticationStateProvider`
  (`Microsoft.AspNetCore.Components.Authorization`), which is already available to JumpStart core
  for free via its existing `Microsoft.AspNetCore.App` `FrameworkReference` - no new package
  dependency.
- A brand-new demo user calling an auto-discovered client (Forms/Roles/UserPermissions) before ever
  triggering the manually-wired `IProductApiClient` pipeline will get a real but
  permission-empty token, since `DemoBootstrapHandler` only sits in that one pipeline. Accepted as
  a narrow, demo-only limitation, not a framework concern.

## Alternatives Considered

- **`ITokenProvisioner` interface + generic handler calling it**: rejected - see Context. JumpStart
  had already opinionatedly decided both the exchange flow and how to get the current user; adding
  an interface here would be extensibility for its own sake, not a genuine per-app choice.
- **Always attach `JwtExchangeHandler`/`JwtAuthenticationHandler` unconditionally**: rejected -
  would throw a DI resolution error for any application that doesn't register the four
  prerequisite services (e.g. a pure API project with no Blazor client at all).
- **A `JumpStartOptions` flag to enable/disable this behavior explicitly**: considered, but
  rejected as redundant - the four-service presence check already expresses exactly the condition
  under which this flow makes sense, without asking the consumer to declare it twice.

## References

- [ADR-013: JWT Token Exchange for Permission Resolution](013-jwt-token-exchange.md) - the exchange
  flow this handler implements
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - where the
  "prefer concrete, opinionated solutions over pluggable abstractions" principle was first stated
  in this session
- [ADR-005: Refit for API Clients](005-refit-api-clients.md) - `RegisterApiClients`'s existing
  route/base-address auto-discovery, extended here to also auto-attach handlers
