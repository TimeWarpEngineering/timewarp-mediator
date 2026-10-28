# Task 022: Update outdated packages after 15.0.0-beta.1 (keep the Roslyn 5.0.0 floor)

## Description

`ganda nuget outdated` on master (2026-10-28, after the 15.0.0-beta.1 release) lists seven
outdated pins. Five can move. The two `Microsoft.CodeAnalysis.CSharp*` rows are the deliberate
Roslyn floor from task 021-005 and must stay at 5.0.0: the shipped analyzer and generator have
to load inside the 10.0.100 SDK compiler. Bump everything else in one PR so Dependabot stops
opening five separate ones.

## Goal (implementer restatement)

Move the five central package pins that `ganda nuget outdated` reported on 2026-10-28 up to those latest versions in this one change, so Dependabot does not open a separate pull request for each pin. This is not a release: the product `<Version>` stays `15.0.0-beta.1`.

The two Roslyn compiler packages stay at 5.0.0. That is the compiler the oldest .NET 10 SDK (10.0.100) can load, and the shipped analyzer and generator have to load there. The project `VersionOverride`, the Dependabot ignore for `Microsoft.CodeAnalysis.CSharp*`, and the `Compiler_IsTheDeclaredFloor` test stay as they are.

Acceptance criteria:

- `Directory.Packages.props` pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at 4.0.0-beta.6, `Microsoft.Extensions.DependencyInjection` at 10.0.14, `Microsoft.NET.Test.Sdk` at 18.11.0, and `Microsoft.SourceLink.GitHub` at 10.0.403. `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` remain 5.0.0. `Compiler_IsTheDeclaredFloor` still passes.
- `ganda repo audit --fix` is applied, its changes are committed, and `ganda repo audit` exits 0.
- The AOT dev binary is rebuilt on the new `TimeWarp.Nuru.DevCli` pin, and `./bin/dev --capabilities` lists every command named in `agent.md`.
- `./bin/dev workflow` is green: clean, build with 0 warnings, tests, sample verification (the AOT sample publishes with trim and AOT warnings as errors), and pack of the two nupkgs with package smoke succeeding.

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
- [x] Five pins bumped in `Directory.Packages.props`; Roslyn rows untouched at 5.0.0
- [x] `ganda repo audit --fix` applied and committed
- [x] dev-cli self-install rebuilt; `./bin/dev --capabilities` complete
- [x] `./bin/dev workflow` green (0 warnings)
- [x] `Compiler_IsTheDeclaredFloor` still passes
### Review
- [x] Code Review
- [ ] PR with merge commit

## Review

- Round 1: Done

## Notes

- Source: `ganda nuget outdated` run by Steven on 2026-10-28. The `TimeWarp.Mediator` and
  `TimeWarp.Mediator.Generators` rows show as skipped because they reference
  `$(TimeWarpMediatorVersion)`; that is the self-reference and is correct.
- The Roslyn floor decision and its guards: task 021-005, review round 1 finding M1 (PR #111).
- If the Nuru 4.0.0-beta.6 bump breaks a dev-cli endpoint, fix the call site here (boy-scout);
  do not pin back to beta.5.

## Implementation Notes

- `ganda nuget outdated` on 2026-10-28 matched the five pins in the requirements. `TimeWarp.Nuru` 4.0.0-beta.6 did not require a dev-cli call-site change.
- The branch was behind origin/master by kanban lands only (task 022 board move already on master, plus task 023). Merged that before the pin bump.
- `VersionOverride` on the analyzer and generator projects, the Dependabot ignore for `Microsoft.CodeAnalysis.CSharp*`, and `Compiler_IsTheDeclaredFloor` were left in place.

## Results

`Directory.Packages.props` now pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at 4.0.0-beta.6, `Microsoft.Extensions.DependencyInjection` at 10.0.14, `Microsoft.NET.Test.Sdk` at 18.11.0, and `Microsoft.SourceLink.GitHub` at 10.0.403. `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` stay at 5.0.0. `<Version>` stays `15.0.0-beta.1` (pack wrote `TimeWarp.Mediator.15.0.0-beta.1.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.1.nupkg`).

`dotnet run --file tools/dev-cli/dev.cs -- self-install` rebuilt `./bin/dev`. `--capabilities` lists `benchmark`, `build`, `pack`, `test`, `verify-samples`, `workflow`, `check-version`, `clean`, `release`, and `self-install`, which covers every command named in `agent.md`.

`./bin/dev workflow --mode pr` succeeded: clean, build with 0 warnings and 0 errors, tests 57 + 42 + 69 passed, both samples verified (the AOT sample published), pack of the two nupkgs, and `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`. `Compiler_IsTheDeclaredFloor` passed on its own (1/1) after that run.

`ganda repo audit --fix` scaffolded root `glossary.md` from the embedded template and refreshed `.githooks/pre-push.cs` so an empty ref list is allowed. The following audit exits 0. One advisory remains: `kanban-landed` warns that claim 021 has no usable worktree. That claim is not this task, and `--fix` landed 0 kitchens.

### How to validate

Smoke: `ganda nuget outdated`, then `./bin/dev workflow --mode pr`, then `dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj --filter Compiler_IsTheDeclaredFloor -c Release`.

Expect: outdated lists only `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` (5.0.0 → 5.9.0); the five bumped packages are current and the `TimeWarp.Mediator` rows stay skipped. Workflow prints `0 Warning(s)`, tests 57 + 42 + 69 passed, `Samples verified successfully!`, both `15.0.0-beta.1` nupkgs, `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`, and `Pipeline SUCCEEDED`. The filter reports `Passed: 1`.

### Review disposition

- Rounds: 1. Effort 1, roster: general.
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix).
- Disposition: `clean`. No findings were raised. On review, `Compiler_IsTheDeclaredFloor` passed again (1/1, Release), and the Roslyn floor guards and `<Version>` 15.0.0-beta.1 were confirmed unchanged. The goal restatement matches the task, so no misread was recorded.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-28)
- Created: cockpit session 04be23b7 (2026-10-28) from Steven's ganda nuget outdated run
- Implementation: 01a18153-dc43-76d1-93d8-fac4fe3ce8f9 (2026-10-28). Audit `--fix` added `glossary.md` and updated `.githooks/pre-push.cs`. Left the `kanban-landed` advisory for claim 021 (no usable worktree; not this task).
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a18153-dc43-76d1-93d8-fac4fe3ce8f9, max-turns 500, Done — 2026-10-28T07:14:53Z
- Review oracle: claude (claude-opus-5-5), tw-implementation-review effort 1, general reviewer, 1 round, disposition clean — 2026-10-28
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 80 — 2026-10-28T07:16:48Z
