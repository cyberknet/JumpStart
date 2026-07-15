# ADR-009: Guid-Only Entities (Removal of Custom Key Type Support)

**Status:** Accepted

**Date:** 2026-01-25

**Decision Makers:** JumpStart Core Team

**Supersedes:** [ADR-002: Simple vs Advanced Entities](002-simple-advanced-entities.md)

## Context

ADR-002 established a "unified but extensible" entity system: `Entity`, `AuditableEntity`, and
`NamedEntity` defaulted to `Guid` keys, while an underlying `Entity<TKey>` / `IEntity<TKey>`
generic layer (and a parallel `Simple`/`Advanced` naming split for user context, repositories,
and API clients) remained available for custom key types (`int`, `long`, etc.).

In practice this dual system produced parallel interfaces and base classes across nearly every
part of the framework (`ISimpleUserContext` vs `IUserContext<TKey>`, `SimpleApiController` vs
`AdvancedApiController`, `SimpleApiClient` vs `AdvancedApiClient`, and so on). This:

- Doubled the surface area consumers had to learn and doubled the code JumpStart had to maintain.
- Made XML documentation harder to keep consistent (see cleanup commits `3330caa`, `0e1ea7b`).
- Was rarely used in practice — new applications built on JumpStart consistently chose Guid keys.

## Decision

JumpStart removed the generic key-type system entirely. `Entity`, `AuditableEntity`,
`NamedEntity`, and `AuditableNamedEntity` are Guid-keyed only, with no `TKey` generic parameter
anywhere in the entity, repository, controller, or API client base classes. JumpStart is now
explicitly opinionated: **all entities use `Guid` identifiers.**

```csharp
public class Product : AuditableNamedEntity
{
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, IUserContext? userContext = null)
        : base(context, userContext) { }
}
```

Applications that need non-Guid primary keys (e.g. integrating with a legacy table using an
`int` identity column) are expected to model that as a separate concern outside JumpStart's
entity system, rather than JumpStart supporting it as a first-class option.

## Consequences

### Positive Consequences

- Single set of base classes and interfaces to learn, document, and test.
- No more parallel `Simple`/`Advanced` naming across entities, repositories, controllers, and
  API clients.
- Smaller surface area for XML documentation and future maintenance.
- Removes an entire class of "which one do I inherit from" decisions for new consumers.

### Negative Consequences

- Breaking change for any code written against the ADR-002 generic/Advanced system (there were
  no external consumers at the time of this decision, so migration cost was limited to the
  in-repo demo apps).
- Applications requiring non-Guid keys must work around JumpStart's entity system rather than
  extend it directly.

### Neutral Consequences

- Docs and samples referencing `Entity<TKey>`, `AuditableEntity<int, Guid>`, `ISimpleUserContext`,
  `SimpleApiController`/`AdvancedApiController`, etc. are stale and need to be updated to the
  Guid-only examples shown above.

## Alternatives Considered

Same three alternatives evaluated in ADR-002 (generic-only, globally-configured key type,
parallel Simple/Advanced systems) were re-considered here. The parallel-systems approach
(ADR-002's actual decision) was rejected retroactively for the reasons in Context above; a
fully generic-only approach was rejected again as too verbose for the common case, which
remains Guid keys.

## References

- Commit `6c2da55` — "Remove the Simple/Advanced entity id options. JumpStart is opinionated
  and requires Guid Ids on entities."
- [ADR-002: Simple vs Advanced Entities](002-simple-advanced-entities.md) (superseded by this ADR)
- [ADR-001: Repository Pattern](001-repository-pattern.md)
