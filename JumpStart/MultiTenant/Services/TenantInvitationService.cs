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
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JumpStart.MultiTenant.Services;

/// <inheritdoc cref="ITenantInvitationService" />
/// <remarks>
/// Takes <see cref="DbContext"/> rather than <see cref="JumpStartDbContext"/> to match how every
/// other framework repository is wired: <c>EnsureDbContextResolution</c> registers the base type as
/// a factory over whichever concrete context the application declared, and the derived type itself is
/// not resolvable. Everything used here - <c>Set&lt;T&gt;</c> and the query-filter extensions - is
/// available on the base.
/// </remarks>
public class TenantInvitationService(
    DbContext context,
    TimeProvider timeProvider,
    ILogger<TenantInvitationService> logger) : ITenantInvitationService
{
    /// <summary>Long enough to survive a weekend and a forwarded mail; short enough to go stale.</summary>
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(14);

    /// <inheritdoc />
    public async Task<TenantInvitation> InviteAsync(
        Guid tenantId,
        string email,
        Guid? roleId,
        Guid invitedByUserId,
        TimeSpan? lifetime = null,
        CancellationToken cancellationToken = default)
    {
        var address = Normalise(email);

        if (address.Length == 0)
        {
            throw new ArgumentException("An email address is required.", nameof(email));
        }

        var now = timeProvider.GetUtcNow();

        // Re-inviting withdraws whatever is outstanding - see InviteAsync's remarks. Written as a
        // sweep rather than "the" previous one because nothing stops two from existing if an earlier
        // race got past the check, and leaving a second live link is the failure that matters.
        foreach (var superseded in await PendingQuery(tenantId, now)
            .Where(i => i.Email == address)
            .ToListAsync(cancellationToken))
        {
            superseded.RevokedOn = now;
            superseded.RevokedById = invitedByUserId;
        }

        var invitation = new TenantInvitation
        {
            TenantId = tenantId,
            Email = address,
            Token = NewToken(),
            RoleId = roleId,
            ExpiresOn = now.Add(lifetime ?? DefaultLifetime),
            CreatedById = invitedByUserId,
            CreatedOn = now
        };

        context.Set<TenantInvitation>().Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Invited {Email} to tenant {TenantId}, expiring {ExpiresOn:u}.",
            address, tenantId, invitation.ExpiresOn);

        return invitation;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TenantInvitation>> PendingAsync(
        Guid tenantId, CancellationToken cancellationToken = default) =>
        await PendingQuery(tenantId, timeProvider.GetUtcNow())
            .OrderBy(i => i.CreatedOn)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> RevokeAsync(
        Guid tenantId, Guid invitationId, Guid revokedByUserId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        var invitation = await PendingQuery(tenantId, now)
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation is null)
        {
            return false;
        }

        invitation.RevokedOn = now;
        invitation.RevokedById = revokedByUserId;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Withdrew the invitation for {Email} to tenant {TenantId}.", invitation.Email, tenantId);

        return true;
    }

    /// <inheritdoc />
    public async Task<InvitationPreview?> PeekAsync(
        string token, CancellationToken cancellationToken = default)
    {
        var invitation = await ByTokenAsync(token, cancellationToken);

        if (invitation is null)
        {
            return null;
        }

        var name = await context.Set<Tenant>()
            .AcrossAllTenants()
            .Where(t => t.Id == invitation.TenantId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new InvitationPreview(
            name ?? string.Empty,
            invitation.Email,
            invitation.ExpiresOn,
            invitation.IsPendingAt(timeProvider.GetUtcNow()));
    }

    /// <inheritdoc />
    public async Task<RedemptionResult> RedeemAsync(
        string token, Guid userId, string userEmail, CancellationToken cancellationToken = default)
    {
        var invitation = await ByTokenAsync(token, cancellationToken);

        if (invitation is null)
        {
            return new RedemptionResult(InvitationRedemption.NotFound, null, null);
        }

        var now = timeProvider.GetUtcNow();

        var tenantName = await context.Set<Tenant>()
            .AcrossAllTenants()
            .Where(t => t.Id == invitation.TenantId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken);

        RedemptionResult Result(InvitationRedemption outcome) =>
            new(outcome, invitation.TenantId, tenantName);

        if (invitation.RevokedOn is not null)
        {
            return Result(InvitationRedemption.Revoked);
        }

        if (invitation.AcceptedOn is not null)
        {
            return Result(InvitationRedemption.AlreadyUsed);
        }

        if (invitation.ExpiresOn <= now)
        {
            return Result(InvitationRedemption.Expired);
        }

        // The second factor. Checked before anything is written, and reported as its own outcome so
        // the screen can say "this was sent to someone else" rather than "invalid link" - which is
        // the difference between a person signing in with their other address and giving up.
        if (!string.Equals(Normalise(userEmail), invitation.Email, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "An invitation addressed to {Invited} was presented by a different account.",
                invitation.Email);

            return Result(InvitationRedemption.WrongRecipient);
        }

        // AcrossAllTenants throughout: the caller is not a member yet, so there is no ambient tenant
        // and the filter would deny every one of these reads. The token is what authorises them.
        var alreadyMember = await context.Set<UserTenant>()
            .AcrossAllTenants()
            .AnyAsync(
                ut => ut.TenantId == invitation.TenantId && ut.UserId == userId, cancellationToken);

        invitation.AcceptedOn = now;
        invitation.AcceptedByUserId = userId;

        if (alreadyMember)
        {
            // Consume it anyway. Leaving a live link for somebody who is already inside is a loose
            // credential that grants nothing today and might not stay that way.
            await context.SaveChangesAsync(cancellationToken);
            return Result(InvitationRedemption.AlreadyMember);
        }

        context.Set<UserTenant>().Add(new UserTenant
        {
            UserId = userId,
            TenantId = invitation.TenantId,
            IsActive = true,
            CreatedById = userId,
            CreatedOn = now
        });

        await GrantRoleAsync(invitation, userId, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{Email} accepted their invitation to tenant {TenantId}.", invitation.Email, invitation.TenantId);

        return Result(InvitationRedemption.Accepted);
    }

    /// <summary>Grants the role the invitation carried, if it still exists.</summary>
    private async Task GrantRoleAsync(
        TenantInvitation invitation, Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (invitation.RoleId is not { } roleId)
        {
            return;
        }

        // A role deleted between issue and redemption is skipped, not recreated. The soft-delete
        // filter does the work here - a retired role simply is not found.
        var stillExists = await context.Set<Role>()
            .AcrossAllTenants()
            .AnyAsync(r => r.Id == roleId, cancellationToken);

        if (!stillExists)
        {
            logger.LogWarning(
                "The role an invitation to tenant {TenantId} carried no longer exists; adding the "
                + "member without it.", invitation.TenantId);

            return;
        }

        context.Set<UserRole>().Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = invitation.TenantId,
            CreatedById = userId,
            CreatedOn = now
        });
    }

    /// <summary>
    /// Finds an invitation by its token, ignoring the tenant boundary.
    /// </summary>
    /// <remarks>
    /// This is the one lookup in the framework that deliberately has no tenant to scope by, because
    /// the caller is on the outside asking to be let in. The token is the authorization, which is
    /// why it has to be unguessable rather than sequential.
    /// </remarks>
    private async Task<TenantInvitation?> ByTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return await context.Set<TenantInvitation>()
            .AcrossAllTenants()
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    private IQueryable<TenantInvitation> PendingQuery(Guid tenantId, DateTimeOffset now) =>
        context.Set<TenantInvitation>()
            .AcrossAllTenants()
            .Where(i => i.TenantId == tenantId
                && i.AcceptedOn == null
                && i.RevokedOn == null
                && i.ExpiresOn > now);

    private static string Normalise(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

    /// <summary>256 bits from the cryptographic generator, URL-safe.</summary>
    private static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
