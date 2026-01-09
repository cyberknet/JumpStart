using System;
using JumpStart.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring JumpStart with DbContext.
/// </summary>
public static class JumpStartServiceCollectionExtensionsDbContext
{
    /// <summary>
    /// Adds JumpStart services along with DbContext configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">Configuration action for DbContext.</param>
    /// <param name="configure">Optional configuration action for JumpStart.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // For SQL Server (requires Microsoft.EntityFrameworkCore.SqlServer package):
    /// services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
    ///     options => options.UseSqlServer(connectionString),
    ///     jumpStart => jumpStart.RegisterUserContext&lt;BlazorUserContext&gt;()
    /// );
    /// 
    /// // For SQLite:
    /// services.AddJumpStartWithDbContext&lt;ApplicationDbContext&gt;(
    ///     options => options.UseSqlite(connectionString),
    ///     jumpStart => jumpStart.RegisterUserContext&lt;BlazorUserContext&gt;()
    /// );
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