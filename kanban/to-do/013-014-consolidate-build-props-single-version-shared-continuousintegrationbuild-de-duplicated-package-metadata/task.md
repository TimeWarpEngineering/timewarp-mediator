# Consolidate build props: single Version, shared ContinuousIntegrationBuild, de-duplicated package metadata

## Description

Package metadata and the version live in several files that can drift. Parent review task 013, findings B-3, B-13, C-23.

- B-3: `<Version>14.0.0-beta.4</Version>` is declared in both `Directory.Build.props` and `source/Directory.Build.props`; the equality assert runs only on the release path.
- B-13: `ContinuousIntegrationBuild` is set only in `timewarp-mediator.csproj`, so Contracts, Analyzers, and Generators nupkgs are not fully deterministic.
- C-23: `Authors` conflicts (root "Steven T. Cramer" vs source "TimeWarpEngineering"); `PackageProjectUrl` conflicts (GitHub vs timewarp.software); icon, readme, license, repository fields repeated; `LangVersion` repeated in csproj files; the TimeWarp.Mediator description is still the MediatR tagline and says nothing about source generation; both core csproj files start with a "Modified by" comment.

## Requirements

- One `Version` in the root props; delete the duplicate; keep the SSOT assert but run it in the PR pipeline too (or delete it once the duplicate is gone).
- Move `ContinuousIntegrationBuild` to the root props.
- One owner per property; resolve the Authors and URL conflicts with the maintainer.
- Rewrite package descriptions for all four packages.
- Verify with `./bin/dev pack` and inspect the nuspec of each nupkg.

## Checklist

- [ ] Single Version
- [ ] Shared CI build flag
- [ ] Metadata de-duplication and conflict resolution
- [ ] Package descriptions
- [ ] nuspec inspection recorded in Results; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
