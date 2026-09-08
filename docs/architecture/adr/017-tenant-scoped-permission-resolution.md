# ADR-017: Tenant-Scoped Permission Resolution

**Status:** Accepted

**Date:** 2026-09-07

**Decision Makers:** JumpStart Core Team

**Amends:** [ADR-012](012-role-based-permission-management.md) §7, [ADR-013](013-jwt-token-exchange.md)

## Context

[ADR-012](012-role-based-permission-management.md) §7 decided that permission resolution needs no
tenant parameter:

> No tenant parameter is needed: the query is automatically scoped by the `ITenantScopedOptional`
> global filter described above, which naturally includes both the current tenant's grants and any
> global (`TenantId == null`) grants in the same pass.

That is correct whenever a tenant is current. The framework does not guarantee one.

`ITenantScopedOptional`'s filter is `CurrentTenantId == null || e.TenantId == null || e.TenantId ==
CurrentTenantId`. Its first clause makes the whole filter a no-op, and `CurrentTenantId` is null
whenever the request carries no `tenant_id` claim. JumpStart mints exactly such tokens itself: the
reentrant path in `JwtExchangeHandler` issues a deliberately tenant-less assertion so that the
tenant list can be fetched in the first place (the comment there says so). Any resolution performed
under one of those returns **the union of a user's permissions across every tenant they belong to**.

The consequence is not a leak of another tenant's rows - [ADR-010](010-multi-tenant-data-isolation.md)'s
query filter still scopes *which* rows an operation touches. It is that a user's standing in one
tenant silently becomes their standing in all of them. A user who is an administrator in tenant A and
a read-only member of tenant B carries `Product.Delete` in a token stamped `tenant_id: B`, and
[ADR-011](011-entity-authorization.md)'s handler - which compares a flat claim string and knows
nothing about tenancy - grants it. Their deletions land on tenant B's rows, correctly scoped and
entirely unauthorized.

This makes "different standing in different tenants", which [ADR-010](010-multi-tenant-data-isolation.md)
and [ADR-012](012-role-based-permission-management.md) both explicitly set out to support,
inexpressible in practice.

A second, independent defect was found in the same query. Permissions are resolved by joining
`UserRole` directly to `RolePermission`:

```csharp
_context.Set<UserRole>()
    .Where(ur => ur.UserId == userId)
    .Join(_context.Set<RolePermission>(), ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.Permission);
```

`Role` is never joined. `Role` is an `AuditableNamedEntity` and therefore `IDeletable`, so
`Repository<Role>.DeleteAsync` soft-deletes it - the row survives with `DeletedOn` set. The role
then disappears from every query that *does* go through `Role` (its own controller, any admin
screen, the global soft-delete filter) while every permission it grants keeps resolving, for as long
as the `UserRole` rows exist. Deleting a role appears to work completely and revokes nothing.

## Decision

### 1. Resolution takes a tenant explicitly

```csharp
Task<IReadOnlyCollection<string>> GetPermissionClaimsForUserAsync(Guid userId, Guid? tenantId);
```

The tenant is a parameter, not an ambient condition the caller has to know is being applied. The
query filters `UserRole`/`UserPermission` to `TenantId == tenantId || TenantId == null` regardless of
`CurrentTenantId`, so the answer no longer depends on whether the caller happens to have a tenant
context established.

`tenantId: null` means "platform-wide grants only" - grants whose own `TenantId` is null. It
deliberately does **not** mean "everything", which is what the ambient filter did and what this ADR
exists to stop. A caller that genuinely wants every grant a user holds anywhere asks for it by name
(`GetAllPermissionClaimsForUserAsync`), which exists for administrative display and is never used to
mint a token.

The single-argument overload is removed rather than kept as a convenience. Its correctness depended
on ambient state, which is precisely the property that made the defect invisible.

### 2. Resolution joins `Role` and honours its soft delete

Both grant paths pass through their owning entity, so a deleted role grants nothing:

```csharp
var fromRoles = _context.Set<UserRole>()
    .Where(ur => ur.UserId == userId && (ur.TenantId == tenantId || ur.TenantId == null))
    .Join(_context.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => r)   // soft-delete filtered
    .Join(_context.Set<RolePermission>(), r => r.Id, rp => rp.RoleId, (r, rp) => rp.Permission);
