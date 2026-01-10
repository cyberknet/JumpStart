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
using JumpStart.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring JumpStart framework with Entity Framework Core DbContext.
/// This convenience method combines DbContext registration with JumpStart framework setup.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods simplify the common scenario of registering both Entity Framework Core's DbContext
/// and the JumpStart framework together. This is a convenience wrapper that combines two registrations into one
/// fluent call while automatically configuring sensible defaults.
/// </para>
/// <para>
/// <strong>Automatic Configuration:</strong>
/// When using these methods, the framework automatically:
/// - Registers the specified DbContext with Entity Framework Core
/// - Registers all JumpStart framework services
/// - Scans the DbContext's assembly for repository implementations
/// - Applies any custom JumpStart configuration provided
/// </para>
/// <para>
/// <strong>Supported Database Providers:</strong>
/// Works with all Entity Framework Core database providers, including:
/// - SQL Server (Microsoft.EntityFrameworkCore.SqlServer)
/// - SQLite (Microsoft.EntityFrameworkCore.Sqlite)
/// - PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
/// - MySQL (Pomelo.EntityFrameworkCore.MySql)
/// - In-Memory (Microsoft.EntityFrameworkCore.InMemory) - for testing
/// - And any other EF Core provider
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this method when:
/// - Your repositories are in the same assembly as your DbContext
/// - You want simplified setup with sensible defaults
/// - You're following the standard pattern of DbContext + repositories in one assembly
/// </para>
/// <para>
/// <strong>Alternative Approach:</strong>
/// For more control, you can register DbContext and JumpStart separately:
/// <code>
/// services.AddDbContext&lt;AppDbContext&gt;(options => options.UseSqlServer(connectionString));
/// services.AddJumpStart(options => options.ScanAssembly(typeof(AppDbContext).Assembly));
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: SQL Server setup
/// services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
///     options => options.UseSqlServer(connectionString),
///     jumpStart => jumpStart.RegisterUserContext&lt;CurrentUserService&gt;());
/// 
/// // Example 2: SQLite setup
/// services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
///     options => options.UseSqlite("Data Source=app.db"),
///     jumpStart => jumpStart.RegisterUserContext&lt;UserContext&gt;());
/// 
/// // Example 3: PostgreSQL setup
/// services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
///     options => options.UseNpgsql(connectionString));
/// 
/// // Example 4: In-Memory database for testing
/// services.AddJumpStartWithDbContext&lt;TestDbContext&gt;(
///     options => options.UseInMemoryDatabase("TestDb"));
/// 
/// // Example 5: Complete application setup
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// builder.Services
///     .AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
///         options => options.UseSqlServer(
///             builder.Configuration.GetConnectionString("DefaultConnection")),
///         jumpStart => jumpStart
///             .RegisterUserContext&lt;HttpUserContext&gt;()
///             .UseRepositoryLifetime(ServiceLifetime.Scoped))
///     .AddJumpStartAutoMapper(typeof(Program));
/// 
/// // Example DbContext
/// public class ApplicationDbContext : DbContext
/// {
///     public ApplicationDbContext(DbContextOptions&lt;ApplicationDbContext&gt; options)
///         : base(options)
///     {
///     }
///     
///     public DbSet&lt;Product&gt; Products { get; set; }
///     public DbSet&lt;Order&gt; Orders { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         base.OnModelCreating(modelBuilder);
///         // Configure entities
///     }
/// }
/// </code>
/// </example>
public static class JumpStartServiceCollectionExtensionsDbContext
{
    /// <summary>
    /// Adds JumpStart framework services along with Entity Framework Core DbContext registration.
    /// This convenience method combines both registrations with automatic assembly scanning.
    /// </summary>
    /// <typeparam name="TContext">
    /// The DbContext type to register. Must inherit from <see cref="DbContext"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="optionsAction">
    /// Configuration action for DbContext options. Use this to specify the database provider
    /// (UseSqlServer, UseSqlite, etc.) and connection string.
    /// </param>
    /// <param name="configure">
    /// Optional configuration action for customizing JumpStart behavior. Use this to register
    /// user context, configure lifetimes, or manually register repositories.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations in order:
    /// 1. Registers the specified DbContext with Entity Framework Core using the provided options
    /// 2. Registers all JumpStart framework services
    /// 3. Automatically scans the DbContext's assembly for repository implementations
    /// 4. Applies any custom JumpStart configuration provided
    /// </para>
    /// <para>
    /// <strong>Automatic Assembly Scanning:</strong>
    /// The method automatically adds the DbContext's assembly to the repository scanning list.
    /// This means all repository implementations in the same assembly as your DbContext will be
    /// automatically discovered and registered. If you have repositories in other assemblies,
    /// use the configure action to add them:
    /// <code>
    /// configure: jumpStart => jumpStart.ScanAssembly(typeof(OtherRepository).Assembly)
    /// </code>
    /// </para>
    /// <para>
    /// <strong>DbContext Lifetime:</strong>
    /// The DbContext is registered with Scoped lifetime by default (EF Core standard).
    /// Repositories are also registered as Scoped by default to match the DbContext lifetime.
    /// </para>
    /// <para>
    /// <strong>Performance Consideration:</strong>
    /// Assembly scanning happens once at startup. If your DbContext assembly is large,
    /// consider using manual repository registration for faster startup times.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Minimal setup - SQL Server
    /// services.AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseSqlServer(connectionString));
    /// 
    /// // With user context for audit tracking
    /// services.AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseSqlServer(connectionString),
    ///     jumpStart => jumpStart.RegisterUserContext&lt;CurrentUserService&gt;());
    /// 
    /// // With additional configuration
    /// services.AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseSqlServer(connectionString),
    ///     jumpStart => jumpStart
    ///         .RegisterUserContext&lt;UserContext&gt;()
    ///         .ScanAssembly(typeof(ExtraRepository).Assembly)
    ///         .UseRepositoryLifetime(ServiceLifetime.Scoped));
    /// 
    /// // SQLite for development
    /// services.AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseSqlite("Data Source=app.db"));
    /// 
    /// // PostgreSQL with connection pooling
    /// services.AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseNpgsql(
    ///         connectionString,
    ///         npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));
    /// 
    /// // Chained with other services
    /// services
    ///     .AddJumpStartWithDbContext&lt;AppDbContext&gt;(
    ///         options => options.UseSqlServer(connectionString),
    ///         jumpStart => jumpStart.RegisterUserContext&lt;UserContext&gt;())
    ///     .AddJumpStartAutoMapper(typeof(Program))
    ///     .AddAuthentication()
    ///     .AddCookie();
    /// </code>
    /// </example>
    public static IServiceCollection AddJumpStartWithDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction,
        Action<JumpStartOptions>? configure = null)
        where TContext : DbContext
    {
        // Register DbContext
        services.AddDbContext<TContext>(optionsAction);

        // Register JumpStart services
        services.AddJumpStart(options =>
        {
            // Apply custom configuration
            configure?.Invoke(options);

            // Auto-scan the DbContext's assembly
            options.ScanAssembly(typeof(TContext).Assembly);
        });

        return services;
    }
}