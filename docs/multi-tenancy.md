# Multi-Tenancy

Learn how JumpStart isolates each tenant's data automatically, and how to let users who belong to
more than one tenant switch between them.

## Overview

Multi-tenancy answers one critical question for every query, create, update, and delete:

- **Which tenant does this row belong to, and does the current request have access to it?**

JumpStart handles this **automatically** for any entity you mark as tenant-scoped - the same way
soft delete is automatic for any entity implementing `IDeletable`. See [ADR-010: Multi-Tenant Data
Isolation](architecture/adr/010-multi-tenant-data-isolation.md) for the full design rationale.

## Why Multi-Tenancy?

### SaaS Applications
Serve multiple customers/organizations from one deployment, with each customer's data invisible to
every other customer.

### Enterprise Multi-Division Apps
Isolate data between divisions or business units in a single application.

### Consultant/Partner Access
Support users who legitimately need access to more than one tenant (e.g. a consultant working
with several client organizations) via [tenant selection](#tenant-selection-for-multi-tenant-users).

## Quick Start

### 1. Mark Entities as Tenant-Scoped

```csharp
public class Invoice : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

The `TenantId` → `Tenant` foreign key is configured automatically in `OnModelCreating` - you don't
need a `[ForeignKey]` attribute or Fluent API call.

### 2. Implement ITenantContext

Tell JumpStart how to resolve the current tenant, the same way `IUserContext` tells it who the
current user is:

```csharp
public class JwtTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        var tenantClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("tenant_id")?.Value;

        return Task.FromResult(Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : (Guid?)null);
    }
}
```

### 3. Declare and Forward ITenantContext on Your DbContext

**This step is easy to miss:** registering `ITenantContext` in DI is not enough by itself. Your
`DbContext` must declare its own constructor parameter and forward it to the base class:

```csharp
public class ApplicationDbContext : JumpStartDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext? tenantContext = null)
        : base(options, tenantContext)
    {
    }
}
```

### 4. Register Services

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        jumpStart.RegisterTenantContext<JwtTenantContext>();
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });
```

### 5. That's It!

```csharp
var invoices = await invoiceRepository.GetAllAsync(); // Only the current tenant's invoices
var invoice = await invoiceRepository.AddAsync(new Invoice { InvoiceNumber = "INV-001", Amount = 199.99m });
// invoice.TenantId is now set to the current tenant automatically
```

## Tenant and UserTenant Entities

### Tenant

The top-level organizational unit - a customer, company, or organization:

```csharp
public class Tenant : AuditableNamedEntity
{
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? Settings { get; set; }
}
```

Extend it in your application for custom fields:

```csharp
public class MyTenant : Tenant
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
}
```

### UserTenant

A many-to-many junction recording which users belong to which tenants:

```csharp
public class UserTenant : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
```

