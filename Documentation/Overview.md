# NuGet Versioning and Release Process Overview

This document outlines the simplified versioning and release process for the TimeWarp.Mediator and TimeWarp.Mediator.Contracts NuGet packages, using MinVer and GitHub Actions.

## Repository Structure

- **Projects**: Located in `src/TimeWarp.Mediator` and `src/TimeWarp.Mediator.Contracts`.
- **Primary Branch**: `master`.
- **Workflow**: `.github/workflows/ci-cd.yml`.
- **Version Configuration**: `Directory.Build.props`.

## Versioning Strategy

### Unified Versioning
Both packages share the same version number, configured in a single location.

### Source of Truth
`<Version>` in `Directory.Build.props` (e.g., `12.4.0`) applies to both packages.

### MinVer Configuration
- **Tag Prefix**: `v` (e.g., `v12.5.0`)
- **Stable releases**: Tag `v12.5.0` → Version `12.5.0` for both packages
- **CI prereleases**: Appends `-alpha.<commit-height>` (e.g., `12.5.0-alpha.5`) based on commits since the last tag

### Version Properties
- **`<AssemblyVersion>`**: Set to `Major.Minor.Patch.0` (e.g., `12.5.0.0`)
- **`<FileVersion>` and `<InformationalVersion>`**: Use full MinVer version (e.g., `12.5.0-alpha.5`)

### Package References
TimeWarp.Mediator uses `ProjectReference` to TimeWarp.Mediator.Contracts, ensuring they're always built and versioned together.

## CI Builds

### Triggers
- Push to `master`
- Pull requests to `master`
- GitHub releases

### Process
1. Builds both packages with MinVer-generated versions
2. For pushes: Publishes prerelease packages to MyGet
3. For PRs: Build and test only (no publishing)
4. For releases: Publishes stable packages to both MyGet and NuGet.org

### Prerelease Detection
Automatically detects alpha/beta versions and prevents accidental stable releases.

## Stable Releases

### Simple Process
1. Update `<Version>` in `Directory.Build.props` (e.g., from `12.4.0` to `12.5.0`)
2. Commit and push changes
3. Create GitHub release with tag `v12.5.0`
4. CI/CD automatically builds and publishes both packages

### GitHub Release Creation
```bash
# Using GitHub CLI
gh release create v12.5.0 --generate-notes

# Or use GitHub UI to create release
```

## Usage

### Setup
1. Configure `MYGET_TIMEWARP_MEDIATOR_CI_API_KEY` and `NUGET_API_KEY` in GitHub Secrets
2. MyGet feed is already configured in workflow

### CI Prereleases
1. Push to `master`
2. Both packages (e.g., `12.5.0-alpha.5`) published to MyGet automatically

### Stable Release
1. Edit `Directory.Build.props`:
   ```xml
   <Version>12.5.0</Version> <!-- Update this line -->
   ```
2. Commit and push:
   ```bash
   git add Directory.Build.props
   git commit -m "Bump version to 12.5.0"
   git push origin master
   ```
3. Create release:
   ```bash
   gh release create v12.5.0 --title "Release v12.5.0" --generate-notes
   ```

### Verification
1. Check GitHub Actions logs
2. Confirm packages on MyGet (prereleases) or MyGet/NuGet.org (stable)

## Key Benefits

- **Simplicity**: Single version number for both packages in one file
- **Consistency**: Packages always version together
- **No Scripts**: Simple enough to do manually
- **Reliable**: ProjectReference ensures packages build together
- **Transparent**: Anyone can understand and execute the release process