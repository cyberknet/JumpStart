# RFC-003: Approvals

**Status:** Draft

**Date:** 2026-07-16

**Author:** JumpStart Core Team

## Summary

A generic "this record needs sign-off before it's final" primitive: any entity can request
approval from one or more designated approvers - selected explicitly, by group/role, or by
management hierarchy - and the outcome (Approved/Rejected) at each level is computed using one of
five configurable resolution methods. Approvals may run as a single flat vote or as an ordered
sequence of levels, each with its own approver selection and resolution rule.

## Motivation

The original [roadmap](../../roadmap.md#3-approvals) sketch assumed a single decider - one
`DecidedById`/`DecidedOn` pair. Real approval processes routinely need more than one eligible
approver (a purchase order might need any manager to sign off; a policy change might need every
member of a committee to agree), and different records - even within the same application - may
need different rules for turning a set of individual responses into a single outcome. Hard-coding
one resolution rule (e.g. "first response wins") would make the feature unusable for the other four
common cases; hard-coding the choice application-wide would prevent an app from using, say,
`Unanimous` for compliance sign-off and `Majority` for routine expense approval side by side.

Two further requirements push on this design:

1. **Applications need generic ways to select approvers, not just a caller-supplied list.**
   Common cases are group/role-based ("anyone in Finance Managers") and hierarchical ("the
   requester's manager"). JumpStart has no concept of a manager relationship today, so hierarchical
   selection is a new dependency this RFC has to surface honestly, not quietly assume.
2. **Some approvals need multiple levels** - a manager approves, then their manager approves - which
   is exactly the "sequential chains" case this RFC's first draft deferred to Workflows outright.
   That deferral doesn't survive contact with hierarchical approval, which is *inherently* sequential
   (you don't ask a chain of managers to vote simultaneously in one flat pool). Rather than stretch
   Workflows' arbitrary-state-graph machinery to cover it, this revision brings a deliberately narrow
   version of staging into Approvals itself: a **fixed, linear** sequence of levels, each of which is
   still just a `Pending → Approved/Rejected` vote using the machinery below. See
   [Multi-Level Approvals](#multi-level-approvals) for exactly where the line is drawn against
   Workflows' arbitrary branching/looping.

As flagged in the roadmap: this is, underneath, a two-state workflow (`Pending` →
`Approved`/`Rejected`) - now potentially repeated level by level, but never branching.

## Detailed Design

### Entities

The single-decider `ApprovalRequest` from the original draft is now the outer record spanning one or
more **stages** - `ApprovalStage` is new; `ApprovalParticipant` and `ApprovalResponse` now belong to
a stage rather than the request directly, since each stage runs its own vote against its own
participant pool:

```csharp
public enum ApprovalStatus { NotStarted, Pending, Approved, Rejected, Cancelled }
public enum ApprovalDecision { Approve, Reject }
public enum ApprovalResolutionStrategy { FirstResponse, Majority, Supermajority, Percentage, Unanimous }

public class ApprovalRequest : AuditableEntity, ITenantScopedOptional
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending; // overall outcome across all stages
    public DateTimeOffset? ResolvedOn { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}

public class ApprovalStage : Entity
{
    public Guid ApprovalRequestId { get; set; }
    public ApprovalRequest ApprovalRequest { get; set; } = null!;
    public int SequenceNumber { get; set; } // 1, 2, 3... stages resolve strictly in order
    public ApprovalStatus Status { get; set; } = ApprovalStatus.NotStarted; // this stage's own outcome
    public ApprovalResolutionStrategy ResolutionStrategy { get; set; }
    public double? RequiredPercentage { get; set; } // only used (and required) when ResolutionStrategy == Percentage
    public DateTimeOffset? ResolvedOn { get; set; }
}

public class ApprovalParticipant : Entity
{
    public Guid ApprovalStageId { get; set; }
    public ApprovalStage ApprovalStage { get; set; } = null!;
    public Guid UserId { get; set; }
}

public class ApprovalResponse : Entity, ICreatable
{
    public Guid ApprovalStageId { get; set; }
    public ApprovalStage ApprovalStage { get; set; } = null!;
    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset CreatedOn { get; set; } // when they responded
    public Guid? CreatedById { get; set; }        // who responded - no separate ResponderId needed
}
```

The single-level case this RFC originally described is simply "one stage" - `IApprovalService`
below still lets the common case be created without the caller ever naming a stage explicitly.

**Why a fixed participant snapshot per stage, not a live permission-based pool:** `Majority`,
`Supermajority`, `Percentage`, and `Unanimous` all need a stable denominator (how many people get a
say) to compute against. If eligibility were instead "anyone currently holding the
`{OwnerType}.Approve` claim," that pool could grow or shrink mid-vote (a role assignment changes)
and "unanimous" would become a moving target. `ApprovalParticipant` rows for a stage are written
once, when that stage *activates* - not necessarily all at once when the overall request is created,
since a later stage's participants may not be knowable yet (see
[Approver Selection](#approver-selection) - a hierarchical stage 2 needs to know who stage 1's
approver was before it can find *their* manager).

### Resolution: one parameterized evaluator, not five

Resolution is evaluated per-stage, against that stage's own participant pool. `FirstResponse` is
genuinely different in kind (it resolves on response *order*, not a count) and stays its own case.
The other four all reduce to the same question - "has one side reached a required share of this
stage's participant pool?" - with different default thresholds:

| Strategy | Required share (of participant count `N`) |
|---|---|
| Majority | strictly more than half: `floor(N/2) + 1` |
| Supermajority | a fixed, opinionated default of two-thirds: `ceil(N * 2/3)` (not independently configurable - use `Percentage` if two-thirds isn't the right bar) |
| Percentage | `ceil(N * RequiredPercentage)` - fully custom, set on the stage |
| Unanimous | `N` (100%) |

```csharp
public interface IApprovalResolutionEvaluator
{
    ApprovalResolutionStrategy Strategy { get; }
    ApprovalEvaluationResult Evaluate(int participantCount, int approveCount, int rejectCount, double? requiredPercentage);
}

public enum ApprovalEvaluationResult { Pending, Approved, Rejected }
```

The threshold evaluator resolves **early**, without waiting for every participant to respond,
whenever the outcome is already mathematically guaranteed:

- **Approved** once `approveCount >= required`.
- **Rejected** once `approveCount + (N - approveCount - rejectCount) < required` - i.e. even if
  every still-pending participant went on to approve, the bar can no longer be reached.

This single rule elegantly covers `Unanimous` too: with `required == N`, a *single* rejection makes
the remaining approve-eligible total `< N`, so one "no" immediately resolves the stage - no
special-cased early-reject logic needed for that strategy specifically.

`FirstResponse` is a separate, trivial evaluator: the first `ApprovalResponse` recorded resolves the
stage to match that response's `Decision`, regardless of `N`.

### Approver Selection

Rather than every caller building a raw `List<Guid>` of participants, approver selection is its own
extension point - the same "framework defines the contract, application/JumpStart supplies
implementations" shape as `IUserContext`/`ITenantContext`/`IAttachmentStorage`:

```csharp
public interface IApprovalParticipantSelector
{
    Task<IReadOnlyCollection<Guid>> ResolveAsync(ApprovalStageContext context, CancellationToken ct = default);
}

public class ApprovalStageContext
{
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public Guid ApprovalRequestId { get; init; }
    public int SequenceNumber { get; init; }
    public IReadOnlyList<ApprovalResponse> PriorStageResponses { get; init; } = Array.Empty<ApprovalResponse>();
}
```

`ResolveAsync` runs exactly once, when a stage activates, to produce that stage's `ApprovalParticipant`
snapshot - selection is pluggable, but the "resolve once, don't drift" guarantee from the previous
section is unchanged regardless of which selector produced the list. Three built-in selectors:

- **`ExplicitApprovalParticipantSelector`** - constructed with a fixed user-ID list; ignores
  `context`. This is what a caller uses today when they already know exactly who should approve.
- **`RoleApprovalParticipantSelector`** - resolves to every `UserId` currently holding a given
  `Role`, reusing the existing `Role`/`UserRole` infrastructure from
  [ADR-012](../adr/012-role-based-permission-management.md) rather than inventing a parallel "Group"
  concept JumpStart doesn't otherwise have. Optionally tenant-scoped, following the same
  `ITenantScopedOptional` role pattern ADR-012 already defines.
- **`HierarchicalApprovalParticipantSelector`** - **depends on a capability JumpStart does not have
  today.** There is no manager/reporting-line concept anywhere in the framework. Rather than JumpStart
  owning an organizational-hierarchy data model (a large scope expansion, and arguably its own future
  roadmap feature independent of Approvals), this selector depends on a new, minimal,
  consumer-implemented contract:

  ```csharp
  public interface IManagerHierarchyProvider
  {
      Task<Guid?> GetManagerIdAsync(Guid userId, CancellationToken ct = default);
  }
  ```

  For stage 1, the selector resolves the manager of the record's requester; for stage *N > 1*, it
  resolves the manager of whoever responded at stage *N - 1* (via `context.PriorStageResponses`) -
  which is exactly why participant resolution had to move from "all at request creation" to
  "per-stage, at activation." A `null` return (no manager - top of the chain) needs a defined
  behavior; see Open Questions.

Selector and resolution strategy are independent, orthogonal choices: a hierarchical stage typically
resolves to a single participant (`N = 1`, where every strategy trivially means "wait for this one
person"), while a role-based stage typically resolves to several and is where `Majority`/`Percentage`/
`Unanimous` actually do their intended work.

### Multi-Level Approvals

An `ApprovalRequest` may have more than one `ApprovalStage`, in a **fixed, strictly linear order** -
this is the deliberate boundary against Workflows:

- Stage 1 activates immediately when the request is created: its selector resolves, participants are
  snapshotted, and they're notified.
- Stage *N > 1* stays `NotStarted` until stage *N - 1* resolves `Approved` - only then does its
  selector run and its participants get notified.
- A `Rejected` outcome at **any** stage immediately sets the whole `ApprovalRequest.Status` to
  `Rejected` and no further stages activate.
- The overall `ApprovalRequest.Status` becomes `Approved` only once the final stage resolves
  `Approved`.

This is meaningfully narrower than generalized Workflows: there is no branching, no conditional
routing based on record data, and no looping back to an earlier stage - just a fixed ordered list
with two exits (an early reject, or completing every stage). Anything needing to choose *which*
stage comes next based on data, or revisit an earlier stage, is still out of scope and still the
Workflows feature's problem to solve, not this one's.

### Service Layer

```csharp
public class ApprovalStageDefinition
{
    public IApprovalParticipantSelector Selector { get; init; } = null!;
    public ApprovalResolutionStrategy Strategy { get; init; }
    public double? RequiredPercentage { get; init; }
}

public interface IApprovalService
{
    Task<ApprovalRequest> RequestApprovalAsync(string ownerType, Guid ownerId,
        IReadOnlyList<ApprovalStageDefinition> stages, CancellationToken ct = default);

    Task<ApprovalRequest> RespondAsync(Guid approvalRequestId, ApprovalDecision decision,
        string? comments = null, CancellationToken ct = default);

    Task<ApprovalRequest> CancelAsync(Guid approvalRequestId, CancellationToken ct = default);
}
```

A single-level approval is simply a one-element `stages` list; a convenience overload taking a bare
`IReadOnlyCollection<Guid>` + strategy (wrapping it in one `ExplicitApprovalParticipantSelector`
stage) keeps the common case from ever having to touch `ApprovalStageDefinition` directly.
`RequestApprovalAsync` persists the `ApprovalRequest` and all `ApprovalStage` rows up front (only
stage 1's participants/notifications happen immediately - later stages' selectors run at
activation). `RespondAsync` deliberately still takes the *request* ID, not a stage ID - a linear
sequence only ever has one active stage at a time, so the service resolves internally which stage is
currently `Pending`, validates the caller is one of its participants, persists the `ApprovalResponse`,
re-runs that stage's `IApprovalResolutionEvaluator`, and - if the stage resolved - either activates
the next stage or finalizes the overall request.

### Repository

```csharp
public interface IApprovalRepository : IRepository<ApprovalRequest>
{
    Task<ApprovalRequest?> GetCurrentForOwnerAsync(string ownerType, Guid ownerId);
    Task<IEnumerable<ApprovalRequest>> GetForOwnerAsync(string ownerType, Guid ownerId);
    Task<ApprovalStage?> GetActiveStageAsync(Guid approvalRequestId);
}
```

### API Surface

```
POST   /api/approvals/{ownerType}/{ownerId}      - create a request (one or more stage definitions)
GET    /api/approvals/{ownerType}/{ownerId}       - approval history for an owner
GET    /api/approvals/{id}                        - a request + its stages + participants + responses
POST   /api/approvals/{id}/respond                - cast a response (caller must be a participant of the active stage)
POST   /api/approvals/{id}/cancel                 - cancel a still-pending request
```

Each stage definition in the `POST` body needs a selector *type* plus that selector's own
configuration (a role ID, a hierarchy depth, an explicit user list) - a polymorphic/discriminated
shape the DTO design hasn't been worked out yet; see Open Questions.

## Alternatives Considered

### Five independent evaluator implementations, no shared threshold math

Simpler to reason about in isolation, and doesn't assume the four percentage-shaped strategies stay
conceptually aligned forever. **Not adopted as the primary design** - the unification above removes
duplicated boundary-condition logic (four copies of "count responses, compare to N") and the early-
resolution behavior falls out for free rather than being reimplemented four times. Worth revisiting
if a future strategy doesn't fit the "share of N" shape at all.

### Live, permission-based approver pool instead of a captured snapshot

Rejected - see "Why a fixed participant snapshot" above. A moving denominator breaks the majority/
percentage/unanimous math's correctness guarantee.

### Single approver (the original roadmap sketch)

Superseded by this RFC per explicit requirement - multiple eligible approvers per request is now a
first-class case, with single-approver simply being the `N = 1` case of any strategy.

### Flat, single-stage-only design (this RFC's own first draft)

Simpler - no `ApprovalStage` entity, no activation-ordering logic. **Superseded** once hierarchical
approver selection was added to scope: a manager chain is inherently sequential (you don't ask a
whole reporting line to vote at once), so "selectable approvers" and "multi-level" turned out not to
be independent requirements - supporting one properly required supporting the other.

### JumpStart owns an organizational-hierarchy data model

Instead of the consumer-implemented `IManagerHierarchyProvider` contract, JumpStart could ship its
own `Employee`/`ManagerId` entity and query it directly. **Rejected** - this is a much larger surface
(org charts, effective-dated reporting changes, cross-tenant reporting structures) that deserves its
own roadmap entry if it's ever justified by more than one consuming feature, not something to fold
into Approvals as a side effect. The provider interface keeps Approvals decoupled from however (or
whether) an application already models its org chart.

### Unbounded/dynamic hierarchical escalation instead of a fixed stage list

Rather than the caller declaring "3 levels" up front, a hierarchical approval could keep climbing the
management chain automatically until someone approves or the chain runs out (`GetManagerIdAsync`
returns `null`). **Not adopted as the only option** - open-ended stage counts don't fit cleanly into
`ApprovalStage.SequenceNumber` being fixed at creation. Left as an open question rather than settled,
since it may still be the more natural mental model for pure hierarchical escalation specifically.

## Open Questions

- **Authorization - the same fork RFC-001 and RFC-002 already raised, now a third time.** Who may
  *create* a request (and choose its participants/strategy) and who may *cancel* one still needs an
  answer consistent with whatever RFC-001/002 settle on for owned-sub-resource permissions
  ([ADR-011](../adr/011-entity-authorization.md)). *Responding* is comparatively simple - eligibility
  is just "is this user an `ApprovalParticipant` on this specific request" - but request creation
  and cancellation are not.
- **Can a response be revised?** Once cast, is `ApprovalResponse` immutable, or can a participant
  change their vote before the request resolves? Immutable is simpler but may not match real
  approval processes where someone changes their mind after seeing others' comments.
- **Responses after resolution.** If a request has already resolved (bar cleared, or mathematically
  rejected) and a straggler still tries to respond, is that a `409 Conflict`, or accepted and simply
  ignored by evaluation (kept for the record, but doesn't reopen the decision)?
- **Non-responsive or deactivated participants.** The snapshot is fixed at creation, but a
  participant who never responds (or is deactivated mid-flight) can stall `Unanimous` or a high
  `Percentage` threshold indefinitely. Does v1 need a way to remove/replace a participant on a
  still-`Pending` request, or a deadline/expiration/escalation - or is that explicitly deferred (see
  Non-Goals) until real usage shows it's needed?
- **Who gets notified of the final outcome?** The requester/owner-record stakeholders, all
  participants (even the ones who already voted), or something not yet designed (a "watchers" list)?
- **Denormalized status on the owner entity.** The original roadmap sketch had an `IApprovable`
  marker exposing a cached `ApprovalStatus` directly on the owning entity, for cheap filtering
  ("show me all Pending purchase orders") without a join. This RFC's design treats `ApprovalRequest`
  as the sole source of truth and would require `IApprovalRepository.GetCurrentForOwnerAsync` for
  the same lookup. Is the query performance of a denormalized field worth the risk of it drifting
  out of sync with the authoritative request row?
- **Is the five-strategy menu closed?** Are `FirstResponse`/`Majority`/`Supermajority`/`Percentage`/
  `Unanimous` the complete, JumpStart-defined set (an enum, not extensible), or should
  `IApprovalResolutionEvaluator` be a registered extension point for a consumer-supplied sixth
  strategy (e.g. weighted votes)?
- **Is two-thirds the right opinionated default for `Supermajority`?** Some domains use
  three-quarters. If this needs to vary per-app, `Supermajority` may need to collapse into
  `Percentage` with a documented default rather than existing as its own enum value.
- **Fixed vs. dynamic stage count for hierarchical chains.** Does the caller declare the number of
  levels up front (e.g. "3 levels of hierarchical approval"), or does a hierarchical approval keep
  escalating until `IManagerHierarchyProvider.GetManagerIdAsync` returns `null` (top of the chain)?
  The fixed-`SequenceNumber` design above assumes the former; see the corresponding Alternative.
- **What happens when `GetManagerIdAsync` returns `null` mid-chain?** If a declared stage 3 needs a
  manager but stage 2's approver has none, does that stage auto-resolve (approved by default?
  rejected? skipped?), or is this a hard configuration error surfaced at request-creation time?
- **What happens when a role-based selector resolves to zero users?** An empty `Role` membership at
  the moment a stage activates leaves a stage with no possible participants - auto-skip to the next
  stage, auto-reject the whole request, or hold the stage open until someone is added to the role?
- **Should `IApprovalParticipantSelector` be a closed set (Explicit/Role/Hierarchical, JumpStart-
  defined) or a fully open extension point** applications can implement their own selectors against
  (e.g. "the customer's assigned account manager")? The interface shape above already permits this;
  the open question is whether JumpStart documents/supports arbitrary custom selectors in v1 or only
  ships the three built-ins.
- **DTO shape for stage definitions over the API.** `ApprovalStageDefinition.Selector` is an
  in-process interface instance; the `POST /api/approvals/...` body needs a serializable,
  discriminated equivalent (selector type + that type's own configuration payload) that hasn't been
  designed yet.

## Non-Goals

Weighted votes (some approvers counting for more than one "share"), delegation/proxy voting, and
deadlines/auto-escalation timers are still out of scope. So is anything requiring **conditional or
branching** stage sequencing - choosing which stage comes next based on the record's data, skipping
levels dynamically, or revisiting an earlier stage. `ApprovalStage`'s ordering is fixed and linear
precisely so that this line against Workflows stays clear: multi-level, fixed-order approval chains
(including hierarchical ones) are now in scope for this RFC; anything that needs to branch or loop
is still Workflows' problem to solve.

## References

- [Roadmap: Approvals](../../roadmap.md#3-approvals)
- [RFC-001: Notifications](001-notifications.md) - `INotificationService` fan-out to participants,
  and the first appearance of the owned-sub-resource authorization question
- [RFC-002: Attachments](002-attachments.md) - the polymorphic `OwnerType`/`OwnerId` pattern reused
  here, and the second appearance of the same authorization question
- [ADR-011: Entity-Level Authorization](../adr/011-entity-authorization.md)
- [ADR-012: Role-Based Permission Management](../adr/012-role-based-permission-management.md) -
  relevant to how an application decides who's eligible to be named a participant in the first place
