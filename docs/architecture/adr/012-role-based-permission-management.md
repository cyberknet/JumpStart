# ADR-012: Role-Based Permission Management

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

[ADR-011](011-entity-authorization.md) requires every API call to carry a `Permission` claim
(`"{EntityName}.{Action}"`), but the framework has never provided a way to determine which
permissions a given user should actually have. `UserTenant.Role` (a free-text string field) looked
like it might serve this purpose, but it was never read by any authorization logic anywhere in the
framework - a purely descriptive label, not a real mechanism.

Consuming applications need a genuine way to say: here are some roles, here are the permission
claims each role grants, and here are the users who hold each role. Because [ADR-010](010-multi-tenant-data-isolation.md)
already establishes that a user's standing can differ per tenant (admin in one tenant, viewer in
another), permission grants need to support the same per-tenant scoping - but not require it. A
global system-administrator role, for example, is not owned by any single tenant, and an
application may want that kind of role to be assignable across tenants. Roles and permissions need
to be *capable of* tenant scoping, not *mandated* to have it - `ITenantScoped`'s non-nullable
`TenantId` cannot represent "this role is global." Real-world administration also sometimes needs
to grant a specific permission to a specific user without going through a role at all - an escape
hatch, not the primary model, but a real and common need.

## Decision

### 1. A Self-Contained Authorization Module

`Role`, `RolePermission`, `UserRole`, and `UserPermission` live under `JumpStart/Authorization/`
(namespace `JumpStart.Authorization`), structured exactly like the Forms module - entities at the
module root, with `Repositories/`, `Controllers/`, `DTOs/`, `Mapping/`, and `Clients/` subfolders.
The module sits alongside the framework's existing enforcement classes (`EntityAuthorizeAttribute`,
`EntityPolicyProvider`, `EntityPermissionHandler`), which already live in this folder.

### 2. ITenantScopedOptional - A New, Weaker Sibling of ITenantScoped

```csharp
public interface ITenantScopedOptional
{
    Guid? TenantId { get; set; }
    Tenant? Tenant { get; set; }
}
```

`ITenantScoped`'s `TenantId` is non-nullable by design (see [ADR-010](010-multi-tenant-data-isolation.md))
- every row *must* belong to exactly one tenant. That's the wrong contract for `Role`, `UserRole`,
and `UserPermission`: a role can be global (not owned by any tenant) or tenant-specific, and that
choice is made per-row, by whoever creates it - not fixed at the entity-type level. `TenantId == null`
means "global - applies regardless of which tenant is current."

`JumpStartDbContext.OnModelCreating` applies a second reflection-based global query filter (parallel
to the existing `ITenantScoped` one, added to `JumpStart/Data/JumpStartDbContext.OnModelCreating.cs`)
for entities implementing this interface:

```csharp
// WHERE CurrentTenantId IS NULL OR e.TenantId IS NULL OR e.TenantId == CurrentTenantId
```

Unlike `ITenantScoped`, `Repository<TEntity>.AddAsync` does **not** auto-populate `TenantId` for
`ITenantScopedOptional` entities. Auto-defaulting to the current tenant would silently remove the
"can be global" choice from whoever creates the row - the caller must set `TenantId` explicitly
(a real tenant ID, or leave it `null` for a global grant).

### 3. Role - An Optionally Tenant-Scoped Role Catalog

```csharp
public class Role : AuditableNamedEntity, ITenantScopedOptional
{
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
```

A tenant can own its own roles; a role can also be created with `TenantId = null` to make it global
(e.g. a platform-level "System Administrator" role assignable across every tenant). Which one an
application allows a given caller to do is an authorization-policy decision left to the consuming
application, the same way JumpStart leaves tenant/onboarding policy decisions elsewhere.

### 4. RolePermission - Grants on a Role

```csharp
public class RolePermission : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public string Permission { get; set; } = string.Empty; // e.g. "Product.Get"
}
```

Not itself tenant-scoped in any sense - isolation (or lack of it) flows transitively through
`RoleId → Role.TenantId`, the same way `QuestionOption` relates to `Question` without its own
`TenantId`.

### 5. UserRole - Optionally Tenant-Scoped Role Assignment

```csharp
public class UserRole : AuditableEntity, ITenantScopedOptional
{
    public Guid UserId { get; set; } // no FK/nav - JumpStart has no owned User entity
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
```

A user can hold multiple roles within the same tenant simultaneously, and can also hold a global
role (`TenantId == null`) assigned independently of any tenant. This is a deliberately separate
table from `UserTenant` - `UserTenant.Role` was a single nullable string (one value per
`UserId`+`TenantId`, enforced by its own unique index) and could not represent a user holding more
than one role in a tenant at once, let alone a global one. `UserTenant.Role` is removed as part of
this decision, having never been read by anything.

### 6. UserPermission - Direct Grant Escape Hatch

```csharp
public class UserPermission : AuditableEntity, ITenantScopedOptional
{
    public Guid UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
```

Grants a permission directly to a user, bypassing roles entirely, for the one-off case where routing
a grant through a role would be overhead rather than structure. Also optionally tenant-scoped, for
the same reason as `UserRole`.

### 7. Permission Resolution

`IRoleRepository.GetPermissionClaimsForUserAsync(Guid userId)` returns the full set of permission
strings a user holds - the union of permissions reached via `UserRole → RolePermission` and direct
`UserPermission` grants, distinct. No tenant parameter is needed: the query is automatically scoped
by the `ITenantScopedOptional` global filter described above, which naturally includes both the
current tenant's grants and any global (`TenantId == null`) grants in the same pass.

### 8. Explicit Per-Action Authorization

