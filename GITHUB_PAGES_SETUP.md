# GitHub Pages Setup Guide

This guide will help you deploy the JumpStart documentation to GitHub Pages using GitHub Actions.

## Prerequisites

- Repository pushed to GitHub at: https://github.com/cyberknet/JumpStart
- Admin access to the repository settings

## Step 1: Enable GitHub Pages

1. Go to your repository: https://github.com/cyberknet/JumpStart
2. Click **Settings** (top menu)
3. Click **Pages** (left sidebar under "Code and automation")
4. Under **Source**, select: **GitHub Actions**
   - (Not "Deploy from a branch")
5. Save changes

## Step 2: Commit and Push Workflow

The workflow file is already created at `.github/workflows/documentation.yml`

```bash
# Add the workflow and updated docfx.json
git add .github/workflows/documentation.yml
git add docfx.json
git commit -m "Add automated documentation deployment"
git push origin main
```

## Step 3: Monitor the Deployment

1. Go to the **Actions** tab in your repository
2. You should see the "Build and Deploy Documentation" workflow running
3. Wait for it to complete (usually 2-3 minutes)
4. Look for a green checkmark ?

## Step 4: Access Your Documentation

Once deployed, your documentation will be available at:

```
https://cyberknet.github.io/JumpStart/
```

The API reference will be at:
```
https://cyberknet.github.io/JumpStart/api/
```

## What Happens on Each Push

Every time you push to the `main` branch:

1. ? GitHub Actions runner starts
2. ? Checks out your repository
3. ? Sets up .NET 10
4. ? Restores NuGet packages
5. ? Builds JumpStart project (generates XML docs)
6. ? Installs DocFX
7. ? Runs DocFX to generate documentation site
8. ? Deploys to GitHub Pages

## Manual Trigger

You can also trigger the workflow manually:

1. Go to **Actions** tab
2. Select "Build and Deploy Documentation"
3. Click **Run workflow**
4. Select branch: `main`
5. Click **Run workflow** button

## Troubleshooting

### Workflow Fails

**Check the logs:**
1. Click on the failed workflow run
2. Click on the failed job
3. Expand the failed step to see error details

**Common issues:**

- **Permission denied**: Make sure Pages is enabled with "GitHub Actions" source
- **Build fails**: Check that JumpStart.csproj builds successfully locally
- **.NET version**: Workflow uses .NET 10.0.x (matches your project)

### Documentation Not Showing

**Clear browser cache:**
- GitHub Pages can take a few minutes to update
- Try Ctrl+F5 to hard refresh
- Try incognito/private browsing mode

**Check deployment:**
- Go to Actions tab
- Verify both "build" and "deploy" jobs succeeded
- Check the deployment URL in the workflow output

### Local Build Works But CI Fails

The workflow builds in **Release** mode. Test locally:

```bash
# Build in Release mode
dotnet build JumpStart/JumpStart.csproj --configuration Release

# Build docs
build-docs.cmd
```

## Updating Documentation

### Updating XML Comments

1. Edit code in `JumpStart/*.cs` files
2. Update XML documentation comments
3. Commit and push:
   ```bash
   git add JumpStart/
   git commit -m "Update API documentation"
   git push
   ```
4. Docs automatically rebuild and deploy!

### Updating Conceptual Docs

1. Edit markdown files in `docs/*.md`
2. Commit and push:
   ```bash
   git add docs/
   git commit -m "Update getting started guide"
   git push
   ```
3. Docs automatically rebuild and deploy!

## Configuration Files

### `.github/workflows/documentation.yml`
GitHub Actions workflow that builds and deploys documentation.

### `docfx.json`
DocFX configuration:
- Metadata extraction from DLL
- Content sources
- Build settings
- Website configuration

### `.gitignore`
Excludes generated files:
- `_site/` - Generated website
- `api/` - Generated API metadata
- `obj/` - Build artifacts

## Testing Locally Before Push

Always test documentation builds locally before pushing:

```bash
# Windows
build-docs.cmd

# Linux/Mac
./build-docs.sh

# Verify at: http://localhost:8080
docfx serve _site
```

## Custom Domain (Optional)

To use a custom domain like `docs.jumpstart.dev`:

1. Add `CNAME` file to repository root:
   ```
   docs.jumpstart.dev
   ```

2. Configure DNS:
   - Add CNAME record pointing to: `cyberknet.github.io`

3. In GitHub Settings ? Pages:
   - Enter custom domain
   - Enable HTTPS

## Next Steps

? Enable GitHub Pages (see Step 1 above)  
? Push the workflow file (see Step 2 above)  
? Watch your documentation deploy automatically!  

---

**Questions?** Open an issue at: https://github.com/cyberknet/JumpStart/issues
