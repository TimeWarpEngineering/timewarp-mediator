# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** `master..HEAD` (2f8b932), kanban paths excluded. Two independent general
passes: the review oracle (pass A) and a read-only sub-agent (pass B, a489660e36348b5b0).
Each issue names the pass that raised it, and the oracle re-checked every pass-B claim against the repo.

## Summary

The change does what the task asks. There is one emission, and it always takes `IServiceProvider` and
weaves behaviors. ServiceGen, the Aot and Link profiles, and TWM005, TWM006, and TWM009 are gone,
with no leftover references in `source/`, `tests/`, `samples/`, `tools/`, the props files, or the
slnx. Contracts is now the `TimeWarp.Mediator` package and carries
`analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`. Generators is the only other nupkg. The
generator now compiles linked copies of the five shared graph sources, which avoids the CS8784
load problem. The emitter edits are mechanical removals of the `aot` branch and are correct.

Checks the oracle ran on this branch:
- `./bin/dev build`: succeeded.
- `./bin/dev test --no-build`: succeeded.
- `./bin/dev pack`: two nupkgs plus a snupkg. Both layouts verified. The smoke run printed
  `package-pong via TimeWarp.Mediator.Generated.Mediator`.
- `dotnet publish -c Release` of the AOT sample: no warnings. The binary printed `aot-pong` and exited 0.
- `ganda repo audit` (read-only): 32 passed, 0 failed.

The main risk is that the new Roslyn floor has no guard. The other findings are small: a weaker
AOT gate than its comment says, stale package config, and small cleanups.

## Issues

### Issue 1 — Severity: bug
- File: Directory.Packages.props:37 (also source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj:24, source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:25)
- Description: The Roslyn floor is now held only by the central pin
  `Microsoft.CodeAnalysis.CSharp Version="5.0.0"`, and nothing enforces it.
  - The two shipped Roslyn components dropped `VersionOverride`.
  - The floor guard `Compiler_IsTheDeclaredFloor` was deleted with the floor test project. Its
    Purpose said it existed to stop "whatever a central package bump resolves to".
  - `.github/dependabot.yml` updates nuget weekly with `patterns: [ "*" ]`, and 5.9.0 is already
    available. Master's central pin had already drifted to 5.9.0 while `VersionOverride` held the
    shipped projects at 4.8.0, so the drift is not hypothetical.
  - CI installs `dotnet-version: 10.0.x` (the newest 10.0 SDK), and this machine built with SDK
    11.0.100-rc.2, so no leg compiles on the 10.0.100 compiler.
  - After the next grouped Dependabot bump, both DLLs would reference a newer Roslyn than the
    oldest .NET 10 SDK has. Consumers on that SDK would get CS9057, the generator would not load,
    and every gate would stay green. The floor is a stated contract of this task.
- Suggestion: Pin the floor where it cannot drift silently. Either:
  - add `VersionOverride="5.0.0"` on the `Microsoft.CodeAnalysis.CSharp` reference in both shipped
    csprojs, or
  - add a Dependabot `ignore` for `Microsoft.CodeAnalysis.CSharp*`.

  Also bring the one-assert guard back in the compilation tests: assert that
  `typeof(CSharpCompilation).Assembly.GetName().Version` has major.minor 5.0, so a bump fails the build.
- Source: A, B
- Status: open

### Issue 2 — Severity: suggestion
- File: samples/timewarp-mediator-examples-aot/timewarp-mediator-examples-aot.csproj:15-16
- Description: The comment says "trim and AOT warnings fail this sample", but only IL2026 and
  IL3050 are escalated.
  - `samples/Directory.Build.props:10` sets `TreatWarningsAsErrors=false`, so ILC gets only
    `--warnaserr:IL2026;IL3050`, not `--warnaserror`.
  - With `TrimmerSingleWarn`, warnings from package assemblies such as MS.DI collapse to IL2104
    or IL3053. Those, and the sample's own IL2070/IL2072/IL2075/IL2091/IL3051, would only warn.
  - `verify-samples` prints publish output only when the exit code is non-zero, so those warnings
    would never surface.
  - Today's publish is warning-free (verified), so this is a gate-strength gap, not a live failure.
- Suggestion: Set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in the sample csproj. It flows
  to ILLink and ILC. Alternatively add IL2104 and IL3053 to `WarningsAsErrors`, or make the comment
  match what the gate actually does.
- Source: B (oracle confirmed the props, and that the current publish has no warnings)
- Status: open

### Issue 3 — Severity: suggestion
- File: .timewarp/dev.jsonc:4
- Description: `checkVersionConfig.packages` is still
  `TimeWarp.Mediator,TimeWarp.Mediator.Contracts,TimeWarp.Mediator.Analyzers,TimeWarp.Mediator.Generators`.
  The release pipeline's check-version step (`workflow-command.cs:151-156`) reads this list. So
  release still queries the two retired ids, which conflicts with this task's "two packages" contract.
