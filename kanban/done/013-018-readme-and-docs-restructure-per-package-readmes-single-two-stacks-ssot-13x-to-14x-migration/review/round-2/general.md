# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** re-check of M1–M7 against the post-fix text, plus the fix delta in `migration.md`, `documentation/m1-generated-mediator.md`, and the four `source/*/readme.md` files.

## Summary

Each round-1 finding is fixed and matches the emitter, the license expressions, and the Membership anchor. The fix delta adds no new relative links, does not change package-readme H1s, and does not reintroduce a version pin or an internal task id. No new issues.

## Issues

<!-- None. -->

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: migration.md:196
- Description: The Host cell is `scoped`. The paragraph under the table says `AddGeneratedMediator()` registers the mediator, sender, and publisher as transient and the handlers and pipeline behaviors as scoped. That matches `EmitRegistration` (`AddTransient`) and `EmitHandlerRegistrations` (`AddScoped`).
- Status: fixed

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/readme.md:15
- Description: Install leads with `dotnet add package TimeWarp.Mediator --prerelease` for this net10.0 package and keeps `--version 13.0.0` as the last stable multi-target line. The H1 is still `# TimeWarp.Mediator`.
- Status: fixed

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator/readme.md:56
- Description: Mediator and Contracts say `Unlicense AND Apache-2.0`, matching their csproj `PackageLicenseExpression`. Generators and Analyzers say The Unlicense and that `notice` and `unlicense` ship in the package, matching `source/Directory.Build.props`.
- Status: fixed

### M4 — Severity: suggestion — Status: fixed
- File: documentation/m1-generated-mediator.md:17
- Description: The beta.4 bullet keeps internal visibility and says `GeneratedMediatorServiceCollectionExtensions` is Host-only. Aot and Link do not emit `AddGeneratedMediator()`, which matches `MediatorEmitter.Emit`.
- Status: fixed

### M5 — Severity: suggestion — Status: fixed
- File: migration.md:120
- Description: The 13.x license paragraph says Unlicense for 13.x and that MediatR-derived code stays Apache 2.0, so redistributions carry NOTICE. The "without attribution requirements" sentence is gone.
- Status: fixed

### M6 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/readme.md:34
- Description: The requirements line still states Roslyn 4.8 / SDK 8.0.100 and adds that the library needs .NET SDK 10.0.100 because Contracts targets net10.0.
- Status: fixed

### M7 — Severity: nit — Status: fixed
- File: migration.md:220
- Description: Step 3 links `./documentation/generated-vs-legacy.md#membership`. That heading exists.
- Status: fixed
