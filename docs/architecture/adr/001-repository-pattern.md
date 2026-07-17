# ADR-001: Repository Pattern

**Status:** Accepted (partially superseded - see note)

**Date:** 2025-01-15

**Decision Makers:** JumpStart Core Team

> **⚠️ Partially superseded (2026-01-25):** The core decision here - use the repository pattern,
> with CRUD, pagination, audit tracking, soft delete, and DI/assembly-scanning support - still
> stands. But the "Simple vs Advanced" (`ISimpleRepository`/`SimpleRepository` vs
> `IRepository<TEntity, TKey>`/`Repository<TEntity, TKey>`) dual system described below was
> removed; there is now a single `IRepository<TEntity>`/`Repository<TEntity>` pair, Guid-only.
> See [ADR-009: Guid-Only Entities](009-guid-only-entities.md). Code samples below that reference
> `ISimpleUserContext`, `SimpleRepository`, or `IUserContext<TKey>` are historical and do not
> compile against the current codebase - see [Core Concepts](../../core-concepts.md) for current
> repository examples.

## Context

Modern applications require a clean separation between business logic and data access. Direct use of Entity Framework Core DbContext throughout the application leads to several problems:

- **Tight coupling** between business logic and data access implementation
- **Difficult testing** due to direct database dependencies
- **Code duplication** of common queries across services
- **Inconsistent** data access patterns across the codebase
- **Limited abstraction** making it hard to switch ORMs or add caching
- **Scattered business rules** mixed with data access code

We needed a pattern that would provide a clean abstraction over data access while maintaining the power and flexibility of Entity Framework Core.

## Decision

We will implement the **Repository Pattern** with a single level of abstraction:

### 1. Generic Repository Base Class

- `IRepository<TEntity>` - Interface for entities with Guid key type
- `Repository<TEntity>` - Abstract base implementation with EF Core

### 2. Entity-Specific Repositories

Application developers extend the base repositories to create entity-specific repositories:

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IList<Product>> GetByCategoryAsync(Guid categoryId);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, IUserContext? userContext = null)
        : base(context, userContext)
    {
    }
    
    public async Task<IList<Product>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}
```

### Key Features

1. **Complete CRUD Operations** - GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync
2. **Pagination Support** - QueryOptions with sorting and filtering
3. **Automatic Audit Tracking** - CreatedOn, ModifiedOn, DeletedOn fields
4. **Soft Delete** - Entities marked as deleted, not physically removed
5. **User Context Integration** - Automatic population of audit user fields
6. **Async Throughout** - All operations use async/await
7. **Virtual Methods** - Allow overriding for custom behavior
8. **EF Core Optimizations** - AsNoTracking for read operations

### Registration

Automatic repository discovery via assembly scanning:

```csharp
builder.Services.AddJumpStartWithDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    jumpStart =>
    {
        jumpStart.RegisterUserContext<BlazorUserContext>();
        jumpStart.ScanAssembly(typeof(Program).Assembly);
    });
```

## Consequences

### Positive Consequences

- **Clean Separation** - Business logic separated from data access implementation
- **Testability** - Easy to mock repositories for unit testing
- **Consistency** - Uniform data access patterns across the application
- **Reusability** - Common queries defined once and reused
- **Maintainability** - Changes to data access logic centralized in repositories
- **Flexibility** - Can add caching, logging, or other cross-cutting concerns
- **Type Safety** - Compile-time checking of entity types and operations
- **Developer Productivity** - Reduced boilerplate code for common operations
- **Automatic Auditing** - Audit fields populated without manual intervention
- **Soft Delete** - Data preservation for compliance and recovery

### Negative Consequences

- **Additional Layer** - Adds one more layer of abstraction
- **Learning Curve** - Developers must understand repository pattern
- **Potential Over-Abstraction** - Risk of creating unnecessary repositories for simple scenarios
- **Code Generation** - More interface and class files to maintain

### Neutral Consequences

- **Not a Pure Repository Pattern** - Exposes `IQueryable<T>` in some methods for flexibility
- **Entity Framework Dependency** - Repositories still use EF Core types (DbContext, IQueryable)
- **Guid-Only Approach** - Opinionated approach eliminates complexity of maintaining parallel systems

## Alternatives Considered

### 1. Direct DbContext Usage

**Pros:**
- Simpler, no additional abstraction
- Full EF Core power directly available
- Less code to write and maintain

**Cons:**
- Tight coupling to EF Core
- Difficult to test
- Code duplication
- Inconsistent patterns

**Why Rejected:** Violates separation of concerns and makes testing difficult.

### 2. Generic Repository Only

**Pros:**
- Single implementation
- Maximum flexibility

**Cons:**
- More complex API with generic parameters
- Harder to use for common Guid-based scenarios

**Why Rejected:** Decided to provide a simpler Guid-only approach.

### 3. Specification Pattern

**Pros:**
- Very flexible query composition
- Reusable query logic

**Cons:**
- More complex to implement and understand
- Overkill for many scenarios

**Why Rejected:** Can be added later if needed without changing the repository pattern.

## Implementation Notes

### User Context Abstraction (current)

Repositories accept a single `IUserContext` (async, Guid-only) to determine the current user for audit fields:

```csharp
public interface IUserContext
{
    Task<Guid?> GetCurrentUserIdAsync();
}
```

### Soft Delete Filter (current)

The base repository automatically filters soft-deleted entities:

```csharp
protected virtual IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
{
    if (typeof(IDeletable).IsAssignableFrom(typeof(TEntity)))
    {
        return query.Where(e => EF.Property<DateTimeOffset?>(e, nameof(IDeletable.DeletedOn)) == null);
    }
    return query;
}
```

### Pagination (current)

`QueryOptions<TEntity>` provides pagination and sorting (page number/size are nullable - omitting
both returns everything):

```csharp
public class QueryOptions<TEntity>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Expression<Func<TEntity, object>>? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
```

## References

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ADR-002: Simple vs Advanced Entities](002-simple-advanced-entities.md)
- [ADR-003: Audit Tracking Implementation](003-audit-tracking.md)

## Related Documentation

- [How-To: Custom Repository](../../how-to/custom-repository.md)
- [How-To: Pagination](../../how-to/pagination.md)
- [API Reference: ISimpleRepository](../../api/repositories/isimplerepository.md)
