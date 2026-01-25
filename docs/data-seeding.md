# JumpStart Data Seeding Guide

## Overview

JumpStart provides data seeding through **two complementary approaches**:

1. **Framework Data (HasData)** - Essential reference data seeded via EF Core migrations - **fully automatic**
2. **Consumer Data (Runtime Seeding)** - Optional application data seeded at runtime - **opt-in, explicit call**

This ensures framework features work out-of-the-box while giving consumers full control over their own data.

## Framework Data Seeding (Automatic)

### How It Works

Framework-required data (like QuestionTypes for Forms) is seeded using EF Core's `HasData()` method in entity configurations. This data:

- ✅ **Included in migrations** - Part of the schema definition
- ✅ **Automatic** - No consumer action needed
- ✅ **Version-controlled** - Tracked in migration history
- ✅ **Idempotent** - EF Core prevents duplicates
- ✅ **Production-safe** - Uses fixed GUIDs, stable across environments

### QuestionTypes Example

The Forms module seeds 8 default question types automatically:

```csharp
// In QuestionTypeConfiguration.cs
builder.HasData(
    new QuestionType
    {
        Id = new Guid("10000000-0000-0000-0000-000000000001"),
        Code = "ShortText",
        Name = "Short Text",
        HasOptions = false,
        InputType = "text",
        DisplayOrder = 1
    },
    // ... 7 more types
);
```

### Consumer Requirements

**Your DbContext must inherit from `JumpStartDbContext`:**

```csharp
// ✅ CORRECT
public class ApplicationDbContext : JumpStartDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⚠️ Call base first to apply framework configurations
        base.OnModelCreating(modelBuilder);

        // Your entity configurations here
    }
}
```

**Runtime validation ensures this:**

```csharp
services.AddJumpStart(); // ✅ Validates DbContext inheritance
```

If you forget to inherit from `JumpStartDbContext`, you'll get a clear error:

```
System.InvalidOperationException: DbContext type 'ApplicationDbContext' must inherit from 'JumpStartDbContext' to ensure framework data is seeded correctly.
```

### Creating Migrations

When you create a migration, framework seed data is automatically included:

```bash
dotnet ef migrations add InitialCreate
# Migration includes QuestionTypes seeding
```

Apply the migration:

```bash
dotnet ef database update
# QuestionTypes are now in the database
```

**That's it!** No consumer code needed. Forms module works immediately.

## Consumer Data Seeding (Optional Runtime Approach)

### Components

1. **`IDataSeeder` Interface** - Contract with `IsFrameworkRequired` property
2. **`DataSeederExtensions`** - Discovery and execution engine
3. **`FrameworkSeedingExtensions`** - Auto-execution trigger
4. **Module Seeders** - Individual implementations (e.g., `FormsDataSeeder`)

### Two-Tier System

| Type | Property | Execution | Use Cases |
|------|----------|-----------|-----------|
| **Framework** | `IsFrameworkRequired = true` | Automatic on startup | QuestionTypes, system roles, core lookup data |
| **Consumer** | `IsFrameworkRequired = false` | Explicit via `SeedDataAsync()` | Sample products, test users, demo data |

### Execution Flow

```
Startup → Framework Seeders (Auto) → Migrations → Consumer Seeders (Opt-In)
```

## Creating a Framework-Required Seeder

Framework seeders provide data that JumpStart **needs to function**. They run automatically.

### 1. Implement IDataSeeder

```csharp
using JumpStart.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Data.Seeding.MyModule;

public class MyModuleDataSeeder(ILogger<MyModuleDataSeeder> logger) : IDataSeeder
{
    public string Name => "My Module Reference Data";

    // ✅ Mark as framework-required
    public bool IsFrameworkRequired => true;

    // Framework data should run early (0-199)
    public int Order => 100;

    public async Task SeedAsync(DbContext context)
    {
        logger.LogInformation("Seeding {SeederName}...", Name);

        // ✅ IDEMPOTENT CHECK - Don't duplicate data
        if (await context.Set<MyEntity>().AnyAsync())
        {
            logger.LogInformation("{SeederName}: Data already exists, skipping.", Name);
            return;
        }

        // Create seed data
        var data = new[]
        {
            new MyEntity { Code = "A", Name = "Option A" },
            new MyEntity { Code = "B", Name = "Option B" }
        };

        // Bulk insert
        context.Set<MyEntity>().AddRange(data);
        await context.SaveChangesAsync();

        logger.LogInformation("{SeederName}: Seeded {Count} records.", Name, data.Length);
    }
}
```

