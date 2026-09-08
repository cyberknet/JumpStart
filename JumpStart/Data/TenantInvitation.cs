// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel.DataAnnotations.Schema;
using JumpStart.Data.Auditing;
using JumpStart.Data.MultiTenant;

namespace JumpStart.Data;

/// <summary>
/// A standing offer for one person to join one tenant.
/// </summary>
/// <remarks>
/// <para>
/// The membership equivalent of a pending <see cref="UserTenant"/>: it names somebody who is not a
/// member yet, and the terms on which they would become one. It exists because there is otherwise no
/// way for a tenant to grow - membership can only be created by somebody who is already inside, and
/// the person being added may not have an account at all yet.
/// </para>
/// <para>
/// <strong>Addressed to an email, not to a user.</strong> Identity is not JumpStart's to own, and in
/// a split deployment it may live in an entirely different database - so an invitation names the
/// recipient the only way it reliably can, and is bound to a real user id only at the moment it is
/// redeemed.
/// </para>
/// <para>
/// <strong>The token is a secret, and the email is the second factor.</strong> Anyone holding the
/// token can attempt redemption, which is what makes a link in an email work at all; binding the
/// redemption to the invited address as well means a forwarded or leaked link still cannot put a
/// stranger inside the tenant. See <c>ITenantInvitationService.RedeemAsync</c>.
/// </para>
/// <para>
/// Nothing here is deleted on use. An accepted, revoked or expired invitation stays as the record of
/// who let whom in, and when - which is the first question asked after an account is compromised.
/// </para>
/// </remarks>
[Table("TenantInvitation")]
public class TenantInvitation : AuditableEntity, ITenantScoped
{
    /// <summary>The tenant being joined.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Navigation to <see cref="TenantId"/>.</summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// The address this was sent to, lower-cased.
    /// </summary>
    /// <remarks>
    /// Stored already normalised rather than compared case-insensitively at every call site: one of
    /// those call sites eventually forgets, and the failure is silent - an invitation that simply
    /// never matches.
    /// </remarks>
    public string Email { get; set; } = string.Empty;

    /// <summary>The unguessable secret that appears in the invitation link.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// A role to grant when this is redeemed, or <c>null</c> to add them with no role.
    /// </summary>
    /// <remarks>
    /// Whether the inviter was allowed to hand out this role is decided when the invitation is
    /// issued, because that is when the inviter is present to be checked. See
    /// <c>ITenantInvitationService.RedeemAsync</c> for what is re-checked at the other end.
    /// </remarks>
    public Guid? RoleId { get; set; }

    /// <summary>When this stops being redeemable.</summary>
    public DateTimeOffset ExpiresOn { get; set; }

    /// <summary>When it was redeemed, or <c>null</c> while it is still outstanding.</summary>
    public DateTimeOffset? AcceptedOn { get; set; }

    /// <summary>Who redeemed it - the answer to "how did this account get in?".</summary>
    public Guid? AcceptedByUserId { get; set; }

    /// <summary>When it was withdrawn, or <c>null</c>.</summary>
    public DateTimeOffset? RevokedOn { get; set; }

    /// <summary>Who withdrew it.</summary>
    public Guid? RevokedById { get; set; }

    /// <summary>Whether this can still be redeemed at <paramref name="now"/>.</summary>
    public bool IsPendingAt(DateTimeOffset now) =>
        AcceptedOn is null && RevokedOn is null && ExpiresOn > now;
}
