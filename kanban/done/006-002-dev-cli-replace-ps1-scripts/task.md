# Dev CLI replace PS1 scripts

## Description

Parent: **006**. Add TimeWarp `tools/dev-cli` + `bin/dev`. Replace PowerShell as the build/test/pack/push/hooks path.

## Depends on

- 006-001

## Requirements

- `tools/dev-cli/dev.cs` (Nuru) with `#region Purpose`; `bin/dev` executable
- Commands at least: build, test, pack (map today’s `Build.ps1`)
- `.github/scripts/Push.ps1` and CI pack steps call `dev` (or `dotnet run tools/dev-cli/dev.cs --`) not ad-hoc pwsh
- GitHooks: TimeWarp `.githooks` / `ganda` memsearch path, not `Tools/GitHooks/*.ps1` as the live hook
- `dev --capabilities` works (`dev-cli-capabilities`, `nuru` package ref)
- Delete or quarantine leftover `.ps1` once `dev` covers them (attribution/file-mapping one-shots can go to `documentation/` or `tools/` as runfiles if still needed)

## Scripts to retire

- `Build.ps1`
- `.github/scripts/Push.ps1`
- `Tools/GitHooks/SetupGitHooks.ps1`, `PreCommit.ps1`
- `Tools/FileSync/SyncConfigurableFiles.ps1`, `.github/scripts/SyncConfigurableFiles.ps1`
- One-shots: `Add-AttributionComments.ps1`, `Create-FileMapping.ps1`, `Filter-Files.ps1`

## Out of scope

- Kebab rename (**006-003**)
- Full audit green (**006-005**)

## Checklist

- [x] `tools/dev-cli/dev.cs` (Nuru) with Purpose/Design; `bin/dev` via `self-install`
- [x] Commands: build, test, pack, workflow, plus audit set (clean, check-version, self-install, verify-samples)
- [x] CI `workflow.yml` calls `dotnet run --file tools/dev-cli/dev.cs -- workflow` (probe/break-glass kept)
- [x] `.githooks` memsearch path; `core.hooksPath=.githooks`; retired `Tools/GitHooks/*.ps1`
- [x] `./bin/dev --capabilities` description + required commands
- [x] Deleted leftover `.ps1` (Build, Push, GitHooks, FileSync, one-shots)
- [x] Implementation review (effort 1, general) — disposition clean

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-02)
- Review: grok, effort 1, roster general (2026-09-02)
- Review round-1 fixes (M1 Version SSOT assert; M2 wipe artifacts/packages): grok (2026-09-02)
- Review round 2: grok, effort 1, roster general (2026-09-02)

## Results

Replaced PowerShell as the build/test/pack/push/hooks path with TimeWarp `tools/dev-cli` (Nuru) and `bin/dev`.

`ganda repo audit --fix --checks nuru,region-annotations,memsearch-scaffold` scaffolded the baseline. Local endpoints then map the former `Build.ps1`: `dev build` / `dev test` name `timewarp-mediator.slnx` (MSB1011), `dev pack` packs the four product projects to `artifacts/packages` and asserts analyzer DLL entries via `NupkgLayoutCheck`. `dev workflow` is mode-aware (PR/merge: clean → build → test → pack; release: clean → build → pack → assert Version SSOT → check-version → push, no test gate). Until 006-003, release asserts root and `source/` `Directory.Build.props` `<Version>` match before shared check-version. `dev pack` wipes `artifacts/packages` before writing nupkgs. CI no longer shells `pwsh` or ad-hoc `dotnet nuget push`.

Git hooks are TimeWarp `.githooks` (memsearch + master/main refuse on pre-commit/pre-push). Live `Tools/GitHooks/*.ps1` is gone. One-shots were already spent; they were deleted, not converted.

**Audit:** 14 passed / 7 failed → **21 passed / 2 failed / 3 skipped**. Remaining: `kebab-path-names` (006-003), `vscode-window-icon` warning (006-005). In-scope checks `bin-dev`, `dev-cli-capabilities`, `nuru`, `region-annotations`, `memsearch-scaffold`, `runfile-shebang`, `cpm-consistency` pass.

