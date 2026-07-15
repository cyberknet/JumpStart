# ADR-011: Entity-Level Authorization

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

`ApiControllerBase<TEntity, ...>` provides full CRUD for any entity with almost no code written by
the consumer. Without some enforcement mechanism, every entity added to an application would need
its own hand-written authorization policy - and it would be easy to add a new controller, forget
the policy, and ship an unprotected endpoint. JumpStart needs access control that is consistent
across every entity automatically, fine-grained enough to distinguish reading from writing, and
composable into roles without hand-registering a policy per entity/action combination.

## Decision

### 1. The Permission Claim Format

Access is controlled by a `Permission` claim whose value follows `"{EntityName}.{Action}"` -
for example `"Product.Get"`, `"Order.List"`, `"Invoice.Delete"`. A user is authorized for a given
action on a given entity type if and only if they hold the matching claim.

### 2. EntityAuthorizeAttribute

Every action on `ApiControllerBase<TEntity, ...>` is decorated with `[EntityAuthorize(action: "...")]`:

| Method (base class) | Action   |
|----------------------|----------|
| `GetById`            | `Get`    |
| `GetAll`             | `List`   |
| `Create`             | `Create` |
| `Update`             | `Update` |
| `Delete`             | `Delete` |

`EntityAuthorizeAttribute` implements `IAuthorizeData` - the same interface `[Authorize]` itself
implements - so it participates in the standard ASP.NET Core authorization pipeline rather than
requiring a separate one. It also enforces authentication: an anonymous request is rejected before
the permission check ever runs.

```csharp
public class EntityAuthorizeAttribute : Attribute, IAuthorizeData
{
    public string Action { get; }
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }

    public EntityAuthorizeAttribute(string action)
    {
        Action = action;
        Policy = EntityPolicyProvider.PolicyName; // "EntityPolicy"
    }
}
```

Consumers can apply the same attribute to their own custom controller actions to get the same
enforcement:

```csharp
[HttpGet("featured")]
[EntityAuthorize(action: "List")] // reuses the "Product.List" permission
public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeatured() { ... }
```

### 3. Reflection-Based Entity Resolution

`EntityPermissionHandler` resolves `TEntity` by walking up the controller's inheritance chain to
find its generic base type (`ApiControllerBase<TEntity, ...>`), so the entity name in the
`"{EntityName}.{Action}"` policy string is derived automatically from the controller's own generic
argument - it never needs to be configured separately:

```csharp
var requiredPolicy = $"{entityType.Name}.{attr.Action}";
if (context.User.HasClaim("Permission", requiredPolicy))
{
    context.Succeed(requirement);
}
```

### 4. Global Registration via AddJumpStart

`EntityPolicyProvider`, `EntityPermissionHandler`, and the `"EntityPolicy"` policy (requiring
`EntityPermissionRequirement`) are registered unconditionally inside `AddJumpStart` - there is no
`JumpStartOptions` flag to disable this. Any application that calls `AddJumpStart()` gets entity
authorization wired in for every `ApiControllerBase`-derived controller.

## Consequences

### Positive Consequences

- No entity can ship with an unprotected CRUD endpoint by accident - protection comes from the base
  class, not from a policy each controller author must remember to add.
- Fine-grained, per-action permissions (`Get`/`List`/`Create`/`Update`/`Delete`) compose freely into
  roles without hand-registering a policy per entity.
- Reuses the standard ASP.NET Core `IAuthorizeData`/`IAuthorizationHandler` pipeline, so it
  interoperates with existing authentication middleware, `[Authorize]` conventions, and tooling
  instead of introducing a parallel enforcement mechanism.
- Custom controller actions opt into the exact same enforcement with one attribute.

### Negative Consequences

- This is mandatory, with no opt-out today. A consuming application must design and issue
  `Permission` claims before *any* JumpStart-generated endpoint will return anything but `401`/`403`
  - there is no "just get something running" path that skips it.
- `IJwtTokenService.GenerateToken`'s `additionalClaims` parameter is a flat
  `Dictionary<string, string>` (one value per key), so it cannot add multiple `Permission` claims by
  itself. Applications needing more than one permission per user must build the `ClaimsIdentity`
  directly rather than using the built-in token service as-is.
- Cross-tenant-style "not found" ambiguity does not apply here - a missing permission is reported as
  `403`, distinct from a genuinely missing entity (`404`), so this does not hide data existence the
  way the multi-tenancy filter does (see [ADR-010](010-multi-tenant-data-isolation.md)).

### Neutral Consequences

- All entities share one claim type (`Permission`); the entity name segment of the claim value is
  the CLR class name of `TEntity`, so renaming an entity class changes the permission strings every
  consumer must issue.
- The reflection-based entity-type lookup depends on the controller directly (or indirectly)
  deriving from `ApiControllerBase<TEntity, ...>`; controllers that don't derive from it are
  unaffected by this system entirely and need their own authorization if desired.

## Alternatives Considered

- **Role-based `[Authorize(Roles = "...")]` per controller**: rejected - roles don't compose per
  entity/action without proliferating role names (`"CanReadProducts"`, `"CanDeleteProducts"`, ...),
  and nothing would enforce that a new controller actually gets a `[Authorize]` attribute at all.
- **Hand-registered `[Authorize(Policy = "...")]` per entity**: rejected as pure boilerplate - every
  new entity would require registering a matching named policy before its controller could be used,
  reintroducing exactly the "easy to forget" problem this design avoids.
- **Making entity authorization configurable/optional**: considered, but not implemented - see
  Negative Consequences. Could be revisited if the mandatory-by-default behavior proves too rigid
  for applications that want authentication without per-entity permissions.

## References

- [ADR-010: Multi-Tenant Data Isolation](010-multi-tenant-data-isolation.md) - another automatic,
  always-on data-access concern with a similar "resolved once, applied globally" shape
- [ADR-001: Repository Pattern](001-repository-pattern.md)
