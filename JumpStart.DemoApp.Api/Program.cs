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
using JumpStart.DemoApp.Api.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. DATABASE CONTEXT
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================
// 2. JUMPSTART FRAMEWORK SERVICES
// ============================================
builder.Services.AddJumpStart(options =>
{
    options.RegisterUserContext<ApiUserContext>();
    options.AutoDiscoverRepositories = true; // ? Required for EnsureDbContextResolution
    options.ScanAssembly(typeof(Program).Assembly);
    options.RegisterFormsController = true;
});

// ============================================
// 3. AUTOMAPPER
// ============================================
builder.Services.AddJumpStartAutoMapper(
    typeof(Program).Assembly,                    // API project profiles
    typeof(JumpStart.Forms.Form).Assembly);      // JumpStart framework profiles (includes FormsProfile)

// ============================================
// 4. JWT AUTHENTICATION CONFIGURATION
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
// 5. AUTHORIZATION
// ============================================
builder.Services.AddAuthorization();

// ============================================
// 6. CORS CONFIGURATION
// ============================================
// Allow Blazor Server to call this API
var blazorServerUrl = builder.Configuration["CorsSettings:BlazorServerUrl"] ?? "https://localhost:7099";

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
// 7. HTTP CONTEXT ACCESSOR (for IUserContext)
// ============================================
builder.Services.AddHttpContextAccessor();

// ============================================
// 8. API DOCUMENTATION
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "JumpStart DemoApp API", Version = "v1" });
});

// ============================================
// 9. CONTROLLERS
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// ============================================
// 10. API DOCUMENTATION (development only)
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "JumpStart DemoApp API v1");
    });
}

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
