# ADR-002: Simple vs Advanced Entities

**Status:** Accepted

**Date:** 2025-01-15

**Decision Makers:** JumpStart Core Team

## Context

When designing a framework for rapid application development, we faced a fundamental decision about entity design. Different applications have different needs:

- **New applications** typically use Guid identifiers and modern patterns
- **Legacy systems** may require int, long, or custom key types
- **Distributed systems** benefit from Guid's global uniqueness
- **Traditional databases** often use auto-increment integer keys
- **Developer experience** is improved with simpler APIs when possible
- **Flexibility** is needed for advanced scenarios

We needed to balance **simplicity for common cases** with **flexibility for complex scenarios** without forcing developers to choose one or the other.

## Decision

JumpStart provides a unified entity system using `Entity`, `AuditableEntity`, and `NamedEntity` base classes with Guid keys.

**Example:**
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

public class ProductService
{
    private readonly IRepository<Product> _repository;
    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
}
```

## Consequences

### Positive Consequences

- **Unified API** – Most applications use a single, simple set of base classes (`Entity`, `AuditableEntity`, `NamedEntity`) with Guid keys by default.
- **Extensibility** – Advanced/legacy scenarios are supported by allowing custom key types via inheritance, without duplicating the core logic.
- **Modern Defaults** – Encourages best practices (Guid IDs, audit tracking, clean separation of concerns).
- **Backward Compatibility** – Existing code using custom key types can be supported with minimal changes.
- **Reduced Complexity** – No need to choose between two parallel systems; the default is simple, but extensible.
- **Consistent Patterns** – All repositories, services, and controllers follow the same conventions.

### Negative Consequences

- **Slightly More Verbose for Custom Keys** – Using custom key types requires explicit inheritance and configuration, but this is rare for new projects.
- **Migration Effort** – Legacy code using the old "Simple" or "Advanced" types may require refactoring to the new unified base classes.
- **Documentation Updates** – Existing documentation and samples must be updated to reflect the unified approach.

## Alternatives Considered

### 1. Generic-Only Approach

Require all entities and repositories to specify key types (e.g., `Entity<TKey>`, `Repository<TEntity, TKey>`).

**Rejected:** Too verbose for the common case; most modern applications use Guid keys and benefit from a simpler default.

### 2. Configuration-Based Approach

Allow a single key type to be configured globally for the application.

**Rejected:** Inflexible for applications that need to mix key types (e.g., during migrations or when integrating with legacy systems).

### 3. Parallel Systems (Simple/Advanced)

Maintain two separate sets of base classes and infrastructure for Guid and custom key types.

**Rejected:** Duplicates logic, increases maintenance burden, and causes confusion. A unified, extensible system is simpler and easier to maintain.

## Implementation Details

### Inheritance Chain Example

```csharp
// Advanced system
public interface IEntity<TKey> where TKey : struct
{
    TKey Id { get; set; }
}

public abstract class Entity<TKey> : IEntity<TKey> where TKey : struct
{
    [Key]
    public TKey Id { get; set; }
}

// Simple system inherits and specializes
public interface ISimpleEntity : IEntity<Guid>
{
}

public abstract class SimpleEntity : Entity<Guid>, ISimpleEntity
{
}
```

### User Context Parallel

```csharp
// Advanced
public interface IUserContext<TKey> where TKey : struct
{
    TKey? UserId { get; }
}

// Simple
public interface ISimpleUserContext : IUserContext<Guid>
{
}
```

## References

- [ADR-001: Repository Pattern](001-repository-pattern.md)
- [ADR-003: Audit Tracking Implementation](003-audit-tracking.md)
- [Guid vs Int as Primary Key - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

## Related Documentation

- [Getting Started: Creating Entities](../../getting-started/creating-entities.md)
- [How-To: Custom Repository](../../how-to/custom-repository.md)
- [API Reference: SimpleEntity](../../api/data/simpleentity.md)
- [API Reference: Entity<TKey>](../../api/data/entity.md)
