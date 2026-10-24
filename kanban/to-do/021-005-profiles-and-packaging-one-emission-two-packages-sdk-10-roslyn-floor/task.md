# Profiles and packaging: one emission, two packages, SDK-10 Roslyn floor

## Description

The generator emits three profiles (Host, Aot, Link) with `ServiceGen` and diagnostics TWM005
and TWM006 to describe them; the Aot and Link profiles drop pipeline behaviors. Four packages
ship where two are needed, and the Roslyn 4.8 floor plus its test project exist only to support
hosts no TimeWarp consumer uses. Parent review task 021, findings F11, F14, F15; decisions D4
and D5 signed off 2026-10-23.

## Goal (implementer restatement)

Ship one generated mediator. The host always supplies `IServiceProvider`, pipeline behaviors
are always woven, and the Aot and Link emission paths, `ServiceGen`, `TimeWarpMediatorProfile`,
TWM005, and TWM006 are gone. TWM009 goes with them because the stream feature is already gone.

The contracts assembly becomes the `TimeWarp.Mediator` package (id, assembly name, and project
folder). That package carries the TWM001–TWM004 analyzer DLL as an analyzer asset. The separate
`TimeWarp.Mediator.Analyzers` nupkg is deleted; the analyzer project stays because it is
netstandard2.0 and the library is net10.0. `TimeWarp.Mediator.Generators` stays and reports
TWM007, TWM008, TWM010, and TWM011. It no longer bundles the analyzer DLL.

The Roslyn floor for the shipped components is the stable compiler package that matches the
oldest .NET 10 SDK (Microsoft.CodeAnalysis.CSharp 5.0.0). The Roslyn 4.8 floor test project
is deleted. `dev pack` fails unless the artifacts directory contains exactly the two expected
nupkgs, and the package-smoke consumer references only those two packages. The AOT sample
publishes with PublishAot, IsAotCompatible, trim and AOT analyzers on, no NoWarn of IL2026 or
IL3050, and a behavior that actually runs. `documentation/diagnostics.md` matches the new
diagnostic ownership. TreatWarningsAsErrors, the banned-API analyzer, and Source Link stay.

Done when `./bin/dev workflow` is green on this branch and the changes are committed and
pushed. Opening the pull request is the host open-pr node, not this walk.

Round 1 still left that floor unprotected. The shipped analyzer and generator pin
`Microsoft.CodeAnalysis.CSharp` 5.0.0 with `VersionOverride`, Dependabot ignores
`Microsoft.CodeAnalysis.CSharp*`, and a compilation test fails unless the loaded
`CSharpCompilation` assembly and the shipped `Microsoft.CodeAnalysis` reference are 5.0.
The AOT sample treats every warning as an error, so IL2104, IL3053, and the other trim
warnings fail the publish the same way IL2026 and IL3050 do. Release check-version names
only `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators`. Packing the analyzer drops
the caller's `TargetFramework`, `RuntimeIdentifier`, and `SelfContained`. The manifest
is version 2 because the profile field is gone. Member names skip notification handlers.
Handlers with constructor parameters or required members still compile on the host path.
The stale package docs stay on 021-007. The manifest shape change belongs in that
release note.

## Requirements

- Remove `TimeWarpMediatorProfile`, the Aot and Link emission paths, `ServiceGen`, TWM005 and
  TWM006. One emission; the host supplies `IServiceProvider`.
- The AOT sample publishes with `PublishAot`, `IsAotCompatible`, trim and AOT analyzers on, no
  `NoWarn` on IL2026/IL3050, with behaviors woven (gate from review section 9).
- Rename the contracts package to `TimeWarp.Mediator` (`PackageId`, `AssemblyName`, project
  folder via `git mv`). The analyzer DLL (TWM001-TWM004) ships as an analyzer asset inside it;
  the separate `TimeWarp.Mediator.Analyzers` package is deleted. `TimeWarp.Mediator.Generators`
  stays, bundling TWM007-TWM011. TWM009 is deleted with the stream feature.
- Raise the Roslyn floor to the version shipped with the .NET 10 SDK; delete
  `tests/timewarp-mediator-generators-roslyn-floor-tests`; update `Directory.Packages.props`.
- `dev pack` asserts exactly two nupkgs with the expected layout; package smoke consumer
  references the two packages only.
- `documentation/diagnostics.md` updated for the removed and moved diagnostics.
- Keep `TreatWarningsAsErrors`, banned-API analyzer and Source Link intact.

## Checklist

- [x] Profiles, `ServiceGen`, TWM005, TWM006 removed
- [x] AOT sample publishes with behaviors woven and analyzers on
- [x] Contracts package renamed to `TimeWarp.Mediator`, analyzer bundled, Analyzers package deleted
- [x] Roslyn floor raised, floor test project deleted
- [x] `dev pack` asserts two packages; smoke consumer updated
- [x] `documentation/diagnostics.md` updated
- [x] `./bin/dev workflow` green
- [x] Round-1 findings M1–M8 applied; M9 stays on 021-007
- [ ] PR with merge commit

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7, M8
- Fix pass: M1–M8 applied on this id. M9 stays wontfix for 021-007.