```

The join to `Role` is not decoration: `Role`'s global soft-delete filter applies to it, which is what
makes the revocation real. `RolePermission` carries its own soft-delete filter already; what was
missing was the parent's.

### 3. Token exchange resolves against the tenant it verified

`TokenController.Exchange` already validates membership of the requested tenant before stamping
`tenant_id`. It now resolves permissions for that same tenant, so the claim set and the tenant claim
in a token always describe the same organization. A token with no `tenant_id` carries only
platform-wide permissions.

## Consequences

### Positive Consequences

- "Administrator in tenant A, read-only in tenant B" becomes expressible and enforced, as
  [ADR-010](010-multi-tenant-data-isolation.md) and [ADR-012](012-role-based-permission-management.md)
  always intended.
- A token's permission claims and its tenant claim can no longer disagree about which organization
  they describe.
- Deleting a role revokes it. Any application that has ever soft-deleted a role has been carrying
  live grants for it.
- Resolution no longer depends on ambient state, so it can be called correctly from a background
  job, a seeder, or a test without first establishing a tenant context.

### Negative Consequences

- **Breaking change** to `IRoleRepository`. The single-argument overload is gone; every caller must
  say which tenant it means. This is deliberate - a silent behaviour change under an unchanged
  signature would leave existing applications believing they were scoped when they were not.
- A user holding grants in several tenants now needs a token per tenant. That was already true of
  `tenant_id` itself; it is now true of the claims as well, so switching tenants always requires a
  fresh exchange.
- Applications that (knowingly or not) relied on the union behaviour to give users cross-tenant
  reach will find those users lose permissions. That is the fix, but it will present as a
  regression.

### Neutral Consequences

- The `Permission` claim format is unchanged, and [ADR-011](011-entity-authorization.md)'s handler
  needs no modification: it still compares a flat string. What changes is only which strings reach
  the token.
- Direct `UserPermission` grants are scoped by the same rule as role-derived ones, preserving
  [ADR-012](012-role-based-permission-management.md) §6's intent that the escape hatch behave like
  the primary path in every respect except how it is administered.

## Alternatives Considered

- **Keep the single-argument signature and read `ITenantContext` inside the repository**: rejected.
  It preserves the property that made this defect invisible - the answer depends on state the caller
  cannot see at the call site - and it makes the repository unusable from any context without an
  ambient tenant.
- **Fix only the filter (see [ADR-018](018-fail-closed-tenant-isolation.md)) and leave resolution
  ambient**: rejected as insufficient on its own. Denying on a null tenant would stop the union
  happening by accident, but resolution would still be silently coupled to ambient state, and a
  future caller that legitimately establishes a *different* tenant context before resolving would
  still get a wrong answer with no indication.
- **Encode the tenant into the claim value** (`"tenantA:Product.Delete"`): rejected. It changes the
  claim format for every application, defeats [ADR-011](011-entity-authorization.md)'s simple string
  comparison, and solves a problem that resolving correctly in the first place does not have.
- **Leave the `Role` join out and simply hard-delete roles instead**: rejected. It would make role
  deletion destroy its own audit trail, contradicting [ADR-003](003-audit-tracking.md), and any
  application calling the inherited `DeleteAsync` would still soft-delete and still be wrong.

## References

- [ADR-010: Multi-Tenant Data Isolation](010-multi-tenant-data-isolation.md) - the row-scoping this
  decision is deliberately *not* a substitute for
- [ADR-011: Entity-Level Authorization](011-entity-authorization.md) - the claim format and handler,
  unchanged by this decision
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - §7 of which
  this amends
- [ADR-013: JWT Token Exchange](013-jwt-token-exchange.md) - where resolved claims are minted
- [ADR-018: Fail-Closed Tenant Isolation](018-fail-closed-tenant-isolation.md) - the companion
  decision that stops a missing tenant meaning "everything"