### 2. Register the Seeder

In your module's `ServiceCollectionExtensions` partial class:

```csharp
private static void RegisterMyModuleServices(IServiceCollection services, JumpStartOptions options)
{
    // Register your seeder as transient
    services.TryAddTransient<IDataSeeder, MyModuleDataSeeder>();

    // ... other registrations
}
```

### 3. Framework Seeders Run Automatically

In your **API/Blazor project's** `Program.cs`:

```csharp
var app = builder.Build();

// ✅ Framework seeders run automatically here
await app.Services.EnsureFrameworkDataSeededAsync();

app.Run();
```

**Done!** Your module's data is now automatically available.

## Creating a Consumer Seeder

Consumer seeders provide **optional application data**. They only run when explicitly called.

### 1. Implement IDataSeeder

```csharp
public class SampleProductsSeeder(ILogger<SampleProductsSeeder> logger) : IDataSeeder
{
    public string Name => "Sample Products";

    // ✅ Mark as consumer data
    public bool IsFrameworkRequired => false;

    // Consumer data should run later (300+)
    public int Order => 500;

    public async Task SeedAsync(DbContext context)
    {
        // Only seed in development
        if (!Environment.IsDevelopment())
            return;

        if (await context.Set<Product>().AnyAsync())
            return;

        var products = new[]
        {
            new Product { Name = "Widget", Price = 9.99m },
            new Product { Name = "Gadget", Price = 19.99m }
        };

        context.Set<Product>().AddRange(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} sample products.", products.Length);
    }
}
```

### 2. Register the Seeder

```csharp
// In your application's Startup/Program.cs
services.TryAddTransient<IDataSeeder, SampleProductsSeeder>();
```

### 3. Execute Consumer Seeders Explicitly

```csharp
var app = builder.Build();

// Framework seeding (required)
await app.Services.EnsureFrameworkDataSeededAsync();

// Consumer seeding (optional)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    await context.SeedDataAsync(scope.ServiceProvider); // ✅ Runs SampleProductsSeeder
}

app.Run();
```

## Execution Order Guidelines

Use these ranges for `Order` property based on seeder type:

| Range | Type | Purpose | Examples |
|-------|------|---------|----------|
| **0-99** | Framework | Critical core data | System roles, core config |
| **100-199** | Framework | Module reference data | QuestionTypes, StatusCodes |
| **200-299** | Consumer | Application lookup data | Categories, Countries |
| **300-999** | Consumer | Sample/demo data | Test users, sample products |
| **1000+** | Consumer | Optional data | Can be skipped |

### Example Order Values

```csharp
// Framework seeders (IsFrameworkRequired = true)
public int Order => 10;   // System config (critical framework)
public int Order => 100;  // Forms QuestionTypes (module framework)
public int Order => 150;  // Workflow StatusCodes (module framework)

// Consumer seeders (IsFrameworkRequired = false)
public int Order => 200;  // Product Categories (app data)
public int Order => 500;  // Sample products (demo data)
```

## Built-In Seeders

### FormsDataSeeder

**Type:** Framework-Required (`IsFrameworkRequired = true`)  
**Order:** 100  
**Seeds:** 8 default QuestionTypes (ShortText, LongText, Number, Date, Boolean, SingleChoice, MultipleChoice, Dropdown)  
**Execution:** Automatic when Forms module is used

```csharp
// Automatically registered and executed via:
await app.Services.EnsureFrameworkDataSeededAsync();
```

## Best Practices

### ✅ DO - All Seeders
- **Populate Ids** - Use fixed GUIDs for framework data, never `Guid.NewGuid()`

### ✅ DO - Framework Seeders

