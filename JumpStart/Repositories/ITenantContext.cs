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
using System.Threading.Tasks;

namespace JumpStart.Repositories;

/// <summary>
/// Provides access to the current tenant context for multi-tenant applications.
/// This is the recommended interface for most multi-tenant scenarios using Guid tenant identifiers.
/// </summary>
/// <remarks>
/// <para>
/// Similar to <see cref="IUserContext"/> which provides the current user ID,
/// this interface provides the current tenant ID for automatic tenant scoping in repositories.
/// </para>
/// <para>
/// <strong>Common Implementation Sources:</strong>
/// - <strong>HTTP Headers:</strong> X-Tenant-Id header (API keys, tenant selection)
/// - <strong>JWT Claims:</strong> tenant_id or org_id claim in access token
/// - <strong>Subdomain:</strong> tenant.myapp.com → tenant ID lookup via database/cache
/// - <strong>User Claims:</strong> Organization/company ID from authenticated user
/// - <strong>Database Routing:</strong> Connection string selection based on tenant
/// - <strong>Route Values:</strong> /tenants/{tenantId}/... from route parameters
/// - <strong>Session/Cookie:</strong> Tenant selection stored in session
/// </para>
/// <para>
/// <strong>Implementation Strategies:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Single Tenant per User:</strong> Users belong to one tenant. Tenant ID stored in user claims. Most common for SaaS.</description></item>
/// <item><description><strong>Explicit Selection:</strong> Users select tenant from dropdown. Tenant ID stored in session/header.</description></item>
/// <item><description><strong>Subdomain-Based:</strong> Tenant determined by subdomain (acme.myapp.com). Requires tenant lookup.</description></item>
/// <item><description><strong>API Key-Based:</strong> API keys include tenant ID. Common for B2B integrations.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Using TenantSelectionService (recommended for multi-tenant membership)
/// public class SelectionBasedTenantContext : ITenantContext
/// {
///     private readonly ITenantSelectionService _tenantSelection;
///     
///     public SelectionBasedTenantContext(ITenantSelectionService tenantSelection)
///     {
///         _tenantSelection = tenantSelection;
///     }
///     
///     public async Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         return await _tenantSelection.GetCurrentTenantIdAsync();
///     }
/// }
/// 
/// // Example 2: JWT claim-based tenant context (single tenant per user)
/// public class JwtTenantContext : ITenantContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     
///     public JwtTenantContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         var tenantClaim = _httpContextAccessor.HttpContext?.User
///             .FindFirst("tenant_id")?.Value;
///         
///         if (Guid.TryParse(tenantClaim, out var tenantId))
///         {
///             return Task.FromResult&lt;Guid?&gt;(tenantId);
///         }
///         
///         return Task.FromResult&lt;Guid?&gt;(null);
///     }
/// }
/// 
/// // Example 2: HTTP header-based tenant context
/// public class HeaderTenantContext : ITenantContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     
///     public HeaderTenantContext(IHttpContextAccessor httpContextAccessor)
///     {
///         _httpContextAccessor = httpContextAccessor;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         var tenantHeader = _httpContextAccessor.HttpContext?
///             .Request.Headers["X-Tenant-Id"].FirstOrDefault();
///         
///         if (Guid.TryParse(tenantHeader, out var tenantId))
///         {
///             return Task.FromResult&lt;Guid?&gt;(tenantId);
///         }
///         
///         return Task.FromResult&lt;Guid?&gt;(null);
///     }
/// }
/// 
/// // Example 3: Subdomain-based tenant context with lookup
/// public class SubdomainTenantContext : ITenantContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private readonly ITenantResolver _tenantResolver;
///     
///     public SubdomainTenantContext(
///         IHttpContextAccessor httpContextAccessor,
///         ITenantResolver tenantResolver)
///     {
///         _httpContextAccessor = httpContextAccessor;
///         _tenantResolver = tenantResolver;
///     }
///     
///     public async Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
///         if (string.IsNullOrEmpty(host))
///             return null;
///         
///         // Extract subdomain (e.g., "acme" from "acme.myapp.com")
///         var parts = host.Split('.');
///         if (parts.Length &lt; 3)
///             return null; // No subdomain
///         
///         var subdomain = parts[0];
///         
///         // Look up tenant ID by subdomain
///         return await _tenantResolver.GetTenantIdBySubdomainAsync(subdomain);
///     }
/// }
/// 
/// // Example 4: Blazor Server with AuthenticationStateProvider
/// public class BlazorTenantContext : ITenantContext
/// {
///     private readonly AuthenticationStateProvider _authStateProvider;
///     
///     public BlazorTenantContext(AuthenticationStateProvider authStateProvider)
///     {
///         _authStateProvider = authStateProvider;
///     }
///     
///     public async Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         var authState = await _authStateProvider.GetAuthenticationStateAsync();
///         var tenantClaim = authState.User.FindFirst("tenant_id")?.Value;
///         
///         if (Guid.TryParse(tenantClaim, out var tenantId))
///         {
///             return tenantId;
///         }
///         
///         return null;
///     }
/// }
/// 
/// // Example 5: Testing with fixed tenant
/// public class TestTenantContext : ITenantContext
/// {
///     private readonly Guid _tenantId;
///     
///     public TestTenantContext(Guid tenantId)
///     {
///         _tenantId = tenantId;
///     }
///     
///     public Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         return Task.FromResult&lt;Guid?&gt;(_tenantId);
///     }
/// }
/// 
/// // Example 6: Cached tenant context (performance optimization)
/// public class CachedTenantContext : ITenantContext
/// {
///     private readonly IHttpContextAccessor _httpContextAccessor;
///     private Guid? _cachedTenantId;
///     private bool _isRetrieved;
///     
///     public Task&lt;Guid?&gt; GetCurrentTenantIdAsync()
///     {
///         if (_isRetrieved)
///         {
///             return Task.FromResult(_cachedTenantId);
///         }
///         
///         var tenantClaim = _httpContextAccessor.HttpContext?.User
///             .FindFirst("tenant_id")?.Value;
///         
///         _cachedTenantId = Guid.TryParse(tenantClaim, out var tenantId) 
///             ? tenantId 
///             : null;
///         _isRetrieved = true;
///         
///         return Task.FromResult(_cachedTenantId);
///     }
/// }
/// 
/// // Example 7: Registration with JumpStart
/// services.AddJumpStart(options =>
/// {
///     options.RegisterUserContext&lt;HttpUserContext&gt;();
///     options.RegisterTenantContext&lt;JwtTenantContext&gt;();
/// });
/// 
/// // Example 8: Manual registration
/// services.AddScoped&lt;ITenantContext, JwtTenantContext&gt;();
/// </code>
/// </example>
/// <seealso cref="Data.MultiTenant.ITenantScoped"/>
/// <seealso cref="Data.SimpleTenant"/>
public interface ITenantContext
{
    /// <summary>
    /// Asynchronously retrieves the unique identifier of the current tenant.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The tenant's identifier if multi-tenant context is established
    /// - <c>null</c> if no tenant context exists (single-tenant mode or system operations)
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called by repositories during entity operations to:
    /// - Populate TenantId on AddAsync
    /// - Filter queries to current tenant
    /// - Validate tenant ownership on UpdateAsync/DeleteAsync
    /// </para>
    /// <para>
    /// <strong>Return Value Guidelines:</strong>
    /// - Return the tenant ID if multi-tenant context is established
    /// - Return null for single-tenant applications (tenant filtering disabled)
    /// - Return null for system-wide operations (e.g., background jobs across tenants)
    /// - Consider caching the result per request/scope for performance
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// This method may be called multiple times per request. Consider caching the result
    /// within the current scope to avoid redundant lookups.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// Implementations must be thread-safe as this method may be called concurrently.
    /// Use AsyncLocal, scoped services, or HttpContext which are inherently per-request.
    /// </para>
    /// </remarks>
    Task<Guid?> GetCurrentTenantIdAsync();
}
