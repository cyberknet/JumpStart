# Multi-Tenant Architecture with Multiple Tenant Membership

## Overview

This document describes the complete multi-tenant architecture for JumpStart, supporting users who belong to **multiple tenants** (organizations, companies, departments).

## Key Features

✅ **Multiple Tenant Membership** - Users can belong to multiple organizations  
✅ **Tenant Switching** - Users select which tenant context they're working in  
✅ **Automatic Data Filtering** - Repository automatically filters by selected tenant  
✅ **Security** - Validates user has access before switching tenants  
✅ **Blazor UI Components** - Ready-to-use tenant switcher component  
✅ **Flexible Authorization** - Per-tenant roles and permissions  

---

## Architecture Components

### 1. **Core Entities**

#### `Tenant` (`JumpStart/Data/Tenant.cs`)
- Represents an organization/company/customer
- Properties: `Name`, `Code`, `IsActive`, `ContactEmail`, `Settings`
- Inherits from `SimpleAuditableNamedEntity` (full audit tracking)
- Unique `Code` for subdomain/URL routing

#### `UserTenant` (`JumpStart/Data/UserTenant.cs`)
- Junction table for many-to-many User ↔ Tenant relationship
- Properties: `UserId`, `TenantId`, `Role`, `IsActive`, `Settings`
- Enables per-tenant roles (Admin in Tenant A, Viewer in Tenant B)
- Tenant-specific user settings

### 2. **Tenant Selection**

#### `ITenantSelectionService` (`JumpStart/Services/ITenantSelectionService.cs`)
- Interface for managing active tenant selection
- Methods:
  - `GetCurrentTenantIdAsync()` - Current selected tenant
  - `SetCurrentTenantAsync(tenantId)` - Switch tenant (with validation)
  - `GetAvailableTenantsAsync()` - List of user's tenants
  - `HasAccessToTenantAsync(tenantId)` - Check tenant access
- Event: `TenantChanged` - Notify UI when tenant switches

#### `BlazorTenantSelectionService` (`JumpStart/Services/BlazorTenantSelectionService.cs`)
- Blazor Server implementation
- Scoped lifetime (per SignalR circuit)
- Auto-selects first available tenant
- Validates access via `UserTenant` table

### 3. **Tenant Context**

#### `ITenantContext` (`JumpStart/Repositories/Advanced/ITenantContext.cs`)
- Provides current tenant ID for repository filtering
- **Recommended Implementation**: Wrap `ITenantSelectionService`

```csharp
public class SelectionBasedTenantContext(ITenantSelectionService tenantSelection) 
    : ITenantContext
{
    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        return tenantSelection.GetCurrentTenantIdAsync();
    }
}
```

### 4. **Tenant-Scoped Entities**

#### `ITenantScoped` Interface (`JumpStart/Data/Advanced/MultiTenant/ITenantScoped.cs`)
- Marks entities as belonging to a tenant
- Properties:
  - `Guid TenantId` - Foreign key to Tenant
  - `Tenant Tenant` - Navigation property

**Example:**
```csharp
public class Invoice : SimpleAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

### 5. **Blazor UI Component**

#### `TenantSwitcher.razor` (`JumpStart/Components/TenantSwitcher.razor`)
- Drop-down to switch between user's tenants
- Automatically hides if user has only one tenant
- Reloads page after switch (configurable)
- Event callback for custom handling

**Usage:**
```razor
@* In MainLayout.razor or NavMenu.razor *@
<TenantSwitcher />

@* Custom handling *@
<TenantSwitcher ReloadOnChange="false" 
                OnTenantChangedCallback="HandleTenantChanged" />
```

---

## Database Schema

### Tables Created

1. **Tenant**
   - `Id` (PK, Guid)
   - `Name` (varchar(200))
   - `Code` (varchar(50), unique)
   - `IsActive` (bit)
   - `ContactEmail` (varchar(255), nullable)
   - `Settings` (nvarchar(max), JSON)
   - Audit fields (CreatedOn, CreatedById, ModifiedOn, ModifiedById, DeletedOn, DeletedById)

2. **UserTenant**
   - `Id` (PK, Guid)
   - `UserId` (Guid, FK)
   - `TenantId` (Guid, FK to Tenant)
   - `Role` (varchar(50), nullable)
   - `IsActive` (bit)
   - `Settings` (nvarchar(max), JSON, nullable)
   - Audit fields
   - **Unique constraint**: (UserId, TenantId)

3. **Form** (Updated)
   - Added `TenantId` (Guid, FK to Tenant)
   - Added `Tenant` navigation property

### Indexes

- `IX_Tenant_Code` (unique) - Subdomain lookups
- `IX_Tenant_IsActive` - Filter active tenants
- `IX_UserTenant_UserId_TenantId` (unique) - Prevent duplicates
- `IX_UserTenant_UserId_IsActive` - Get user's tenants
- `IX_UserTenant_TenantId_UserId_IsActive` - Check access
- `IX_Form_TenantId` - Filter forms by tenant
- `IX_Form_TenantId_IsActive` - Common query pattern

---

## Repository Integration

### Automatic Tenant Filtering

When you update `Repository.cs` (next step), it will:

1. **On Query** - Automatically filter by current tenant
   ```csharp
   var forms = await repository.GetAllAsync(); // Only current tenant's forms
   ```

2. **On Create** - Automatically set TenantId
   ```csharp
   var form = new Form { Name = "Survey" };
   await repository.AddAsync(form); // TenantId set automatically
   ```

3. **On Update/Delete** - Validate tenant ownership
   ```csharp
   await repository.UpdateAsync(form); // Fails if form belongs to different tenant
   ```

---

## Usage Scenarios

### Scenario 1: Single Tenant Per User (Simplest)
- User belongs to one tenant only
- Tenant auto-selected on login
- No UI switcher needed

### Scenario 2: Multiple Tenants, Rare Switching
- User belongs to multiple tenants
- Tenant selected on login, rarely changes
- Switcher in user menu/profile

### Scenario 3: Multiple Tenants, Frequent Switching (Your Case)
- User belongs to multiple tenants
- Switches frequently during work
- Prominent switcher in main navigation
- Page reload after switch to refresh all data

### Scenario 4: Cross-Tenant Scenarios (Advanced)
- System administrators view all tenants
- Reports comparing across tenants
- User has "system admin" role with special access

---

## Registration (Program.cs - Blazor Server)

```csharp
// Register services
builder.Services.AddScoped<ITenantSelectionService, BlazorTenantSelectionService>();
builder.Services.AddScoped<ITenantContext, SelectionBasedTenantContext>();

