# Task 025: Update TimeWarp.Nuru pins to 4.0.0-beta.10 (keep the Roslyn 5.0.0 floor)

## Description

`ganda nuget outdated` on master (2026-10-31, commit 9d0568a) lists four outdated pins. Two
can move: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli`, 4.0.0-beta.7 → 4.0.0-beta.10 (three
betas; nuget.org has beta.8, beta.9 and beta.10). The two `Microsoft.CodeAnalysis.CSharp*` rows
(5.0.0 → 5.9.0) are the deliberate Roslyn floor from task 021-005 and stay at 5.0.0: the
shipped analyzer and generator must load inside the 10.0.100 SDK compiler. Everything else
(DI 10.0.14 / 11.0.0-rc.3, Test.Sdk 18.11.0, SourceLink 10.0.403, BenchmarkDotNet per TFM,
MediatR 14.2.0, martinothamar 3.0.2, xunit, Shouldly, Amuru, Terminal) is already current.

Same shape as task 022 (PR #115). Only the dev CLI consumes Nuru, so the product packages are
untouched; this is not a release.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to `4.0.0-beta.10`
  (or whatever `ganda nuget outdated` reports as the newest 4.0.0 beta on the day).
- Do **not** change `Microsoft.CodeAnalysis.CSharp` or `Microsoft.CodeAnalysis.CSharp.Workspaces`
  from 5.0.0. The `VersionOverride` on the shipped projects, the Dependabot ignore for
  `Microsoft.CodeAnalysis.CSharp*`, and the `Compiler_IsTheDeclaredFloor` compilation test
  guard that floor; leave all three in place and confirm the test still passes.
- Three Nuru betas may have moved an API the dev CLI uses (`tools/dev-cli/**`). Fix call
  sites here (boy-scout); do not pin back. If Nuru beta.10 ships a `lib/net11.0` asset, note
  it in Results; moving the dev CLI off `#:property TargetFramework=net10.0` is optional and
  must be stated either way.
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` after the bump so the AOT dev binary
  is rebuilt on the new pin. Confirm `./bin/dev --capabilities` still lists every command in
  `agent.md` (build, test, pack, verify-samples, benchmark with `--framework/--filter/--record/
  --gate/--aot`, workflow, check-version, release, clean).
- Run `ganda repo audit --fix` and commit whatever it changes (audit fixes are policy).
- `./bin/dev workflow` green on both TFMs: clean, build with 0 warnings, test, verify-samples,
  pack (two nupkgs, `lib/net10.0` and `lib/net11.0`, smoke prints `package-pong` on both).
- `./bin/dev benchmark --gate` is not required; nothing under `source/` or the benchmark
  projects changes.
- `<Version>` stays `15.0.0-beta.2`. Not a release.

## Checklist

### Implementation
- [ ] Nuru and Nuru.DevCli pins at 4.0.0-beta.10; Roslyn rows untouched at 5.0.0
- [ ] dev-cli call sites fixed if the Nuru API moved; net11 asset presence noted
- [ ] `ganda repo audit --fix` applied and committed
- [ ] dev-cli self-install rebuilt; `./bin/dev --capabilities` complete
- [ ] `./bin/dev workflow` green on net10.0 and net11.0 (0 warnings)
- [ ] `Compiler_IsTheDeclaredFloor` still passes
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Notes

- Source: `ganda nuget outdated` run from the cockpit on 2026-10-31. The `TimeWarp.Mediator`
  and `TimeWarp.Mediator.Generators` rows show as skipped because they reference
  `$(TimeWarpMediatorVersion)`; that is the self-reference and is correct.
- The Roslyn floor decision and its guards: task 021-005, review round 1 finding M1 (PR #111).
  Task 022 (PR #115) kept it the same way on 2026-10-28.
- Task 023 pinned the dev CLI to net10.0 because Nuru beta.7 shipped `lib/net10.0` only.

## Implementation Notes

-

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
