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

- [x] Single Version
- [x] Shared CI build flag
- [x] Metadata de-duplication and conflict resolution
- [x] Package descriptions
- [ ] nuspec inspection recorded in Results; PR with merge commit (nuspec recorded; PR is the host open-pr node)

## Notes

Parent: 013.

Review trail: `review/` (effort 1, general). Disposition is clean — `review/disposition.md`.

## Results

### Decisions

- **Version owner is `source/Directory.Build.props`, not root.** The task says "root props", but the shared
  `TimeWarp.Nuru.DevCli` `check-version` / `release` / tag-assertion commands and the `publish` job guard in
  `workflow.yml` all read `<Version>` from `source/Directory.Build.props` (TimeWarp convention rule 6; same layout
  as timewarp-nuru, -amuru, -jaribu, -state). The root duplicate is deleted. With one copy left there is nothing
  to drift, so `AssertVersionSsot` (and the now-unused `RepoLayout.TryReadVersion`) were deleted from the dev CLI
  rather than wired into the PR pipeline.
- **Package metadata owner is also `source/Directory.Build.props`** (Authors, Product, Copyright, PackageProjectUrl,
  PackageTags, Repository*, license, icon, readme, SourceLink/symbol settings). Root props now keeps only
  build settings (`GenerateDocumentationFile`, `Deterministic`, `ContinuousIntegrationBuild`, LangVersion, analyzers, signing).
- **Conflicts resolved to the TimeWarp sibling-repo convention** (no maintainer was available in the headless walk;
  every sibling package uses these values): `Authors` = `Steven T. Cramer`; `PackageProjectUrl` =
  `https://timewarp.software/projects/timewarp-mediator/`; `RepositoryUrl` without the `.git` suffix.
  Maintainer can override in review.
- **`ContinuousIntegrationBuild`** moved to root props (`Condition="'$(GITHUB_ACTIONS)' == 'true'"`), so all
  four packages are deterministic in CI.
- Removed redundant `LangVersion 10.0` from the core and Contracts csproj (inherited from root). Analyzers and Generators
  keep `latest`, an intentional override. Removed the `<!-- Modified by -->` csproj comments. The per-file `.cs`
  modification notices stay because they are Apache attribution for the MediatR fork.
- Rewrote all four `<Description>`s. TimeWarp.Mediator no longer uses the MediatR tagline and now mentions
  `AddMediator` vs the generated `AddGeneratedMediator`.
- `documentation/overview.md` now points version bumps at `source/Directory.Build.props`.
- Review corrected three metadata sentences before disposition (see Review below): the runtime
  description no longer says the generated mediator builds on this package; `source-generator` and
  `aot` tags are only on TimeWarp.Mediator.Generators; the generator description names analyzer
  diagnostics TWM001–TWM004 and generator diagnostics TWM005–TWM011.

### Verification

- `./bin/dev workflow`: clean, build, test (all pass), and pack with the analyzer layout asserts. **Pipeline SUCCEEDED.**
  The first run hit a stale VBCSCompiler `TypeLoadException` (Analyzers 14.0.0.0 identity cached from another
  worktree). It went green after `dotnet build-server shutdown`. This is unrelated to the change.
- `ganda repo audit`: passes all checks.
- `dotnet msbuild <csproj> -getProperty:... -p:GITHUB_ACTIONS=true` on all four projects:
  `ContinuousIntegrationBuild=true`, `Version=14.0.0-beta.4`, `Authors=Steven T. Cramer`, single RepositoryUrl.
- nuspec inspection (`unzip -p artifacts/packages/<pkg>.nupkg '*.nuspec'`). All four (TimeWarp.Mediator, .Contracts,
  .Analyzers, .Generators) show `version 14.0.0-beta.4`, `authors Steven T. Cramer`,
  `projectUrl https://timewarp.software/projects/timewarp-mediator/`, `license Unlicense`, `icon logo.png`,
  `readme readme.md`, `repository type=git url=https://github.com/TimeWarpEngineering/timewarp-mediator` with commit,
  and the new descriptions.
- Review fix: `ganda repo audit` passes all checks again. `./bin/dev pack` succeeded (analyzer layout asserts passed). Repacked nuspecs:
  TimeWarp.Mediator description states `AddGeneratedMediator()` does not reference this package and
  its tags omit `source-generator` and `aot`; Generators tags include both and the description
  reports TWM001–TWM004 (analyzers) and TWM005–TWM011 (generator).

### Review

Effort 1. Roster: general. Session: Grok `01a12fe0-802e-79d3-9b10-c6a7253e84ad` (2026-10-12).

Rounds: 2. Round 1 open counts were 2 bugs and 1 suggestion. Round 2 is the final ledger:

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

Disposition: **clean**. No wontfix. No escalation.

- M1 (bug, fixed): runtime description no longer claims the generated mediator builds on `TimeWarp.Mediator`.
- M2 (bug, fixed): `source-generator` and `aot` tags are only on TimeWarp.Mediator.Generators.
- M3 (suggestion, fixed): generator description matches `documentation/diagnostics.md` (TWM001–TWM004 bundled analyzers, TWM005–TWM011 reported here).

Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

**Smoke**

```bash
dotnet build-server shutdown
./bin/dev workflow
for f in artifacts/packages/*.nupkg; do echo "== $f"; unzip -p "$f" '*.nuspec' | grep -E '<version>|<authors>|<projectUrl>|<repository|<description>'; done
grep -c '<Version>' Directory.Build.props source/Directory.Build.props
dotnet msbuild source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj -getProperty:ContinuousIntegrationBuild -p:GITHUB_ACTIONS=true
```

**Expect**

- `Pipeline SUCCEEDED`. Each of the 4 nupkgs reports `14.0.0-beta.4`, `Steven T. Cramer`, the timewarp.software
  projectUrl, a single github repository url, and its own new description.
- `grep -c` prints `Directory.Build.props:0` and `source/Directory.Build.props:1`.
- The msbuild query prints `"ContinuousIntegrationBuild": "true"`.

## Session

- Created: 686431 (2026-10-11)
- Review: Grok session 01a12fe0-802e-79d3-9b10-c6a7253e84ad (2026-10-12)
