# ADR-015: Wiring Multi-Tenancy Into the Demo App

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

[ADR-010](010-multi-tenant-data-isolation.md) built the full multi-tenancy mechanism
(`ITenantScoped`/`ITenantScopedOptional`, the global query filter, `Tenant`/`UserTenant`,
`ITenantContext`, `ITenantSelectionService`), but it was never wired into the actual demo app -
explicitly deferred in [ADR-012](012-role-based-permission-management.md#deferred--flagged-items-not-part-of-this-pass).
Attempting to close that gap surfaced several real problems beyond "just register it":

1. **No migration exists for anything built this session.** `Tenant`, `UserTenant`, `Role`,
   `RolePermission`, `UserRole`, and `UserPermission` have zero schema in
   `JumpStart.DemoApp.Api`'s migrations - nothing added since ADR-010 can actually persist yet.
2. **`Tenant`/`UserTenant` have no administration surface at all** - no repository, controller,
   API client, or UI. There is no way to create a tenant or assign a user to one except raw SQL.
3. **`BlazorTenantSelectionService` queries the database directly** via
   `IDbContextFactory<JumpStartDbContext>`. The demo app has no `JumpStartDbContext` by design
   ([ADR-014](014-automatic-jwt-exchange-for-api-clients.md)'s "everything through API clients"
   principle) - this existing implementation cannot be used here as-is.
4. **Tenant selection has to reach the JWT exchange flow somehow**, and the obvious first design
   (let the client embed the selected `tenant_id` in its identity assertion and have the server
   trust it) is a real security defect: tenancy is meant to be a hard isolation boundary, and
   trusting a client-supplied tenant claim without server-side verification would let a bug or
   manipulation in client-side selection logic produce a real, server-signed token claiming access
   to a tenant the user was never actually granted - a classic confused-deputy problem. This
   violates the same "never trust the client" principle already applied earlier this session to
   referential integrity.

## Decision

### 1. Migration

A single EF Core migration in `JumpStart.DemoApp.Api` adds schema for every entity introduced this
session: `Tenant`, `UserTenant`, `Role`, `RolePermission`, `UserRole`, `UserPermission`.

### 2. Tenant/UserTenant Repositories, Controller, Client, and Admin UI

Full administration surface, mirroring the Roles module's shape
([ADR-012](012-role-based-permission-management.md)):

- `ITenantRepository : IRepository<Tenant>` - standard Tenant CRUD.
- `IUserTenantRepository : IRepository<UserTenant>` - standard CRUD (Create = add a user to a
  tenant, Delete = remove) plus `GetTenantsForUserAsync(Guid userId)` and
  `HasAccessAsync(Guid userId, Guid tenantId)` (checking both the membership row's and the
  tenant's `IsActive` flag) - the same query `BlazorTenantSelectionService` already performs, just
  callable from a repository instead of inline in a Blazor service.
- `TenantsController : ApiControllerBase<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto, ITenantRepository>`
  - standard CRUD (real tenant administration, `[EntityAuthorize]`-protected like any other entity)
  plus:
  - `GET /api/tenants/mine` - **self-service**, `[Authorize]` only (not `[EntityAuthorize]`) -
    returns only the tenants the calling user belongs to. This is deliberately not gated by a
    `Tenant.List` permission: a regular user asking "what tenants am I in" is not administering the
    `Tenant` entity, and gating it behind `[EntityAuthorize]` would either require every user to
    hold a `Tenant.List` permission just to log in, or leak the existence of every tenant in the
    system to every caller who does. Mirrors `TokenController.Exchange`/`DemoBootstrapController.EnsureAdmin`'s
    existing self-service, identity-only authorization shape.
  - `POST /api/tenants/{tenantId}/users/{userId}` / `DELETE .../users/{userId}` - membership
    management, `[EntityAuthorize(action: "ManageMembership")]` (real administration, unlike the
    self-service endpoint above).
- Unlike the Authorization module, `Tenant`/`UserTenant` entities themselves are **not** relocated
  into a new module folder - they're referenced from many existing places (`ITenantScoped`,
  `ITenantScopedOptional`, `JumpStartDbContext`) and moving them is unjustified churn for what is
  fundamentally an additive change. The new repository/controller/client/DTOs live under
  `JumpStart/MultiTenant/` even though the entities stay in `JumpStart/Data/` - a deliberate,
  documented exception to the "one module, one folder" convention.
- Demo app gets `TenantsList.razor`/`TenantEditor.razor` (mirroring `RolesList.razor`/`RoleEditor.razor`)
  for creating tenants and managing membership.

### 3. TokenController Validates Tenant Membership Server-Side

`TokenController.Exchange` reads an optional `tenant_id` claim from the incoming identity
assertion. If present, it calls `IUserTenantRepository.HasAccessAsync(userId, tenantId)` **before**
trusting it:

- **Valid membership:** the real token gets a `tenant_id` claim matching the request.
- **No `tenant_id` claim present:** the real token is issued with no tenant claim (global/no-tenant
  context - existing null-tenant semantics apply, unchanged).
- **Invalid membership (claimed tenant the user doesn't belong to):** the exchange is **rejected
  outright** (`403 Forbidden`) rather than silently dropping the claim. A client asserting access to
  a tenant it doesn't have is treated as a hard failure to surface loudly, not a soft fallback to
  paper over - consistent with tenancy being a hard security boundary, not a convenience feature.

This is the one piece of server-side state the framework didn't previously need for `TokenController`
- it now takes a dependency on `IUserTenantRepository` alongside `IRoleRepository`.

### 4. JwtExchangeHandler Becomes Optionally Tenant-Aware

`JwtExchangeHandler` gains optional tenant awareness: when an `ITenantSelectionService` is
registered, it adds a `tenant_id` claim (from `GetCurrentTenantIdAsync()`) to the identity assertion
before exchanging. Applications that don't use multi-tenancy simply don't register
`ITenantSelectionService`, and nothing changes for them.

**This is resolved via `IServiceProvider` inside `SendAsync`, not constructor injection** - an
initial implementation took `ITenantSelectionService? tenantSelectionService = null` as a
constructor parameter (mirroring how `JumpStartDbContext` takes an optional `ITenantContext?`), but
that shape is a genuine circular dependency, not just a superficial one: an API-client-based
`ITenantSelectionService` (`ApiTenantSelectionService`) depends on an API client whose own HTTP
pipeline includes this very handler. Since `JwtExchangeHandler` is constructed while
`DefaultHttpClientFactory` builds *that same client's* handler pipeline, resolving
`ITenantSelectionService` in the constructor forces resolving that API client - and therefore
re-entering construction of the pipeline currently being built - before the handler object even
exists. This reliably reproduced as `InvalidOperationException: ValueFactory attempted to access
the Value property of this instance` (`System.Lazy<T>`'s own reentrancy guard tripping) the first
time any component resolved an API client whose pipeline included both this handler and a
tenant-selection dependency on that same client.

The fix has two parts:
- **Lazy resolution:** the handler takes `IServiceProvider` in its constructor and calls
  `serviceProvider.GetService<ITenantSelectionService>()` inside `SendAsync`, not the constructor.
  This defers resolution until well after the handler's own construction (and its owning client's
  pipeline) has completed and been cached - by then, resolving `ApiTenantSelectionService`'s API
  client dependency is safe, whether it reuses an already-built pipeline or builds a different one.
- **Reentrancy guard:** even resolved lazily, the *first* exchange (no token yet) still involves
  `ApiTenantSelectionService` calling its own API client to discover the current tenant, which goes
  through this same handler's `SendAsync` a second time, reentrant. An `AsyncLocal<bool>` guard
  detects that nested call and skips the tenant lookup for it - the inner call only needs *a* valid
  token to complete (a plain, tenant-less exchange), not a tenant-aware one. Once the lookup
  resolves in the outer call, it re-exchanges once more with the tenant claim.

### 5. ApiTenantSelectionService - API-Client-Based Tenant Selection

A new `ITenantSelectionService` implementation for applications with no direct database access:

```csharp
public class ApiTenantSelectionService(
    AuthenticationStateProvider authStateProvider,
    ITenantsApiClient tenantsClient,
    ITokenStore tokenStore) : ITenantSelectionService
{
    // GetAvailableTenantsAsync() -> tenantsClient.GetMineAsync()
    // SetCurrentTenantAsync(tenantId) -> validates against the already-fetched "mine" list,
    //   updates in-memory state, clears ITokenStore so the next call re-triggers
    //   JwtExchangeHandler with the new tenant_id - no separate "select" endpoint call needed
    // HasAccessToTenantAsync(tenantId) -> checks the cached "mine" list
}
```

`BlazorTenantSelectionService` (direct-DB) is unchanged and remains the right choice for
applications that do have their own `JumpStartDbContext` - this is a second, alternative
implementation for the properly-separated topology, not a replacement.

### 6. Demo-Only Tenant Bootstrap

Mirroring `DemoBootstrapController`'s existing "grant a first-time user the Demo Administrator
role" behavior: if a user has zero tenant memberships, they're also added to a shared "Demo
Tenant" (created if it doesn't exist), so tenant selection isn't a dead end on first login. Not a
framework concept - demo convenience only, same as the existing role bootstrap.

## Consequences

### Positive Consequences

- Closes a real, previously-unnoticed gap: nothing built this session (Forms, Roles, Permissions)
  could actually persist without this migration - this was blocking, not cosmetic.
- Server-side tenant validation in `TokenController` closes a real security hole before it ever
  shipped, rather than after.
- `ApiTenantSelectionService` establishes the same "framework prescribes the properly-separated
  pattern" precedent as `JwtExchangeHandler` (ADR-014) - a Blazor-Server-plus-separate-API
  JumpStart app now has a complete, working reference for every layer (data, permissions, tenancy).

### Negative Consequences

- `TokenController` now depends on `IUserTenantRepository` unconditionally, even for applications
  that register `RegisterTokenController` but don't use multi-tenancy. This mirrors
  `IRoleRepository`'s existing unconditional dependency and is accepted as consistent with it, but
  is a second mandatory dependency for a controller whose core job (identity exchange) doesn't
  intrinsically require either.
- Two different `ITenantSelectionService` implementations now exist
  (`BlazorTenantSelectionService`, direct-DB; `ApiTenantSelectionService`, API-client-based) with no
  shared base - an application must know which topology it has and pick correctly. There is no
  automatic detection/attachment for this the way ADR-014 auto-attaches JWT handlers, since which
  implementation to use is a structural choice (does this app have a `JumpStartDbContext`?), not a
  service-presence signal.
- `Tenant`/`UserTenant`'s repository/controller/client living under `JumpStart/MultiTenant/` while
  the entities themselves stay in `JumpStart/Data/` is an intentional but real inconsistency with
  the "one module, one folder" convention Forms and Authorization established.
- Any `ITenantSelectionService` implementation that itself depends on an API client (like
  `ApiTenantSelectionService`) inherently risks the DI/HttpClientFactory circular-construction
  failure mode described in section 4, if some future change makes `JwtExchangeHandler` resolve it
  any more eagerly than "lazily, inside `SendAsync`, guarded against reentrancy." This is a real,
  non-obvious constraint on that class of implementation going forward, not just a one-time bug fix.

### Neutral Consequences

- The demo tenant bootstrap and the demo admin-role bootstrap ([ADR-013](013-jwt-token-exchange.md))
  are separate, independent mechanisms that happen to run at similar times - neither depends on the
  other.

## Alternatives Considered

- **Trust the client-supplied `tenant_id` claim without server-side re-validation**: rejected -
  the security concern that prompted this ADR. A confused-deputy risk is not an acceptable
  trade-off for a hard isolation boundary.
- **Give the Blazor app its own (read-only) `JumpStartDbContext` so `BlazorTenantSelectionService`
  works unmodified**: rejected - reopens the exact "no `JumpStartDbContext` in the Blazor project"
  architecture violation just corrected in `docs/samples.md`.
- **Relocate `Tenant`/`UserTenant` into a `JumpStart/MultiTenant/` module folder alongside the new
  repository/controller/client**: rejected for this pass - too much unrelated churn across every
  existing reference to these entities for a change that's additive in spirit.

## References

- [ADR-010: Multi-Tenant Data Isolation](010-multi-tenant-data-isolation.md) - the mechanism this
  decision finally wires end-to-end
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - where
  wiring this was first explicitly deferred, and the admin-module shape this mirrors
- [ADR-013: JWT Token Exchange for Permission Resolution](013-jwt-token-exchange.md) /
  [ADR-014: Automatic JWT Exchange for Auto-Discovered API Clients](014-automatic-jwt-exchange-for-api-clients.md) -
  the exchange flow `TokenController`/`JwtExchangeHandler` extend here
