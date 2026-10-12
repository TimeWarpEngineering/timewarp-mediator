# NuGet Versioning and Release Process Overview

This document outlines the simplified versioning and release process for the TimeWarp.Mediator and TimeWarp.Mediator.Contracts NuGet packages, using MinVer and GitHub Actions.

## Repository Structure

- **Projects**: Located in `source/timewarp-mediator` and `source/timewarp-mediator-contracts`.
- **Primary Branch**: `master`.
- **Workflow**: `.github/workflows/workflow.yml` (jobs `ci` and `publish`).
- **Version Configuration**: `source/Directory.Build.props`.

## Versioning Strategy

### Unified Versioning
Both packages share the same version number, configured in a single location.

### Source of Truth
`<Version>` in `source/Directory.Build.props` (e.g., `12.4.0`) applies to both packages.

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
1. Update `<Version>` in `source/Directory.Build.props` (e.g., from `12.4.0` to `12.5.0`)
2. Move every rule row in `source/timewarp-mediator-analyzers/AnalyzerReleases.Unshipped.md` to the
   `## Release 12.5.0` section of `AnalyzerReleases.Shipped.md` (see [Analyzer release tracking](#analyzer-release-tracking))
3. Commit and push changes
4. Create GitHub release with tag `v12.5.0`
5. CI/CD automatically builds and publishes both packages

### GitHub Release Creation
```bash
# Using GitHub CLI
gh release create v12.5.0 --generate-notes

# Or use GitHub UI to create release
```

## Usage

### Setup
1. Configure `MYGET_TIMEWARP_MEDIATOR_CI_API_KEY` in GitHub Secrets (MyGet CI feed)
2. NuGet.org releases use **Trusted Publishing (OIDC)** — no long-lived `NUGET_API_KEY` secret. Only the
   `publish` job in `workflow.yml` holds `id-token: write`; it runs on a published GitHub Release (or the
   break-glass / probe `workflow_dispatch` modes), exchanges a GitHub OIDC token via `NuGet/login` (owner
   `TimeWarp.Enterprises`), and hands the key to `dev workflow --mode release` through the `NUGET_API_KEY`
   environment variable. The NuGet.org policy binds the repo and workflow file `workflow.yml` (not job
   names). Check it with `gh workflow run workflow.yml -f mode=probe`.
3. Before pushing, the `publish` job refuses unless the release tag equals `v` + `<Version>` from
   `source/Directory.Build.props`, the tag points at the checked-out commit, that commit is on `master`, and
   it has a successful `push` CI run of `workflow.yml`. Tests are not re-run in the publish job; that green
   CI run is the test gate. Break-glass dispatch (`mode=release`, `confirm=release`) must run with
   `--ref v<Version>`.

### CI Prereleases
1. Push to `master`
2. Both packages (e.g., `12.5.0-alpha.5`) published to MyGet automatically

### Stable Release
1. Edit `source/Directory.Build.props`:
   ```xml
   <Version>12.5.0</Version> <!-- Update this line -->
   ```
2. Move the rule rows from `AnalyzerReleases.Unshipped.md` to the `## Release 12.5.0` section of
   `AnalyzerReleases.Shipped.md` (both under `source/timewarp-mediator-analyzers/`).
3. Commit and push:
   ```bash
   git add source/Directory.Build.props source/timewarp-mediator-analyzers/AnalyzerReleases.*.md
   git commit -m "Bump version to 12.5.0"
   git push origin master
   ```
4. Create release:
   ```bash
   gh release create v12.5.0 --title "Release v12.5.0" --generate-notes
   ```

### Analyzer release tracking

`TimeWarp.Mediator.Analyzers` uses Roslyn analyzer release tracking (RS2000–RS2008). New or changed
TWM rules go in `AnalyzerReleases.Unshipped.md`. Every release that publishes packages — prereleases
included — moves those rows to `AnalyzerReleases.Shipped.md` and leaves Unshipped with only its header.

Release headers must parse as `System.Version` (RS2007 rejects `## Release 14.0.0-beta.1`), so a
prerelease files its rows under its target version: every 14.0.0 beta adds to `## Release 14.0.0`
and notes the beta in the Notes column (`RequestHasNoHandler, shipped in 14.0.0-beta.1`). Create the
section if it does not exist yet. TWM001–TWM004 shipped in 14.0.0-beta.1; TWM005–TWM011 move at the
next release.

### Verification
1. Check GitHub Actions logs
2. Confirm packages on MyGet (prereleases) or MyGet/NuGet.org (stable)

## Key Benefits

- **Simplicity**: Single version number for both packages in one file
- **Consistency**: Packages always version together
- **No Scripts**: Simple enough to do manually
- **Reliable**: ProjectReference ensures packages build together
- **Transparent**: Anyone can understand and execute the release process