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

using System.Collections.Concurrent;
using JumpStart.Data;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Caches each circuit's resolved current-tenant selection, keyed by <see cref="CircuitServicesAccessor.CircuitId"/> -
/// the one thing about a circuit that's genuinely shared across every render-mode island on the page
/// (see that property's own remarks, and <see cref="CircuitServicesAccessor.Services"/>'s, for why a
/// Scoped <see cref="ITenantSelectionService"/> implementation's own instance fields can't be trusted
/// to do this on their own).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Registered as Singleton, deliberately</strong> - the whole point is to be the one piece of
/// state every DI scope on a given circuit can reach, regardless of which render-mode island's scope
/// happens to be resolving <see cref="ITenantSelectionService"/> at the time. A Singleton holding
/// per-circuit data is normally a memory-leak smell; <see cref="Clear"/> exists specifically so
/// <see cref="ITenantSelectionService"/> implementations can call it when a circuit's data should no
/// longer matter (e.g. on logout, alongside clearing <see cref="ITokenStore"/>) - not a full
/// circuit-closed sweep, since neither implementation currently has a hook for that, but enough to
/// bound the common case. A stale entry left behind by an ungracefully-terminated circuit is a single
/// small dictionary entry (a string key and a completed <c>Task&lt;Guid?&gt;</c>), not a growing leak.
/// </para>
/// <para>
/// Caches the in-flight resolution <see cref="Task{TResult}"/> itself, not just the eventual result -
/// see <see cref="GetOrResolveAsync"/>'s own remarks for why a simple "have we resolved yet" flag or
/// even a plain result cache is insufficient here: concurrent callers (from different islands, or
/// different components within the same island) must all await the exact same in-flight resolution,
/// not risk one of them observing a not-yet-finished state as "nothing found."
/// </para>
/// </remarks>
public class CircuitTenantCache
{
    private readonly ConcurrentDictionary<string, Task<Guid?>> _resolutions = new();

    // A second, independently-keyed cache rather than folding the tenant list into _resolutions:
    // the two answer different questions ("which tenant is selected" vs. "what tenants exist and what
    // are they called") and are invalidated on different triggers - a real tenant switch always
    // updates the first, but never needs to touch the second, while editing a tenant's own name (see
    // InvalidateTenants) needs the opposite. Cleared together in Clear, since both are meaningless
    // once nothing is known about the circuit's user any more (e.g. logout).
    private readonly ConcurrentDictionary<string, Task<List<Tenant>>> _tenantLists = new();

    /// <summary>
    /// Returns the cached resolution for <paramref name="circuitId"/> if one is already in flight or
    /// complete; otherwise starts <paramref name="resolver"/> exactly once and caches its task so every
    /// concurrent or subsequent caller for this circuit gets that same task.
    /// </summary>
    /// <remarks>
    /// <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,System.Func{TKey,TValue})"/> can, per
    /// its own documented contract, invoke the value factory more than once if two threads race to add
    /// the same key - but only one of those resulting tasks ever actually gets stored and returned to
    /// every caller (losing invocations' tasks are discarded, not awaited by anyone), so a caller here
    /// still only ever awaits one shared task for this circuit, never a mix of two different callers'
    /// independently-resolving ones. Losing the factory-invocation race here at worst starts (and then
    /// discards) one redundant tenant-list fetch - not the "different callers observe different,
    /// wrong answers" bug this cache exists to prevent.
    /// </remarks>
    public Task<Guid?> GetOrResolveAsync(string circuitId, Func<Task<Guid?>> resolver) =>
        _resolutions.GetOrAdd(circuitId, static (_, r) => r(), resolver);

    /// <summary>
    /// Overwrites the cached resolution for <paramref name="circuitId"/> with an already-known value -
    /// used after a real tenant switch (<c>SetCurrentTenantAsync</c>), where the new value is known
    /// synchronously and every island's next resolution should see it immediately rather than
    /// re-deriving it.
    /// </summary>
    public void SetResolved(string circuitId, Guid? tenantId) =>
        _resolutions[circuitId] = Task.FromResult(tenantId);

    /// <summary>
    /// Removes any cached resolution for <paramref name="circuitId"/> - call alongside
    /// <see cref="ITokenStore.ClearToken"/> on logout, so a later sign-in on a reused circuit doesn't
    /// see a stale tenant left over from the previous session.
    /// </summary>
    public void Clear(string circuitId)
    {
        _resolutions.TryRemove(circuitId, out _);
        _tenantLists.TryRemove(circuitId, out _);
    }

    /// <summary>
    /// Same shape as <see cref="GetOrResolveAsync"/>, for the caller's available-tenants list rather
    /// than their current selection - the same in-flight-task-sharing reasoning applies, and for the
    /// same reason: every island on a circuit needs the same answer, not each resolving (and possibly
    /// caching) its own independently.
    /// </summary>
    public Task<List<Tenant>> GetOrResolveTenantsAsync(string circuitId, Func<Task<List<Tenant>>> resolver) =>
        _tenantLists.GetOrAdd(circuitId, static (_, r) => r(), resolver);

    /// <summary>
    /// Discards the cached tenant list for <paramref name="circuitId"/>, so the next
    /// <see cref="GetOrResolveTenantsAsync"/> call re-fetches instead of returning what may now be
    /// stale data - call this after a tenant's own details (most visibly its name) are edited from
    /// anywhere in the app, via <see cref="ITenantSelectionService.RefreshAvailableTenantsAsync"/>.
    /// Deliberately does not touch <see cref="GetOrResolveAsync"/>'s cache: which tenant is selected
    /// hasn't changed just because one tenant's name did.
    /// </summary>
    public void InvalidateTenants(string circuitId) => _tenantLists.TryRemove(circuitId, out _);
}
