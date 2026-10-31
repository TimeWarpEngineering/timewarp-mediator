# Task 026: Drop the Roslyn 5.0.0 floor; guard that the referenced compiler is not newer than the SDK's

## Description

Steven's direction (2026-10-31): TimeWarp is the primary consumer of these packages, every
TimeWarp repo is moving to .NET 11 (GA expected in about two weeks), and nobody at TimeWarp is
on the 10.0.100 SDK. The Roslyn 5.0.0 floor from task 021-005 (PR #111, review round 1 M1)
protected consumers on that oldest SDK and costs a permanent Dependabot ignore, a
`VersionOverride` on both shipped projects, a floor test, and a stale row in every
`ganda nuget outdated` run. It buys TimeWarp nothing. Remove it.

The invariant that actually matters is: the `Microsoft.CodeAnalysis.CSharp` version the
analyzer and generator are compiled against must not be newer than the compiler that loads
them. For this repo that compiler is the SDK in `global.json`. Measured on 2026-10-31: the
11.0.100-rc.3 SDK ships Roslyn 5.12 (file version 5.1200.26.52348); the newest stable
`Microsoft.CodeAnalysis.CSharp` on nuget.org is 5.9.0. So the bump is safe today and at 11 GA.

## Requirements

- `Directory.Packages.props`: `Microsoft.CodeAnalysis.CSharp` and
  `Microsoft.CodeAnalysis.CSharp.Workspaces` to the newest stable (5.9.0 on 2026-10-31, or
  whatever `ganda nuget outdated` reports on the day). Replace the "oldest .NET 10 SDK" comment
  with the new rule: the pin may be any version the SDK in `global.json` can load; the
  compilation test below enforces it.
- Remove `VersionOverride="5.0.0"` and its comment from
  `source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj` and
  `source/timewarp-mediator-generators/timewarp-mediator-generators.csproj`; both resolve
  the central pin.
- `.github/dependabot.yml`: remove the `Microsoft.CodeAnalysis.CSharp*` ignore and its
  comment. Dependabot may now move the pin; the test turns CI red if a bump outruns the SDK.
- `tests/timewarp-mediator-generators-compilation-tests/roslyn-floor-tests.cs`: replace
  `Compiler_IsTheDeclaredFloor` (which asserts the reference equals 5.0.0) with a test that
  the `Microsoft.CodeAnalysis` assembly version the shipped generator references is less than
  or equal to the Roslyn version of the compiler running the build. Read the referenced
  version from the generator assembly's references (`AssemblyName` of
  `Microsoft.CodeAnalysis`) and the host version from the loaded
  `Microsoft.CodeAnalysis.dll` in the test process or from `$(RoslynTargetsPath)` /
  the SDK's `Roslyn/bincore/Microsoft.CodeAnalysis.dll` informational version. State in a
  comment which source is used and why. Rename the file and class to match
  (`roslyn-compiler-tests.cs` / `RoslynCompilerTests` or similar; kebab-case file).
- Keep the existing analyzer and generator loading proof: the compilation tests already
  drive the generator through `CSharpGeneratorDriver` on the pinned Roslyn; the package smoke
  (`dev pack`) builds an outside consumer on the `global.json` SDK. Both stay green.
- Documentation: every mention of the 5.0.0 floor is updated. Known places:
  `agent.md` (Test Architecture and Key Dependencies), `readme.md`, the three
  `source/*/readme.md` files, `documentation/architecture.md`, `documentation/migration.md`,
  `skills/tw-mediator/SKILL.md`. Replace with one sentence: the packages need a .NET SDK whose
  compiler is at least the Roslyn version they reference (11.0.100 or later; a 10.0.x band
  only if its compiler is at least that version); an older SDK reports CS8032/CS9057 and the
  generator emits nothing. Add the same sentence to `documentation/release-notes/` as a
  new entry for the next version (file named for the next version, 15.0.0-beta.3, without
  bumping `<Version>`; Steven decides when to cut).
- `ganda repo audit --fix` applied and committed.
- `./bin/dev workflow` green on both TFMs: clean, build with 0 warnings, test, verify-samples,
  pack (two nupkgs, both lib folders, smoke pongs on net10.0 and net11.0).
- `ganda nuget outdated` afterwards reports zero outdated packages. Record its output in
  `## Results`.
- `<Version>` stays `15.0.0-beta.2`. Not a release in this task.

## Checklist