## Notes

Parent: 021. Depends on: 021-004 (the old `TimeWarp.Mediator` package id must be free before
the rename). Review sections 3 (F11, F14, F15), 4 (packages, diagnostics), 7 (D4, D5).
Nuru reads `build_property.TimeWarpMediatorProfile` today; its replacement is the
`[GeneratedMediator]` attribute from 021-002/021-003, tracked in the Nuru consumer task.

## Results

One emission. The generated mediator takes `IServiceProvider`, weaves behaviors, and no longer
emits `ServiceGen` or a profile field. TWM005, TWM006, and TWM009 are removed. The contracts
folder is `source/timewarp-mediator` with package id `TimeWarp.Mediator`, and that nupkg carries
`analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`. `TimeWarp.Mediator.Generators` is the
second nupkg and does not bundle the analyzer. Roslyn is pinned at `Microsoft.CodeAnalysis.CSharp`
5.0.0. The AOT sample publishes with trim and AOT analyzers as errors and fails unless
`StampBehavior` runs on both sends. `documentation/diagnostics.md` and the packed readmes match.
`agent.md`, the skill, migration, overview, and release notes stay for 021-007. The pull request
is the host open-pr node.

Round 1 findings M1–M8 are applied. The shipped analyzer and generator use
`VersionOverride="5.0.0"` on `Microsoft.CodeAnalysis.CSharp`, Dependabot ignores
`Microsoft.CodeAnalysis.CSharp*`, and `Compiler_IsTheDeclaredFloor` fails unless the loaded
`CSharpCompilation` assembly and both shipped `Microsoft.CodeAnalysis` references are 5.0.
The AOT sample sets `TreatWarningsAsErrors`, so IL2104, IL3053, and the other trim warnings
fail publish along with IL2026 and IL3050. `checkVersionConfig.packages` is
`TimeWarp.Mediator,TimeWarp.Mediator.Generators`. The analyzer pack target removes
`TargetFramework`, `RuntimeIdentifier`, and `SelfContained`. The manifest is version 2
because the profile field is gone; 021-007 should say that in the release notes. Member
names skip notification handlers. `HandlerWithConstructorParameters_IsFineUnderHost` and
`HandlerWithRequiredMember_IsFineUnderHost` compile on the host path. M9 stays wontfix:
the stale docs and readmes remain 021-007's work.

`ganda repo audit`: Passed 32, Failed 0, Skipped 2.

### How to validate

Smoke: `./bin/dev workflow`

Expect: the log ends with `Pipeline SUCCEEDED`. Analyzers 42 passed, generators 57 passed,
compilation 56 passed, failed 0. NativeAOT publish of `timewarp-mediator-examples-aot` exits 0
and its stdout contains `aot-pong` (the process exits 1 unless `StampBehavior` ran twice).
Pack reports layout verified for `TimeWarp.Mediator.14.0.0.nupkg` and
`TimeWarp.Mediator.Generators.14.0.0.nupkg` only. Package smoke prints
`package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.

### Review

Round 1 is done (effort 3, roster general, two independent general passes). The fix pass
applied M1–M8. M9 stays wontfix. Round 2 is the host review node. Counts below are this
pass's claim; `review/round-1/merged.md` stays the round-1 ledger.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 1 |
| nit | 0 | 5 | 0 |

- **M1–M8:** applied in this pass. M1 pins Roslyn 5.0.0 with `VersionOverride`, a Dependabot
  ignore, and `Compiler_IsTheDeclaredFloor`.
- **M9 (wontfix):** the stale docs and readmes stay on 021-007, which already owns them.
  That release note should also record manifest version 2 (the profile field is gone).

The implementation matches the original task and the implementer restatement. No misread was found.

Artifacts:
- `review/review-framework.md`
- `review/round-1/general.md`
- `review/round-1/merged.md` (live ledger)

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: implement oracle (2026-10-24). `./bin/dev workflow` Pipeline SUCCEEDED.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-24T09:58:29Z
- Review oracle round 1: reviewer-claude (claude-opus-5-5, headless), effort 3, roster general.
  Second general pass by sub-agent a489660e36348b5b0. The review oracle ran build, test, pack,
  AOT publish and a read-only audit. All were green. One bug and seven other findings are open,
  so a fix pass was requested (2026-10-24).
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-24T10:13:00Z
- Fix pass: implement-fix-2 (grok, model grok-4.7). `./bin/dev workflow` Pipeline SUCCEEDED
  (analyzers 42, generators 57, compilation 56). `ganda repo audit` Passed 32, Failed 0, Skipped 2.
  M1–M8 applied; M9 left for 021-007 (2026-10-24).
