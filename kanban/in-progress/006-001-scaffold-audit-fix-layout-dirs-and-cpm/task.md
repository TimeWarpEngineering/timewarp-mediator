# Scaffold audit-fix layout dirs and CPM

## Description

Parent: **006**. Mechanical TimeWarp scaffold. Prefer `ganda repo audit --fix` where Fixable=YES. Do **not** kebab-rename the tree here (**006-003**).

Today’s errors this slice should kill or shrink: envrc, routine-journals-gitignore, Directory.Packages.props, banned-symbols, banned-api-analyzers, assembly-metadata (Build.Tasks), msbuild/repository.props, source/Directory.Build.props, slnx, editorconfig sentinels (editorconfig is **not** auto-overwritten — add sentinels by hand), kanban/backlog|in-progress|archived placeholders.

## Requirements

- Central Package Management (`Directory.Packages.props`)
- `msbuild/repository.props` imported from root Directory.Build.props
- `source/Directory.Build.props` exists (may still point at `src/` until 006-003)
- Root `.slnx` (can wrap existing `.sln` projects)
- `.envrc` PATH_add bin; gitignore routine journals
- TimeWarp `.editorconfig` sentinels
- Empty `kanban/{backlog,in-progress,archived}/` as required by directory-structure

## Out of scope

- `tools/dev-cli` body (**006-002**)
- Renaming `src/` → `source/` contents (**006-003**)
- Rewriting 188 `.cs` files (**006-004**)

## Checklist

- [x] `ganda repo audit --fix` for in-scope Fixable=YES checks (not kebab, not dev-cli)
- [x] Hand-add TimeWarp `.editorconfig` sentinels (file not auto-overwritten)
- [x] Pin discovered package versions in `Directory.Packages.props` and strip csproj `Version` attributes
- [x] Wrap existing `.sln` projects in root `.slnx`
- [x] Restore + Release build of `timewarp-mediator.slnx`
- [x] Implementation review (effort 1, general) — disposition clean
- [x] CI: `Build.ps1` must not hit MSB1011 (both `.sln` and `.slnx` at repo root)

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-01)
- Review: grok, effort 1, roster general (2026-09-01)
- Reopened: cockpit 2026-09-01 — PR #55 MSB1011; dispatched back onto this id
- Implementer (MSB1011): grok (2026-09-01)

## Results

Scaffolded the TimeWarp repo baseline on this fork without kebab-renaming `src/` / `test/` / `Documentation/` (006-003) and without adding `tools/dev-cli` (006-002).

`ganda repo audit --fix` applied: envrc, routine-journals-gitignore, directory-packages-props, banned-symbols, banned-api-analyzers, assembly-metadata, msbuild-repository-props, source-directory-build-props, slnx, directory-structure. Editorconfig sentinels were added by hand (`root = true` plus the three `csharp_style_*` keys). CPM consistency needed a manual pin of the existing product/test/sample packages (audit `--fix` will not auto-pin non-baseline ids).

**Audit:** 1 passed / 18 failed / 7 skipped → **14 passed / 7 failed / 5 skipped**. Remaining error failures are out of scope: `bin-dev`, `dev-cli-capabilities`, `nuru`, `region-annotations` (006-002), `kebab-path-names` (006-003). Warnings `memsearch-scaffold` and `vscode-window-icon` remain.

**Build:** `dotnet restore` and `dotnet build -c Release` on `timewarp-mediator.slnx` exit 0. Samples/tests emit RS0030 warnings for `System.Console` / `ProcessStartInfo` (TreatWarningsAsErrors is already off there). Core `TimeWarp.Mediator.Tests` could not execute on this machine (testhost wants net8.0; only net10/net11 runtimes are installed) — restore/build is the scaffold proof.

### Files changed

- Added: `.envrc`, `BannedSymbols.txt`, `Directory.Packages.props`, `msbuild/repository.props`, `source/Directory.Build.props`, `timewarp-mediator.slnx`, `documentation/.gitkeep`, `tests/.gitkeep`, `skills/.gitkeep`, `kanban/{backlog,archived}/.gitkeep`
- Updated: root `Directory.Build.props` (repository.props import, BannedApiAnalyzers, TimeWarp.Build.Tasks, SourceLink without Version), `.editorconfig` sentinels, `.gitignore` routine-journal patterns, 19 csproj files (Version attributes removed)
- Unchanged layout: product still lives under `src/` and `test/` until 006-003; `source/Directory.Build.props` Version is 13.0.0 to match root