builder.Services.AddJumpStart(options =>
{
    options.RegisterUserContext<BlazorUserContext>();
    options.RegisterTenantContext<SelectionBasedTenantContext>();
});

// Add DbContextFactory for tenant service
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

## Security Considerations

### ✅ **What's Protected**

1. **Tenant Switching** - Validates user belongs to tenant before switching
2. **Data Access** - Repository filters by current tenant automatically
3. **Cross-Tenant Queries** - Prevented by EF Core query filters
4. **API Endpoints** - Repository filtering applies to all CRUD operations

### ⚠️ **What You Need to Add**

1. **Authorization Policies** - Define who can manage tenants
2. **Role Enforcement** - Use `UserTenant.Role` for permissions
3. **API Authentication** - Ensure endpoints require authentication
4. **Tenant Admin** - Special UI for managing user-tenant relationships

---

## Common Queries

```csharp
// Get user's tenants
var userTenants = await context.UserTenants
    .Include(ut => ut.Tenant)
    .Where(ut => ut.UserId == userId && ut.IsActive)
    .ToListAsync();

// Check if user has access to tenant
var hasAccess = await context.UserTenants
    .AnyAsync(ut => ut.UserId == userId 
        && ut.TenantId == tenantId 
        && ut.IsActive);

// Get user's role in tenant
var role = await context.UserTenants
    .Where(ut => ut.UserId == userId && ut.TenantId == tenantId)
    .Select(ut => ut.Role)
    .FirstOrDefaultAsync();

// Add user to tenant
var userTenant = new UserTenant
{
    UserId = userId,
    TenantId = tenantId,
    Role = "Admin",
    IsActive = true
};
await context.UserTenants.AddAsync(userTenant);
await context.SaveChangesAsync();
```

---

## Next Steps

1. ✅ **Update `Repository.cs`** - Add tenant filtering and assignment logic
2. ✅ **Create Migration** - Add Tenant, UserTenant tables and Form.TenantId
3. ✅ **Update Controllers** - Consider authorization per tenant
4. ✅ **Create Tenant Management UI** - Admin interface for UserTenant relationships
5. ✅ **Add to NavMenu** - Include TenantSwitcher component
6. ✅ **Seed Data** - Create default tenant and user-tenant relationships
7. ✅ **Testing** - Test tenant switching and data isolation

---

## Files Created

- `JumpStart/Data/Tenant.cs` - Tenant entity
- `JumpStart/Data/UserTenant.cs` - User-Tenant junction
- `JumpStart/Data/Advanced/MultiTenant/ITenantScoped.cs` - Interface
- `JumpStart/Repositories/Advanced/ITenantContext.cs` - Tenant context interface
- `JumpStart/Services/ITenantSelectionService.cs` - Tenant selection interface
- `JumpStart/Services/BlazorTenantSelectionService.cs` - Blazor implementation
- `JumpStart/Components/TenantSwitcher.razor` - UI component
- `JumpStart/Data/Configuration/TenantConfiguration.cs` - EF configuration
- `JumpStart/Data/Configuration/UserTenantConfiguration.cs` - EF configuration
- `JumpStart/Data/Configuration/Forms/FormConfiguration.cs` - Form EF configuration
- Updated: `JumpStart/Data/JumpStartDbContext.cs` - Added DbSets
- Updated: `JumpStart/Forms/Form.cs` - Implements ITenantScoped

---

## Questions?

- **Q: Can a user be in no tenants?**  
  A: Yes, but they won't see any data. Consider creating a default "Personal" tenant on user registration.

- **Q: Can tenants share data?**  
  A: By default, no. For shared reference data, don't implement `ITenantScoped` on those entities.

- **Q: How do I handle system-wide entities?**  
  A: Don't implement `ITenantScoped`. Example: `QuestionType` is system-wide.

- **Q: What about background jobs?**  
  A: Background jobs may need to process all tenants. Return `null` from `ITenantContext` for system operations.

- **Q: Can I use subdomains for tenant resolution?**  
  A: Yes! Add subdomain parsing to your `ITenantContext` implementation.
