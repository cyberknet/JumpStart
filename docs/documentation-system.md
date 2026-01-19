# JumpStart Documentation System Overview

This document describes the documentation architecture for the JumpStart framework.

## Documentation Goals

1. **Comprehensive** - Cover all public APIs with detailed descriptions
2. **Accessible** - Easy to navigate and search
3. **Practical** - Include real-world examples and use cases
4. **Up-to-date** - Automatically generated from source code
5. **Beautiful** - Professional, modern appearance

## Architecture

### Three Documentation Layers

#### 1. Conceptual Documentation (`docs/`)
Hand-written Markdown files covering:
- Getting started guides
- Core concepts
- How-to guides
- Architecture decisions
- Samples and tutorials

**Format:** Markdown (`.md`)  
**Maintained:** Manually by contributors  
**Purpose:** Explain concepts, provide context, teach patterns

#### 2. API Reference (`api/`)
Auto-generated from XML documentation comments:
- All public types (classes, interfaces, enums)
- All public members (properties, methods, fields)
- Parameter descriptions
- Return value descriptions
- Usage examples
- Remarks and warnings

**Format:** YAML (`.yml`) - generated  
**Source:** XML comments in C# code  
**Purpose:** Complete API surface documentation

#### 3. Website (`_site/`)
Static HTML website combining both:
- Beautiful, searchable interface
- Syntax highlighting
- Cross-references
- Mobile-friendly
- Fast and lightweight

**Format:** HTML/CSS/JS  
**Generated:** By DocFX from Markdown + XML  
**Purpose:** User-friendly browsing experience

## Technology Stack

### DocFX
Microsoft's documentation generation tool

**Features:**
- Converts XML comments to documentation
- Processes Markdown to HTML
- Creates navigation and search
- Handles cross-references
- Supports custom themes

**Website:** https://dotnet.github.io/docfx/

### Markdown
Simple, readable markup language

**Advantages:**
- Easy to write and edit
- Version control friendly
- Widely supported
- GitHub native

### YAML
Data serialization format for metadata

**Used for:**
- Table of contents (TOC)
- Navigation structure
- API metadata

## File Organization

```
JumpStart/
?
??? docs/                          # Conceptual documentation
?   ??? index.md                  # Documentation home page
?   ??? toc.yml                   # Main navigation
?   ??? getting-started.md        # Quick start
?   ??? core-concepts.md          # Framework fundamentals
?   ??? audit-tracking.md         # Audit feature guide
?   ??? api-development.md        # API building guide
?   ??? authentication.md         # Security guide
?   ??? samples.md                # Sample applications
?   ??? faq.md                    # Common questions
?   ??? troubleshooting.md        # Problem solving
?   ?
?   ??? how-to/                   # Task-oriented guides
?   ?   ??? index.md             # How-to index
?   ?   ??? custom-repository.md
?   ?   ??? pagination.md
?   ?   ??? secure-endpoints.md
?   ?
?   ??? architecture/             # Design documentation
?   ?   ??? index.md
?   ?   ??? adr/                 # Architecture decisions
?   ?   ?   ??? index.md
?   ?   ?   ??? 001-repository-pattern.md
?   ?   ?   ??? 002-simple-advanced-entities.md
?   ?   ?   ??? 003-audit-tracking.md
?   ?   ?   ??? 004-jwt-authentication.md
?   ?   ?   ??? 005-refit-api-clients.md
?   ?
?   ??? api/                      # API reference
?       ??? index.md              # API home
?       ??? toc.yml               # API navigation
?
??? JumpStart/                     # Source code
?   ??? *.cs                      # With XML comments
?
??? docfx.json                     # DocFX configuration
??? build-docs.cmd                 # Windows build script
??? build-docs.sh                  # Unix build script
??? DOCUMENTATION.md               # This overview
?
??? _site/                         # Generated website (gitignored)
    ??? [Generated HTML/CSS/JS]
```

## XML Documentation Standards

### Required Elements

#### Classes/Interfaces
```csharp
/// <summary>
/// Brief one-line description.
/// </summary>
/// <remarks>
/// Detailed explanation with multiple paragraphs if needed.
/// Include when to use, design decisions, etc.
/// </remarks>
/// <example>
/// <code>
/// var instance = new MyClass();
/// </code>
/// </example>
public class MyClass { }
```

