# ADR-018: Fail-Closed Tenant Isolation

**Status:** Accepted

**Date:** 2026-09-07

**Decision Makers:** JumpStart Core Team

**Amends:** [ADR-010](010-multi-tenant-data-isolation.md)

## Context

[ADR-010](010-multi-tenant-data-isolation.md) established the global query filter that keeps one
tenant's rows away from another. Both variants begin the same way:

```csharp
// ITenantScoped
e => CurrentTenantId == null || e.TenantId == CurrentTenantId;

// ITenantScopedOptional
e => CurrentTenantId == null || e.TenantId == null || e.TenantId == CurrentTenantId;
```

The leading clause exists for a real reason: single-tenant applications, background jobs, seeders and
migrations all run with no tenant, and they must be able to see everything. Making the absence of a
tenant mean "no restriction" was the simplest way to let those work.

The cost is that the isolation guarantee is conditional on a claim being present, and it fails in the
permissive direction. Any request that reaches a tenant-scoped entity without a `tenant_id` claim
reads across every tenant, silently and successfully. Nothing distinguishes "there is legitimately no
tenant here" from "the tenant should have been established and was not" - a bug in tenant resolution,
a token minted on a path that forgot to include the claim, an endpoint reached with a bootstrap
token - and the failure produces plausible data rather than an error.

The framework itself mints tenant-less tokens (see [ADR-017](017-tenant-scoped-permission-resolution.md)),
so this is not a hypothetical path.

Since [ADR-010](010-multi-tenant-data-isolation.md) was accepted, the framework gained
`AcrossAllTenants()` - an explicit, greppable opt-out that drops only the tenant filter and keeps the
soft-delete one. Every genuinely cross-tenant read in a well-written consumer already announces
itself that way. That changes the calculus: the permissive default is no longer carrying the weight
it did when it was the only way to perform a system-wide operation.

## Decision

### 1. A null tenant matches nothing

```csharp
// ITenantScoped
e => e.TenantId == CurrentTenantId;

// ITenantScopedOptional
e => e.TenantId == null || e.TenantId == CurrentTenantId;
```

With no tenant established, a scoped entity yields no rows. `ITenantScopedOptional` continues to
return global (`TenantId == null`) rows, because those genuinely belong to no tenant and a
platform-level role must remain visible without one.

### 2. System operations opt in, exactly as cross-tenant reads already do

`AcrossAllTenants()` is unchanged and becomes the sanctioned way to run without a tenant: a seeder,
a background sweep, or an administrative report says so at the call site. The mechanism already
exists, is already documented, and is already greppable for audit - this decision only makes it
mandatory rather than merely available.

For a genuinely single-tenant application, `JumpStartOptions.SingleTenantMode` restores the previous
behaviour globally. It is an explicit statement that the application has no tenant boundary, made
once at startup, rather than an accident that looks identical to correct operation.

### 3. Absent tenancy is refused before it reaches the data

An endpoint that operates on tenant-scoped entities and receives no tenant should not be relying on
the query filter to return nothing - an empty list and a refusal are different answers, and only one
of them is diagnosable. `RequireTenantAttribute` short-circuits such a request with 403 and a message
naming the cause, so the failure is loud at the boundary rather than silent in the results.

## Consequences

### Positive Consequences

- Tenant isolation stops being conditional on a claim's presence. The guarantee becomes structural:
  no tenant, no rows.
- Every remaining cross-tenant read is visible in a grep for `AcrossAllTenants()`, which makes the
  full set auditable - previously, an omission was indistinguishable from an intentional system
  operation.
- A tenant-resolution bug now presents as an empty result or an explicit refusal instead of another
  tenant's data.

### Negative Consequences

- **This is the highest-risk change in the framework's history.** Every code path that runs without a
  tenant and expects rows will silently start returning none, and "returns nothing" is a quiet
  failure mode in its own right. Consumers must audit their startup seeders, background services,
  migrations and administrative screens before adopting.
- Applications that never established a tenant context at all - relying on the permissive default as
  a de facto single-tenant mode - will appear to lose all their data until they set
  `SingleTenantMode`. The flag exists precisely so the fix is one line, but the failure will be
  alarming.
- The two filter variants now differ from each other in a second way (one admits global rows, one
  does not), which is one more thing anyone extending the framework must hold in their head.

### Neutral Consequences

- No schema change and no data migration. This is a change to how existing rows are read.
- `IgnoringDeleted()`/`IncludingDeleted()` and the soft-delete filter are untouched; the two filters
  remain independently addressable by name, per [ADR-010](010-multi-tenant-data-isolation.md).

## Alternatives Considered

- **Leave the filter permissive and require every endpoint to assert a tenant**: rejected. It relies
  on every author of every future endpoint remembering, which is the property that produced the
  problem. The attribute in §3 is worth having as a diagnostic, but not as the guarantee.
- **Throw when `CurrentTenantId` is null on a scoped query**: rejected. It cannot distinguish a
  legitimate system operation from a mistake any better than the current design can, and it would
  make `AcrossAllTenants()` the only way to run a seeder - turning a safety mechanism into
  boilerplate that authors would learn to apply reflexively, which erodes exactly the audit value
  §2 depends on.
- **Make the fail-closed behaviour opt-in per application**: rejected. A safety property that is off
  by default protects only the applications that already knew to ask, which are the ones least
  likely to need protecting. `SingleTenantMode` inverts this correctly: the application that
  genuinely has no boundary declares it.

## References

- [ADR-010: Multi-Tenant Data Isolation](010-multi-tenant-data-isolation.md) - the filter this
  amends
- [ADR-012: Role-Based Permission Management](012-role-based-permission-management.md) - introduced
  `ITenantScopedOptional` and its variant of the filter
- [ADR-017: Tenant-Scoped Permission Resolution](017-tenant-scoped-permission-resolution.md) - the
  companion decision; together they close the case where a missing tenant meant both "every row" and
  "every permission"
