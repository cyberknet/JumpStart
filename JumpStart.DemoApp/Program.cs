// Copyright �2026 Scott Blomfield
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

using AutoMapper;
using JumpStart.Api.Clients;
using JumpStart.DemoApp.Clients;
using JumpStart.DemoApp.Components;
using JumpStart.DemoApp.Components.Account;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Services;
using JumpStart.Services;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. BLAZOR COMPONENTS
// ============================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================
// 2. DATABASE CONTEXT (Identity only)
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ============================================
// 3. IDENTITY SERVICES
// ============================================
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// ============================================
// 4. JWT TOKEN SERVICES (for API calls)
// ============================================
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddTransient<JwtAuthenticationHandler>();
builder.Services.AddTransient<JwtExchangeHandler>();
// API-client-based tenant selection (see ADR-015) - JwtExchangeHandler picks this up automatically
// (resolved lazily via IServiceProvider, not constructor injection - see its remarks) to add a
// tenant_id claim to the identity assertion. ITenantsApiClient itself is auto-discovered below
// (AutoDiscoverApiClients).
builder.Services.AddScoped<ITenantSelectionService, ApiTenantSelectionService>();

// Get the API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";

// Token-exchange client passes its bearer token explicitly per call (see JwtExchangeHandler) -
// it must not go through JwtAuthenticationHandler, which would attach whatever's currently in
// ITokenStore (nothing, the first time). Registered before AddJumpStart so RegisterApiClients'
// auto-attachment check (ADR-014) sees ITokenExchangeApiClient as already present.
builder.Services.AddApiClient<ITokenExchangeApiClient>(apiBaseUrl);

// ============================================
// 5. API CLIENT REGISTRATION
// ============================================
builder.Services.AddJumpStart(options =>
{
    options.ApiBaseUrl = apiBaseUrl;
    options.AutoDiscoverApiClients = true;
    options.AutoDiscoverRepositories = false;
});

// Demo-only: grants a brand-new user the "Demo Administrator" role, called directly from
// Register.razor/ExternalLogin.razor right after account creation - see DemoNewUserBootstrapper's
// remarks and ADR-014's correction note for why this replaced an earlier DelegatingHandler-based
// design. Not part of RegisterApiClients' auto-attachment detection.
builder.Services.AddApiClient<IDemoBootstrapApiClient>(apiBaseUrl);
builder.Services.AddScoped<DemoNewUserBootstrapper>();

// IProductApiClient predates [ApiClientFor<...>] and is registered manually, so it doesn't
// benefit from RegisterApiClients' auto-attachment (ADR-014) - the chain must be wired by hand.
// Handler order (first added = outermost, runs first): JwtExchangeHandler ensures a real token
// exists; JwtAuthenticationHandler attaches whatever's now in ITokenStore.
builder.Services.AddApiClient<IProductApiClient>($"{apiBaseUrl}/api/products")
    .AddHttpMessageHandler<JwtExchangeHandler>()
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

var app = builder.Build();

// ============================================
// APPLY PENDING MIGRATIONS (Identity only)
// ============================================
// Demo-app convenience: automatically brings the Identity schema up to date on startup so the app
// "just runs" against a fresh LocalDB instance with no manual `dotnet ef database update` step. Not
// a recommended pattern for production services with multiple scaled-out instances (concurrent
// migration application), but appropriate for a reference/demo app.
using (var migrationScope = app.Services.CreateScope())
{
    migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Authentication & Authorization must run before UseAntiforgery, so HttpContext.User is already
// populated when antiforgery validates the token's embedded claims - otherwise every check compares
// against the wrong (unauthenticated) principal, causing AntiforgeryValidationException on every
// request, not just with stale cookies. See
// https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery#antiforgery-middleware-order.
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components
app.MapAdditionalIdentityEndpoints();

app.Run();
