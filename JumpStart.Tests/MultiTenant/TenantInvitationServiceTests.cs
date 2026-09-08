// Copyright ©2026 Scott Blomfield

using System;
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Data;
using JumpStart.MultiTenant.Services;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JumpStart.Tests.MultiTenant;

/// <summary>
/// Tests for <see cref="TenantInvitationService"/> - the one path by which a tenant gains a member
/// who could not already reach it.
/// </summary>
/// <remarks>
/// The properties worth the most here are the ones that make an emailed link safe to send: it works
/// once, it stops working, it can be withdrawn, and holding it is not by itself enough.
/// </remarks>
public class TenantInvitationServiceTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _inviterId = Guid.NewGuid();
    private readonly TestClock _clock = new(DateTimeOffset.Parse(
        "2026-03-01T10:00:00Z", System.Globalization.CultureInfo.InvariantCulture));

    private const string Invited = "newcomer@example.com";

    /// <summary>
    /// A clock the test moves by hand.
    /// </summary>
    /// <remarks>
    /// Expiry is a rule about time, so testing it means controlling time rather than waiting for it.
    /// Written here rather than pulled in from Microsoft.Extensions.TimeProvider.Testing: a handful
    /// of lines is cheaper than a new dependency on the framework's own test project.
    /// </remarks>
    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now = _now.Add(by);
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext? tenantContext = null)
        : JumpStartDbContext(options, tenantContext)
    {
    }

    /// <summary>A tenant is established, so the ambient filter is realistic for the issuing side.</summary>
    private class FixedTenantContext(Guid? tenantId) : ITenantContext
    {
        public Task<Guid?> GetCurrentTenantIdAsync() => Task.FromResult(tenantId);
    }

    private TestDbContext CreateContext(Guid? tenantId) =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(_dbName).Options,
            new FixedTenantContext(tenantId));

    private TenantInvitationService CreateService(TestDbContext context) =>
        new(context, _clock, NullLogger<TenantInvitationService>.Instance);

    private async Task SeedTenantAsync()
    {
        await using var context = CreateContext(_tenantId);
        context.Set<Tenant>().Add(new Tenant { Id = _tenantId, Name = "Acme", IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task RedeemingAnInvitationMakesTheRecipientAMember()
    {
        await SeedTenantAsync();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);
        var result = await service.RedeemAsync(invitation.Token, userId, Invited);

        Assert.Equal(InvitationRedemption.Accepted, result.Outcome);
        Assert.True(result.IsMember);
        Assert.Equal("Acme", result.TenantName);

        Assert.True(await context.Set<UserTenant>()
            .AcrossAllTenants()
            .AnyAsync(ut => ut.UserId == userId && ut.TenantId == _tenantId));
    }

    [Fact]
    public async Task TheRoleTheInvitationCarriedIsGranted()
    {
        await SeedTenantAsync();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);

        var role = new Role { Name = "Moderator", TenantId = _tenantId };
        context.Set<Role>().Add(role);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, role.Id, _inviterId);
        await service.RedeemAsync(invitation.Token, userId, Invited);

        Assert.True(await context.Set<UserRole>()
            .AcrossAllTenants()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id && ur.TenantId == _tenantId));
    }

    /// <summary>
    /// A role deleted between issue and redemption is skipped rather than resurrected - the person
    /// still joins, just without it.
    /// </summary>
    [Fact]
    public async Task ARoleDeletedBeforeRedemptionIsSkipped()
    {
        await SeedTenantAsync();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);

        var role = new Role { Name = "Doomed", TenantId = _tenantId };
        context.Set<Role>().Add(role);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var invitation = await service.InviteAsync(_tenantId, Invited, role.Id, _inviterId);

        role.DeletedOn = _clock.GetUtcNow();
        await context.SaveChangesAsync();

        var result = await service.RedeemAsync(invitation.Token, userId, Invited);

        Assert.Equal(InvitationRedemption.Accepted, result.Outcome);
        Assert.False(await context.Set<UserRole>().AcrossAllTenants().AnyAsync(ur => ur.UserId == userId));
    }

    /// <summary>
    /// The property that makes a forwarded link harmless: the token was right, the recipient was not.
    /// </summary>
    [Fact]
    public async Task SomebodyElseCannotRedeemAForwardedLink()
    {
        await SeedTenantAsync();
        var interloper = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        var result = await service.RedeemAsync(invitation.Token, interloper, "someone.else@example.com");

        Assert.Equal(InvitationRedemption.WrongRecipient, result.Outcome);
        Assert.False(result.IsMember);
        Assert.Empty(await context.Set<UserTenant>().AcrossAllTenants().ToListAsync());

        // ...and it is still usable by the person it was actually for.
        var rightful = await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited);
        Assert.Equal(InvitationRedemption.Accepted, rightful.Outcome);
    }

    [Fact]
    public async Task TheAddressIsMatchedRegardlessOfCasingOrSurroundingSpace()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, "  Newcomer@Example.COM ", null, _inviterId);

        var result = await service.RedeemAsync(invitation.Token, Guid.NewGuid(), "newcomer@example.com");

        Assert.Equal(InvitationRedemption.Accepted, result.Outcome);
    }

    [Fact]
    public async Task AnInvitationWorksOnlyOnce()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited);
        var second = await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited);

        Assert.Equal(InvitationRedemption.AlreadyUsed, second.Outcome);
        Assert.Single(await context.Set<UserTenant>().AcrossAllTenants().ToListAsync());
    }

    [Fact]
    public async Task AnExpiredInvitationIsRefused()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(
            _tenantId, Invited, null, _inviterId, lifetime: TimeSpan.FromDays(14));

        _clock.Advance(TimeSpan.FromDays(15));

        var result = await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited);

        Assert.Equal(InvitationRedemption.Expired, result.Outcome);
        Assert.Empty(await context.Set<UserTenant>().AcrossAllTenants().ToListAsync());
    }

    [Fact]
    public async Task ARevokedInvitationIsRefused()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        Assert.True(await service.RevokeAsync(_tenantId, invitation.Id, _inviterId));

        var result = await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited);

        Assert.Equal(InvitationRedemption.Revoked, result.Outcome);
    }

    /// <summary>
    /// "Send it again" has to stop the first link, or a withdrawn-by-implication invitation stays
    /// live in somebody's inbox.
    /// </summary>
    [Fact]
    public async Task ReinvitingTheSameAddressRetiresTheEarlierLink()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var first = await service.InviteAsync(_tenantId, Invited, null, _inviterId);
        var second = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        Assert.NotEqual(first.Token, second.Token);

        // One live invitation for this address, not two.
        var pending = await service.PendingAsync(_tenantId);
        Assert.Equal([second.Id], pending.Select(i => i.Id));

        Assert.Equal(
            InvitationRedemption.Revoked,
            (await service.RedeemAsync(first.Token, Guid.NewGuid(), Invited)).Outcome);

        Assert.Equal(
            InvitationRedemption.Accepted,
            (await service.RedeemAsync(second.Token, Guid.NewGuid(), Invited)).Outcome);
    }

    [Fact]
    public async Task AnUnknownTokenIsNotFound()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);

        var result = await CreateService(context).RedeemAsync("not-a-real-token", Guid.NewGuid(), Invited);

        Assert.Equal(InvitationRedemption.NotFound, result.Outcome);
        Assert.Null(result.TenantId);
    }

    /// <summary>
    /// Somebody who is already inside consumes the invitation without gaining a second membership -
    /// and, more to the point, without leaving a live link behind them.
    /// </summary>
    [Fact]
    public async Task AnExistingMemberConsumesTheInvitationWithoutDuplicatingMembership()
    {
        await SeedTenantAsync();
        var userId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);

        context.Set<UserTenant>().Add(
            new UserTenant { UserId = userId, TenantId = _tenantId, IsActive = true });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        var result = await service.RedeemAsync(invitation.Token, userId, Invited);

        Assert.Equal(InvitationRedemption.AlreadyMember, result.Outcome);
        Assert.True(result.IsMember);
        Assert.Single(await context.Set<UserTenant>().AcrossAllTenants().ToListAsync());

        // Consumed, so the link is spent.
        Assert.Equal(
            InvitationRedemption.AlreadyUsed,
            (await service.RedeemAsync(invitation.Token, userId, Invited)).Outcome);
    }

    [Fact]
    public async Task PendingListsOnlyLiveInvitationsForThisTenant()
    {
        await SeedTenantAsync();
        var otherTenantId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var live = await service.InviteAsync(_tenantId, "live@example.com", null, _inviterId);
        var withdrawn = await service.InviteAsync(_tenantId, "withdrawn@example.com", null, _inviterId);
        await service.InviteAsync(otherTenantId, "elsewhere@example.com", null, _inviterId);

        await service.RevokeAsync(_tenantId, withdrawn.Id, _inviterId);

        var pending = await service.PendingAsync(_tenantId);

        Assert.Equal([live.Id], pending.Select(i => i.Id));
    }

    [Fact]
    public async Task RevokingAnInvitationBelongingToAnotherTenantDoesNothing()
    {
        await SeedTenantAsync();
        var otherTenantId = Guid.NewGuid();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var theirs = await service.InviteAsync(otherTenantId, Invited, null, _inviterId);

        Assert.False(await service.RevokeAsync(_tenantId, theirs.Id, _inviterId));

        // Still usable by its own tenant's recipient.
        Assert.Equal(
            InvitationRedemption.Accepted,
            (await service.RedeemAsync(theirs.Token, Guid.NewGuid(), Invited)).Outcome);
    }

    [Fact]
    public async Task PeekDescribesTheInvitationWithoutConsumingIt()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var invitation = await service.InviteAsync(_tenantId, Invited, null, _inviterId);

        var preview = await service.PeekAsync(invitation.Token);

        Assert.NotNull(preview);
        Assert.Equal("Acme", preview.TenantName);
        Assert.Equal(Invited, preview.Email);
        Assert.True(preview.IsPending);

        Assert.Equal(
            InvitationRedemption.Accepted,
            (await service.RedeemAsync(invitation.Token, Guid.NewGuid(), Invited)).Outcome);
    }

    [Fact]
    public async Task PeekReturnsNothingForATokenThatNamesNothing()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);

        Assert.Null(await CreateService(context).PeekAsync("nonsense"));
    }

    [Fact]
    public async Task TokensAreUnguessableAndDistinct()
    {
        await SeedTenantAsync();

        await using var context = CreateContext(_tenantId);
        var service = CreateService(context);

        var tokens = new List<string>();

        for (var i = 0; i < 25; i++)
        {
            tokens.Add((await service.InviteAsync(_tenantId, $"person{i}@example.com", null, _inviterId)).Token);
        }

        Assert.Equal(25, tokens.Distinct(StringComparer.Ordinal).Count());

        // 256 bits, base64url - long enough that guessing is not a strategy, and URL-safe so the
        // link survives being pasted.
        Assert.All(tokens, token =>
        {
            Assert.True(token.Length >= 40);
            Assert.DoesNotContain('+', token);
            Assert.DoesNotContain('/', token);
            Assert.DoesNotContain('=', token);
        });
    }
}
