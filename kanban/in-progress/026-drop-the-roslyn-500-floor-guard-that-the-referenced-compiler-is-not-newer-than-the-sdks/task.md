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
- [ ] Both Roslyn pins at the newest stable; comment states the new rule
- [ ] `VersionOverride` removed from the analyzer and generator projects
- [ ] Dependabot ignore removed
- [ ] Floor test replaced by the "referenced <= host compiler" test; file and class renamed
- [ ] `ganda repo audit --fix` applied and committed
- [ ] `./bin/dev workflow` green on net10.0 and net11.0 (0 warnings)
- [ ] `ganda nuget outdated` clean; output in Results
### Documentation
- [ ] Every floor mention replaced (agent.md, readmes, architecture, migration, SKILL.md)
- [ ] Release-notes entry for the next version written
### Review
- [ ] Code Review
- [ ] PR with merge commit

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

## Implementation Notes

-

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
