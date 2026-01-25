# Architecture

Understanding the design and architecture of JumpStart.

## Overview

This section provides insight into the architectural decisions, design patterns, and extensibility points in JumpStart. Whether you're contributing to the framework or building advanced applications, understanding the architecture will help you make the most of JumpStart.

## Contents

### [Design Philosophy](design-philosophy.md)
The principles and values that guide JumpStart's development.
- Convention over configuration
- Developer productivity
- Type safety
- Extensibility
- Performance considerations

### [Project Structure](project-structure.md)
How the JumpStart solution is organized.
- Core library (JumpStart)
- Demo applications (JumpStart.DemoApp, JumpStart.DemoApp.Api)
- Test project (JumpStart.Tests)
- Documentation

### [Extension Points](extension-points.md)
Where and how you can extend JumpStart.
- Custom entity base classes
- Repository customization
- Custom API controllers
- Authentication providers
- User context implementations

### [Architecture Decision Records](adr/index.md)
Documented decisions about significant architectural choices.
- [ADR-001: Repository Pattern](adr/001-repository-pattern.md)
- [ADR-002: Simple vs Advanced Entities](adr/002-simple-advanced-entities.md)
- [ADR-003: Audit Tracking Implementation](adr/003-audit-tracking.md)
- [ADR-004: JWT Authentication](adr/004-jwt-authentication.md)
- [ADR-005: Refit for API Clients](adr/005-refit-api-clients.md)

## Design Patterns

JumpStart implements several well-established design patterns:

### Repository Pattern
Provides an abstraction layer between the domain/business layer and data mapping layers, allowing for cleaner separation of concerns and easier testing.

### Unit of Work
The DbContext in Entity Framework Core serves as the Unit of Work, managing transactions and tracking changes.

### Data Transfer Objects (DTO)
Separates internal entity representation from external API contracts, providing flexibility and security.

### Dependency Injection
First-class support for ASP.NET Core's built-in DI container, promoting loose coupling and testability.

### Factory Pattern
Used in service collection extensions to simplify configuration and registration of JumpStart services.

## Technology Stack

- **.NET 10** - Latest .NET platform features
- **Entity Framework Core 10** - ORM and data access
- **ASP.NET Core 10** - Web framework
- **Refit 8** - Type-safe REST API client
- **AutoMapper 16** - Object-to-object mapping
- **xUnit** - Testing framework
- **Moq** - Mocking framework

## Key Architectural Decisions

### Why Two Entity Systems?
JumpStart provides both "Simple" (Guid-based) and "Advanced" (generic key type) entity systems to balance ease of use with flexibility. See [ADR-002](adr/002-simple-advanced-entities.md) for details.

### Why Separate API Project?
The API can be deployed independently from the Blazor Server application, enabling:
- Separate scaling strategies
- Independent deployment
- Different authentication mechanisms
- Reuse across multiple clients

### Why User Context Abstraction?
`IUserContext` and `IUserContext` abstract user information retrieval, allowing different implementations for:
- Blazor Server (cookie authentication)
- Web API (JWT bearer tokens)
- Testing (mock implementations)
- Background jobs (system user)

## Performance Considerations

### Async All the Way
All repository methods are async to prevent thread pool starvation and improve scalability.

### Lazy Loading Disabled
JumpStart examples disable lazy loading by default to prevent N+1 query problems. Explicit eager loading is encouraged.

### Pagination Built-In
Repository methods support pagination to prevent loading large datasets into memory.

## Security Considerations

### No Direct Entity Exposure
API controllers use DTOs instead of exposing entities directly, preventing over-posting attacks.

### Audit Trail
Automatic tracking of who created, modified, or deleted entities provides accountability.

### JWT Best Practices
Token validation includes issuer, audience, lifetime, and signing key checks with zero clock skew.

---

**Contributing to Architecture?** See [Contributing Guidelines](../../CONTRIBUTING.md) and propose new [Architecture Decision Records](adr/index.md).
