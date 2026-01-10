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

using System.Text;
using JumpStart.DemoApp.Api.Infrastructure.Authentication;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. JWT AUTHENTICATION CONFIGURATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
        };
    });

// ============================================
// 2. AUTHORIZATION
// ============================================
builder.Services.AddAuthorization();

// ============================================
// 3. CORS CONFIGURATION
// ============================================
// Allow Blazor Server to call this API
var blazorServerUrl = builder.Configuration["CorsSettings:BlazorServerUrl"] ?? "https://localhost:7001";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorServer", policy =>
    {
        policy.WithOrigins(blazorServerUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================
// 4. HTTP CONTEXT ACCESSOR (for ISimpleUserContext)
// ============================================
builder.Services.AddHttpContextAccessor();

// ============================================
// 5. USER CONTEXT (for audit tracking)
// ============================================
builder.Services.AddScoped<ISimpleUserContext, ApiUserContext>();

// ============================================
// 6. CONTROLLERS
// ============================================
builder.Services.AddControllers();

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// HTTPS redirection
app.UseHttpsRedirection();

// CORS must be before Authentication
app.UseCors("AllowBlazorServer");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