- **Mark essential data as framework-required** - Data the framework needs to function
- **Keep framework seeders idempotent** - Check if data exists before adding
- **Run early** - Use Order 0-199 for framework data
- **Never fail silently** - Framework seeders throw on error (app won't start without required data)
- **Minimize dependencies** - Framework seeders should be self-contained

### ✅ DO - Consumer Seeders

- **Mark optional data as consumer** - `IsFrameworkRequired = false`
- **Use for dev/test only** - Check environment before seeding sample data
- **Log progress** - Help developers understand what's being seeded
- **Use later order** - Order 200+ for consumer data
- **Allow failures** - Non-critical seeders log warnings but don't stop app

### ❌ DON'T

- **Don't duplicate data** - Always check if records exist first
- **Don't use `Guid.NewGuid()`** - use hard coded ids, or new initial records will be generated at every migration.
- **Don't seed production data** - Keep seeders for dev/test only (check environment)
- **Don't throw on duplicates** - Return early if data exists
- **Don't modify existing data** - Only add new records
- **Don't mark consumer data as framework-required** - Only truly essential data

## Complete Example

### Framework Seeder (Runs Automatically)

```csharp
// 1. Create framework seeder
namespace JumpStart.Data.Seeding.Workflow;

public class WorkflowDataSeeder(ILogger<WorkflowDataSeeder> logger) : IDataSeeder
{
    public string Name => "Workflow Status Codes";
    public bool IsFrameworkRequired => true;  // ✅ Framework needs this!
    public int Order => 150;

    public async Task SeedAsync(DbContext context)
    {
        if (await context.Set<WorkflowStatus>().AnyAsync())
            return;

        var statuses = new[]
        {
            new WorkflowStatus { Code = "Draft", Name = "Draft" },
            new WorkflowStatus { Code = "Pending", Name = "Pending" },
            new WorkflowStatus { Code = "Approved", Name = "Approved" }
        };

        context.Set<WorkflowStatus>().AddRange(statuses);
        await context.SaveChangesAsync();
    }
}

// 2. Register in ServiceCollectionExtensions
private static void RegisterWorkflowServices(IServiceCollection services, JumpStartOptions options)
{
    services.TryAddTransient<IDataSeeder, WorkflowDataSeeder>();
}

// 3. Runs automatically in Program.cs
await app.Services.EnsureFrameworkDataSeededAsync(); // ✅ WorkflowDataSeeder runs here
```

### Consumer Seeder (Runs Explicitly)

```csharp
// 1. Create consumer seeder
public class SampleDataSeeder(ILogger<SampleDataSeeder> logger) : IDataSeeder
{
    public string Name => "Sample Products";
    public bool IsFrameworkRequired => false;  // ✅ Optional consumer data
    public int Order => 500;

    public async Task SeedAsync(DbContext context)
    {
        // Only in development
        if (!Environment.IsDevelopment())
            return;

        if (await context.Set<Product>().AnyAsync())
            return;

        var products = new[] { /* sample products */ };
        context.Set<Product>().AddRange(products);
        await context.SaveChangesAsync();
    }
}

// 2. Register in Program.cs
builder.Services.TryAddTransient<IDataSeeder, SampleDataSeeder>();

// 3. Execute explicitly after framework seeding
await app.Services.EnsureFrameworkDataSeededAsync();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    await context.SeedDataAsync(scope.ServiceProvider); // ✅ SampleDataSeeder runs here
}
```

## Summary

✅ **Two-Tier System** - Framework vs. Consumer seeding  
✅ **Automatic Framework Seeding** - Essential data seeds on startup  
✅ **Opt-In Consumer Seeding** - Application data requires explicit call  
✅ **Modular** - Each module owns its seed data  
✅ **Ordered** - Control execution sequence  
✅ **Idempotent** - Safe to run multiple times  
✅ **Testable** - Unit test individual seeders  
✅ **Flexible** - Add new seeders without modifying existing code  

### Key Takeaways

1. **Framework seeders** (`IsFrameworkRequired = true`) run automatically - no consumer action needed
2. **Consumer seeders** (`IsFrameworkRequired = false`) run only when explicitly called
3. Always call `EnsureFrameworkDataSeededAsync()` in `Program.cs` after building the app
4. Only call `SeedDataAsync()` if you have optional consumer data to seed

For questions or issues, see the [JumpStart documentation](../README.md).
