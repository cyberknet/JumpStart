# Custom Repository Filtering Guide

## Overview

JumpStart's Repository pattern provides **three levels of automatic filtering** that work together:

1. **Soft Delete Filtering** - Automatic (built into framework)
2. **Tenant Filtering** - Automatic when using multi-tenant (built into framework)
3. **Custom Filtering** - **Your extension point** for application-specific rules

This guide shows you how to implement custom filtering for scenarios like:
- Hierarchical organizational access
- Affiliate/partner chains
- Department-based access
- Regional restrictions
- User-owned data only
- Custom business rules

---

## How It Works

### Filter Chain Order

```csharp
IQueryable<TEntity> query = _dbSet;

// 1. Soft delete filter (framework)
query = ApplySoftDeleteFilter(query);

// 2. Tenant filter (framework, if applicable)
query = ApplyTenantFilter(query);

// 3. Custom filters (YOUR CODE HERE)
query = ApplyCustomFilters(query);

return query.ToList();
```

### The Extension Point

Override the `ApplyCustomFilters` method in your repository:

```csharp
protected virtual IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
{
    // Base implementation does nothing
    // Override this in your repository!
    return query;
}
```

---

## Common Patterns

### Pattern 1: Hierarchical Organizational Access

**Scenario:** Users can only see data for their organization and child organizations.

```csharp
// 1. Define marker interface for hierarchical entities
public interface IHierarchical
{
    Guid OrganizationId { get; set; }
}

// 2. Apply to your entities
public class Invoice : SimpleAuditableEntity, ITenantScoped, IHierarchical
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// 3. Create hierarchy service
public interface IHierarchyService
{
    List<Guid> GetAccessibleOrganizationIds();
}

public class HierarchyService : IHierarchyService
{
    private readonly IUserContext _userContext;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    
    public List<Guid> GetAccessibleOrganizationIds()
    {
        var userId = _userContext.GetCurrentUserIdAsync().GetAwaiter().GetResult();
        if (!userId.HasValue) return new();
        
        using var context = _contextFactory.CreateDbContext();
        
        // Get user's organization
        var userOrg = context.UserOrganizations
            .Where(uo => uo.UserId == userId.Value)
            .Select(uo => uo.OrganizationId)
            .FirstOrDefault();
        
        // Get all child organizations recursively
        var accessible = new List<Guid> { userOrg };
        accessible.AddRange(GetChildOrganizations(context, userOrg));
        
        return accessible;
    }
    
    private List<Guid> GetChildOrganizations(DbContext context, Guid parentId)
    {
        var children = context.Set<Organization>()
            .Where(o => o.ParentId == parentId)
            .Select(o => o.Id)
            .ToList();
        
        var allChildren = new List<Guid>(children);
        foreach (var childId in children)
        {
            allChildren.AddRange(GetChildOrganizations(context, childId));
        }
        
        return allChildren;
    }
}

// 4. Create custom repository
public class HierarchicalRepository<TEntity> : SimpleRepository<TEntity>
    where TEntity : class, ISimpleEntity
{
    private readonly IHierarchyService _hierarchyService;
    
    public HierarchicalRepository(
        DbContext context,
        ISimpleUserContext userContext,
        ISimpleTenantContext tenantContext,
        IHierarchyService hierarchyService)
        : base(context, userContext, tenantContext)
    {
        _hierarchyService = hierarchyService;
    }
    
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        // Only apply to hierarchical entities
        if (typeof(IHierarchical).IsAssignableFrom(typeof(TEntity)))
        {
            var accessibleOrgIds = _hierarchyService.GetAccessibleOrganizationIds();
            query = query.Where(e => accessibleOrgIds.Contains(
                EF.Property<Guid>(e, nameof(IHierarchical.OrganizationId))));
        }
        
        return query;
    }
}

// 5. Register
services.AddScoped<IHierarchyService, HierarchyService>();
services.AddScoped(typeof(ISimpleRepository<>), typeof(HierarchicalRepository<>));
```

---

### Pattern 2: Affiliate/Partner Chain

**Scenario:** Users can only see data for affiliates in their downline.

