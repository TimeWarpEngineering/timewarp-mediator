# Kebab cs files and tw-csharp conventions

## Description

Parent: **006**. ~188 `.cs` files in MediatR fork shape (`Mediator.cs`, per-file usings, block namespaces). Make them TimeWarp: kebab file names, file-scoped namespaces, global usings, explicit types (no `var`), Allman already likely.

Public **type** names stay PascalCase. This is the large rewrite-of-the-tree slice. Keep tests green.

## Depends on

- 006-003

## Requirements

- TW0001 / kebab `.cs` basenames
- `global-usings.cs` per project; strip redundant per-file usings
- File-scoped `namespace TimeWarp.Mediator;`
- Explicit types; target-typed `new()`
- Analyzers/generators/tests still compile and pass
- Do not change mediator **behavior** except as required by file moves

## Out of scope

- Deleting reflection Mediator
- New features (streams, interceptors)

## Checklist

- [x] TW0001 kebab `.cs` basenames (`Mediator.cs` → `mediator.cs`, `IRequest.cs` → `i-request.cs`)
- [x] `global-usings.cs` per product/test/sample project; strip redundant per-file usings
- [x] File-scoped namespaces (remaining block namespaces converted; Handlers.cs split)
- [x] Explicit types; target-typed `new()` where the constructed type matches the target
- [x] Analyzers/generators/tests compile and pass; pack ids stay PascalCase
- [x] Public type names unchanged; no mediator behavior change

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-02)

## Results

Rewrote the MediatR-fork `.cs` tree to TimeWarp C# conventions. Public **type** names stay PascalCase. Package ids stay `TimeWarp.Mediator*`. Commit `168bc5c` (`refactor: kebab-case cs files and apply tw-csharp conventions`).

**Kebab (TW0001 regex)** — 159 PascalCase `.cs` files renamed (`Mediator.cs` → `mediator.cs`, `IRequest.cs` → `i-request.cs`, `Program.cs` → `program.cs`). Every compiled `.cs` basename matches `^[a-z][a-z0-9]*(?:-[a-z0-9]+)*(?:\.[a-z][a-z0-9]*(?:-[a-z0-9]+)*)*\.cs$`. `tools/dev-cli` was already kebab (006-002).

**Namespaces** — most files were already file-scoped. Converted the remaining 12 Allman block namespaces (benchmarks + generic-handler tests). Split `Handlers.cs` (two namespaces in one file, illegal for file-scoped) into `handlers.cs` + `included-handlers.cs`. Hoisted inner usings (including relative `using Wrappers` / `using Internal`) above the namespace and expanded them.

**Global usings** — added `global-usings.cs` to each of the 21 product/test/sample projects (22 including existing `tools/dev-cli`). Stripped redundant per-file usings. Left file-level usings only for:

- alias `using GeneratedMediator = TimeWarp.Mediator.Generated.Mediator`
- colliding `Mediator` vs `TimeWarp.Mediator` in `tests/timewarp-mediator-benchmarks-comparison` (both packages export `IRequest` / `IMediator`)

**Explicit types / `new()`** — `dotnet format` IDE0008/IDE0090 plus a leftover pass. Code `var` is gone (only comments/docs remain). Target-typed `new()` is used when the constructed type matches the target (`ServiceCollection services = new()`). `IServiceCollection services = new ServiceCollection()` is kept: `new()` cannot construct an interface. Same for `object x = new PingException()` and `IList<string> = new List<string>()`.

**Editorconfig** — `csharp_style_var_*` flipped to `false`; `csharp_style_implicit_object_creation_when_type_is_apparent` enabled. Indent stays 4 spaces (repo `.editorconfig` wins over the skill’s 2-space prose).

**Not wired here:** TimeWarp.SourceGenerators TW0001 analyzer (opt-in, `isEnabledByDefault: false`). Basenames already match the analyzer regex; 006-005 can enable the package.

**Tests / pack** (`DOTNET_ROLL_FORWARD=LatestMajor`):

- `dotnet build timewarp-mediator.slnx -c Release` exit 0
- `dotnet test timewarp-mediator.slnx -c Release --no-build`: Mediator.Tests **163 passed / 2 skipped**; Analyzers **6 passed**; Generators **19 passed**
- Pack nupkgs: `TimeWarp.Mediator.13.0.0.nupkg` (+ snupkg), Contracts (+ snupkg), Analyzers, Generators — no kebab package ids

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-006-004-kebab-cs-files-and-tw-csharp-conventions
test -f source/timewarp-mediator/mediator.cs
test ! -e source/timewarp-mediator/Mediator.cs
test -f source/timewarp-mediator-contracts/i-request.cs
test -f source/timewarp-mediator/global-usings.cs
# no PascalCase .cs basenames left:
find source tests samples -name '*.cs' -not -path '*/obj/*' | awk -F/ '{print $NF}' | grep -E '[A-Z]' && echo FAIL || echo PASS
# no code var left (comments/docs only):
rg -n '\bvar\b' --glob '*.cs' --glob '!**/obj/**' --glob '!tools/dev-cli/**' | rg -v '//|///'
```

**Expect**

- `mediator.cs` / `i-request.cs` exist; `Mediator.cs` / `IRequest.cs` do not
- Smoke `find | grep -E '[A-Z]'` prints **PASS** (no output then PASS)
- Remaining `var` hits are comments (`//var`, `/// foreach (var handler`) only
- Each sample/source/tests project has `global-usings.cs`
- Public types remain `Mediator`, `IRequest`, `IMediator` (file rename only)

**Automated gate**

```bash
dotnet restore timewarp-mediator.slnx
dotnet build timewarp-mediator.slnx -c Release --no-restore
DOTNET_ROLL_FORWARD=LatestMajor dotnet test timewarp-mediator.slnx -c Release --no-build
dotnet run --file tools/dev-cli/dev.cs -- pack --no-build
ls artifacts/packages/TimeWarp.Mediator*.nupkg
# expect: TimeWarp.Mediator.13.0.0.nupkg, Contracts, Analyzers, Generators
# expect: tests 163 passed / 2 skipped; analyzers 6; generators 19
# expect: no artifacts/packages/timewarp-mediator.13.0.0.nupkg (kebab id)
```

**Not in scope:** enabling the TW0001 analyzer package (006-005); deleting reflection Mediator; 2-space indent (editorconfig is 4).
