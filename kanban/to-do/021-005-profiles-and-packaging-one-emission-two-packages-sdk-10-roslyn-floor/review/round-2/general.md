# Round 2 — general
**Date:** 2026-10-24
**Scope reviewed:** the fix delta `969c4d8..07088c4` (`fix: hold the Roslyn 5.0 floor and close
review round 1`), kanban paths excluded. Round-1 base is `review/round-1/base.sha`.
Effort 2, one general pass (the review oracle).

## Summary

The fix pass applies M1–M8 as the round-1 ledger asked. M9 stays wontfix for 021-007. The
Roslyn 5.0 floor now has three guards: a `VersionOverride`, a Dependabot ignore, and a
compilation test. The other fixes are small and correct. No new defects were found in the
delta.

Checks the oracle ran on 07088c4:
- `./bin/dev workflow` (clean → build → test → verify-samples → pack) finished with
  `Pipeline SUCCEEDED`. Build had 0 warnings and 0 errors.
- Test counts: generators 57, analyzers 42, compilation 56, with 0 failed. The compilation count
  includes `Compiler_IsTheDeclaredFloor` and the two moved host facts.
- Pack verified the layout of `TimeWarp.Mediator.14.0.0.nupkg` and
  `TimeWarp.Mediator.Generators.14.0.0.nupkg`. Smoke printed
  `package-pong via TimeWarp.Mediator.Generated.Mediator`.
- The AOT sample published and ran under `TreatWarningsAsErrors=true` inside verify-samples.

## Re-verification of round-1 findings

| ID | Result | Evidence |
|----|--------|----------|
| M1 | fixed | Both shipped csprojs set `VersionOverride="5.0.0"`. `.github/dependabot.yml` ignores `Microsoft.CodeAnalysis.CSharp*`, which also covers the `.Workspaces` test pin. `roslyn-floor-tests.cs` asserts that the loaded `CSharpCompilation` is 5.0 and that both shipped assemblies reference `Microsoft.CodeAnalysis` 5.0. The test project references both shipped projects, so a central bump or an override bump fails the test. |
| M2 | fixed | The AOT sample csproj sets `TreatWarningsAsErrors=true`, and the comment now matches what the gate does. The sample still publishes clean. |
| M3 | fixed | `.timewarp/dev.jsonc` lists `TimeWarp.Mediator,TimeWarp.Mediator.Generators`. |
| M4 | fixed | The `MSBuild` task now has `RemoveProperties="TargetFramework;RuntimeIdentifier;SelfContained"`, and the comment explains it. The `_GetPackageFiles` hook remains. Round 1 offered either remedy, and the layout assert covers this. |
| M5 | fixed | The manifest const and JSON are version 2. The Design note gives the reason, and both version tests were updated. Task Results hands the release-note line to 021-007. |
| M6 | fixed | The notification-handler loop is removed and a Design note was added. `MemberNames.For` callers (`mediator-emitter.cs:314`, `:794`, `:800`) pass only `request.HandlerType` (built at `:817-823`), and `For` falls back to `Sanitize` anyway. Goldens pass. |
| M7 | fixed | The Design comment wrap at `mediator-emitter.cs:8` is fixed. |
| M8 | fixed | `host-dispatch-tests.cs` now has `HandlerWithConstructorParameters_IsFineUnderHost` and `HandlerWithRequiredMember_IsFineUnderHost` (constructor, private constructor, optional parameter, notification handler, required member, and inherited required member). |
| M9 | wontfix (unchanged) | The stale docs stay on 021-007. |

## Issues

None in the fix delta.

## Plan alignment

The `## Goal (implementer restatement)` now ends with a paragraph about the round-1 fixes. That
paragraph matches the round-1 ledger and the original Requirements. It does not widen or narrow
the task. No misread was found.
