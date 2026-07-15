# ADR-010: Multi-Tenant Data Isolation

**Status:** Accepted

**Date:** 2026-07-15

**Decision Makers:** JumpStart Core Team

## Context

Multi-tenant SaaS applications need to isolate each tenant's data from every other tenant's data,
without every repository method having to remember to add a `WHERE TenantId = ...` clause by hand.
JumpStart already solves an analogous problem - hiding soft-deleted rows - via a global EF Core
query filter configured once in `JumpStartDbContext.OnModelCreating`; multi-tenancy follows the
same shape: mark entities that belong to a tenant, and let the framework handle isolation, tenant
assignment on create, and rejection of cross-tenant access on update/delete, automatically.

Not every entity is tenant-specific - reference data (QuestionTypes, lookup tables), system
configuration, and `Tenant`/`UserTenant` themselves are shared across (or *are*) tenants and must
not be filtered.

Some applications also need users to belong to multiple tenants and switch between them (e.g.
consultants, partners with multiple customer accounts), rather than a single fixed tenant per user
via a JWT/claim.

## Decision

### 1. Tenant and UserTenant Entities

`JumpStart.Data.Tenant` (extends `AuditableNamedEntity`) represents an organization, customer, or
company - the top-level isolation boundary. `JumpStart.Data.UserTenant` is a many-to-many junction
between users and tenants, carrying a per-tenant `Role` and `IsActive` flag, so a single user can
belong to multiple tenants with different roles in each.

```csharp
public class Tenant : AuditableNamedEntity
{
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? Settings { get; set; }
}

public class UserTenant : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
}
```

Consumers can subclass `Tenant` to add application-specific fields (e.g. `Code`, `Subdomain`).

### 2. ITenantScoped Marker Interface

Any entity that belongs to a single tenant implements `ITenantScoped`:

```csharp
public interface ITenantScoped
{
    Guid TenantId { get; set; }
    Tenant Tenant { get; set; }
}

public class Invoice : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

`JumpStartDbContext.OnModelCreating` automatically configures the `TenantId` → `Tenant` foreign
key for every `ITenantScoped` entity - no `[ForeignKey]` attribute or manual Fluent API needed.

Reference/lookup data, system configuration, and `Tenant`/`UserTenant` themselves do not implement
`ITenantScoped`.

### 3. Automatic Isolation via Global Query Filter

`JumpStartDbContext` resolves the current tenant once per instance (scoped per request) from
`ITenantContext`, and applies a global EF Core query filter to every `ITenantScoped` entity - the
same mechanism `IDeletable` uses for soft delete:

```csharp
public Guid? CurrentTenantId { get; }

