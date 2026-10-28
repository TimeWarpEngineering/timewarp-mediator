# Task 022: Update outdated packages after 15.0.0-beta.1 (keep the Roslyn 5.0.0 floor)

## Description

`ganda nuget outdated` on master (2026-10-28, after the 15.0.0-beta.1 release) lists seven
outdated pins. Five can move. The two `Microsoft.CodeAnalysis.CSharp*` rows are the deliberate
Roslyn floor from task 021-005 and must stay at 5.0.0: the shipped analyzer and generator have
to load inside the 10.0.100 SDK compiler. Bump everything else in one PR so Dependabot stops
opening five separate ones.

## Requirements

- `Directory.Packages.props` bumps:
  - `TimeWarp.Nuru` 4.0.0-beta.5 → 4.0.0-beta.6
  - `TimeWarp.Nuru.DevCli` 4.0.0-beta.5 → 4.0.0-beta.6
  - `Microsoft.Extensions.DependencyInjection` 10.0.12 → 10.0.14
  - `Microsoft.NET.Test.Sdk` 18.10.1 → 18.11.0
  - `Microsoft.SourceLink.GitHub` 10.0.401 → 10.0.403
  (use whatever `ganda nuget outdated` reports as latest stable on the day; prerelease only
  for the two TimeWarp.Nuru pins, which track the 4.0.0 beta line).
- Do **not** change `Microsoft.CodeAnalysis.CSharp` or `Microsoft.CodeAnalysis.CSharp.Workspaces`
  from 5.0.0. The `VersionOverride` on the shipped projects, the Dependabot ignore for
  `Microsoft.CodeAnalysis.CSharp*`, and the `Compiler_IsTheDeclaredFloor` compilation test all
  guard that floor; leave all three in place and confirm the test still passes.
- Run `ganda repo audit --fix` and commit whatever it changes (audit fixes are policy).
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` after the Nuru.DevCli bump so the AOT
  dev binary is rebuilt on the new pin; confirm `./bin/dev --capabilities` still lists every
  command in `agent.md`.
- `./bin/dev workflow` green: clean, build with 0 warnings, test, verify-samples (AOT sample
  publishes with trim and AOT warnings as errors), pack (two nupkgs, package smoke pongs).
- `./bin/dev benchmark --gate` is not required; the DI bump is a patch and the gate is not
  part of `workflow`.
- `<Version>` stays `15.0.0-beta.1`. This is not a release.

## Checklist

### Implementation
- [ ] Five pins bumped in `Directory.Packages.props`; Roslyn rows untouched at 5.0.0
- [ ] `ganda repo audit --fix` applied and committed
- [ ] dev-cli self-install rebuilt; `./bin/dev --capabilities` complete
- [ ] `./bin/dev workflow` green (0 warnings)
- [ ] `Compiler_IsTheDeclaredFloor` still passes
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Notes

- Source: `ganda nuget outdated` run by Steven on 2026-10-28. The `TimeWarp.Mediator` and
  `TimeWarp.Mediator.Generators` rows show as skipped because they reference
  `$(TimeWarpMediatorVersion)`; that is the self-reference and is correct.
- The Roslyn floor decision and its guards: task 021-005, review round 1 finding M1 (PR #111).
- If the Nuru 4.0.0-beta.6 bump breaks a dev-cli endpoint, fix the call site here (boy-scout);
  do not pin back to beta.5.

## Implementation Notes

- 

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-28)
- Created: cockpit session 04be23b7 (2026-10-28) from Steven's ganda nuget outdated run