**Build / pack / test:** `./bin/dev build` exit 0 (0 errors). `./bin/dev pack --no-build` exit 0; four nupkgs; analyzer/generator layout asserts pass. `DOTNET_ROLL_FORWARD=LatestMajor ./bin/dev test --no-build` exit 0: Mediator.Tests 163 passed / 2 skipped; Analyzers 6 passed; Generators 19 passed. Without roll-forward, Mediator.Tests aborts on this image (no net8 testhost); CI installs `8.0.x` plus `10.0.x`.

### Files changed

- Added: `tools/dev-cli/**` (`dev.cs`, endpoints, `RepoLayout`, Directory.Build.props, global-usings), `.githooks/**`, `.memsearch.toml`, `.timewarp/dev.jsonc`
- Updated: `.github/workflows/workflow.yml`, `Directory.Packages.props` (Nuru 3.0.0-beta.76 + DevCli/Amuru/Terminal), `Agent.md`, `.gitignore` (`artifacts/`), `timewarp-mediator.slnx`, `TimeWarp.Mediator.sln`
- Deleted: `Build.ps1`, `.github/scripts/Push.ps1`, `.github/scripts/SyncConfigurableFiles.ps1`, `Tools/GitHooks/*`, `Tools/FileSync/SyncConfigurableFiles.ps1`, `Add-AttributionComments.ps1`, `Create-FileMapping.ps1`, `Filter-Files.ps1`

### Key decisions

- Pack output is `artifacts/packages` (repository.props), not the former `Artifacts/`.
- PR/merge still packs so the analyzer nupkg layout gate runs before merge (this repo already shipped hollow analyzer packages once).
- Product still lives under `src/` until 006-003; `.timewarp/dev.jsonc` lists the four package ids for `check-version`.
- `bin/dev` is AOT-local (`[Bb]in/` gitignored). Fresh clones: `dotnet run --file tools/dev-cli/dev.cs -- self-install`.

### How to validate

**Smoke**

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev --capabilities
./bin/dev build
./bin/dev pack --no-build
ls artifacts/packages/*.nupkg
git config --get core.hooksPath
test ! -e Build.ps1 && test ! -e .github/scripts/Push.ps1
```

**Expect**

- `./bin/dev --capabilities` JSON `description` is `Development CLI for timewarp-mediator` and `endpoints[].pattern` includes `build`, `test`, `pack`, `workflow`, `clean`, `check-version`, `self-install`, `verify-samples`.
- `./bin/dev build` prints `Building …/timewarp-mediator.slnx` and `Build completed successfully!` (exit 0, no MSB1011).
- `./bin/dev pack --no-build` creates `artifacts/packages/TimeWarp.Mediator{,.Contracts,.Analyzers,.Generators}.13.0.0.nupkg` and prints `Package layout verified` for Analyzers and Generators (exit 0).
- `core.hooksPath` is `.githooks`. `Build.ps1` and `.github/scripts/Push.ps1` are absent.
- Root and `source/Directory.Build.props` `<Version>` both `13.0.0` (release `AssertVersionSsot`). `dev pack` leaves only this run's nupkgs under `artifacts/packages`.

**Automated gate**

```bash
# CI installs net8; this image needs roll-forward for testhost
DOTNET_ROLL_FORWARD=LatestMajor ./bin/dev test --no-build
# expect: TimeWarp.Mediator.Tests 163 passed / 2 skipped; Analyzers 6 passed; Generators 19 passed; exit 0

ganda repo audit
# expect: bin-dev, dev-cli-capabilities, nuru, region-annotations, memsearch-scaffold PASS
# expect: kebab-path-names still FAIL (006-003); vscode-window-icon warning remains
```

**Depends on:** .NET 10 SDK for the runfile/AOT CLI. Tests target net8.0 (GitHub `setup-dotnet` `8.0.x` + `10.0.x`). Fresh clone needs `self-install` before `./bin/dev`.

**Not in scope:** kebab-path-names (006-003); full audit green (006-005); live NuGet push (needs OIDC on a GitHub Release).

### Review disposition

- **Outcome:** clean
- **Rounds:** 2
- **Effort / roster:** 1, general (both rounds)
- **Counts (final, round 2):** bug 0 open / 1 fixed; suggestion 0 open / 1 fixed; nit 0 — final open count 0
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`
