# Kebab paths source tests documentation

## Description

Parent: **006**. Move the tree to TimeWarp layout and kebab-case **folders / csproj / slnx / non-cs files**. Leave `.cs` file kebab + body style to **006-004** if that keeps the diff reviewable; or do `.cs` renames here and style in 006-004 — pick one and don’t split a file’s rename across both.

Audit today: **112** non-kebab path names. `src/` → `source/`, `test/` → `tests/`, `Documentation/` → `documentation/`, `Assets/` → `assets/`. `TimeWarp.Mediator/` folder → `timewarp-mediator/`. Dual `Kanban/` + `kanban/` → `kanban/` only.

## Depends on

- 006-001

## Requirements

- Package **ids** remain `TimeWarp.Mediator` etc.
- Solution and CI paths updated
- Samples under `samples/` kebab
- `kebab-path-names` no longer fails on folders/csproj (`.cs` only if included in this slice)
- Strong-name `.snk` stay put; don’t bikeshed crypto in this task

## Out of scope

- C# language style (**006-004**)
- Dev CLI (**006-002**) — rebase if both land

## Checklist

- [x] Leave `.cs` basenames PascalCase for **006-004** (do not split a file rename)
- [x] `src/` → `source/` with kebab project folders/csproj; package ids stay `TimeWarp.Mediator*`
- [x] `test/` → `tests/` kebab
- [x] Samples under `samples/` kebab
- [x] `Documentation/` → `documentation/`; `Assets/` → `assets/` (`logo.png`)
- [x] Dual `Kanban/` archived under `kanban/archived/legacy-mediatr-rename/`; live board is `kanban/` only
- [x] Delete `TimeWarp.Mediator.sln`; keep `timewarp-mediator.slnx`; CI path filters `source/**` `tests/**`
- [x] `kebab-path-names` PASS; `.snk` bytes unchanged (`timewarp-mediator.snk`)
- [x] Restore + Release build + tests + pack (Pascal package ids)
- [x] Implementation review (effort 1, general) — disposition clean

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-02)
- Review: grok, effort 1, roster general (2026-09-02)
- Review round-1 fix (M1 live doc kebab paths): grok (2026-09-02)
- Review round 2: grok, effort 1, roster general (2026-09-02)

## Results

Moved the tree to TimeWarp layout and kebab-case **folders / csproj / slnx / non-cs files**. `.cs` basenames stay PascalCase for **006-004**. Package **ids** remain `TimeWarp.Mediator`, `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, `TimeWarp.Mediator.Generators` via explicit `<PackageId>` + `<AssemblyName>`. Strong-name key bytes are unchanged; the file was kebab-renamed to `timewarp-mediator.snk` (path only, not a new key).

**Layout**

| Was | Now |
|-----|-----|
| `src/TimeWarp.Mediator*` | `source/timewarp-mediator*` |
| `test/TimeWarp.Mediator*` | `tests/timewarp-mediator-*` |
| `samples/TimeWarp.Mediator.Examples*` | `samples/timewarp-mediator-examples*` |
| `Documentation/` | `documentation/` |
| `Assets/Logo.png` | `assets/logo.png` |
| `Analysis/` | `analysis/` |
| `Kanban/` (PascalCase leftover) | `kanban/archived/legacy-mediatr-rename/` |
| `TimeWarp.Mediator.sln` | deleted (MSB1011); `timewarp-mediator.slnx` only |
| `README.md` / `Agent.md` / `NOTICE` / `UNLICENSE` / `NuGet.Config` | `readme.md` / `agent.md` / `notice` / `unlicense` / `nuget.config` |

Inner folders kebab (`microsoft-extensions-di`, `exception-handler`, `build-transitive`, …). Analyzer release notes, `Properties/`, and `launchSettings.json` stay allowlisted. Generator nupkg still ships `buildTransitive/TimeWarp.Mediator.Generators.props` so NuGet auto-import keeps the PackageId filename.

**Audit:** `kebab-path-names` **PASS**. `nuget-package-icon` and `nuget-package-urls` now PASS (packable projects under `source/`). Remaining failures are out of scope: `bin-dev` / `dev-cli-capabilities` (no `./bin/dev` in this clone until `self-install`; 006-002 owns the CLI) and `vscode-window-icon` warning (006-005).

**Build / test / pack** (`DOTNET_ROLL_FORWARD=LatestMajor` on this image; CI installs net8.0):

- `dotnet restore timewarp-mediator.slnx` exit 0
- `dotnet build timewarp-mediator.slnx -c Release --no-restore` exit 0
- `dotnet test timewarp-mediator.slnx -c Release --no-build`: Mediator.Tests **163 passed / 2 skipped**; Analyzers **6 passed**; Generators **19 passed**
- Pack nupkgs: `TimeWarp.Mediator.13.0.0.nupkg` (+ snupkg), Contracts (+ snupkg), Analyzers, Generators — no kebab package ids

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-006-003-kebab-paths-source-tests-documentation
test ! -e src && test ! -e test && test ! -e Documentation && test ! -e Assets && test ! -e Kanban && test ! -e TimeWarp.Mediator.sln
ls source/timewarp-mediator/timewarp-mediator.csproj tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj assets/logo.png documentation/overview.md
ganda repo audit 2>&1 | rg "kebab-path-names|nuget-package"
```

**Expect**

- Leftover `src/`, `test/`, `Documentation/`, `Assets/`, `Kanban/`, `TimeWarp.Mediator.sln` are gone
- `kebab-path-names` line shows **PASS** / “All path basenames are kebab-case”
- `nuget-package-icon` and `nuget-package-urls` **PASS**
- `.cs` files still PascalCase (`Mediator.cs`, not `mediator.cs`)

**Automated gate**

```bash
dotnet restore timewarp-mediator.slnx
dotnet build timewarp-mediator.slnx -c Release --no-restore
DOTNET_ROLL_FORWARD=LatestMajor dotnet test timewarp-mediator.slnx -c Release --no-build
dotnet run --file tools/dev-cli/dev.cs -- pack --no-build
ls artifacts/packages/TimeWarp.Mediator*.nupkg
# expect: TimeWarp.Mediator.13.0.0.nupkg, Contracts, Analyzers, Generators
# expect: no artifacts/packages/timewarp-mediator.13.0.0.nupkg (kebab id)
```

**Not in scope:** `.cs` kebab + file-scoped namespaces / global-usings (**006-004**); `vscode-window-icon` (**006-005**); `./bin/dev` present without `self-install` (gitignored; 006-002).

### Review disposition

- **Outcome:** clean
- **Rounds:** 2
- **Effort / roster:** 1, general (both rounds)
- **Counts (final, round 2):** bug 0; suggestion 0 open / 1 fixed; nit 0 — final open count 0
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`
