# ADR-002: Guid-Only Entity System

**Status:** Accepted

**Date:** 2025-01-15 (Original Decision)
**Revised:** 2026-06-13 (Revised to Guid-Only)

**Decision Makers:** JumpStart Core Team

## Context

When designing a framework for rapid application development, we initially explored supporting both Guid-based and custom key type entities.

After implementation, we found that:
- The "Advanced" generic entity system with custom key types proved overly complex
- The C# generic constraints made the code difficult to maintain and extend
- The added complexity didn't justify the marginal benefit
- Most applications benefit from a simple, opinionated approach

We decided to simplify by committing to **Guid-based entities exclusively**, removing the Advanced entity system entirely.

## Decision

JumpStart uses a **single, unified entity system** with Guid-based keys exclusively.

**Base Classes:**
```csharp
// Simple entity with Guid key
public class Product : Entity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Entity with audit tracking
public class Order : AuditableEntity
{
    public decimal Total { get; set; }
}

// Named entity with audit tracking
public class Category : AuditableNamedEntity
{
    public string Description { get; set; } = string.Empty;
}
```

**Repository Pattern:**
```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IList<Product>> GetByCategoryAsync(Guid categoryId);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, IUserContext? userContext = null)
        : base(context, userContext) { }
}
```

## Consequences

### Positive Consequences

- **Simpler Codebase** – No complex generic constraints, easier to maintain and extend
- **Faster Development** – Opinionated approach means less decision-making for developers
- **Consistent API** – All entities use the same pattern, no confusion about which base class to use
- **Easier Onboarding** – New developers learn one system, not multiple parallel systems
- **Fewer Edge Cases** – Guid-based entities eliminate edge cases around key type handling
- **Better Documentation** – Documentation can focus on one system, not complex conditional logic

### Negative Consequences

- **No Custom Key Types** – Applications that absolutely require int, long, or custom key types cannot use JumpStart without significant modification
- **Legacy Migration** – Applications with existing int-based schemas may need to migrate to Guid
- **Slight Performance Overhead** – Guid keys are slightly larger than int keys (negligible for most applications)
- **Sequel Compatibility** – Applications using SQL Server SEQUENTIALIDGUID benefit from Guid.NewGuid() overhead

## Alternatives Considered

### 1. Keep Both Systems
Maintain both Guid-based and generic entity systems in parallel.

**Rejected:** Proved too complex to maintain, confusing for developers, and the added complexity didn't justify the marginal benefit of supporting custom key types.

### 2. Configuration-Based System
Allow applications to configure their preferred key type globally.

**Rejected:** Still required complex generic constraints underneath, didn't solve the core maintainability issues.

### 3. Hybrid Approach
Provide Guid-based as default, with extension points for custom key types.

**Rejected:** Still required maintaining complex inheritance hierarchies and generic constraints. Simpler to just commit to one approach.

## Implementation Details

### Removed Components

The following components were removed as part of this decision:

- `JumpStart.Data.Advanced.Entity<TKey>` - Generic entity with custom key types
- `JumpStart.Data.Advanced.Auditing.AuditableEntity<TKey>` - Advanced audit tracking
- `JumpStart.Repositories.Advanced.Repository<TEntity, TKey>` - Generic repository
- All `IRepository<TEntity, TKey>` interfaces with custom key type constraints
- ADR-001 decision discussing parallel systems

### Migration Path

For applications with existing int-based schemas, JumpStart recommends:

1. **Migrate to Guid IDs** – Convert primary keys to Guid
2. **Use Entity framework migrations** – EF Core handles Guid migrations smoothly
3. **Update application code** – Change all repository and service code to use Guid
4. **Benefit from better distributed system design** – Guid IDs work better across services

### Why Guid?

Guid-based identifiers were chosen over other options because:

- **Uniqueness** – Globally unique, no coordination needed
- **Performance** – Comparable to int for most workloads
- **Best Practices** – Recommended for modern distributed systems
- **EF Core Support** – Excellent Entity Framework Core support for Guid keys
- **Security** – Harder to enumerate than sequential int IDs

## Decision Owners

- Scott Blomfield - Framework Lead

## References

- [Guid vs Int as Primary Key - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [When to Use Guid vs Int - Stack Overflow](https://stackoverflow.com/questions/1060057/when-should-i-use-guid-vs-int-as-primary-key)
- [EF Core Guid Best Practices](https://docs.microsoft.com/en-us/ef/core/modeling/property-types)

## Related Documentation

- [Getting Started: Creating Entities](../../getting-started.md)
- [Core Concepts: Entity System](../../core-concepts.md)
- [Architecture: Extension Points](../../architecture/extension-points.md)
- [API Reference: Entity](../../api/data/entity.md)
- [API Reference: AuditableEntity](../../api/data/auditableentity.md)