Every custom action on `RolesController` and `UserPermissionsController` (permission grant/revoke,
role assign/unassign) carries its own explicit `[EntityAuthorize(action: "...")]`. This module does
not rely on inherited protection for anything beyond the base CRUD actions `ApiControllerBase`
already secures.

### 9. DI Wiring

`JumpStartOptions.RegisterAuthorizationController` gates registration of both repositories and both
controllers, following the exact same opt-in shape as `RegisterFormsController`.

## Consequences

### Positive Consequences

- Real permission administration exists for the first time - roles, their granted permissions, and
  user assignments are all persisted, queryable, and administrable through a standard CRUD API.
- Roles and permissions can be either tenant-owned or global, matching real-world needs (a
  platform-level system-administrator role does not belong to any one tenant) without forcing every
  application into one model or the other.
- The direct `UserPermission` grant path covers the "not ideal, but it happens" real-world case
  without forcing every one-off grant through role administration.
- No changes were needed to `EntityPermissionHandler` or the `Permission` claim format itself - this
  module only changes how claims are *produced*, not how they're *checked*.
- `ITenantScopedOptional` is a small, generalizable addition to the framework - any future entity
  that needs the same "tenant-owned or global" shape can adopt it without inventing a new mechanism.

### Negative Consequences

- **Bootstrapping**: the first user in a newly created tenant holds zero roles and zero direct
  permissions, and therefore cannot call the very endpoints that would let them grant themselves
  access (unless the application seeds a global administrator role/assignment ahead of time - now
  possible precisely because of `ITenantScopedOptional`, but not automatic). This mirrors an
  already-accepted gap - tenant creation itself has no seeding/onboarding flow in the framework - and
  remains the consuming application's responsibility.
- Because `ITenantScopedOptional` entities are **not** auto-populated on create the way
  `ITenantScoped` ones are, whoever creates a `Role`/`UserRole`/`UserPermission` must remember to set
  `TenantId` explicitly if tenant-scoping is intended - an "easy to forget" surface the framework's
  other automatic mechanisms were specifically designed to avoid. This is an intentional trade-off
  (the alternative removes the ability to create a global grant at all) but is a real, novel gap
  compared to every other tenant-aware entity in the framework.
- Permission claims are still baked into the JWT at issuance time (per [ADR-011](011-entity-authorization.md)'s
  existing constraints); granting or revoking a role or permission does not take effect for an
  already-issued token until the next token issuance. The framework has no token-refresh mechanism
  today.
- Role-derived and directly-granted permissions are indistinguishable once resolved into claims -
  revoking a role does not affect a `UserPermission` grant of the same permission string, and vice
  versa. This is intentional (they are independent grants), but can surprise an administrator
  expecting role removal to fully revoke a permission.
- The framework now has two parallel tenant-scoping contracts (`ITenantScoped`, mandatory; and
  `ITenantScopedOptional`, optional) that behave differently in both query filtering and
  auto-population. Anyone extending the framework needs to know which one a given entity should use.

### Neutral Consequences

- `RolePermission` implements neither interface; its isolation is transitive through `Role`,
  matching the existing `QuestionOption`/`Question` precedent for child entities of a scoped parent.
- `UserRole.UserId`/`UserPermission.UserId` are bare `Guid` values with no foreign key or navigation
  property, matching the existing `FormResponse.RespondentUserId` precedent - JumpStart has no owned
  `User` entity, by design (see [ADR-009](009-guid-only-entities.md)).
- Deciding *who* is allowed to create a global (`TenantId == null`) `Role`/`UserRole`/`UserPermission`
  is an authorization-policy question, deliberately left to the consuming application (e.g. gating it
  behind a specific permission claim) rather than baked into the framework.
- Wiring live multi-tenant tenant-selection into the demo application end-to-end (a JWT tenant claim,
  a migration for `Tenant`/`UserTenant`, registering `BlazorTenantSelectionService`) is out of scope
  for this decision. The entities and repository logic described here are fully tenant-aware and
  correct for any consumer that does wire up `ITenantContext`; the demo app itself continues to
  operate in its current implicit single-tenant mode until that separate work is done.

## Alternatives Considered

- **Make `Role`/`UserRole`/`UserPermission` strictly `ITenantScoped` (this ADR's original draft)**:
  rejected - `ITenantScoped`'s non-nullable `TenantId` cannot represent a global role (e.g. a
  platform-level system administrator), and every tenant-scoping mechanism in the framework prior to
  this decision assumed mandatory, not optional, tenancy.
- **Extend `UserTenant.Role` into a foreign key instead of introducing `UserRole`**: rejected -
  `UserTenant`'s unique index on `(UserId, TenantId)` allows only one role value per tenant
  membership, which cannot represent a user holding multiple roles (or a global role) at once.
- **A single global (non-tenant-scoped) role catalog shared across all tenants, with no per-tenant
  option**: rejected - doesn't fit the "each tenant manages its own permissions" SaaS model
  [ADR-010](010-multi-tenant-data-isolation.md) already establishes for the common case, and would
  have made a tenant-owned role catalog impossible rather than optional.
- **Roles only, no direct `UserPermission` grant path**: rejected per explicit requirement - direct,
  role-bypassing grants are a real administrative need, not a hypothetical one.

## References

- [ADR-010: Multi-Tenant Data Isolation](010-multi-tenant-data-isolation.md) - the mandatory
  tenant-scoping mechanism `ITenantScopedOptional` deliberately relaxes for this module's entities
- [ADR-011: Entity-Level Authorization](011-entity-authorization.md) - the `Permission` claim format
  and enforcement mechanism this design produces claims for, without changing
- [ADR-009: Guid-Only Entities](009-guid-only-entities.md) - why `UserId` fields have no owned
  `User` entity to reference