#### Methods
```csharp
/// <summary>
/// Brief description of what the method does.
/// </summary>
/// <param name="id">Description of the parameter.</param>
/// <param name="options">Description with more detail.</param>
/// <returns>
/// Description of what is returned.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when id is null.
/// </exception>
/// <example>
/// <code>
/// var result = await GetByIdAsync(id);
/// </code>
/// </example>
public async Task<Entity?> GetByIdAsync(Guid id) { }
```

#### Properties
```csharp
/// <summary>
/// Gets or sets the unique identifier.
/// </summary>
/// <value>
/// A Guid representing the entity's unique identifier.
/// </value>
/// <remarks>
/// This value is automatically generated when the entity is created.
/// </remarks>
public Guid Id { get; set; }
```

### Style Guidelines

1. **Be Concise** - Summary should be one clear sentence
2. **Be Complete** - Include remarks for complex scenarios
3. **Be Helpful** - Add examples for non-obvious usage
4. **Be Consistent** - Follow established patterns
5. **Be Accurate** - Keep documentation in sync with code

## Markdown Best Practices

### Structure
- Use clear heading hierarchy (H1 ? H2 ? H3)
- Keep paragraphs short
- Use lists for steps or options
- Include code blocks with language tags

### Links
```markdown
[Link Text](relative/path/to/file.md)
[External Link](https://example.com)
[API Reference](xref:JumpStart.Data.SimpleEntity)
```

### Code Blocks
````markdown
```csharp
public class Example
{
    // Code here
}
```
````

### Callouts
```markdown
> **Note:** Important information
> **Warning:** Proceed with caution
> **Tip:** Helpful suggestion
```

## Build Process

### Local Build
1. Run `build-docs.cmd` (Windows) or `build-docs.sh` (Unix)
2. DocFX reads `docfx.json`
3. Extracts XML from compiled assemblies
4. Converts XML to YAML metadata
5. Processes Markdown files
6. Generates HTML website
7. Output to `_site/` folder

### CI/CD Pipeline
1. Push to `main` branch triggers workflow
2. GitHub Actions runs build
3. Deploys to GitHub Pages
4. Accessible at project documentation URL

## Maintenance

### When Code Changes
- Update XML comments if API changes
- Run documentation build locally
- Verify changes look correct
- Commit both code and conceptual doc updates

### When Adding Features
- Add XML comments to new public APIs
- Create how-to guide if complex
- Update relevant conceptual docs
- Add to API TOC if needed
- Include in ADR if architectural

### Regular Reviews
- Check for broken links (quarterly)
- Update screenshots if UI changed
- Refresh examples with current practices
- Archive outdated guides
- Improve clarity based on questions

## Quality Checklist

Before merging documentation changes:

- [ ] All public APIs have XML comments
- [ ] Examples compile and run
- [ ] Links work (no 404s)
- [ ] Code follows style guide
- [ ] Markdown renders correctly
- [ ] DocFX build succeeds
- [ ] Generated site looks good
- [ ] Mobile-friendly
- [ ] Search works
- [ ] No spelling errors

## Tools and Resources

### Documentation Tools
- **DocFX** - https://dotnet.github.io/docfx/
- **Markdig** - Markdown processor
- **Modern Template** - DocFX theme

### Writing Tools
- **VS Code** - Markdown editor with preview
- **Grammarly** - Grammar checking
- **Hemingway** - Readability checking

### Reference
- [Microsoft .NET Docs](https://docs.microsoft.com/dotnet)
- [DocFX Tutorial](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html)
- [Markdown Guide](https://www.markdownguide.org/)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for:
- How to write good documentation
- Code style for examples
- Review process
- Documentation templates

## Support

Questions about documentation?
- **Issues:** https://github.com/cyberknet/JumpStart/issues
- **Discussions:** https://github.com/cyberknet/JumpStart/discussions
- **Email:** See CONTRIBUTING.md

---

**Last Updated:** 2026-01-15  
**DocFX Version:** 2.75+  
**Framework Version:** 1.0.0
