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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Data;

namespace JumpStart.MultiTenant.Services;

/// <summary>Why a redemption did or did not put somebody into a tenant.</summary>
/// <remarks>
/// An enum rather than an exception because none of these are exceptional - a link that was used
/// yesterday, or has expired, or was withdrawn, is an ordinary thing for a person to click, and each
/// one deserves a different sentence on the screen. Only <see cref="Accepted"/> and
/// <see cref="AlreadyMember"/> mean the caller is now inside.
/// </remarks>
public enum InvitationRedemption
{
    /// <summary>No invitation carries this token.</summary>
    NotFound,

    /// <summary>Past its expiry.</summary>
    Expired,

    /// <summary>Withdrawn by the tenant before it was used.</summary>
    Revoked,

    /// <summary>Already redeemed. Invitations are single use.</summary>
    AlreadyUsed,

    /// <summary>
    /// Redeemed by somebody other than the person it was addressed to.
    /// </summary>
    /// <remarks>
    /// The case that makes a forwarded link harmless: the token was right, the recipient was not.
    /// </remarks>
    WrongRecipient,

    /// <summary>They were already a member, so the invitation was consumed and nothing changed.</summary>
    AlreadyMember,

    /// <summary>They are now a member.</summary>
    Accepted
}

/// <summary>The outcome of a redemption, and the tenant it concerned.</summary>
public record RedemptionResult(InvitationRedemption Outcome, Guid? TenantId, string? TenantName)
{
    /// <summary>Whether the caller now belongs to <see cref="TenantId"/>.</summary>
    public bool IsMember =>
        Outcome is InvitationRedemption.Accepted or InvitationRedemption.AlreadyMember;
}

/// <summary>What an invitation looks like to somebody deciding whether to accept it.</summary>
/// <param name="TenantName">The tenant they are being asked to join.</param>
/// <param name="Email">The address it was sent to.</param>
/// <param name="ExpiresOn">When it stops working.</param>
/// <param name="IsPending">False when it is spent, withdrawn or expired.</param>
public record InvitationPreview(string TenantName, string Email, DateTimeOffset ExpiresOn, bool IsPending);

/// <summary>
/// Issuing, withdrawing and redeeming invitations to join a tenant.
/// </summary>
/// <remarks>
/// <para>
/// The one path by which a tenant gains a member who could not already reach it. Everything else in
/// the framework assumes membership exists; this is where it comes from.
/// </para>
/// <para>
/// <strong>What this deliberately does not do.</strong> It does not send email - delivery, wording
/// and the shape of the link are the application's, and a framework that sent mail would have to own
/// templates, branding and a transport. It does not decide whether the inviter may hand out the role
/// they attached: that is an application's authorization question, answered before
/// <see cref="InviteAsync"/> is called. And it does not create accounts. It owns the lifecycle -
/// unguessable single-use tokens, expiry, revocation, and binding a redemption to the invited
/// address - which is the part every application would otherwise write again, slightly differently,
/// and get subtly wrong.
/// </para>
/// </remarks>
public interface ITenantInvitationService
{
    /// <summary>
    /// Creates an invitation for <paramref name="email"/> to join <paramref name="tenantId"/>.
    /// </summary>
    /// <remarks>
    /// Re-inviting an address that already has a pending invitation withdraws the old one and issues
    /// a fresh token, rather than leaving two live links or refusing. "Send it again" is what
    /// somebody means when the first mail went astray, and the previous link stopping is the point.
    /// </remarks>
    /// <param name="roleId">A role to grant on redemption, already checked as one this inviter may grant.</param>
    /// <param name="lifetime">How long the link works for. Defaults to fourteen days.</param>
    Task<TenantInvitation> InviteAsync(
        Guid tenantId,
        string email,
        Guid? roleId,
        Guid invitedByUserId,
        TimeSpan? lifetime = null,
        CancellationToken cancellationToken = default);

    /// <summary>Outstanding invitations for a tenant - not the spent or withdrawn ones.</summary>
    Task<IReadOnlyList<TenantInvitation>> PendingAsync(
        Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraws an invitation, so its link stops working.
    /// </summary>
    /// <returns><c>false</c> when no such pending invitation belongs to this tenant.</returns>
    Task<bool> RevokeAsync(
        Guid tenantId, Guid invitationId, Guid revokedByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes an invitation without consuming it, for the screen that asks somebody to accept.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c> for a token that names nothing, so a wrong or tampered link and a real one
    /// are not distinguishable by anything except the answer to this call.
    /// </remarks>
    Task<InvitationPreview?> PeekAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Redeems an invitation, making <paramref name="userId"/> a member.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <paramref name="userEmail"/> must be the address the caller is actually authenticated as -
    /// taken from their verified identity, never from anything they typed - because it is what stops
    /// a leaked link from being usable by whoever finds it.
    /// </para>
    /// <para>
    /// The role attached at issue time is granted here without re-asking whether it may be granted,
    /// with one exception: a role that has since been deleted is skipped rather than resurrected. The
    /// authorization decision was made when the invitation was issued, by somebody who held what they
    /// were handing out; re-deciding it now, against a person who is not present, has no better
    /// answer available to it.
    /// </para>
    /// </remarks>
    Task<RedemptionResult> RedeemAsync(
        string token, Guid userId, string userEmail, CancellationToken cancellationToken = default);
}
