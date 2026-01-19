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

using JumpStart.Api.Clients;
using JumpStart.DemoApp.Clients;
using JumpStart.DemoApp.Components;
using JumpStart.DemoApp.Components.Account;
using JumpStart.DemoApp.Data;
using JumpStart.DemoApp.Services;
using JumpStart.Services.Authentication;
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

// ============================================
// 5. API CLIENT REGISTRATION
// ============================================

// Get the API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7030";
builder.Services.AddJumpStart(options =>
{
    options.ApiBaseUrl = apiBaseUrl;
    options.AutoDiscoverApiClients = true;
});

//// Register API clients with JWT authentication handler
builder.Services.AddSimpleApiClient<IProductApiClient>($"{apiBaseUrl}/api/products")
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

var app = builder.Build();

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
app.UseAntiforgery();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Add additional endpoints required by the Identity /Account Razor components
app.MapAdditionalIdentityEndpoints();

app.Run();
