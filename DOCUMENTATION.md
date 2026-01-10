# Documentation Quick Start Guide

This guide will help you build and view the JumpStart API documentation locally.

## Prerequisites

1. **.NET 10 SDK** - Already installed if you're building JumpStart
2. **DocFX** - Documentation generation tool

### Install DocFX

```bash
dotnet tool install -g docfx
```

Or if already installed, update to latest:

```bash
dotnet tool update -g docfx
```

Verify installation:

```bash
docfx --version
```

## Building Documentation

### Option 1: Use Build Scripts (Recommended)

**Windows:**
```cmd
build-docs.cmd
```

**Linux/macOS:**
```bash
chmod +x build-docs.sh
./build-docs.sh
```

The script will:
1. Check DocFX installation
2. Clean previous builds
3. Generate API metadata from XML comments
4. Build the documentation site
5. Optionally serve it locally

### Option 2: Manual Build

```bash
# From the repository root directory

# Build documentation
docfx docfx.json

# Serve locally (optional)
docfx serve _site
```

## Viewing Documentation

After building:

### Option 1: Open in Browser
Open `_site/index.html` in your web browser

### Option 2: Local Web Server
```bash
docfx serve _site
```

Then navigate to: http://localhost:8080

## Documentation Structure

```
JumpStart/
??? docs/                      # Conceptual documentation
?   ??? index.md              # Documentation home
?   ??? getting-started.md    # Quick start guide
?   ??? core-concepts.md      # Framework concepts
?   ??? api/                  # API reference
?   ?   ??? index.md         # API home
?   ?   ??? toc.yml          # API table of contents
?   ??? how-to/              # Task guides
?   ??? architecture/        # Design docs
?   ??? samples.md           # Examples
??? docfx.json               # DocFX configuration
??? build-docs.cmd           # Windows build script
??? build-docs.sh            # Linux/Mac build script
```

## Generated Files (Not in Git)

After building, these folders are created:

- `_site/` - Complete documentation website
- `api/` - Generated API metadata (YAML files)
- `obj/` - Temporary build files

These are in `.gitignore` and should not be committed.

## Troubleshooting

### DocFX Not Found

**Error:** `'docfx' is not recognized as an internal or external command`

**Solution:**
```bash
dotnet tool install -g docfx
```

Make sure `~/.dotnet/tools` is in your PATH.

### Build Errors

**Error:** `Cannot find project file`

**Solution:** Run the build command from the repository root directory (where `docfx.json` is located).

### Missing XML Documentation

**Error:** `No XML documentation found`

**Solution:** The JumpStart project already has `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the csproj. If you still see this error, rebuild the project:

```bash
dotnet build JumpStart/JumpStart.csproj
```

### Port Already in Use

**Error:** `Address already in use: bind to 127.0.0.1:8080`

**Solution:** Either stop the process using port 8080, or specify a different port:

```bash
docfx serve _site --port 8081
```

## Continuous Integration

Documentation is automatically built and deployed when:

- Code is pushed to the `main` branch
- A pull request is created

The documentation is deployed to GitHub Pages.

### Manual Deployment

The GitHub Actions workflow can also be triggered manually:

1. Go to the repository on GitHub
2. Click "Actions" tab
3. Select "Build and Deploy Documentation"
4. Click "Run workflow"

## Writing Documentation

### XML Comments in Code

All public APIs should have XML documentation:

```csharp
/// <summary>
/// Gets an entity by its unique identifier.
/// </summary>
/// <param name="id">The entity's unique identifier.</param>
/// <returns>The entity if found; otherwise, null.</returns>
/// <example>
/// <code>
/// var product = await repository.GetByIdAsync(productId);
/// if (product != null)
/// {
///     Console.WriteLine($"Found: {product.Name}");
/// }
/// </code>
/// </example>
public async Task<Product?> GetByIdAsync(Guid id)
{
    return await _dbSet.FindAsync(id);
}
```

### Conceptual Documentation

Markdown files in `docs/` folder:

- Use clear headings
- Include code examples
- Link to related topics
- Add diagrams when helpful

## Documentation Best Practices

1. **Be Consistent** - Follow existing patterns
2. **Include Examples** - Show real-world usage
3. **Keep It Updated** - Update docs with code changes
4. **Test Locally** - Build docs before committing
5. **Link Generously** - Connect related topics

## Getting Help

- **DocFX Documentation:** https://dotnet.github.io/docfx/
- **JumpStart Issues:** https://github.com/cyberknet/JumpStart/issues
- **Contributing Guide:** See CONTRIBUTING.md

## Next Steps

- Read [Contributing to Documentation](../CONTRIBUTING.md#documentation)
- Explore the [DocFX documentation](https://dotnet.github.io/docfx/)
- Review existing XML comments for examples
- Check the [Architecture Decision Records](architecture/adr/index.md)
