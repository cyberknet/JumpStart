# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for JumpStart. An ADR documents an important architectural decision made along with its context and consequences.

## What is an ADR?

An Architecture Decision Record (ADR) is a document that captures an important architectural decision made along with its context and consequences. ADRs help:

- Understand why certain decisions were made
- Onboard new team members
- Review past decisions
- Make future decisions with historical context

## Format

Each ADR follows this format:

```markdown
# ADR-XXX: [Title]

**Status:** [Proposed | Accepted | Deprecated | Superseded]

**Date:** YYYY-MM-DD

**Decision Makers:** [Who was involved in this decision]

## Context

What is the issue that we're seeing that motivates this decision or change?

## Decision

What is the change that we're proposing and/or doing?

## Consequences

What becomes easier or more difficult to do because of this change?

### Positive Consequences
- [Benefit 1]
- [Benefit 2]

### Negative Consequences
- [Trade-off 1]
- [Trade-off 2]

### Neutral Consequences
- [Change 1]
- [Change 2]

## Alternatives Considered

What other options were considered?

### Alternative 1
[Description and why it was rejected]

### Alternative 2
[Description and why it was rejected]

## References

- [Links to relevant issues, PRs, discussions]
```

## Records

### Active Records

- [ADR-001: Repository Pattern](001-repository-pattern.md) - Why we use the repository pattern
- [ADR-002: Simple vs Advanced Entities](002-simple-advanced-entities.md) - Dual entity system rationale
- [ADR-003: Audit Tracking Implementation](003-audit-tracking.md) - Automatic audit tracking approach
- [ADR-004: JWT Authentication](004-jwt-authentication.md) - JWT authentication strategy
- [ADR-005: Refit for API Clients](005-refit-api-clients.md) - Using Refit for API clients
- [ADR-006: AutoMapper Integration](006-automapper-integration.md) - DTO mapping approach
- [ADR-007: Entity Framework Core](007-entity-framework-core.md) - ORM choice
- [ADR-008: Separate API Project](008-separate-api-project.md) - Why DemoApp.Api is separate

### Superseded Records

_(None yet)_

## Creating a New ADR

When making a significant architectural decision:

1. Copy the template above
2. Number it sequentially (ADR-XXX)
3. Fill in all sections thoughtfully
4. Open a PR for discussion
5. Update this index once accepted

## Decision Criteria

Not every decision needs an ADR. Create an ADR when:

- The decision impacts multiple parts of the system
- The decision is difficult to reverse
- The decision involves significant trade-offs
- The decision affects developers using the framework
- The decision might be questioned or reconsidered later

---

**Want to propose a new ADR?** See [Contributing Guidelines](../../../CONTRIBUTING.md) for the process.
