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

### [RFCs](rfc/index.md)
Design proposals for roadmap features that are still open, before they become an ADR.
- [RFC-001: Notifications](rfc/001-notifications.md)
- [RFC-002: Attachments](rfc/002-attachments.md)
- [RFC-003: Approvals](rfc/003-approvals.md)

### [Architecture Decision Records](adr/index.md)
Documented decisions about significant architectural choices.
- [ADR-001: Repository Pattern](adr/001-repository-pattern.md)
- [ADR-002: Simple vs Advanced Entities](adr/002-simple-advanced-entities.md) (superseded by ADR-009)
- [ADR-003: Audit Tracking Implementation](adr/003-audit-tracking.md)
- [ADR-004: JWT Authentication](adr/004-jwt-authentication.md)
- [ADR-005: Refit for API Clients](adr/005-refit-api-clients.md)
- [ADR-009: Guid-Only Entities](adr/009-guid-only-entities.md)
- [ADR-010: Multi-Tenant Data Isolation](adr/010-multi-tenant-data-isolation.md)
- [ADR-011: Entity-Level Authorization](adr/011-entity-authorization.md)
- [ADR-012: Role-Based Permission Management](adr/012-role-based-permission-management.md)
- [ADR-013: JWT Token Exchange for Permission Resolution](adr/013-jwt-token-exchange.md)
- [ADR-014: Automatic JWT Exchange for Auto-Discovered API Clients](adr/014-automatic-jwt-exchange-for-api-clients.md)

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

### Why Guid-Only Entities?
JumpStart is opinionated: all entities use `Guid` identifiers. An earlier design (see
[ADR-002](adr/002-simple-advanced-entities.md), now superseded) explored a dual Guid/generic
key-type system, but it was dropped in favor of a single, simpler entity hierarchy - see
[ADR-009](adr/009-guid-only-entities.md) for the rationale.

### Why Separate API Project?
The API can be deployed independently from the Blazor Server application, enabling:
- Separate scaling strategies
- Independent deployment
- Different authentication mechanisms
- Reuse across multiple clients

### Why User Context Abstraction?
`IUserContext` abstracts user information retrieval, allowing different implementations for:
- Blazor Server (cookie authentication)
- Web API (JWT bearer tokens)
- Testing (mock implementations)
- Background jobs (system user)

### Why Multi-Tenancy via Global Query Filter?
`ITenantScoped` entities are isolated automatically using the same EF Core global query filter
mechanism as soft delete, rather than requiring each repository to filter by tenant manually - see
[ADR-010](adr/010-multi-tenant-data-isolation.md) for the full design.

### Why Mandatory Entity-Level Authorization?
Every `ApiControllerBase` action requires a matching `Permission` claim (`"{EntityName}.{Action}"`)
by default, with no opt-out - see [ADR-011](adr/011-entity-authorization.md). This guarantees no
CRUD endpoint ships unprotected, at the cost of requiring every application to design a permission
strategy before any endpoint will respond successfully.

### Why Role-Based Permission Management?
ADR-011 defines how `Permission` claims are checked, but not how an application decides which
claims a user should have. `Role`/`RolePermission`/`UserRole` (optionally tenant-scoped, via the new
`ITenantScopedOptional` interface - a role can be tenant-owned or global) plus a direct
`UserPermission` grant path provide that missing piece - see
[ADR-012](adr/012-role-based-permission-management.md).

### Why a JWT Token Exchange Endpoint?
A Blazor Server client authenticates users via Identity's cookie but has no direct
`IRoleRepository` access to resolve `Permission` claims itself. A short-lived identity assertion
JWT, exchanged for a real permission-bearing JWT via a `[Authorize]`-only (not `[EntityAuthorize]`)
endpoint, closes that gap without introducing a second authentication mechanism - see
[ADR-013](adr/013-jwt-token-exchange.md).

### Why Auto-Attach JWT Exchange Instead of an Extension Interface?
JumpStart is opinionated, not maximally flexible - it already prescribes the Blazor-Server-to-API
token flow (ADR-013), so `RegisterApiClients` auto-attaches a concrete `JwtExchangeHandler` (not a
consumer-implemented interface) whenever that flow's prerequisites are registered. See
[ADR-014](adr/014-automatic-jwt-exchange-for-api-clients.md).

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

### Entity Authorization by Default
Every `ApiControllerBase` action requires a `Permission` claim automatically (see
[ADR-011](adr/011-entity-authorization.md)) - there is no unprotected CRUD endpoint by default.

---

**Contributing to Architecture?** See [Contributing Guidelines](../../CONTRIBUTING.md) and propose new [Architecture Decision Records](adr/index.md).