// Applied once in OnModelCreating for every ITenantScoped entity:
// WHERE CurrentTenantId IS NULL OR TenantId == CurrentTenantId
```

`CurrentTenantId == null` disables filtering entirely - single-tenant applications, and
system/background jobs that must operate across all tenants, simply don't provide a tenant
context.

Because the filter is applied at the `DbContext` level, it covers every query path automatically,
including `Repository<TEntity>.GetAllAsync`, `GetByIdAsync`, and the `FindAsync` lookups inside
`UpdateAsync`/`DeleteAsync` - an entity belonging to a different tenant is indistinguishable from
one that doesn't exist, which is exactly the isolation guarantee this is meant to provide.

### 4. Automatic Tenant Assignment on Create

`Repository<TEntity>.AddAsync` populates `TenantId` for any `ITenantScoped` entity from the current
tenant, the same way it already populates `CreatedById` for `ICreatable` entities:

```csharp
if (entity is ITenantScoped tenantScoped && CurrentTenantId.HasValue)
{
    tenantScoped.TenantId = CurrentTenantId.Value;
}
```

Application code never sets `TenantId` manually.

### 5. ITenantContext Abstraction

`ITenantContext` supplies the current tenant ID, mirroring how `IUserContext` supplies the current
user ID for audit tracking:

```csharp
public interface ITenantContext
{
    Task<Guid?> GetCurrentTenantIdAsync();
}
```

Consumers implement this against whatever their tenant-resolution strategy is - a JWT claim, an
HTTP header, a subdomain lookup, or (for multi-tenant-membership scenarios) `ITenantSelectionService`
- and register it:

```csharp
builder.Services.AddJumpStart(options =>
{
    options.RegisterTenantContext<JwtTenantContext>();
});
```

Returning `null` means no tenant context is established (single-tenant mode, or a system-wide
operation), and the global filter is skipped.

### 6. Tenant Selection for Multi-Tenant Users

For users who belong to more than one tenant, `ITenantSelectionService` manages which tenant is
currently active - validating membership against `UserTenant`, exposing the list of tenants the
user can switch between, and raising a `TenantChanged` event so UI can react:

```csharp
public interface ITenantSelectionService
{
    event Action<Guid?>? TenantChanged;
    Task<Guid?> GetCurrentTenantIdAsync();
    Task<Tenant?> GetCurrentTenantAsync();
    Task<bool> SetCurrentTenantAsync(Guid tenantId);
    Task ClearCurrentTenantAsync();
    Task<List<Tenant>> GetAvailableTenantsAsync();
    Task<bool> HasAccessToTenantAsync(Guid tenantId);
}
```

`BlazorTenantSelectionService` is the framework's Blazor Server implementation, scoped per
circuit. An `ITenantContext` implementation for this scenario simply delegates to
`ITenantSelectionService.GetCurrentTenantIdAsync()`.

## Consequences

### Positive Consequences

- Tenant isolation is automatic and cannot be forgotten - a repository method that queries,
  updates, or deletes never needs to remember to filter by tenant, the same guarantee already
  relied on for soft delete.
- A single global filter mechanism covers both soft delete and multi-tenancy, keeping the
  framework's data-access story consistent rather than introducing a second, differently-shaped
  isolation mechanism.
- Single-tenant applications pay no cost - `ITenantContext` returning null disables filtering
  entirely, and entities that don't implement `ITenantScoped` are unaffected.
- Supports both the common case (one tenant per user, via JWT/claim) and the more complex case
  (users belonging to multiple tenants) through the same `ITenantContext` contract.

### Negative Consequences

- `.IgnoreQueryFilters()` removes every global filter on an entity, not a specific one. An entity
  that is both `IDeletable` and `ITenantScoped` requires care in any code that needs to bypass just
  one of the two (e.g. an admin "view deleted records" feature must re-apply an explicit
  `TenantId` check if it bypasses filters to reach soft-deleted rows).
- Resolving the current tenant happens once per `DbContext` instance at construction; a tenant
  change mid-request (e.g. via `ITenantSelectionService.SetCurrentTenantAsync`) requires a new
  `DbContext` (a page reload or new scope) to take effect, not the same context refetching.
- Cross-tenant access is reported as "not found" rather than "forbidden" (indistinguishable from
  the soft-delete case) - this is the correct behavior for data isolation, but means application
  code can't distinguish "doesn't exist" from "exists in another tenant" without a separate check.

### Neutral Consequences

- `ITenantScoped` is a bare marker interface (just `TenantId`/`Tenant`), not tied to any particular
  base entity class, so it can be added to any entity alongside `IAuditable` or on its own.
- Reference data, lookup tables, and system configuration intentionally do not implement
  `ITenantScoped` and remain shared across all tenants.

## Alternatives Considered

- **Manual per-repository filtering** (each tenant-scoped repository writes its own
  `Where(e => e.TenantId == currentTenantId)`): rejected as exactly the kind of easy-to-forget,
  inconsistently-applied approach the global filter avoids - a single missed filter is a data leak.
- **Separate databases/schemas per tenant**: provides stronger physical isolation but is
  significantly more operationally complex (migrations, connection routing, cross-tenant
  reporting) and unnecessary for the row-level isolation JumpStart targets; applications with that
  requirement can still implement it independently of this feature.
- **Tenant ID baked into JWT only (no `ITenantSelectionService`)**: simpler, but does not support
  users belonging to multiple tenants, a common enough scenario (consultants, partners) that the
  framework provides `ITenantSelectionService` as a first-class option rather than requiring every
  multi-tenant-membership consumer to build it themselves.

## References

- [ADR-003: Audit Tracking Implementation](003-audit-tracking.md) - the soft-delete global query
  filter this design mirrors
- [ADR-001: Repository Pattern](001-repository-pattern.md)
