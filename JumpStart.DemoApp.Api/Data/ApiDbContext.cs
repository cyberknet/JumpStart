using JumpStart.DemoApp.Api.Data;
using JumpStart.DemoApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.DemoApp.Api.Data;

/// <summary>
/// Database context for the API project containing only Product-related entities.
/// Separate from ApplicationDbContext which handles Identity in the Blazor app.
/// </summary>
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