### Key decisions

- Kept existing package versions (no upgrade). Pinned `Microsoft.CodeAnalysis.BannedApiAnalyzers` 5.6.0 and `TimeWarp.Build.Tasks` 1.0.0 from `--fix`.
- Replaced the empty `--fix` slnx with `dotnet sln TimeWarp.Mediator.sln migrate`, then named the file `timewarp-mediator.slnx` so kebab-path-names did not grow.
- `source/Directory.Build.props` is a placeholder (no csproj under `source/` yet); nuget-package-icon/urls stay SKIP until 006-003.

### Reopened 2026-09-01 — CI red on PR #55 (inbound)

`ganda pr merge 55 --task-id 006-001` refused: **build-and-publish** failed in 10s.

```
MSBUILD : error MSB1011: Specify which project or solution file to use because this folder contains more than one project or solution file.
Build.ps1:22 throw ("Exec: " + $errorMessage)
```

Root now has **both** `TimeWarp.Mediator.sln` and `timewarp-mediator.slnx`. `Build.ps1` (and any CI `dotnet` with no `-sln`) is ambiguous.

**This slice:** make CI/`Build.ps1` pass by pointing at **one** solution (prefer `timewarp-mediator.slnx`). Do not start 006-002/006-003. Push to the existing PR branch. Do not merge.

**Fix landed:** `Build.ps1` now uses `$Solution = "timewarp-mediator.slnx"` for `clean` / `build` / `test`. `Agent.md` unadorned `dotnet` commands were updated the same way. `TimeWarp.Mediator.sln` is kept (006-003 kebab-renames the tree; this slice only disambiguates CI). Pack steps already named csproj files.

Local proof (2026-09-01): `dotnet build -c Release` (no sln) → MSB1011 exit 1. `dotnet restore` + `dotnet build timewarp-mediator.slnx -c Release` exit 0. `pwsh` simulation of `Build.ps1` clean+build against the slnx exit 0. Full `Build.ps1` still needs net8 testhost (CI `setup-dotnet` 8.0.x has it; this agent image does not).

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-006-001-scaffold-audit-fix-layout-dirs-and-cpm
# Unadorned build still ambiguous (both .sln and .slnx remain):
dotnet build -c Release; echo "expect MSB1011 exit 1"
# CI path:
rg -n 'timewarp-mediator.slnx' Build.ps1
dotnet restore timewarp-mediator.slnx
dotnet build timewarp-mediator.slnx -c Release --no-restore
pwsh -NoProfile -Command '& ./Build.ps1'   # CI has net8; local testhost may fail after build
```

**Expect**

- Unadorned `dotnet build -c Release` prints `MSB1011` and exits 1 (root still has both solutions)
- `Build.ps1` contains `timewarp-mediator.slnx` on `dotnet clean` / `build` / `test`
- `dotnet restore timewarp-mediator.slnx` exit 0
- `dotnet build timewarp-mediator.slnx -c Release --no-restore` exit 0 (RS0030 warnings on samples/tests are OK)
- On a machine with net8.0 testhost (GitHub `setup-dotnet: 8.0.x`): `pwsh -File ./Build.ps1` exit 0, no MSB1011

**Automated gate**

```bash
dotnet restore timewarp-mediator.slnx
dotnet build timewarp-mediator.slnx -c Release --no-restore
# expect: exit 0; RS0030 warnings on samples/tests are OK
```

**Not in scope:** `bin/dev` / `tools/dev-cli` (006-002); kebab rename of `src/` → `source/` (006-003); C# file-scoped namespaces / Console replacement (006-004). Core xunit run needs a net8.0 runtime this agent image does not have. Deleting `TimeWarp.Mediator.sln` is not this slice.

### Review disposition

- **Outcome:** clean
- **Rounds:** 1
- **Effort / roster:** 1, general
- **Counts (final):** bug 0 / suggestion 0 / nit 0 — all statuses 0 (no issues raised)
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
