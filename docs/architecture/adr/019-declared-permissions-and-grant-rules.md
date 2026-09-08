# ADR-019: Declared Permissions and Grant Rules

**Status:** Accepted

**Date:** 2026-09-07

**Decision Makers:** JumpStart Core Team

**Amends:** [ADR-011](011-entity-authorization.md), [ADR-012](012-role-based-permission-management.md) §9

## Context

[ADR-012](012-role-based-permission-management.md) gave the framework a way to persist grants but no
notion of which grants are meaningful. `IRoleRepository.AddPermissionAsync(Guid roleId, string
permission)` accepts any string. Nothing checks that the permission exists, that it is the kind of
permission that can belong to a tenant, or that whoever is granting it holds it themselves.

That is tolerable while only a platform operator administers roles. It stops being tolerable the
moment an application lets a tenant manage its own - which is the ordinary SaaS requirement
[ADR-012](012-role-based-permission-management.md) §3 anticipated. A tenant administrator with role
management can grant a role any string, including whatever an application uses to mark its own
platform operators, and assign it to themselves. The framework offers no way to express "this
permission is not yours to give".

Two further problems in the same area:

**Naming is conflated with the model.** [ADR-011](011-entity-authorization.md) derives permissions as
`"{EntityName}.{Action}"` from a controller's generic argument, which works only for controllers that
have one. Any capability that is not shaped like an entity - a cross-cutting report, an
administrative capability, a permission spanning several tables - cannot use `[EntityAuthorize]` and
must fall back to a hand-written policy. Applications therefore run two mechanisms for one concept.
Observed in a consuming application: capability-shaped permissions outnumbered entity-shaped ones,
and every one of them bypassed the framework's own attribute.

**Registering a repository publishes an API.**
[ADR-012](012-role-based-permission-management.md) §9 gates repositories *and* controllers behind a
single `RegisterAuthorizationController` flag, mirroring `RegisterFormsController`. An application
that wants `IRoleRepository` in order to seed roles at startup has no way to obtain it without also
exposing `/api/roles` and `/api/userpermissions` as a live CRUD surface. Observed in a consuming
application, with a code comment explaining that the flag was enabled only for the repository.

## Decision

### 1. Permissions are declared, not spelled

```csharp
public enum PermissionScope { Tenant, Platform }

public record PermissionDescriptor(
    string Name,
    PermissionScope Scope,
    string Group,
    bool DelegableByTenantAdmin,
    string? Description = null);

public interface IPermissionRegistry
{
    IReadOnlyCollection<PermissionDescriptor> All { get; }
    bool TryGet(string name, out PermissionDescriptor descriptor);
}
```

An application registers its permissions at startup. The framework treats a name as opaque - it
never parses it, and imposes no convention. `"Product.Delete"` and `"Billing"` are the same kind of
object.

`Scope` distinguishes a permission that can be held within a tenant from one that only makes sense
platform-wide. `Group` exists so an administrative UI can lay permissions out without inferring
structure from the name. `DelegableByTenantAdmin` marks the permissions a tenant may put in its own
roles.

The entity convention from [ADR-011](011-entity-authorization.md) is retained as a *helper* for
declaring CRUD permissions in bulk. It is one way to name a permission, not the shape a permission
must have.

### 2. Four rules, enforced in the repository

Every grant - via a role or directly to a user - is refused unless:

1. the permission is declared in the registry;
2. its `Scope` matches where it is being granted (a `Platform` permission cannot be granted inside a
   tenant);
3. it is `DelegableByTenantAdmin`, when the grant is being made within a tenant;
4. **the grantor already holds it.**

Rule 4 does most of the work, and does it independently of how carefully an application curates its
registry: a grant can never increase the set of permissions in existence, only redistribute it.
Enforcement lives in the repository rather than a controller so that no route into the data - a
seeder, a background job, a future endpoint - can bypass it. Framework-internal seeding of the first
platform operator is the one sanctioned exception and is expressed explicitly, not by omission.

### 3. A policy seam for application-specific eligibility

```csharp
public interface IRoleManagementPolicy
{
    Task<bool> CanManageRolesAsync(Guid tenantId, CancellationToken ct);
    Task<IReadOnlyCollection<string>> GrantablePermissionsAsync(Guid tenantId, CancellationToken ct);
}
```

