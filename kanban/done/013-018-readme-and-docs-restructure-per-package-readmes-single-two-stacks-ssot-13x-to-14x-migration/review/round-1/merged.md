# Round 1 — merged findings
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
- File: migration.md:195
- Description: The feature-matrix "Handler lifetime" row says the Host profile is transient. `EmitHandlerRegistrations` uses `AddScoped` for handlers and closed pipeline behaviors. The generated mediator, sender, and publisher are `AddTransient`. Reflection scanned handlers are `AddTransient`.
- Suggestion: Set the Host cell to scoped, and state under the table that mediator services are transient while handlers and pipeline behaviors are scoped.
- Source: general
- Disposition notes: Fixed on this task before round 2. Host cell is `scoped`. The paragraph under the table states the transient mediator services and the scoped handlers and behaviors.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/readme.md:15
- Description: The readme packed into the 14.x `TimeWarp.Mediator` nupkg tells the reader to install `--version 13.0.0` and never shows the command for the package they are reading.
- Suggestion: Lead with `dotnet add package TimeWarp.Mediator --prerelease` for the net10.0 package, and keep 13.0.0 as the last stable multi-target line.
- Source: general
- Disposition notes: Fixed on this task before round 2. Install leads with `--prerelease` and keeps the 13.0.0 command as the multi-target stable line.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator/readme.md:49
- Description: Mediator and Contracts package readmes say "Released under The Unlicense" while their nuspec license is `Unlicense AND Apache-2.0`. Generators and Analyzers are `Unlicense` and reuse the same derived-code paragraph.
- Suggestion: Name `Unlicense AND Apache-2.0` on Mediator and Contracts. Keep Generators and Analyzers on The Unlicense, and still say `notice` and `unlicense` ship in the package.
- Source: general
- Disposition notes: Fixed on this task before round 2. Mediator and Contracts name the SPDX expression. Generators and Analyzers stay Unlicense and still ship `notice` and `unlicense`.

### M4 — Severity: suggestion — Status: fixed
- File: documentation/m1-generated-mediator.md:17
- Description: The beta.4 bullet lists `GeneratedMediatorServiceCollectionExtensions` as internal in all profiles. That file is emitted only when `!graph.IsAotProfile`.
- Suggestion: Keep the internal-visibility point. Say the DI extension is Host-only.
- Source: general
- Disposition notes: Fixed on this task before round 2. The bullet says the DI extension is Host-only and that Aot and Link do not emit `AddGeneratedMediator()`.

### M5 — Severity: suggestion — Status: fixed
- File: migration.md:120
- Description: The 13.x license paragraph says "without attribution requirements" and then points at Apache 2.0 MediatR code and NOTICE.
- Suggestion: Drop the no-attribution claim. Say MediatR-derived code stays Apache 2.0 and redistributions carry NOTICE.
- Source: general
- Disposition notes: Fixed on this task before round 2. The paragraph no longer says there is no attribution requirement.

### M6 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/readme.md:34
- Description: The Analyzers readme's requirements line stops at SDK 8.0.100 / Roslyn 4.8. The install snippet also references Contracts, which targets net10.0.
- Suggestion: State the practical .NET SDK 10.0.100 floor the way the Generators readme does.
- Source: general
- Disposition notes: Fixed on this task before round 2. The requirements line keeps Roslyn 4.8 and adds the SDK 10.0.100 floor.

### M7 — Severity: nit — Status: fixed
- File: migration.md:214
- Description: Step 3 says the optional `Scope` argument "is in the same section" and does not name the section.
- Suggestion: Link the Membership anchor in `generated-vs-legacy.md`.
- Source: general
- Disposition notes: Fixed on this task before round 2. Step 3 links `#membership`.

## Duplicates / conflicts

- One reviewer. No overlaps.
