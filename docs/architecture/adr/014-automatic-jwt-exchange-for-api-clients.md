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
`JwtExchangeHandler` - it is not something JumpStart prescribes for every application. ~~It becomes
its own thin `DemoBootstrapHandler`, manually slotted between `JwtExchangeHandler` and
`JwtAuthenticationHandler` in the demo app's own manually-registered client
(`IProductApiClient`) - auto-discovered clients registered via `RegisterApiClients` do not get it,
since the framework has no way to know about (or opt into) demo-specific bootstrap logic.~~ **See
the correction below - this specific mechanism was replaced.**

## Correction (post-acceptance): bootstrap moved to registration time, not a message handler

The original `DemoBootstrapHandler` design (a `DelegatingHandler` that inspected the *first* API
response's token for missing `Permission` claims, then triggered the grant) had two real problems,
both found only once the demo app was finally run live end-to-end:

1. **It only ever sat in `IProductApiClient`'s pipeline.** A brand-new user whose first API call
   happened to go through any *other* client (Forms, Roles, Tenants, UserPermissions - all
   auto-discovered, none of which had `DemoBootstrapHandler` attached) got a real but
   permission-empty token and a 403, with the bootstrap check never given a chance to run. This was
   called out as an accepted limitation in the original "Negative Consequences" below, but in
   practice it meant the demo was broken for any user who didn't happen to visit Products first -
   not an acceptable "narrow" limitation once actually exercised.
2. **It had the exact same `AuthenticationStateProvider`-in-a-`DelegatingHandler` bug this ADR's own
   first correction fixed in `JwtExchangeHandler`** - `DemoBootstrapHandler` also took
   `AuthenticationStateProvider` as a direct constructor parameter, and would have thrown the same
   "outside of DI scope" exception the moment its bootstrap-check branch actually executed.

The fix: `DemoBootstrapHandler` is deleted entirely. In its place, a plain scoped service,
`DemoNewUserBootstrapper` (`JumpStart.DemoApp/Services/DemoNewUserBootstrapper.cs`), is called
**directly from `Register.razor` and `ExternalLogin.razor`, immediately after `UserManager.CreateAsync`
succeeds** - the actual moment a new user is created, not an inferred signal from a token's claims:

```csharp
// Register.razor / ExternalLogin.razor, right after account creation succeeds:
await DemoBootstrapper.EnsureAdminAsync(user.Id, Input.Email);
```

This works cleanly because both call sites are genuinely circuit-scoped Razor components -
`DemoNewUserBootstrapper` only needs `IJwtTokenService` (a stateless JWT minter, no
`AuthenticationStateProvider` involved at all) and `IDemoBootstrapApiClient`, both safe to inject
and call directly. There is no message-handler pipeline involved, so there is no "which client's
chain is this attached to" question to get wrong, and no DI-scope mismatch to work around. The call
is deliberately best-effort (caught and logged, never thrown) so a bootstrap failure can't block
registration itself.

This also means every user, regardless of which page they visit first, is bootstrapped at the same
moment - account creation - rather than lazily and inconsistently on first API call.

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
- ~~A brand-new demo user calling an auto-discovered client (Forms/Roles/UserPermissions) before
  ever triggering the manually-wired `IProductApiClient` pipeline will get a real but
  permission-empty token, since `DemoBootstrapHandler` only sits in that one pipeline. Accepted as
  a narrow, demo-only limitation, not a framework concern.~~ **Resolved by the correction above** -
  bootstrap now happens once, at account creation, regardless of which client a user calls first.

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

## Correction (post-acceptance)

The constructor shown in Decision §1 and the "Neutral Consequences" claim that
`AuthenticationStateProvider` is "already available... for free" were both wrong in a way that only
surfaced once the demo app was actually run live for the first time (this session had no working
LocalDB until then, so nothing here had been exercised end-to-end).

**The problem:** `IHttpClientFactory` builds a client's message-handler pipeline (everything
registered via `.AddHttpMessageHandler<T>()`) in its own DI scope, separate from the Blazor circuit
that's calling it. Injecting `AuthenticationStateProvider` directly into `JwtExchangeHandler`'s
constructor resolves an instance from that separate scope - one the framework's rendering pipeline
never "activates" - and calling `GetAuthenticationStateAsync()` on it throws
`InvalidOperationException: Do not call GetAuthenticationStateAsync outside of the DI scope for a
Razor component`. This is a documented, known limitation of `IHttpClientFactory` combined with
Blazor Server circuit scoping, not a JumpStart-specific mistake in isolation - but the original
constructor signature above didn't account for it.

