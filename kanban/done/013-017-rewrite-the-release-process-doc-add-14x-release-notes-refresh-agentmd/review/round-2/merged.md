# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: agent.md:57
- Description: Shared patterns now mark streams as reflection-only (generated `CreateStream` throws `NotSupportedException`, TWM009).
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Fixed on this task. Re-checked against `mediator-emitter.cs` and `documentation/diagnostics.md`.

### M2 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release-notes-v14.0.0.md:84
- Description: Beta.4 notes now say the DI extension is Host-only. Aot and Link do not emit it.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Fixed on this task. `IsAotProfile` is true for Aot and Link.

### M3 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release-notes-v14.0.0.md:30
- Description: Unreleased notes now include the license expression change and the reflection trim annotations.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Fixed on this task. Wording matches the csproj license expressions and the `RequiresUnreferencedCode` / `RequiresDynamicCode` sites.

### M4 — Severity: nit — Status: fixed
- File: agent.md:7
- Description: Dropped "with no dependencies".
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Fixed on this task.

## Duplicates / conflicts

- Prior IDs M1–M4 carried forward. No new findings on the fix delta.
