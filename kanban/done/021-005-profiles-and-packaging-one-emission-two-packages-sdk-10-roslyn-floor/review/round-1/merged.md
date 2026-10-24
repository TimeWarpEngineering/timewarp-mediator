# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general (pass A: review oracle; pass B: sub-agent a489660e36348b5b0)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 2 | 0 | 1 |
| nit | 5 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: Directory.Packages.props:37; source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj:24; source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:25
- Description: The Roslyn 5.0.0 floor is only the central pin, and nothing guards it.
  - The shipped projects no longer use `VersionOverride`.
  - The `Compiler_IsTheDeclaredFloor` test was deleted.
  - Dependabot bumps every NuGet package weekly, and 5.9.0 already exists.
  - CI uses the newest `10.0.x` SDK.

  So a bump would silently ship analyzer and generator DLLs that the 10.0.100 SDK cannot load
  (CS9057), and every gate would stay green.
- Suggestion: Add `VersionOverride="5.0.0"` on both shipped csprojs, or add a Dependabot ignore
  for `Microsoft.CodeAnalysis.CSharp*`. Also add a compilation test that asserts the
  `CSharpCompilation` assembly is 5.0.
- Source: general (A, B)
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: samples/timewarp-mediator-examples-aot/timewarp-mediator-examples-aot.csproj:15-16
- Description: The AOT gate escalates only IL2026 and IL3050. IL2104, IL3053, and the
  DynamicallyAccessedMembers trim warnings would only warn, because samples set
  `TreatWarningsAsErrors=false`. The comment says all trim and AOT warnings fail the sample. The
  current publish is warning-free.
- Suggestion: Set `TreatWarningsAsErrors=true` in the sample csproj, or widen `WarningsAsErrors`
  to cover those warnings, or correct the comment.
- Source: general (B)
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: .timewarp/dev.jsonc:4
- Description: The release check-version step still lists the retired
  `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Analyzers` ids.
- Suggestion: Change the list to `TimeWarp.Mediator,TimeWarp.Mediator.Generators`.
- Source: general (B)
- Disposition notes:

### M4 — Severity: nit — Status: open
- File: source/timewarp-mediator/timewarp-mediator.csproj:22-28
- Description: The pack target hooks the private `_GetPackageFiles` target. Its `MSBuild` call
  also passes the caller's `TargetFramework` and `RuntimeIdentifier` global properties through to
  the netstandard2.0 analyzer.
- Suggestion: Add `RemoveProperties="TargetFramework;RuntimeIdentifier;SelfContained"`, or use
  `TargetsForTfmSpecificContentInPackage`.
- Source: general (B)
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/manifest-emitter.cs:49
- Description: The manifest dropped its `"profile"` key but still reports `"version":1`.
- Suggestion: Bump the version to 2, or record the shape change for the 021-007 release notes in
  task Results.
- Source: general (B)
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/member-names.cs:30-37
- Description: The notification-handler entries in `MemberNames` are dead now that ServiceGen is
  gone, and they can still force a hash suffix onto a colliding name.
- Suggestion: Remove the loop and check the golden files. If you keep it, add a Design note
  saying it stays on purpose.
- Source: general (B)
- Disposition notes:

### M7 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:8
- Description: The Design comment has a broken line wrap.
- Suggestion: Re-flow lines 6-9.
- Source: general (B)
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: tests/timewarp-mediator-generators-compilation-tests/ (aot-profile-tests.cs deleted)
- Description: The deleted file held Host-path coverage that nothing replaced:
  - `HandlerWithConstructorParameters_IsFineUnderHost`
  - `HandlerWithRequiredMember_IsFineUnderHost`
- Suggestion: Move both facts into a remaining compilation test file, without the profile
  parameter.
- Source: general (B)
- Disposition notes:

### M9 — Severity: suggestion — Status: wontfix
- File: readme.md, documentation/m1-generated-mediator.md, samples/readme.md, source/*/readme.md
- Description: Several docs still describe the old design:
  - The root readme, the generated-mediator page, and the samples readme still mention
    Contracts/Analyzers, Roslyn 4.8, the Aot profile, ServiceGen, and TWM005/006/009.
  - The packed readmes install `TimeWarp.Mediator --version 14.0.0`, and that id and version are
    already the published reflection runtime.
- Suggestion: Defer to 021-007.
- Source: general (A, B)
- Disposition notes: wontfix on this id, decided by the review oracle. Task Results explicitly
  defer docs to 021-007. 021-007's requirements already cover the architecture page that replaces
  the profile docs, every package `readme.md`, `agent.md`, the release notes, and the
  `15.0.0-beta.1` version. Fixing it here would duplicate that task.

## Duplicates / conflicts

- Both passes raised M1 independently. Pass A found that CI and the local SDK never exercise the
  10.0.100 compiler. Pass B found the Dependabot grouping and the drift already on master. The
  two were merged into M1.
- Both passes raised M9. The oracle split the shipped-config part out as M3, because
  `.timewarp/dev.jsonc` is tooling config, not docs, and is not in 021-007's scope.
- No severity conflicts.