```csharp
// 1. Define marker interface
public interface IAffiliateScoped
{
    Guid AffiliateId { get; set; }
}

// 2. Apply to entities
public class Sale : SimpleAuditableEntity, ITenantScoped, IAffiliateScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public Guid AffiliateId { get; set; }
    public Affiliate Affiliate { get; set; } = null!;
    
    public decimal Amount { get; set; }
}

// 3. Create affiliate service
public interface IAffiliateService
{
    Guid GetCurrentAffiliateId();
    List<Guid> GetDownlineAffiliates(Guid affiliateId);
}

// 4. Create custom repository
public class AffiliateRepository<TEntity> : SimpleRepository<TEntity>
    where TEntity : class, ISimpleEntity
{
    private readonly IAffiliateService _affiliateService;
    
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        if (typeof(IAffiliateScoped).IsAssignableFrom(typeof(TEntity)))
        {
            var currentAffiliateId = _affiliateService.GetCurrentAffiliateId();
            var affiliateChain = _affiliateService.GetDownlineAffiliates(currentAffiliateId);
            
            query = query.Where(e => affiliateChain.Contains(
                EF.Property<Guid>(e, nameof(IAffiliateScoped.AffiliateId))));
        }
        
        return query;
    }
}
```

---

### Pattern 3: Department-Based Access

**Scenario:** Users can only see data for their department and child departments.

```csharp
// 1. Define marker interface
public interface IDepartmentScoped
{
    int DepartmentId { get; set; }
}

// 2. Create department service
public interface IDepartmentService
{
    int GetUserDepartmentId();
    List<int> GetAccessibleDepartments(int departmentId);
}

// 3. Create custom repository
public class DepartmentRepository<TEntity> : Repository<TEntity, int>
    where TEntity : class, IEntity<int>
{
    private readonly IDepartmentService _departmentService;
    
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        if (typeof(IDepartmentScoped).IsAssignableFrom(typeof(TEntity)))
        {
            var userDepartmentId = _departmentService.GetUserDepartmentId();
            var accessibleDepts = _departmentService.GetAccessibleDepartments(userDepartmentId);
            
            query = query.Where(e => accessibleDepts.Contains(
                EF.Property<int>(e, nameof(IDepartmentScoped.DepartmentId))));
        }
        
        return query;
    }
}
```

---

### Pattern 4: User-Owned Data Only

**Scenario:** Users can only see records they created.

```csharp
public class UserOwnedRepository<TEntity> : SimpleRepository<TEntity>
    where TEntity : class, ISimpleEntity
{
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        // Only show user's own records
        if (_userContext != null && typeof(ISimpleCreatable).IsAssignableFrom(typeof(TEntity)))
        {
            var userId = _userContext.GetCurrentUserIdAsync().GetAwaiter().GetResult();
            if (userId.HasValue)
            {
                query = query.Where(e => 
                    EF.Property<Guid?>(e, nameof(ISimpleCreatable.CreatedById)) == userId.Value);
            }
        }
        
        return query;
    }
}
```

---

### Pattern 5: Regional Restrictions

**Scenario:** Users can only see data for specific regions.

```csharp
// 1. Define marker interface
public interface IRegionalScoped
{
    string Region { get; set; }
}

// 2. Create region service
public interface IRegionService
{
    List<string> GetAccessibleRegions();
}

// 3. Create custom repository
public class RegionalRepository<TEntity> : SimpleRepository<TEntity>
    where TEntity : class, ISimpleEntity
{
    private readonly IRegionService _regionService;
    
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        if (typeof(IRegionalScoped).IsAssignableFrom(typeof(TEntity)))
        {
            var accessibleRegions = _regionService.GetAccessibleRegions();
            
            query = query.Where(e => accessibleRegions.Contains(
                EF.Property<string>(e, nameof(IRegionalScoped.Region))));
        }
        
        return query;
    }
}
```

---

### Pattern 6: Combining Multiple Filters

**Scenario:** Apply multiple custom filters based on entity characteristics.

```csharp
public class MultiFilterRepository<TEntity> : SimpleRepository<TEntity>
    where TEntity : class, ISimpleEntity
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IAffiliateService _affiliateService;
    private readonly IRegionService _regionService;
    
    protected override IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
    {
        // Hierarchical filtering
        if (typeof(IHierarchical).IsAssignableFrom(typeof(TEntity)))
        {
            var accessibleOrgIds = _hierarchyService.GetAccessibleOrganizationIds();
            query = query.Where(e => accessibleOrgIds.Contains(
                EF.Property<Guid>(e, "OrganizationId")));
        }
        
        // Affiliate filtering
        if (typeof(IAffiliateScoped).IsAssignableFrom(typeof(TEntity)))
        {
            var currentAffiliateId = _affiliateService.GetCurrentAffiliateId();
            var affiliateChain = _affiliateService.GetDownlineAffiliates(currentAffiliateId);
            query = query.Where(e => affiliateChain.Contains(
                EF.Property<Guid>(e, "AffiliateId")));
        }
        
        // Regional filtering
        if (typeof(IRegionalScoped).IsAssignableFrom(typeof(TEntity)))
        {
            var accessibleRegions = _regionService.GetAccessibleRegions();
            query = query.Where(e => accessibleRegions.Contains(
                EF.Property<string>(e, "Region")));
        }
        
        return query;
    }
}
```

