# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 4 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: migration.md:196
- Description: Host handler lifetime is scoped. Mediator, sender, and publisher stay transient. Reflection scanned handlers stay transient.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-checked against `EmitHandlerRegistrations` and `EmitRegistration`.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/readme.md:15
- Description: The 14.x package readme installs this package with `--prerelease` and keeps 13.0.0 as the last multi-target stable line.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: H1 remains `# TimeWarp.Mediator`. No relative links added.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator/readme.md:56
- Description: Mediator and Contracts name `Unlicense AND Apache-2.0`. Generators and Analyzers stay The Unlicense.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Matches the four csproj license expressions (`Unlicense` inherited by Generators and Analyzers).

### M4 — Severity: suggestion — Status: fixed
- File: documentation/m1-generated-mediator.md:17
- Description: The beta.4 bullet says the DI extension is Host-only.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Matches `if (!graph.IsAotProfile)` around the registration source.

### M5 — Severity: suggestion — Status: fixed
- File: migration.md:120
- Description: The 13.x license paragraph no longer says there is no attribution requirement.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Apache 2.0 MediatR-derived code still points at NOTICE.

### M6 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/readme.md:34
- Description: Analyzers requirements keep the Roslyn 4.8 floor and state the .NET SDK 10.0.100 floor.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Same practical floor as the Generators readme. Contracts `TargetFramework` is net10.0.

### M7 — Severity: nit — Status: fixed
- File: migration.md:220
- Description: Step 3 links the Membership section for the optional `Scope` argument.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Anchor `#membership` matches `## Membership` in `generated-vs-legacy.md`.

## Duplicates / conflicts

- Carried M1–M7 forward. No new findings on the fix delta.
