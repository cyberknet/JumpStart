# Requests for Comments (RFCs)

This directory contains RFCs for JumpStart. Where an [ADR](../adr/index.md) records a decision
that has already been made, an RFC is the step before that: a proposal, written while the design is
still open, so alternatives and open questions get argued through *before* code exists rather than
rationalized after the fact.

## What is an RFC?

A Request for Comments is a design proposal for a feature that doesn't exist yet. It's expected to
change during discussion - sections get rewritten, alternatives get added or dropped, open
questions get resolved one at a time. An ADR, by contrast, is a closed record of what was actually
decided; ADR-002 shows what happens to a settled decision that turns out to be wrong (it gets
superseded by a new ADR, never edited in place). RFCs are allowed to be messy in a way ADRs
deliberately are not.

Not every feature needs one. The [Roadmap](../../roadmap.md) already flags which candidate features
have enough open design questions to be worth the RFC step versus going straight to an ADR.

## Format

```markdown
# RFC-XXX: [Title]

**Status:** [Draft | Discussion | Accepted | Rejected | Withdrawn | Implemented - see ADR-XXX]

**Date:** YYYY-MM-DD

**Author:** [Who wrote this proposal]

## Summary

One or two sentences: what is this feature?

## Motivation

What problem does this solve? Why does JumpStart need it?

## Detailed Design

The proposed shape - entities, interfaces, integration points with existing framework pieces
(repositories, entity authorization, multi-tenancy, etc.). Code sketches are expected to change
as discussion proceeds.

## Alternatives Considered

Other shapes this could take, and why the proposal above is preferred - or why one of these
might win instead. Unlike an ADR's "Alternatives Considered" (written to explain a decision
already made), this section can still be actively contested.

## Open Questions

Specific unresolved points. Each should be resolved (moved into Detailed Design or Non-Goals)
before this RFC can move to **Accepted**.

## Non-Goals

What this proposal deliberately does not attempt to solve, to keep scope honest.

## References

- [Links to the roadmap entry, related RFCs/ADRs, issues, discussions]
```

## Lifecycle

1. **Draft** - written, not yet reviewed.
2. **Discussion** - open questions are being argued through; the design sketch changes as a
   result. Expect rewrites, not just additions.
3. **Accepted** or **Rejected**/**Withdrawn** - the design is settled enough to build (or it isn't
   going to happen, and the RFC stays as a record of why, the same way
   [ADR-002](../adr/002-simple-advanced-entities.md) stays as a record of a superseded decision).
4. **Implemented** - once the feature ships, write the corresponding ADR documenting the decision
   *as built* (implementation often surfaces details the RFC didn't anticipate - the ADR is the
   authoritative record, not a copy-paste of the RFC). Update this RFC's status to
   `Implemented - see ADR-XXX` and leave the file in place; it remains the historical record of
   alternatives considered and questions argued through, which the terser ADR format doesn't
   preserve as fully.

An RFC is never deleted once it reaches Accepted, Rejected, Withdrawn, or Implemented - only its
status line changes.

## Records

### Draft / Discussion

- [RFC-001: Notifications](001-notifications.md) - in-app + external delivery, the shared
  primitive Approvals and Workflows will call into
- [RFC-002: Attachments](002-attachments.md) - polymorphic file attachments with a pluggable
  storage backend
- [RFC-003: Approvals](003-approvals.md) - multi-approver sign-off with a configurable resolution
  strategy per request

### Accepted (not yet implemented)

*(none yet)*

### Implemented

*(none yet - once an RFC ships, it moves here with a link to the ADR that documents the as-built decision)*

### Rejected / Withdrawn

*(none yet)*

## Creating a New RFC

1. Copy the template above into `docs/architecture/rfc/NNN-title.md`, numbered sequentially in
   its own sequence (independent of ADR numbers - an RFC and the ADR it eventually produces will
   usually have different numbers).
2. Fill in Summary, Motivation, and a first-pass Detailed Design - it doesn't need to be right,
   it needs to be a starting point for Open Questions.
3. Set Status to `Draft`, then `Discussion` once it's open for comment.
4. Update this index.
5. When accepted and built, write the ADR, cross-link both documents, and move this entry to
   **Implemented** above.

## Decision Criteria

Reach for an RFC instead of going straight to an ADR when the feature does **any** of the following
(most candidate features do at least one - "this looks like a straightforward entity + repository"
is not, by itself, a reason to skip):

- Introduces a new extension point/interface consumers implement (another `IUserContext`-shaped
  abstraction), especially one with no safe empty/default state.
- Raises an authorization question that doesn't cleanly fit the existing
  `{EntityName}.{Action}` claim model ([ADR-011](../adr/011-entity-authorization.md)) - e.g.
  ownership-scoped or polymorphic-owner resources.
- Needs a service layer above `Repository<TEntity>` because the operation has a side effect
  (writing to storage, sending mail) that can't be a bare EF Core insert/update.
- Has a cascade/lifecycle interaction with soft delete or multi-tenancy that isn't automatic.
- Will become a dependency other roadmap features call into, where getting the contract right
  matters more than shipping fast.

Two RFCs in a row (Notifications, Attachments) turned out to hit several of these at once despite
initially looking like simple additions - treat "this one's obvious" as a prompt to check the list
above, not a conclusion.

---

**Want to propose a new RFC?** See [Contributing Guidelines](../../../CONTRIBUTING.md) for the process.
