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

We will provide **two parallel entity systems** that coexist in the framework:

### 1. Simple Entity System (Recommended for New Applications)

Uses **Guid** as the identifier type throughout, removing generic type parameters from common scenarios.

**Base Classes:**
- `ISimpleEntity` - Interface for Guid-based entities
- `SimpleEntity` - Base class with Guid Id
- `SimpleNamedEntity` - Adds Name property
- `ISimpleAuditable` - Audit tracking with Guid user IDs
- `SimpleAuditableEntity` - Full audit tracking
- `SimpleAuditableNamedEntity` - Named entity with audit tracking

**Supporting Infrastructure:**
- `ISimpleRepository<TEntity>` - Repository for Guid entities
- `SimpleRepository<TEntity>` - Repository implementation
- `ISimpleUserContext` - User context with Guid UserId
- `ISimpleApiClient<TDto, TCreateDto, TUpdateDto>` - API client
- `SimpleApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto>` - Controller base
- `SimpleEntityDto` - DTO base class

**Example:**
```csharp
// Entity - no generic parameters
public class Product : SimpleAuditableNamedEntity
{
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Repository - single generic parameter
public class ProductRepository : SimpleRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context, ISimpleUserContext userContext)
        : base(context, userContext)
    {
    }
}

// Service - clean, simple API
public class ProductService
{
    private readonly ISimpleRepository<Product> _repository;
    
    public ProductService(ISimpleRepository<Product> repository)
    {
        _repository = repository;
    }
}
```

### 2. Advanced Entity System (For Flexibility)

Uses **generic key types** (`TKey`) allowing any struct type as identifier.

**Base Classes:**
- `IEntity<TKey>` - Interface for entities with custom keys
- `Entity<TKey>` - Base class with TKey Id
- `NamedEntity<TKey>` - Adds Name property
- `IAuditable<TKey>` - Audit tracking with TKey user IDs
- `AuditableEntity<TKey>` - Full audit tracking
- `AuditableNamedEntity<TKey>` - Named entity with audit tracking

**Supporting Infrastructure:**
- `IRepository<TEntity, TKey>` - Repository for custom key types
- `Repository<TEntity, TKey>` - Repository implementation
- `IUserContext<TKey>` - User context with TKey UserId
- `IAdvancedApiClient<TDto, TCreateDto, TUpdateDto, TKey>` - API client
- `AdvancedApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TKey>` - Controller base
- `EntityDto<TKey>` - DTO base class

**Example:**
```csharp
// Entity - explicit key type
public class LegacyProduct : AuditableNamedEntity<int>
{
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Repository - two generic parameters
public class LegacyProductRepository : Repository<LegacyProduct, int>, ILegacyProductRepository
{
    public LegacyProductRepository(DbContext context, IUserContext<int> userContext)
        : base(context, userContext)
    {
    }
}

// Service - explicit key types
public class LegacyProductService
{
    private readonly IRepository<LegacyProduct, int> _repository;
    
    public LegacyProductService(IRepository<LegacyProduct, int> repository)
    {
        _repository = repository;
    }
}
```

### Relationship Between Systems

The Simple system **inherits from** the Advanced system with Guid as the key type:

```csharp
// Simple entity inherits from generic entity with Guid
public abstract class SimpleEntity : Entity<Guid>, ISimpleEntity
{
}

// Simple repository inherits from generic repository with Guid
public abstract class SimpleRepository<TEntity> : Repository<TEntity, Guid>
    where TEntity : class, ISimpleEntity
{
}
```

This means:
- Simple system is a **specialized version** of Advanced
- No code duplication
- Bug fixes to Advanced automatically benefit Simple
- Developers can mix both systems in one application

## Consequences

### Positive Consequences

- **Simplified API** - Simple system removes generic complexity for 90% of use cases
- **Flexibility** - Advanced system handles legacy databases and special requirements
- **Gradual Adoption** - Start with Simple, migrate to Advanced only when needed
- **No Trade-offs** - Get simplicity without sacrificing flexibility
- **Code Reuse** - Simple inherits from Advanced, avoiding duplication
- **Type Safety** - Both systems provide compile-time type checking
- **Consistent Patterns** - Same patterns work across both systems
- **Developer Choice** - Pick the right tool for the job
- **Modern Defaults** - Simple system encourages best practices (Guid IDs)