### Implementation
- [x] Both Roslyn pins at the newest stable; comment states the new rule
- [x] `VersionOverride` removed from the analyzer and generator projects
- [x] Dependabot ignore removed
- [x] Floor test replaced by the "referenced <= host compiler" test; file and class renamed
- [x] `ganda repo audit --fix` applied and committed
- [x] `./bin/dev workflow` green on net10.0 and net11.0 (0 warnings)
- [x] `ganda nuget outdated` clean; output in Results
### Documentation
- [x] Every floor mention replaced (agent.md, readmes, architecture, migration, SKILL.md)
- [x] Release-notes entry for the next version written
### Review
- [x] Code Review
- [ ] PR with merge commit

## Review

- Round 1: Done

## Notes

- Why the floor existed: Roslyn refuses to load an analyzer compiled against a newer
  `Microsoft.CodeAnalysis` than itself (CS8032, CS9057). 5.0.0 matched the 10.0.100 SDK.
  Task 021-005 review M1 (PR #111) added the three guards; task 022 (PR #115) and 025
  (PR #119) kept them.
- Local SDKs on 2026-10-31: 10.0.301 through 10.0.403, 11.0.100-preview.5 through rc.3.
  No 10.0.100 anywhere at TimeWarp.
- Task 023-001 (GA swap) will move `global.json` to 11.0.100; the new test keeps working
  across that change because it reads the host compiler at run time.
- Policy: minimal public API; nothing public changes here.

## Goal (implementer restatement)

Drop the permanent Roslyn 5.0.0 floor. TimeWarp no longer needs the analyzer and generator to load on the 10.0.100 SDK, and keeping that floor forces a Dependabot ignore, a `VersionOverride` on both shipped projects, a floor test, and a stale `ganda nuget outdated` row.

Pin `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` to the newest stable the day of the change. Both shipped projects take that central pin. Dependabot may move it. The compilation test fails when the `Microsoft.CodeAnalysis` version the generator references is newer than the compiler in the SDK that `global.json` selects. Living docs state the consumer rule in one sentence: the packages need an SDK whose compiler is at least the Roslyn version they reference (11.0.100 or later; a 10.0.x SDK only when its compiler is at least that version); an older SDK reports CS8032/CS9057 and the generator emits nothing. The same sentence is the `15.0.0-beta.3` release note. `<Version>` stays `15.0.0-beta.2`.

Acceptance: pins and comment updated, overrides and the Dependabot ignore gone, the renamed compiler test green, living floor mentions replaced, `ganda repo audit` clean, `./bin/dev workflow` green on net10.0 and net11.0 with 0 warnings, and `ganda nuget outdated` reports nothing outdated.

## Implementation Notes

- Central pins for `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` are `5.9.0`, the newest stable `ganda nuget outdated` reported.
- `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` reads the generator and analyzer `Microsoft.CodeAnalysis` assembly references and compares them to the AssemblyVersion of `$(RoslynTargetsPath)/bincore/Microsoft.CodeAnalysis.dll`. That file is the SDK compiler `dotnet build` loads. The test process loads the central package, so `typeof(CSharpCompilation).Assembly` would compare the pin to itself. Informational and file versions use different numberings; Roslyn's load check uses the assembly version.
- On the 11.0.100-rc.3 SDK that assembly is 5.12.0.0. The rebuilt generator references `Microsoft.CodeAnalysis` 5.9.0.0.
- `<Version>` stays `15.0.0-beta.2`. The consumer sentence is in `documentation/release-notes/15.0.0-beta.3.md`.
- Historical kanban and the 2026-10-13 design review still describe the old floor. Living docs and the 15.0.0 F14 replacement do not.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
- Implementation: task-work implementer (2026-10-31)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a190db-8bac-7613-b7ce-f968878c5da6, max-turns 500, Done — 2026-10-31T07:41:12Z
- Review oracle: claude (model claude-opus-5-5), body tw-implementation-review, effort 2, roster general, turn cap 120, 1 round, disposition clean — 2026-10-31
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-31T07:43:38Z

## Results

### Review disposition

Disposition is **clean**. There was 1 round at effort 2, with one general reviewer. The final counts are 0 open, 0 fixed, and 0 wontfix for bug, suggestion, and nit, because round 1 raised nothing. The reviewer re-ran `RoslynCompilerTests` on SDK 11.0.100-rc.3, and it passed on net10.0 and net11.0. The goal restatement matches the task, so no misread is recorded.

The artifacts are `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, and `review/disposition.md`.

`Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` are pinned at 5.9.0. The analyzer and generator projects take that central pin. Dependabot no longer ignores `Microsoft.CodeAnalysis.CSharp*`. `RoslynCompilerTests.ReferencedCompiler_IsNotNewerThanTheSdkCompiler` replaces `Compiler_IsTheDeclaredFloor`. Living docs and `documentation/release-notes/15.0.0-beta.3.md` state the SDK-compiler rule. `<Version>` stays `15.0.0-beta.2`.

`ganda repo audit --fix` exited 0. 33 checks passed, 3 skipped, and one advisory warning: `kanban-landed` says task 021 has no usable worktree. That claim is outside this task. `audit --fix` changed no files.

`./bin/dev workflow` (mode Pr) exited 0. Clean, then build with 0 warnings and 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 42, compilation 69, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.2.nupkg` (`lib/net10.0`, `lib/net11.0`, and the analyzer DLL) and `TimeWarp.Mediator.Generators.15.0.0-beta.2.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0. The log ends with `Pipeline SUCCEEDED`.

`ganda nuget outdated` afterwards:

```text
Checking packages in task-026-drop-the-roslyn-500-floor-guard-that-the-reference...
  [1/27] Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0 (up to date, nuget.org)
  [2/27] Microsoft.SourceLink.GitHub 10.0.403 (up to date, nuget.org)
  [3/27] TimeWarp.Build.Tasks 1.0.0 (up to date, nuget.org)
  [4/27] TimeWarp.SourceGenerators 1.0.0-beta.11 (up to date, nuget.org)
  [5/27] TimeWarp.Mediator [$(TimeWarpMediatorVersion)] (skipped: not a literal version)
  [6/27] TimeWarp.Mediator.Generators [$(TimeWarpMediatorVersion)] (skipped: not a literal version)
  [7/27] TimeWarp.Nuru 4.0.0-beta.10 (up to date, nuget.org)
  [8/27] TimeWarp.Nuru.DevCli 4.0.0-beta.10 (up to date, nuget.org)
  [9/27] TimeWarp.Amuru 2.0.0 (up to date, nuget.org)
  [10/27] TimeWarp.Amuru.Tools 2.0.0 (up to date, nuget.org)
  [11/27] TimeWarp.Terminal 1.0.2 (up to date, nuget.org)
  [12/27] Microsoft.CodeAnalysis.Analyzers 5.9.0 (up to date, nuget.org)
  [13/27] Microsoft.CodeAnalysis.CSharp 5.9.0 (up to date, nuget.org)
  [14/27] Microsoft.Extensions.DependencyInjection 10.0.14 (up to date, nuget.org)
  [15/27] Microsoft.Extensions.DependencyInjection 11.0.0-rc.3.26522.148 (up to date, nuget.org)
  [16/27] Microsoft.CodeAnalysis.CSharp.Workspaces 5.9.0 (up to date, nuget.org)
  [17/27] Microsoft.NET.Test.Sdk 18.11.0 (up to date, nuget.org)
  [18/27] Shouldly 4.3.0 (up to date, nuget.org)
  [19/27] xunit 2.9.3 (up to date, nuget.org)
  [20/27] xunit.runner.visualstudio 4.0.1 (up to date, nuget.org)
  [21/27] BenchmarkDotNet 0.15.8 (up to date, nuget.org)
  [22/27] BenchmarkDotNet 0.16.0-preview.2 (up to date, nuget.org)
  [23/27] Mediator.Abstractions 3.0.2 (up to date, nuget.org)
  [24/27] Mediator.SourceGenerator 3.0.2 (up to date, nuget.org)
  [25/27] MediatR 14.2.0 (up to date, nuget.org)
  [26/27] Microsoft.Extensions.Logging 10.0.14 (up to date, nuget.org)
  [27/27] Microsoft.Extensions.Logging 11.0.0-rc.3.26522.148 (up to date, nuget.org)

All packages are up to date!
```

### How to validate

Smoke: from this worktree, `ganda nuget outdated`, then `./bin/dev workflow`.

Expect: outdated ends with `All packages are up to date!` and lists `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` at 5.9.0. Workflow prints `0 Warning(s)`, tests 61 + 42 + 69 passed on net10.0 and net11.0, `Samples verified successfully!`, both `15.0.0-beta.2` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`. `source/Directory.Build.props` still has `<Version>15.0.0-beta.2</Version>`.
