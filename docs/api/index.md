# API Reference

Welcome to the JumpStart API reference documentation. This section provides detailed information about all public types, methods, and properties in the JumpStart framework.

## Navigation

Browse the API by namespace or use the search feature to find specific types and members.

## Key Namespaces

### JumpStart.Data
Core entity types and interfaces for building domain models.

- **Simple Entities** - Guid-based entities for rapid development
- **Advanced Entities** - Generic entities with custom key types
- **Auditing** - Interfaces and base classes for automatic audit tracking

### JumpStart.Repositories
Repository pattern implementation with async support and pagination.

- **ISimpleRepository** - Repository for Guid-based entities
- **IRepository** - Generic repository with custom key types
- **Query Options** - Pagination, sorting, and filtering
- **User Context** - Integration with authentication systems

### JumpStart.Api
RESTful API development with base controllers, DTOs, and type-safe clients.

- **Controllers** - Base classes for rapid API endpoint creation
- **DTOs** - Data transfer objects for API contracts
- **Clients** - Refit-based type-safe API clients
- **Mapping** - AutoMapper profiles for entity-DTO conversion

### JumpStart.Services
Framework services including JWT authentication.

- **Authentication** - JWT token generation and validation
- **Token Store** - In-memory token management

### JumpStart.Extensions
Dependency injection extensions and configuration.

- **ServiceCollectionExtensions** - DI registration methods
- **JumpStartOptions** - Framework configuration options

## Documentation Conventions

### Type Parameters

- `TEntity` - Entity type implementing IEntity or ISimpleEntity
- `TKey` - Primary key type (must be a struct)
- `TDto` - Data transfer object type
- `TCreateDto` - DTO for create operations
- `TUpdateDto` - DTO for update operations

### Property Conventions

Properties marked as `{ get; set; }` can be both read and written.

### Async Methods

Methods ending with `Async` are asynchronous and return `Task` or `Task<T>`.

### Nullable References

Types marked with `?` are nullable. The framework uses nullable reference types for improved null safety.

## Getting Help

- **[How-To Guides](../how-to/index.md)** - Task-oriented guides
- **[Core Concepts](../core-concepts.md)** - Understand fundamental concepts
- **[Samples](../samples.md)** - Complete sample applications
- **[GitHub Issues](https://github.com/cyberknet/JumpStart/issues)** - Report bugs or request features

## Contributing

Found an issue in the documentation? Please [open an issue](https://github.com/cyberknet/JumpStart/issues) or submit a pull request.
