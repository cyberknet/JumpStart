# JumpStart Framework Documentation

Welcome to the JumpStart Framework documentation! JumpStart is a Blazor framework that provides entity base classes, repository pattern implementation, audit tracking, and API client integration for rapid application development.

## ?? Documentation Sections

### [Getting Started](getting-started.md)
New to JumpStart? Start here to learn the basics and build your first application.
- Installation
- Quick Start Tutorial
- Your First Entity
- Your First Repository
- Your First API

### [Core Concepts](core-concepts.md)
Deep dive into the fundamental concepts that power JumpStart.
- Entity System (Simple & Advanced)
- Repository Pattern
- User Context
- Dependency Injection

### [Audit Tracking](audit-tracking.md)
Learn how JumpStart automatically tracks who created, modified, and deleted entities.
- Automatic Audit Fields
- User Context Integration
- Custom Audit Scenarios

### [API Development](api-development.md)
Build RESTful APIs quickly with JumpStart's base controllers and DTOs.
- API Controllers
- DTOs and Mapping
- API Clients with Refit
- Authentication (JWT & Cookies)

### [Authentication & Security](authentication.md)
Comprehensive guide to JWT and cookie authentication in JumpStart applications.

### [How-To Guides](how-to/index.md)
Task-oriented guides for common scenarios.
- [Create a Custom Repository](how-to/custom-repository.md)
- [Implement Soft Delete](how-to/soft-delete.md)
- [Add Custom Audit Fields](how-to/custom-audit-fields.md)
- [Secure API Endpoints](how-to/secure-endpoints.md)
- [Query with Pagination](how-to/pagination.md)
- [Use Advanced Entities](how-to/advanced-entities.md)

### [Architecture](architecture/index.md)
Understand the design decisions and architecture of JumpStart.
- [Design Philosophy](architecture/design-philosophy.md)
- [Project Structure](architecture/project-structure.md)
- [Extension Points](architecture/extension-points.md)
- [Decision Records](architecture/adr/index.md)

### [API Reference](api/index.html)
Complete API reference generated from XML documentation comments.
> **Note:** API reference is generated using DocFX. See [Contributing to Documentation](contributing-to-docs.md) for details.

### [Sample Applications](samples.md)
Learn from complete sample applications.
- **JumpStart.DemoApp** - Blazor Server application with Identity
- **JumpStart.DemoApp.Api** - RESTful API with JWT authentication

## ?? Quick Links

- [GitHub Repository](https://github.com/cyberknet/JumpStart)
- [NuGet Package](https://www.nuget.org/packages/JumpStart)
- [Report Issues](https://github.com/cyberknet/JumpStart/issues)
- [Contributing Guidelines](../CONTRIBUTING.md)
- [License](../LICENSE.txt)

## ?? Key Features

- **Entity Base Classes** - Simple and advanced entity types with built-in ID management
- **Audit Tracking** - Automatic tracking of created/modified/deleted information
- **Repository Pattern** - Generic repository with async support and pagination
- **API Controllers** - Base controllers for rapid API development
- **API Clients** - Refit-based API clients with automatic configuration
- **AutoMapper Integration** - Simplified DTO mapping
- **JWT Authentication** - Built-in JWT token generation and validation
- **Dependency Injection** - First-class DI support throughout

## ?? Learning Path

**Beginner:** Getting Started ? Core Concepts ? Sample Applications

**Intermediate:** Audit Tracking ? API Development ? How-To Guides

**Advanced:** Architecture ? Extension Points ? Contributing

## ?? Documentation Version

This documentation is for **JumpStart 1.0.0** targeting **.NET 10**.

## ?? Documentation Navigation

This documentation is organized to help you find what you need quickly:

- **New to JumpStart?** Start with [Getting Started](getting-started.md)
- **Need to accomplish a specific task?** Check [How-To Guides](how-to/index.md)
- **Want to understand the framework deeply?** Read [Core Concepts](core-concepts.md) and [Architecture](architecture/index.md)
- **Looking for specific APIs?** Browse the [API Reference](api/index.html)
- **Having issues?** See [FAQ](faq.md) or [Troubleshooting](troubleshooting.md)

## ?? Contributing

We welcome contributions to both the framework and documentation! Please see:
- [Contributing to Code](../CONTRIBUTING.md)
- [Contributing to Documentation](contributing-to-docs.md)

## ?? License

JumpStart is licensed under the GNU General Public License v3.0. See [LICENSE](../LICENSE.txt) for details.

---

**Need Help?** Check our [FAQ](faq.md), browse [How-To Guides](how-to/index.md), or [open an issue](https://github.com/cyberknet/JumpStart/issues).
