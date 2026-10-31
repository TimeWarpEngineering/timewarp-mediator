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
- [x] Nuru and Nuru.DevCli pins at 4.0.0-beta.10; Roslyn rows untouched at 5.0.0
- [x] dev-cli call sites fixed if the Nuru API moved; net11 asset presence noted
- [x] `ganda repo audit --fix` applied and committed
- [x] dev-cli self-install rebuilt; `./bin/dev --capabilities` complete
- [x] `./bin/dev workflow` green on net10.0 and net11.0 (0 warnings)
- [x] `Compiler_IsTheDeclaredFloor` still passes
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

## Goal (implementer restatement)

Move the two dev-CLI package pins, `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli`, from
`4.0.0-beta.7` to the newest `4.0.0` beta that `ganda nuget outdated` reports today
(expected `4.0.0-beta.10`). Leave both `Microsoft.CodeAnalysis.CSharp` pins at `5.0.0`,
and leave the three floor guards in place: the shipped `VersionOverride`, the Dependabot
ignore for `Microsoft.CodeAnalysis.CSharp*`, and `Compiler_IsTheDeclaredFloor`. If the
new Nuru beta changed an API the dev CLI calls, fix those call sites. Record whether
beta.10 includes a `lib/net11.0` asset, and either keep the dev CLI on `net10.0` or
move it, saying which. Rebuild `./bin/dev` with `self-install`, confirm `--capabilities`
still lists every command in `agent.md`, run `ganda repo audit --fix` and commit what
it changes, and get `./bin/dev workflow` green on `net10.0` and `net11.0` with zero
warnings. Do not bump `<Version>` past `15.0.0-beta.2` and do not run the benchmark gate.

Acceptance: both Nuru pins are at the newest 4.0.0 beta; Roslyn stays 5.0.0 and the
floor test still passes; the rebuilt dev binary lists the documented commands; audit
is clean; workflow packs both nupkgs (`lib/net10.0` and `lib/net11.0`) and the smoke
prints `package-pong` on both TFMs.

## Implementation Notes

- `ganda nuget outdated` on this worktree still names only the two Nuru pins (4.0.0-beta.10)
  and the two Roslyn rows (stay at 5.0.0).
- `TimeWarp.Nuru` 4.0.0-beta.10 contains `lib/net10.0/TimeWarp.Nuru.dll` and no `lib/net11.0`.
  The dev CLI stays on `net10.0`.
- `WorkflowCommand.Handler` is internal because `NuGetVersionService`, `IRepoConfigService`,
  and `IPackableProjectService` are internal in `TimeWarp.Nuru.DevCli` 4.0.0-beta.10.
- `ganda repo audit --fix` changed no files. The audit exits 0 with one advisory:
  `kanban-landed` warns that claim 021 has no usable worktree. That is another task.

## Results

`Directory.Packages.props` pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.10`.
`Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` stay at `5.0.0`.
`VersionOverride="5.0.0"` on the analyzer and generator projects, the Dependabot ignore for
`Microsoft.CodeAnalysis.CSharp*`, and `Compiler_IsTheDeclaredFloor` are unchanged. `<Version>`
stays `15.0.0-beta.2`.

`TimeWarp.Nuru` 4.0.0-beta.10 ships `lib/net10.0` only, so `tools/dev-cli/dev.cs` stays
`#:property TargetFramework=net10.0`. `agent.md` names that beta. The only call-site change
is `WorkflowCommand.Handler`, now internal so it can take the internal DevCli services.

`dotnet run --file tools/dev-cli/dev.cs -- self-install` rebuilt `./bin/dev`. `--capabilities`
lists benchmark (`framework`, `filter`, `record`, `gate`, `aot`), build, pack, test,
verify-samples, workflow, check-version, release, clean, and self-install.

`./bin/dev workflow` (PR mode, `GITHUB_EVENT_NAME` unset) ended `Pipeline SUCCEEDED`.
Build: 0 warnings, 0 errors. Tests on both TFMs: generators 61, analyzers 42, compilation
69, all passed. Samples verified. Pack wrote `TimeWarp.Mediator.15.0.0-beta.2.nupkg` and
`TimeWarp.Mediator.Generators.15.0.0-beta.2.nupkg`. Smoke printed
`package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and net11.0.
`Compiler_IsTheDeclaredFloor` then passed 1/1 on both TFMs (`dotnet test --no-build --filter`).

`ganda repo audit --fix` changed nothing and the audit passes. One advisory remains:
claim 021 has no usable worktree.

### How to validate

Smoke: from this worktree, run `ganda nuget outdated`, then `./bin/dev workflow`.

Expect: outdated lists only `Microsoft.CodeAnalysis.CSharp` and
`Microsoft.CodeAnalysis.CSharp.Workspaces` (5.0.0 → 5.9.0). Both Nuru rows are current.
Workflow prints `0 Warning(s)`, the test counts above, `Samples verified successfully!`,
both `15.0.0-beta.2` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`.
A filtered `Compiler_IsTheDeclaredFloor` run reports `Passed: 1` on net10.0 and net11.0.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
- Implementation: 01a1909c-12db-7a41-beb8-989af33e6cdf (2026-10-31)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1909c-12db-7a41-beb8-989af33e6cdf, max-turns 500, Done — 2026-10-31T06:27:52Z