`UserTenant` is what powers [tenant selection](#tenant-selection-for-multi-tenant-users) for users
who belong to more than one tenant - it is not required if every user belongs to exactly one
tenant via a JWT/claim. Role and permission assignment is a separate concern, handled by
`Role`/`UserRole`/`UserPermission` (see [ADR-012](architecture/adr/012-role-based-permission-management.md)),
not by this junction.

## How It Works

### Behind the Scenes

#### GetAllAsync / GetByIdAsync (Read)

```csharp
var invoices = await repository.GetAllAsync();
var invoice = await repository.GetByIdAsync(invoiceId, null);
```

A global EF Core query filter, applied once in `OnModelCreating` for every `ITenantScoped` entity,
excludes rows that don't belong to the current tenant - automatically, for every query, including
lookups by ID. An entity belonging to a different tenant is indistinguishable from one that
doesn't exist.

#### AddAsync (Create)

```csharp
await repository.AddAsync(invoice);
```

1. Repository checks if the entity implements `ITenantScoped`
2. Reads `CurrentTenantId` off the current `DbContext` (resolved once, at construction, from
   `ITenantContext`)
3. Sets `TenantId` automatically
4. Saves to database

#### UpdateAsync / DeleteAsync (Modify/Remove)

```csharp
await repository.UpdateAsync(invoice); // throws InvalidOperationException if it belongs to another tenant
await repository.DeleteAsync(invoiceId); // returns false if it belongs to another tenant
```

Both methods look up the existing entity first - since that lookup is subject to the same global
filter, an entity belonging to another tenant is simply "not found," with no separate validation
step required.

### Single-Tenant Mode

If `ITenantContext` is never registered (or returns `null`), `CurrentTenantId` is `null` and the
global filter becomes a no-op - every entity is visible regardless of `TenantId`. This is also the
correct behavior for system-wide operations (background jobs, admin tooling) that need to see
across all tenants: have that specific `ITenantContext` implementation return `null`.

### Optional Tenancy: ITenantScopedOptional

`ITenantScoped`'s `TenantId` is required - every row must belong to exactly one tenant. Some
entities need the opposite flexibility: a row that's either tenant-owned *or* global, decided per
row rather than fixed for the whole entity type (e.g. a tenant-specific "Billing Manager" role next
to a platform-wide "System Administrator" role). `ITenantScopedOptional` is a separate, weaker
interface for exactly this case:

```csharp
public interface ITenantScopedOptional
{
    Guid? TenantId { get; set; }
    Tenant? Tenant { get; set; }
}
```

- `TenantId == null` means the row is global - always visible, in addition to rows matching the
  current tenant.
- Unlike `ITenantScoped`, `Repository<TEntity>.AddAsync` does **not** auto-populate `TenantId` for
  these entities - the caller must set it explicitly (a real tenant ID, or `null` for global).
  Auto-defaulting to the current tenant would silently remove the "can be global" choice.
- `Role`, `UserRole`, and `UserPermission` (see
  [ADR-012: Role-Based Permission Management](architecture/adr/012-role-based-permission-management.md))
  are the framework's first real production entities using this interface.

## Tenant Context Implementations

### JWT Claim (Single Tenant Per User)

The most common case - shown in [Quick Start](#2-implement-itenantcontext) above.

### HTTP Header

```csharp
public class HeaderTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        var tenantHeader = _httpContextAccessor.HttpContext?
            .Request.Headers["X-Tenant-Id"].FirstOrDefault();

        return Task.FromResult(Guid.TryParse(tenantHeader, out var tenantId) ? tenantId : (Guid?)null);
    }
}
```

### Testing (Fixed Tenant)

```csharp
public class TestTenantContext : ITenantContext
{
    private readonly Guid? _tenantId;

    public TestTenantContext(Guid? tenantId) => _tenantId = tenantId;

    public Task<Guid?> GetCurrentTenantIdAsync() => Task.FromResult(_tenantId);
}
```

## Tenant Selection for Multi-Tenant Users

When users can belong to more than one tenant, `ITenantSelectionService` manages which tenant is
currently active, validating membership against `UserTenant`. `BlazorTenantSelectionService` is
the framework's Blazor Server implementation (scoped per circuit, auto-selects the first available
tenant):

```csharp
builder.Services.AddScoped<ITenantSelectionService, BlazorTenantSelectionService>();
```

Bridge it to `ITenantContext` so the repository layer picks up the selected tenant:

```csharp
public class SelectionBasedTenantContext : ITenantContext
{
    private readonly ITenantSelectionService _tenantSelection;

    public SelectionBasedTenantContext(ITenantSelectionService tenantSelection)
    {
        _tenantSelection = tenantSelection;
    }

    public Task<Guid?> GetCurrentTenantIdAsync() => _tenantSelection.GetCurrentTenantIdAsync();
}
```

Then in a Blazor component:

```razor
@inject ITenantSelectionService TenantSelection

<select @bind="selectedTenantId" @bind:after="OnTenantChanged">
    @foreach (var tenant in availableTenants)
    {
        <option value="@tenant.Id">@tenant.Name</option>
    }
</select>

@code {
    private Guid selectedTenantId;
    private List<Tenant> availableTenants = new();

    protected override async Task OnInitializedAsync()
    {
        availableTenants = await TenantSelection.GetAvailableTenantsAsync();
        var current = await TenantSelection.GetCurrentTenantAsync();
        selectedTenantId = current?.Id ?? Guid.Empty;
    }

    private async Task OnTenantChanged()
    {
        await TenantSelection.SetCurrentTenantAsync(selectedTenantId);
        NavigationManager.NavigateTo("/", forceLoad: true); // New DbContext picks up the new tenant
    }
}
```

> **Note:** the current tenant is resolved once per `DbContext` instance, at construction. Because
> of this, switching tenants requires a new `DbContext` - typically a page reload, as shown above -
> not just updating `ITenantSelectionService`'s in-memory state.

## Best Practices

### Do's ✅

- **Mark every tenant-owned entity** with `ITenantScoped` - forgetting one is the only way data
  leaks between tenants
- **Return `null` from `ITenantContext`** for system-wide/background operations that must see
  across all tenants, rather than working around the filter
- **Keep reference/lookup data un-scoped** (question types, countries, system config) - it should
  not implement `ITenantScoped`
- **Test cross-tenant access explicitly** - verify a user in Tenant A cannot reach Tenant B's data
  by ID

### Don'ts ❌

- **Don't set `TenantId` manually** in application code - let `AddAsync` populate it
- **Don't assume `.IgnoreQueryFilters()` bypasses only tenant isolation** - it removes every global
  filter on the entity, including soft delete
- **Don't forget to declare and forward `ITenantContext`** on your own `DbContext` constructor -
  registering it in DI alone does nothing

## Troubleshooting

### Entities From Other Tenants Are Visible

**Problem:** `GetAllAsync()` returns rows belonging to a different tenant.

**Solutions:**
1. Confirm the entity actually implements `ITenantScoped`
2. Confirm your `DbContext` declares and forwards `ITenantContext` to the base constructor (step 3
   of Quick Start) - this is the most commonly missed step
3. Confirm `ITenantContext` is registered: `jumpStart.RegisterTenantContext<YourTenantContext>()`
4. Verify `ITenantContext.GetCurrentTenantIdAsync()` actually returns a value for the current
   request, not `null`

### TenantId Is Empty After AddAsync

**Problem:** A newly created entity's `TenantId` is `Guid.Empty`.

**Solutions:**
1. Same checks as above - `TenantId` population depends on the same `CurrentTenantId` resolution
2. Confirm the user actually has a tenant selected/claimed at the time of the request

### Switching Tenants Doesn't Change Visible Data

**Problem:** Calling `ITenantSelectionService.SetCurrentTenantAsync` doesn't change what the user
sees.

**Solutions:**
1. Force a new `DbContext` instance - reload the page, or start a new scope - since
   `CurrentTenantId` is resolved once per instance, not re-read on every query

## Next Steps

- **[ADR-010: Multi-Tenant Data Isolation](architecture/adr/010-multi-tenant-data-isolation.md)** - Full design rationale
- **[ADR-012: Role-Based Permission Management](architecture/adr/012-role-based-permission-management.md)** -
  `ITenantScopedOptional` and the roles/permissions system built on it
- **[Entity Authorization](entity-authorization.md)** - Another automatic, always-on concern to be
  aware of alongside multi-tenancy
- **[Audit Tracking](audit-tracking.md)** - The soft-delete global query filter this design mirrors
- **[Core Concepts](core-concepts.md)** - Understand entity and repository patterns

---

**Questions?** See [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
