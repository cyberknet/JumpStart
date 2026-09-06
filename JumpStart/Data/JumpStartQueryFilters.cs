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

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Data;

/// <summary>
/// The keys JumpStart's global query filters are registered under, so a query can drop one of them
/// without dropping the rest.
/// </summary>
/// <remarks>
/// <para>
/// EF Core's parameterless <c>IgnoreQueryFilters()</c> is all-or-nothing: it removes every filter on
/// the entity. That made "read across tenants" and "read soft-deleted rows" the same gesture, so every
/// deliberately cross-tenant query had to re-add <c>DeletedOn == null</c> by hand and would silently
/// resurrect deleted rows if it forgot. Naming the filters (EF Core 10) lets a caller drop exactly the
/// one it means to.
/// </para>
/// <para>
/// Prefer <see cref="JumpStartQueryableExtensions.AcrossAllTenants{TEntity}"/> to using these keys
/// directly - it says what it's for, and it's greppable in a way an <c>IgnoreQueryFilters</c> call
/// isn't when you need to audit every place the tenant boundary is deliberately crossed.
/// </para>
/// </remarks>
public static class JumpStartQueryFilters
{
    /// <summary>Excludes rows where <c>DeletedOn</c> is set. Applied to every <see cref="Advanced.Auditing.IDeletable"/>.</summary>
    public const string SoftDelete = "JumpStart.SoftDelete";

    /// <summary>
    /// Restricts rows to the current tenant. Applied to both <see cref="MultiTenant.ITenantScoped"/>
    /// and <see cref="MultiTenant.ITenantScopedOptional"/> - deliberately one key for both, so
    /// "read across tenants" is a single gesture regardless of which of the two an entity implements.
    /// </summary>
    public const string Tenant = "JumpStart.Tenant";
}

/// <summary>
/// Query helpers for stepping outside JumpStart's global filters on purpose.
/// </summary>
public static class JumpStartQueryableExtensions
{
    /// <summary>
    /// Reads every tenant's rows, not just the current one's. Soft-deleted rows stay excluded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For the handful of operations that are legitimately platform-wide: a background sweep over all
    /// tenants' records, a redemption lookup by a code that isn't scoped yet, a cross-tenant admin
    /// report. Everything else should stay inside the tenant filter.
    /// </para>
    /// <para>
    /// Replaces the <c>IgnoreQueryFilters().Where(e =&gt; e.DeletedOn == null &amp;&amp; ...)</c> pattern.
    /// The manual soft-delete condition is no longer needed <em>or</em> possible to forget, which was
    /// the actual hazard: dropping every filter to cross one boundary quietly un-deleted rows too.
    /// </para>
    /// </remarks>
    public static IQueryable<TEntity> AcrossAllTenants<TEntity>(this IQueryable<TEntity> source)
        where TEntity : class =>
        source.IgnoreQueryFilters([JumpStartQueryFilters.Tenant]);

    /// <summary>
    /// Includes soft-deleted rows. Tenant scoping still applies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For restore-a-deleted-item screens, audit trails, and any history view that has to account for
    /// records the user removed. The tenant filter deliberately stays on: wanting to see deleted rows
    /// is not wanting to see other tenants' deleted rows.
    /// </para>
    /// <para>
    /// That distinction is the whole point of this method existing. Without it the only way to see
    /// soft-deleted rows is a bare <c>IgnoreQueryFilters()</c>, which drops tenancy at the same time -
    /// so a "deleted items" list leaks every tenant's deleted records unless the author remembers to
    /// re-add a <c>TenantId</c> predicate by hand. That's the same hazard
    /// <see cref="AcrossAllTenants{TEntity}"/> removes, pointed the other way, and it's the more
    /// damaging direction of the two: forgetting there is a cross-tenant read.
    /// </para>
    /// </remarks>
    public static IQueryable<TEntity> IncludingDeleted<TEntity>(this IQueryable<TEntity> source)
        where TEntity : class =>
        source.IgnoreQueryFilters([JumpStartQueryFilters.SoftDelete]);

    // Deliberately no helper for dropping both at once. That combination is a genuine platform-wide
    // read of deleted data, it should be rare, and it should look unusual at the call site - a bare
    // IgnoreQueryFilters() already says exactly that and stays greppable as the thing to scrutinise.
}
