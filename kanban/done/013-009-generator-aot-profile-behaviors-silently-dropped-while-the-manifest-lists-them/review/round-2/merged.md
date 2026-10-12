# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/aot-profile.cs:125
- Description: A `required` member still made `new Handler()` CS9035, with no TWM006.
- Suggestion: Treat `IsRequired` members as unconstructible unless an accessible zero-argument constructor has `SetsRequiredMembers`.
- Source: general
- Disposition notes: Confirmed in round 2. Required and inherited-required handlers are TWM006. `[SetsRequiredMembers]` stays constructible under Aot and Link. Host reports no TWM006.

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-generators-compilation-tests/aot-profile-tests.cs:169
- Description: The Aot runtime test could not tell whether the behavior ran.
- Suggestion: Make the behavior observable.
- Source: general
- Disposition notes: Confirmed in round 2. `LoggingBehavior` throws before `next()`. The test still expects `"pong"`.

## Issues

No new issues.

## Duplicates / conflicts

None. Round 2 re-verified M1 and M2 and raised nothing else.
