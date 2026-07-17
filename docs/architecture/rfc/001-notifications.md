# RFC-001: Notifications

**Status:** Draft

**Date:** 2026-07-16

**Author:** JumpStart Core Team

## Summary

A framework primitive for telling a user something happened - persisted in-app (a "bell icon"
inbox) and optionally fanned out to external channels (email, SMS, push) - that Approvals and
Workflows can both call into instead of each inventing their own delivery mechanism.

## Motivation

Every roadmap feature past this one needs to alert someone: Approvals needs to tell an approver a
decision is waiting on them; Workflows needs to tell the next actor a record has entered their
queue. Building that delivery mechanism twice (once per feature) is exactly the kind of repeated
plumbing JumpStart exists to remove. It also stands alone as a feature applications want directly
("notify the manager when a low-stock threshold is hit") independent of Approvals or Workflows ever
shipping.

## Detailed Design

### The Notification Entity

```csharp
public class Notification : Entity, ICreatable, ITenantScopedOptional
{
    public Guid RecipientUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public DateTimeOffset? ReadOn { get; set; } // null = unread, same convention as IDeletable.DeletedOn
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
```

`ICreatable` rather than full `IAuditable` - a notification is never "modified," only created and
later marked read, and read-tracking gets its own field (`ReadOn`) rather than overloading
`ModifiedOn`, since "read" isn't really a content modification.

### Delivery Abstraction

```csharp
public interface INotificationChannel
{
    Task SendAsync(Guid userId, string message, string? linkUrl, CancellationToken ct = default);
}

public interface INotificationService
{
    Task NotifyAsync(Guid userId, string message, string? linkUrl = null, CancellationToken ct = default);
}
```

`INotificationService` is the single call site every feature (including application code) uses. Its
default implementation:

1. Always writes the `Notification` row (the in-app inbox is not optional - it's the fallback that
   guarantees the message is never lost even if every external channel fails).
2. Fans out to every registered `INotificationChannel` (email, SMS, push, ...), best-effort - a
   channel failure logs but does not roll back the persisted notification.

### Registration

Mirrors the existing `RegisterUserContext<T>()` / `RegisterTenantContext<T>()` pattern from
`AddJumpStart`:

```csharp
builder.Services.AddJumpStart(options =>
{
    options.RegisterNotificationChannel<EmailNotificationChannel>();
    options.RegisterNotificationChannel<SmsNotificationChannel>();
});
```

Zero registered channels is a valid configuration - the app gets in-app notifications only.

### Repository and API

```csharp
public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUnreadForUserAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
}
```

Notifications are **not** exposed through `ApiControllerBase` - unlike Products or Invoices, a user
should only ever see or modify their *own* notifications, which doesn't fit the
`{EntityName}.{Action}` permission-claim model from
[ADR-011](../adr/011-entity-authorization.md) (see Open Questions). A small dedicated
`NotificationsController` instead exposes:

- `GET /api/notifications` - the current user's notifications, paginated
- `POST /api/notifications/{id}/read` - mark one as read

## Alternatives Considered

### A central event bus / pub-sub system

Instead of a direct `INotificationService.NotifyAsync` call, features could raise a domain event
that a subscriber turns into a notification. **Rejected for v1** - JumpStart has no event system
today, and building one solely to support Notifications is scope creep the roadmap's Non-Goals
section already warns against. A direct service call is simpler and sufficient; an event bus can be
layered on top later without changing `INotificationService`'s contract.

### Mandatory tenant scoping

`Notification` could implement `ITenantScoped` (required) instead of `ITenantScopedOptional`.
**Rejected** - account-level or system-wide notices (e.g. "your trial ends in 3 days") need to
reach a user outside any single tenant's data, which a mandatory tenant filter would block.

### Real-time push as a core concern

Delivering to an open Blazor circuit via SignalR the moment a notification is created could be
built into `INotificationService` itself. **Rejected as a core design fork** - this is just another
`INotificationChannel` implementation (a `SignalRNotificationChannel`), not a reason to change the
service's shape.

## Open Questions

- **Authorization model mismatch.** Every other JumpStart resource is authorized via the
  `{EntityName}.{Action}` permission claim ([ADR-011](../adr/011-entity-authorization.md)).
  Notifications are inherently row-owner-scoped ("is this my notification?") rather than
  role-permission-scoped. Does `NotificationsController` need a new, narrower authorization
  primitive, or is "filter to `RecipientUserId == currentUserId`, no permission claim needed" an
  acceptable one-off exception? This needs to be resolved before Accepted, since it sets a
  precedent Approvals' "is this my approval request to decide?" question will also run into.
- **Message content: plain string or structured?** V1 sketch above uses a plain `string Message`.
  Should it instead be a template key + parameters (for localization, or for rendering differently
  per channel - e.g. HTML email vs. plain-text SMS)?
- **Batching/digest.** V1 sends immediately per channel per event. Is an hourly/daily digest
  (rather than one email per notification) a v1 requirement or a later addition?
- **Failure visibility.** If every `INotificationChannel` fails, the in-app row still exists - is
  silently logging channel failures sufficient, or does the framework need a retry/dead-letter
  story for external delivery?

## Non-Goals

Scheduled/recurring notifications, push notification device-token registration and management, and
read-receipt analytics/reporting are explicitly out of scope for this RFC.

## References

- [Roadmap: Notifications](../../roadmap.md#2-notifications)
- [ADR-011: Entity-Level Authorization](../adr/011-entity-authorization.md) - the claim model this
  RFC's Open Questions section proposes a possible exception to
- [ADR-010: Multi-Tenant Data Isolation](../adr/010-multi-tenant-data-isolation.md) - the
  `ITenantScopedOptional` pattern this design reuses
