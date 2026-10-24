# Round 2 — merged findings
**Date:** 2026-10-24
**Sources:** general (review oracle, effort 2)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 1 |
| nit | 0 | 5 | 0 |

## Issues

Prior IDs carry forward from `round-1/merged.md`. Round 2 raised no new findings.

### M1 — Severity: bug — Status: fixed
- File: Directory.Packages.props:38; source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj:25; source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:26; .github/dependabot.yml:15-18; tests/timewarp-mediator-generators-compilation-tests/roslyn-floor-tests.cs
- Disposition notes: fixed in 07088c4. The floor is held by a `VersionOverride`, a Dependabot
  ignore, and `Compiler_IsTheDeclaredFloor`. The test passed in `./bin/dev workflow`.

### M2 — Severity: suggestion — Status: fixed
- File: samples/timewarp-mediator-examples-aot/timewarp-mediator-examples-aot.csproj:15-18
- Disposition notes: fixed in 07088c4. `TreatWarningsAsErrors=true`. Verify-samples publishes
  clean.

### M3 — Severity: suggestion — Status: fixed
- File: .timewarp/dev.jsonc:4
- Disposition notes: fixed in 07088c4. The list now names only the two packages that ship.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/timewarp-mediator.csproj:25-26
- Disposition notes: fixed in 07088c4 with `RemoveProperties`.

### M5 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-generators/manifest-emitter.cs:32,51
- Disposition notes: fixed in 07088c4. The manifest is version 2. 021-007 records it in the
  release notes.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-generators/member-names.cs:31-36
- Disposition notes: fixed in 07088c4. The loop is removed. Every `For` caller passes request
  handlers only.

### M7 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:8
- Disposition notes: fixed in 07088c4.

### M8 — Severity: nit — Status: fixed
- File: tests/timewarp-mediator-generators-compilation-tests/host-dispatch-tests.cs:119-133
- Disposition notes: fixed in 07088c4. Both facts are restored on the host path.

### M9 — Severity: suggestion — Status: wontfix
- File: readme.md, documentation/m1-generated-mediator.md, samples/readme.md, source/*/readme.md
- Disposition notes: unchanged from round 1. 021-007 owns the docs, the package readmes, and the
  version bump. Decided by the review oracle.

## Duplicates / conflicts

- None.