- Suggestion: Reduce the list to `TimeWarp.Mediator,TimeWarp.Mediator.Generators`.
- Source: B (oracle confirmed)
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-mediator/timewarp-mediator.csproj:22-28
- Description: The analyzer-packing target has two weaknesses:
  - It hooks the private NuGet target `_GetPackageFiles`.
  - Its `MSBuild` task inherits the caller's global properties. A
    `dotnet pack -r <rid>` or `-p:TargetFramework=…` would reach the netstandard2.0 analyzer and
    return a wrong `TargetPath`.

  What works today: there is no nuspec dependency leak, the layout is right, and `--no-build` and
  Configuration are handled.
- Suggestion: Add `RemoveProperties="TargetFramework;RuntimeIdentifier;SelfContained"` to the
  `MSBuild` task. Or use the documented `TargetsForTfmSpecificContentInPackage` /
  `TfmSpecificPackageFile` hook.
- Source: B (oracle confirmed)
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-mediator-generators/manifest-emitter.cs:49
- Description: The manifest JSON lost its `"profile"` key but still says `"version":1`. The only
  in-repo reader (`manifest-document.cs`) was updated. An outside v1 reader that expects `profile`
  would get no signal.
- Suggestion: Bump to `"version":2`, or record the shape change for the 15.0 release notes (021-007).
- Source: B
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-mediator-generators/member-names.cs:30-37
- Description: Notification handler types are still added to the name map. Their only consumer
  was the ServiceGen fields. No `names.For(handler)` call for a notification handler remains. The
  emitter only names request handlers (cache slots) and notification types. The extra entries can
  only force a hash suffix onto a colliding `Dispatch_*` or `Publish_*` name.
- Suggestion: Drop the notification-handler loop. Removing it can change emitted names inside a
  collision group, so check the golden files. If that churn is not worth it, a one-line Design
  note keeping it on purpose is fine.
- Source: B (oracle confirmed callers)
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-mediator-generators/mediator-emitter.cs:8
- Description: The Design comment re-wrap left "CallSiteInlining prototype is the public" as a
  dangling short line.
- Suggestion: Re-flow lines 6-9.
- Source: B
- Status: open

### Issue 8 — Severity: nit
- File: tests/timewarp-mediator-generators-compilation-tests/aot-profile-tests.cs (deleted)
- Description: The deleted file had two Host-path cases, and nothing replaced them:
  - `HandlerWithConstructorParameters_IsFineUnderHost`
  - `HandlerWithRequiredMember_IsFineUnderHost`

  A grep finds no other compilation test with a `required` member handler. The Host emission still
  has to compile such handlers.
- Suggestion: Move those two facts into `host-dispatch-tests.cs` or `shape-matrix-tests.cs`, minus
  the profile parameter.
- Source: B (oracle confirmed)
- Status: open

### Issue 9 — Severity: suggestion
- File: readme.md, documentation/m1-generated-mediator.md, samples/readme.md, source/*/readme.md
- Description: Docs still describe the removed design:
  - The root readme and `m1-generated-mediator.md` still describe the Contracts and Analyzers
    packages, Roslyn 4.8, the Aot/Link profiles, ServiceGen, and TWM005/006/009.
  - `samples/readme.md` still says the AOT sample is "static-field dispatch … no DI container".
  - The packed readmes say `dotnet add package TimeWarp.Mediator --version 14.0.0`. That id and
    version are already on nuget.org as the reflection runtime. `<Version>` is still 14.0.0.

  Task Results defer docs to 021-007. 021-007's requirements cover the architecture page that
  replaces the profile docs, the package readmes, `agent.md`, and the release notes.
- Suggestion: Leave it to 021-007, and make sure its version bump and readme pass covers the `14.0.0` install lines.
- Source: A, B
- Status: open

## Checked and correct

- No leftover references: `TimeWarpMediatorProfile`, `ServiceGen`, `IsAotProfile`, `AotProfile`,
  `timewarp-mediator-contracts`, `TimeWarp.Mediator.Contracts`, and `roslyn-floor` are gone from
  code, props, and the slnx.
- TWM004 is reported only by the analyzer. `IsGeneratorHost` gates only TWM001–TWM003, so the
  analyzer readme's claim is true and nothing is reported twice.
- Release tracking: both assemblies use the same `AnalyzerReleases` files. The removed-rules
  entries for TWM005, TWM006, and TWM009 are consistent.
- `AssertExactNupkgSet` and `PackageIdFromFileName` handle prerelease versions, `.snupkg`, and
  `.symbols.nupkg`.
- `verify-samples` publishes the AOT sample once and runs the native binary. `StampBehavior.Runs != 2`
  proves the behavior is woven on both the typed and the `object` send.
- `TreatWarningsAsErrors`, the banned-API analyzer, and Source Link are untouched in the root props.
