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
- [ ] CI: `Build.ps1` must not hit MSB1011 (both `.sln` and `.slnx` at repo root)

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-01)
- Review: grok, effort 1, roster general (2026-09-01)
- Reopened: cockpit 2026-09-01 — PR #55 MSB1011; dispatched back onto this id

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

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-006-001-scaffold-audit-fix-layout-dirs-and-cpm
ganda repo audit
test -f Directory.Packages.props && test -f msbuild/repository.props && test -f source/Directory.Build.props && test -f timewarp-mediator.slnx && test -f .envrc
test -d kanban/backlog && test -d kanban/in-progress && test -d kanban/archived
rg -n 'root = true|csharp_style_prefer_top_level_statements|csharp_style_unused_value_expression_statement_preference|csharp_style_namespace_declarations' .editorconfig
rg -n 'task-work.journal.json|stacked-task-set.journal.json|planning.journal.json|rfc.journal.json|debate.journal.json|advisor.journal.json' .gitignore
dotnet restore timewarp-mediator.slnx
dotnet sln timewarp-mediator.slnx list
```

**Expect**

- Audit **PASS** for: `envrc`, `routine-journals-gitignore`, `directory-packages-props`, `cpm-consistency`, `banned-symbols`, `banned-api-analyzers`, `assembly-metadata`, `msbuild-repository-props`, `source-directory-build-props`, `slnx`, `editorconfig`, `directory-structure`
- Audit still **FAIL** (this slice): `bin-dev`, `dev-cli-capabilities`, `nuru`, `region-annotations`, `kebab-path-names`
- `dotnet restore timewarp-mediator.slnx` exit 0
- `dotnet sln timewarp-mediator.slnx list` includes the existing `src/`, `test/`, and `samples/` projects
- `.envrc` contains `PATH_add bin`

**Automated gate**

```bash
dotnet restore timewarp-mediator.slnx
dotnet build timewarp-mediator.slnx -c Release --no-restore
# expect: exit 0; RS0030 warnings on samples/tests are OK
```

**Not in scope:** `bin/dev` / `tools/dev-cli` (006-002); kebab rename of `src/` → `source/` (006-003); C# file-scoped namespaces / Console replacement (006-004). Core xunit run needs a net8.0 runtime this agent image does not have.

### Review disposition

- **Outcome:** clean
- **Rounds:** 1
- **Effort / roster:** 1, general
- **Counts (final):** bug 0 / suggestion 0 / nit 0 — all statuses 0 (no issues raised)
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