Whether a given tenant may define roles at all, and which of the delegable permissions it may use,
are product decisions - a subscription tier, a feature flag, a contract term. The framework asks;
it never learns why the answer is what it is. A default implementation permits everything delegable,
so applications with no such rule configure nothing.

### 4. Checking goes through an interface

```csharp
public interface IPermissionEvaluator
{
    Task<bool> HasAsync(string permission, CancellationToken ct);
}
```

The default implementation reads the `Permission` claims on the current principal, exactly as
[ADR-011](011-entity-authorization.md)'s handler does today. Introducing the interface changes no
behaviour; it exists so that a future model whose grants cannot fit in a token - per-resource
permissions being the obvious case - can be added behind it without revisiting every call site. It
is a seam, not a feature.

### 5. Repository registration is separated from HTTP exposure

`RegisterAuthorizationController` is split into `RegisterAuthorizationRepositories` (services only)
and `RegisterAuthorizationController` (which implies the former, and additionally publishes the
controllers). Registering a service never publishes an endpoint.

## Consequences

### Positive Consequences

- A tenant administrator cannot grant a permission that does not exist, does not belong in a tenant,
  is not marked delegable, or that they do not themselves hold. The escalation path
  [ADR-012](012-role-based-permission-management.md) left open is closed by construction.
- Capability-shaped and entity-shaped permissions become the same kind of object, so an application
  no longer needs two mechanisms and two ways to remember to guard an endpoint.
- An application can obtain `IRoleRepository` without exposing role administration over HTTP.
- The registry gives administrative UIs something to enumerate. Previously the set of permissions an
  application supported existed only as scattered string literals.

### Negative Consequences

- **Applications must declare their permissions.** An application upgrading to this version with no
  registry configured has no valid grants and role administration stops working until it declares
  one. This is a deliberate hard failure: a permissive default would leave exactly the applications
  that most need the guarantee without it.
- Rule 4 makes bootstrapping explicit and slightly awkward: the first platform operator's grant
  cannot come from a grantor who holds it. The framework provides a sanctioned seeding path; that
  path is, by design, the one place the rule does not apply, and it is worth reviewing accordingly.
- `IPermissionEvaluator` adds indirection that buys nothing today. It is accepted on the judgement
  that adding it later costs a rewrite of every call site.

### Neutral Consequences

- The `Permission` claim format and
  [ADR-011](011-entity-authorization.md)'s handler are unchanged. This decision governs which
  permissions may be *granted* and how they are *declared*, not how a claim is compared.
- `PermissionDescriptor.Group` and `Description` are presentation metadata the framework itself never
  reads. They live in the descriptor because the alternative - a parallel structure maintained
  alongside it - would drift.
- Whether a tenant-defined role may be named after a framework built-in is left to the application's
  reserved-name configuration, in keeping with
  [ADR-012](012-role-based-permission-management.md)'s position that role naming policy belongs to
  the consumer.

## Alternatives Considered

- **Validate grants in the controller rather than the repository**: rejected. It leaves every other
  path into the data unguarded, and the paths that matter most - seeders and background jobs - are
  exactly the ones that do not go through a controller.
- **A closed enum of permissions in the framework**: rejected outright. The framework cannot know an
  application's capabilities, and any list it shipped would be either useless or a straitjacket.
- **Infer scope from the name** (a `Platform.` prefix meaning platform scope): rejected. It imposes a
  naming convention on every consumer to encode something a field states plainly, and it silently
  mis-scopes any permission whose name does not follow the convention.
- **Keep `[EntityAuthorize]` as the only mechanism and require every permission to name an entity**:
  rejected. Consumers already worked around it rather than distorting their capabilities to fit, which
  is the outcome that produced two parallel mechanisms.
- **Drop rule 4 and rely on the registry's `DelegableByTenantAdmin` flag alone**: rejected. The flag
  is a static property of a permission; rule 4 is a property of the transaction, and it holds even
  when the registry is misconfigured.

## References

- [ADR-011: Entity-Level Authorization](011-entity-authorization.md) - the naming convention this
  demotes from model to helper
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - the grant
  storage this adds rules to, and §9 whose flag this splits
- [ADR-017: Tenant-Scoped Permission Resolution](017-tenant-scoped-permission-resolution.md) - which
  tenant's grants are resolved
- [ADR-018: Fail-Closed Tenant Isolation](018-fail-closed-tenant-isolation.md) - the row-level
  counterpart to this decision's grant-level rules
