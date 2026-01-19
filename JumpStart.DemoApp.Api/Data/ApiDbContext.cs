using JumpStart.Data;
using JumpStart.DemoApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.DemoApp.Api.Data;

/// <summary>
/// Database context for the API project containing Product-related entities.
/// Inherits from JumpStartDbContext to ensure framework data (QuestionTypes, etc.) is seeded automatically.
/// </summary>
/// <remarks>
/// This context is separate from ApplicationDbContext which handles Identity in the Blazor app.
/// By inheriting from <see cref="JumpStartDbContext"/>, framework-required data like QuestionTypes
/// for the Forms module is automatically included in migrations and seeded.
/// </remarks>
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : JumpStartDbContext(options)
{
    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Configures the model for this context.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model.</param>
    /// <remarks>
    /// ⚠️ IMPORTANT: Always call base.OnModelCreating(modelBuilder) first to ensure
    /// framework configurations are applied.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⚠️ Call base first - applies framework configurations and seeds framework data
        base.OnModelCreating(modelBuilder);

        // Application-specific configurations
        // (Add your entity configurations here)
    }
}
