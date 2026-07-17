# RFC-002: Attachments

**Status:** Draft

**Date:** 2026-07-16

**Author:** JumpStart Core Team

## Summary

A generic way to attach files to any entity - `OwnerType`/`OwnerId` metadata rows plus a pluggable
storage backend - without every application reinventing upload/download plumbing per entity type.

## Motivation

Most LOB entities (invoices, orders, support tickets, employee records) need files attached to
them, and every JumpStart application currently has to build this from scratch: a metadata table,
a storage strategy, upload/download endpoints, and permission checks, repeated per entity type that
needs it.

## Detailed Design

### The Attachment Entity

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
```

`OwnerType` + `OwnerId` rather than a real foreign key per entity type: EF Core can't model a
polymorphic FK cleanly, and generating a join table per consuming entity would defeat the point of
"works for any entity automatically." This accepts the same simple-over-flexible tradeoff
[ADR-009](../adr/009-guid-only-entities.md) already made for entity keys generally. `AuditableEntity`
(not just `Entity`) so a deleted attachment leaves a `DeletedById`/`DeletedOn` trail even after its
blob is purged.

### Storage Abstraction

```csharp
public interface IAttachmentStorage
{
    Task<string> SaveAsync(Guid attachmentId, Stream content, CancellationToken ct = default);
    Task<Stream> OpenAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}
```

Mirrors the `IUserContext`/`ITenantContext` extension-point pattern - the framework defines the
contract, applications (or optional JumpStart-provided packages) supply `LocalDiskAttachmentStorage`,
`AzureBlobAttachmentStorage`, etc. Unlike [RFC-001](001-notifications.md)'s notification channels,
there's no safe "zero registered" state - an upload with no `IAttachmentStorage` registered must
fail fast at startup (see Open Questions on whether to ship a default).

### Upload/Download Composition

A plain `Repository<Attachment>` can't express "write bytes, then persist metadata" as one
operation, so this needs a service layer above the repository:

```csharp
public interface IAttachmentService
{
    Task<Attachment> UploadAsync(string ownerType, Guid ownerId, string fileName,
        string contentType, Stream content, CancellationToken ct = default);
    Task<(Stream Content, Attachment Metadata)> DownloadAsync(Guid attachmentId,
        CancellationToken ct = default);
    Task DeleteAsync(Guid attachmentId, CancellationToken ct = default);
}
```

`UploadAsync` calls `IAttachmentStorage.SaveAsync` to get a `StorageKey`, then
`IAttachmentRepository.AddAsync` to persist the metadata row (populating `TenantId` from the
current tenant context the same way `Repository<TEntity>.AddAsync` already does for any
`ITenantScoped`/`ITenantScopedOptional` entity). `DeleteAsync` removes the storage blob and soft-
deletes the metadata row together, so the two can't drift out of sync.

### Repository

```csharp
public interface IAttachmentRepository : IRepository<Attachment>
{
    Task<IEnumerable<Attachment>> GetForOwnerAsync(string ownerType, Guid ownerId);
}
```

### API Surface

A single generic controller rather than per-entity generated actions, since attachments are
owner-agnostic by design:

- `POST /api/attachments/{ownerType}/{ownerId}` - multipart upload
- `GET /api/attachments/{ownerType}/{ownerId}` - list for an owner
- `GET /api/attachments/{id}/content` - download
- `DELETE /api/attachments/{id}`

## Alternatives Considered

### Store file bytes in the database (varbinary/bytea column)

No separate storage abstraction needed. **Rejected as the only option** - doesn't scale, bloats the
primary database, forecloses CDN/blob-tier storage. Could still exist as a trivial
`DatabaseBlobAttachmentStorage : IAttachmentStorage` for prototyping, without making it the only
choice.

### Real per-entity foreign key instead of OwnerType/OwnerId

Generate a join table (or a nullable FK column) per consuming entity via assembly scanning, giving
referential integrity EF Core can enforce and cascade automatically. **Rejected** - requires
per-entity-type configuration the polymorphic design avoids entirely, reintroducing exactly the
"repeat this per entity type" cost the feature exists to remove.

### Authorization piggybacks entirely on the owner entity's own claim

Skip a separate `Attachments` claim; if the caller has `Invoice.Update`, they can also
upload/delete Invoice attachments; `Invoice.Get` implies download. **Not rejected - listed as an
open question below**, since it's the simplest option but may be too coarse for apps that want "can
view the record but not download its attached documents" (e.g., sensitive HR files).

## Open Questions

- **Authorization model - same shape problem as [RFC-001](001-notifications.md).** Attachment has
  no entity name of its own to hang a `{EntityName}.{Action}` claim on
  ([ADR-011](../adr/011-entity-authorization.md)). Candidates: a compound claim like
  `Invoice.Attachments.Upload`; inheriting the owner's own CRUD claim (see Alternatives above); or a
  single entity-agnostic `Attachment.Upload`/`Attachment.Download` claim (simplest to declare,
  coarsest-grained - can't restrict "attach to Invoices but not Orders"). Given RFC-001 hits the
  same "owned sub-resource" authorization question independently, this may be worth resolving once,
  as a shared convention, rather than twice.
- **Cascade/orphan behavior.** Because `OwnerType`/`OwnerId` isn't a real FK, EF Core cannot
  cascade-delete `Attachment` rows when the owner is soft- or hard-deleted. Does deleting an owner
  need to explicitly delete its attachments first (application responsibility), does the framework
  provide a hook, or does an orphan stay queryable (and its blob retained) indefinitely?
- **Default storage implementation.** Ship a `LocalDiskAttachmentStorage` default so a new project
  works out of the box, or require every consumer to register one from day one (consistent with
  "no unprotected default" precedent set by mandatory entity authorization)?
- **Size/quota limits.** Framework-enforced max file size / per-tenant storage quota, or left
  entirely to the consumer's `IAttachmentStorage` implementation?
- **Versioning.** Does re-uploading to the same owner replace the existing `Attachment` row, or
  does every upload create a new immutable row (with the old one left in place, soft-deleted or
  not)?

## Non-Goals

Virus scanning implementation (a hook for one may be worth adding, but scanning itself is out of
scope), CDN integration, image thumbnailing/transcoding, and resumable/chunked client upload
protocols are explicitly out of scope for this RFC.

## References

- [Roadmap: Attachments](../../roadmap.md#1-attachments)
- [RFC-001: Notifications](001-notifications.md) - the sibling "owned sub-resource" authorization
  question
- [ADR-009: Guid-Only Entities](../adr/009-guid-only-entities.md) - precedent for accepting a
  simpler discriminator-based shape over per-type generality
- [ADR-011: Entity-Level Authorization](../adr/011-entity-authorization.md) - the claim format this
  RFC's Open Questions section needs to reconcile with a polymorphic owner
