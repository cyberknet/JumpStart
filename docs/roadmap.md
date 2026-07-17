# Roadmap

This page tracks candidate features JumpStart doesn't have yet, sketches what each could look like
in terms consistent with the framework's existing patterns, and proposes an implementation order.
It is a planning document, not a commitment - nothing here is designed until it moves through the
[RFC process](architecture/rfc/index.md) (for features with real open questions) or straight to an
[ADR](architecture/adr/index.md) (for features whose shape is already obvious), the same way
[multi-tenancy](architecture/adr/010-multi-tenant-data-isolation.md) and
[entity authorization](architecture/adr/011-entity-authorization.md) did before this process
existed. Once an RFC is accepted and the feature is actually built, its content gets rewritten into
an ADR documenting the decision as-built - the RFC isn't deleted, but the ADR becomes the
authoritative record; see the [RFC lifecycle](architecture/rfc/index.md#lifecycle) for details.

## Candidates

| Feature | Solves | Depends on |
|---|---|---|
| [Attachments](#1-attachments) | Files/documents attached to a business record | Nothing new |
| [Notifications](#2-notifications) | Telling a user something happened | Nothing new |
| [Approvals](#3-approvals) | "This record needs sign-off before it's final" | Notifications, Entity Authorization |
| [Workflows](#4-workflows) | Arbitrary multi-state processes | Approvals (see [ordering rationale](#proposed-order)) |

## 1. Attachments

> 📝 **[RFC-002](architecture/rfc/002-attachments.md) drafted** - see that document for the full
> design discussion; the sketch below is kept short since the RFC is now the source of truth.

**Problem:** most LOB entities (invoices, orders, support tickets) need files attached to them, and
every application currently reinvents this.

**Shape**, following the polymorphic-owner pattern rather than a generic FK (EF Core doesn't do
arbitrary polymorphic associations well, and JumpStart entities are Guid-keyed by
[ADR-009](architecture/adr/009-guid-only-entities.md), so `OwnerType` + `OwnerId` is enough to
resolve an attachment's parent without a new relationship per entity type):

```csharp
public class Attachment : AuditableEntity, ITenantScopedOptional
{
    public string OwnerType { get; set; } = string.Empty; // e.g. "Invoice"
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty; // opaque handle into IAttachmentStorage
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}

public interface IAttachmentStorage
{
    Task<string> SaveAsync(Guid attachmentId, Stream content, CancellationToken ct = default);
    Task<Stream> OpenAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}
```

`IAttachmentStorage` mirrors `IUserContext`/`ITenantContext` as the extension point - JumpStart
ships the contract and an `AttachmentRepository` that manages the metadata row, applications (or
optional JumpStart-provided packages) supply `LocalDiskAttachmentStorage`, `AzureBlobAttachmentStorage`,
etc. Permission checks reuse the existing claim format from
[entity authorization](entity-authorization.md) - e.g. `Invoice.Attachments.Upload`.

**Open questions:** should upload/download be generic endpoints (`/api/attachments/{ownerType}/{ownerId}`)
or generated per-controller like the rest of `ApiControllerBase`? Virus scanning hook?

## 2. Notifications

> 📝 **[RFC-001](architecture/rfc/001-notifications.md) drafted** - see that document for the full
> design discussion; the sketch below is kept short since the RFC is now the source of truth.

**Problem:** applications need to tell a user something happened (approval needed, record changed,
mentioned in a comment), and every application currently reinvents delivery + read tracking.

**Shape**, reusing the same nullable-timestamp convention `IDeletable` already uses for soft delete
(`null` = one state, non-null = the other):

```csharp
public class Notification : Entity, ICreatable, ITenantScopedOptional
{
    public Guid RecipientUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public DateTimeOffset? ReadOn { get; set; } // null = unread
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}

public interface INotificationChannel
{
    Task SendAsync(Guid userId, string message, CancellationToken ct = default);
}

public interface INotificationService
{
    Task NotifyAsync(Guid userId, string message, string? linkUrl = null, CancellationToken ct = default);
}
```

`INotificationService` always writes the in-app `Notification` row (for a "bell icon" inbox), then
fans out to whichever `INotificationChannel` implementations are registered (email, SMS, push).
This is the piece Approvals and Workflows both call into, rather than each inventing its own "tell
someone" mechanism.

**Open questions:** templating for message content; batching/digest delivery vs. immediate.

## 3. Approvals

> 📝 **[RFC-003](architecture/rfc/003-approvals.md) drafted** - supersedes the single-approver
> sketch originally here; see that document for the full design. Summary below is kept short since
> the RFC is now the source of truth.

**Problem:** the single most common LOB workflow - "this record isn't final until someone (or a
group of people, by some agreed rule) signs off on it" - purchase orders, expense reports, leave
requests, published content.

**Shape:** an `ApprovalRequest` (`OwnerType`/`OwnerId`, overall status) made up of one or more ordered
`ApprovalStage`s, each with its own `ApprovalParticipant` snapshot, `ApprovalResponse` rows, and
resolution strategy. Participants are resolved per stage by a pluggable `IApprovalParticipantSelector`
- explicit list, role/group-based, or hierarchical (the last depending on a new
`IManagerHierarchyProvider` contract, since JumpStart has no manager-relationship concept today).
The outcome of each stage is computed by an `IApprovalResolutionEvaluator` chosen from five methods -
First Response, Majority, Supermajority, Percentage, Unanimous - four of which reduce to one shared
"share of N" threshold formula. Creating a request calls `INotificationService.NotifyAsync`
([Notifications](#2-notifications)) once per participant of the currently-active stage.

**Deliberately deferred:** delegation/proxy voting, weighted votes, and deadlines/auto-escalation.
Multi-level approval chains (including hierarchical ones) are now in scope, but only as a **fixed,
linear** sequence of stages - no branching, no conditional routing, no revisiting an earlier stage.
That narrower boundary is what still motivates generalizing into Workflows below, rather than this
RFC growing into a bespoke arbitrary state machine.

## 4. Workflows

**Problem:** processes with more than two outcomes and more than one transition, where the next
state can depend on the record's data or loop back - a support ticket moving through Open → In
Progress → Resolved → Closed (or bouncing back to In Progress if reopened), each with different valid
next-states and different people allowed to make each transition.

**Shape (sketch only - highest uncertainty of the four):** a `WorkflowDefinition` (named states +
allowed transitions between them), a `WorkflowInstance` (current state, `OwnerType`/`OwnerId`), and
a `WorkflowTransition` history row per state change - which is really just Attachments' polymorphic
owner pattern, Notifications' "tell someone" hook, and audit tracking's "who did what when" all
composed together.

**Why this is last, not designed yet:** Approvals *is* a two-state workflow, and per
[RFC-003](architecture/rfc/003-approvals.md) can now even be a fixed *sequence* of them - but always
linear, never branching or conditional. Building the fully general engine first risks guessing at
requirements no real application has hit yet - exactly the kind of speculative flexibility JumpStart
avoids elsewhere (see [ADR-009](architecture/adr/009-guid-only-entities.md), where a similar dual-mode
design was tried and superseded in favor of one opinionated shape). Better to ship Approvals, see
whether applications actually need branching/conditional/looping transitions beyond a fixed line of
stages, and then decide whether Workflows *generalizes* `ApprovalRequest`/`ApprovalStage` (an
approval becomes a linear-only special case of a workflow instance) or stays a separate system - the
same kind of unify-after-evidence call ADR-009 made for entities.

## Proposed order

1. **Attachments** - no dependency on anything else here, so it can go first regardless of how its
   design shakes out. It also validates the polymorphic `OwnerType`/`OwnerId` pattern the other
   three all reuse, and - per [RFC-002](architecture/rfc/002-attachments.md) - surfaces the same
   "owned sub-resource" authorization question Notifications hits, which is worth settling once
   rather than twice.
2. **Notifications** - also has no dependency on the other three, but is a prerequisite *for* them
   (Approvals needs to alert an approver). Sequencing it second, not third, means Approvals doesn't
   have to stub out notification behavior and redo it later.
3. **Approvals** - depends on Notifications (alerting) and the existing Entity Authorization /
   Role-Based Permission Management ADRs (who may decide). Ships the narrow, high-value case.
4. **Workflows** - explicitly deferred until Approvals is in real use. Attempting it earlier risks
   building a generic engine against guessed requirements instead of observed ones.

Attachments and Notifications don't depend on each other and could in principle be built in
parallel or in the opposite order. Attachments is suggested first mainly as a tie-breaker - it has
no consumers waiting on it, whereas Notifications is a prerequisite for Approvals, so getting
Notifications' design right benefits from not being rushed alongside a second new feature at the
same time.

## Non-goals (for now)

To keep this list honest about scope, explicitly **not** on this roadmap yet: full BPMN-style
workflow modeling, scheduled/recurring notifications, virus scanning or antivirus integration for
attachments, and any UI component library for these features (JumpStart's existing Components
folder aside). These may become their own roadmap entries later, but adding them now would be
scope creep ahead of evidence.