### Negative Consequences

- **Two APIs** - Developers must learn which system to use when
- **Documentation** - Must document both systems thoroughly
- **Discovery** - Slightly more complex to discover the right base class
- **Cognitive Load** - Understanding the relationship between systems takes time
- **Naming Confusion** - "Simple" might imply limited functionality

### Neutral Consequences

- **More Classes** - Double the number of base classes and interfaces
- **Namespace Organization** - Simple classes in root namespace, Advanced in `Advanced` namespace
- **IntelliSense Clutter** - Both systems appear in IntelliSense

## Alternatives Considered

### 1. Generic-Only Approach

Use only `Entity<TKey>` and require developers to specify `Guid` everywhere.

**Pros:**
- Single API to learn
- No duplication

**Cons:**
- Verbose for common case: `Repository<Product, Guid>`
- Generic clutter in 90% of use cases
- Worse developer experience

**Why Rejected:** Developer experience matters, and most applications use Guid.

### 2. Simple-Only Approach

Use only `SimpleEntity` and force Guid everywhere.

**Pros:**
- Clean, simple API
- No confusion

**Cons:**
- Cannot support legacy databases with int keys
- Cannot support custom key types
- Limits framework applicability

**Why Rejected:** Too restrictive for real-world scenarios.

### 3. Separate Frameworks

Create two completely separate frameworks.

**Pros:**
- No confusion between systems
- Optimized for each use case

**Cons:**
- Code duplication
- Bugs fixed in one may not be fixed in other
- Harder to migrate between them

**Why Rejected:** Maintenance burden and inability to mix systems.

### 4. Configuration-Based Approach

Single entity system with key type configured once for entire application.

**Pros:**
- Application-wide consistency
- Single API

**Cons:**
- Cannot mix key types in one application
- Complex configuration
- Still need generic parameters somewhere

**Why Rejected:** Too inflexible, doesn't solve the verbosity issue.

## When to Use Each System

### Use Simple When:

? Building new applications from scratch  
? Using ASP.NET Core Identity (Guid-based)  
? Building distributed systems  
? Deploying to cloud-native environments  
? Preferring globally unique identifiers  
? Wanting simpler API with less generic noise  

### Use Advanced When:

? Working with legacy databases  
? Requiring int or long auto-increment keys  
? Using custom struct key types  
? Integrating with existing systems  
? Needing explicit control over key type  
? Migrating from older frameworks  

### Mix Both When:

? Modernizing legacy applications incrementally  
? New features use Guid, legacy uses int  
? Different bounded contexts need different keys  

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

## Migration Path

### From Simple to Advanced

If you start with Simple and later need Advanced:

1. Change entity base class from `SimpleEntity` to `Entity<Guid>`
2. Change repository from `SimpleRepository<T>` to `Repository<T, Guid>`
3. Update interfaces from `ISimpleEntity` to `IEntity<Guid>`
4. Code still works, just more verbose

### From Advanced to Simple

If you're using `Entity<Guid>` and want to simplify:

1. Change entity base class from `Entity<Guid>` to `SimpleEntity`
2. Change repository from `Repository<T, Guid>` to `SimpleRepository<T>`
3. Update interfaces from `IEntity<Guid>` to `ISimpleEntity`
4. Cleaner, more concise code

## References

- [ADR-001: Repository Pattern](001-repository-pattern.md)
- [ADR-003: Audit Tracking Implementation](003-audit-tracking.md)
- [Guid vs Int as Primary Key - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

## Related Documentation

- [Getting Started: Creating Entities](../../getting-started/creating-entities.md)
- [How-To: Custom Repository](../../how-to/custom-repository.md)
- [API Reference: SimpleEntity](../../api/data/simpleentity.md)
- [API Reference: Entity<TKey>](../../api/data/entity.md)