**The fix:** `JwtExchangeHandler` now takes `CircuitServicesAccessor` instead of
`AuthenticationStateProvider` directly, and resolves `AuthenticationStateProvider` from
`circuitServicesAccessor.Services` inside `SendAsync`, not the constructor:

```csharp
public class JwtExchangeHandler(
    CircuitServicesAccessor circuitServicesAccessor,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient,
    IServiceProvider serviceProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetToken() == null)
        {
            var authStateProvider = circuitServicesAccessor.Services?.GetService<AuthenticationStateProvider>();
            if (authStateProvider == null)
                return await base.SendAsync(request, cancellationToken);
            // ... mint assertion, exchange, store token, as before
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
```

`CircuitServicesAccessor` (`JumpStart/Services/Authentication/CircuitServicesAccessor.cs`) is
Microsoft's own documented solution to this exact problem - see [ASP.NET Core Blazor dependency
injection: "Access server-side Blazor services from a different DI
scope"](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/dependency-injection#access-server-side-blazor-services-from-a-different-di-scope)
and [ASP.NET Core server-side and Blazor Web App additional security scenarios: "Circuit activity
handler"](https://learn.microsoft.com/aspnet/core/blazor/security/additional-scenarios#access-authenticationstateprovider-in-outgoing-request-middleware).
It's an `AsyncLocal<IServiceProvider?>`-backed accessor, populated by
`ServicesAccessorCircuitHandler` (a `CircuitHandler` - a genuine Blazor Server extensibility point
that *does* run within the correct circuit scope) for the duration of each inbound circuit activity
(a SignalR message - an event callback, a lifecycle method). Because `AsyncLocal` flows with the
async call chain rather than with DI scope, it correctly resolves the real circuit's
`IServiceProvider` even when read from `JwtExchangeHandler`'s differently-scoped `SendAsync`.

`RegisterApiClients` registers `CircuitServicesAccessor`/`ServicesAccessorCircuitHandler`
automatically (`EnsureCircuitServicesAccessorRegistered`) whenever `CanAttachJwtExchangeHandlers`
succeeds - consistent with this ADR's "zero extra wiring required" positive consequence; that claim
still holds, it just needed one more moving part than originally designed to actually be true.

This correction was made alongside [ADR-015](015-multi-tenancy-in-demo-app.md)'s work, which is
also where the recursive-construction issue documented there (`JwtExchangeHandler` resolving
`ITenantSelectionService` lazily via `IServiceProvider`) was found and fixed - both are instances of
the same underlying lesson: this handler runs in a DI scope with fewer guarantees than a Razor
component's, and every dependency added to it since this ADR has needed to account for that.

**A second correction, found immediately after the first:** `CircuitServicesAccessor.Services` is
only non-null while an actual circuit activity (a SignalR message) is being handled - it is `null`
during Blazor's *static prerender* pass, since prerendering happens before the circuit exists at
all. The first version of this fix handled that case by silently falling through
(`if (authStateProvider == null) return await base.SendAsync(request, cancellationToken);`),
sending the request through with no token. For any endpoint gated by `[Authorize]`
(essentially everything in this framework), that produces a 401 several layers downstream with no
indication of the real cause - exactly the kind of silent-failure path this session's "never trust
the client" discipline argues against on the *security* side, and just as unhelpful on the
*correctness* side. `JwtExchangeHandler` now throws `InvalidOperationException` instead, naming the
cause directly (no active circuit) and the fix (disable prerendering for the calling component).

This is also a real, previously-unknown constraint the demo app's `TenantSwitcher` integration
exposed: **every page/component that calls an auto-discovered API client from `OnInitializedAsync`
(or another lifecycle method) must disable prerendering**, e.g.:

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

Fourteen call sites across the demo app needed this (every page under `Roles/`, `Tenants/`,
`Forms/`, `QuestionTypes/`, plus `Products.razor` and `MainLayout.razor`'s `TenantSwitcher` usage) -
all of them were latently broken this way since ADR-013/014 first shipped, just never exercised
end-to-end until LocalDB finally worked in this session. This is now the exception message's
explicit guidance, so the next occurrence surfaces immediately instead of being rediscovered from
scratch.