---

## Performance Tips

### 1. Use Indexed Columns

Ensure columns used in custom filters are indexed:

```csharp
// In EF Core configuration
builder.HasIndex(e => e.OrganizationId)
    .HasDatabaseName("IX_Invoice_OrganizationId");

builder.HasIndex(e => e.AffiliateId)
    .HasDatabaseName("IX_Sale_AffiliateId");
```

### 2. Cache Service Results

Cache results that don't change frequently:

```csharp
public class CachedHierarchyService : IHierarchyService
{
    private readonly IMemoryCache _cache;
    private readonly IUserContext _userContext;
    
    public List<Guid> GetAccessibleOrganizationIds()
    {
        var userId = _userContext.GetCurrentUserIdAsync().GetAwaiter().GetResult();
        var cacheKey = $"accessible_orgs_{userId}";
        
        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return ComputeAccessibleOrganizations(userId);
        });
    }
}
```

### 3. Avoid N+1 Queries

Use `Contains` with pre-loaded lists instead of subqueries:

```csharp
// ✅ Good - Pre-load IDs
var accessibleOrgIds = _hierarchyService.GetAccessibleOrganizationIds();
query = query.Where(e => accessibleOrgIds.Contains(e.OrganizationId));

// ❌ Bad - Subquery (N+1 issue)
query = query.Where(e => context.Organizations
    .Where(o => o.ParentId == userOrgId)
    .Select(o => o.Id)
    .Contains(e.OrganizationId));
```

---

## Testing Custom Filters

```csharp
[Fact]
public async Task GetAllAsync_AppliesHierarchicalFilter()
{
    // Arrange
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    
    using var context = new TestDbContext(options);
    
    // Seed data
    var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
    var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2", ParentId = org1.Id };
    context.Organizations.AddRange(org1, org2);
    
    var invoice1 = new Invoice { OrganizationId = org1.Id, Amount = 100 };
    var invoice2 = new Invoice { OrganizationId = org2.Id, Amount = 200 };
    context.Invoices.AddRange(invoice1, invoice2);
    await context.SaveChangesAsync();
    
    // Mock hierarchy service to return only org1
    var mockHierarchy = new Mock<IHierarchyService>();
    mockHierarchy.Setup(x => x.GetAccessibleOrganizationIds())
        .Returns(new List<Guid> { org1.Id });
    
    var repository = new HierarchicalRepository<Invoice>(
        context, null, null, mockHierarchy.Object);
    
    // Act
    var results = await repository.GetAllAsync();
    
    // Assert
    Assert.Single(results);
    Assert.Equal(invoice1.Id, results.First().Id);
}
```

---

## Complete Example: Sales System

```csharp
// Entities
public class Sale : SimpleAuditableEntity, ITenantScoped, IHierarchical, IAffiliateScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public Guid OrganizationId { get; set; }
    public Guid AffiliateId { get; set; }
    
    public decimal Amount { get; set; }
}

// Repository
public class SalesRepository : SimpleRepository<Sale>
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IAffiliateService _affiliateService;
    
    public SalesRepository(
        DbContext context,
        ISimpleUserContext userContext,
        ISimpleTenantContext tenantContext,
        IHierarchyService hierarchyService,
        IAffiliateService affiliateService)
        : base(context, userContext, tenantContext)
    {
        _hierarchyService = hierarchyService;
        _affiliateService = affiliateService;
    }
    
    protected override IQueryable<Sale> ApplyCustomFilters(IQueryable<Sale> query)
    {
        // Hierarchical org filtering
        var accessibleOrgIds = _hierarchyService.GetAccessibleOrganizationIds();
        query = query.Where(s => accessibleOrgIds.Contains(s.OrganizationId));
        
        // Affiliate chain filtering
        var currentAffiliateId = _affiliateService.GetCurrentAffiliateId();
        var affiliateChain = _affiliateService.GetDownlineAffiliates(currentAffiliateId);
        query = query.Where(s => affiliateChain.Contains(s.AffiliateId));
        
        return query;
    }
}

// Registration
services.AddScoped<IHierarchyService, HierarchyService>();
services.AddScoped<IAffiliateService, AffiliateService>();
services.AddScoped<ISimpleRepository<Sale>, SalesRepository>();
```

---

## Summary

✅ **Built-in Filters**: Soft delete + tenant (automatic)  
✅ **Extension Point**: `ApplyCustomFilters` method  
✅ **Flexible**: Works with any filtering logic  
✅ **Composable**: Combine multiple filters  
✅ **Testable**: Easy to mock services  
✅ **Performant**: Use indexed columns and caching  

**Key Principle**: Define a marker interface for your filtering needs, create a service to compute access, override `ApplyCustomFilters` in your repository.
