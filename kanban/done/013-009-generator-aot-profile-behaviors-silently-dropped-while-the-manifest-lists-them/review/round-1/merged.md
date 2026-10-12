# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/aot-profile.cs:109
- Description: `HasAccessibleParameterlessConstructor` treats any non-abstract reference type with an accessible all-optional constructor as `new Handler()`. A `required` field or property still makes that call CS9035 in the generated file, and TWM006 is not reported. Value types returned true before the constructor check, so a struct with required members had the same hole.
- Suggestion: If any instance field or property is `IsRequired`, treat the handler as unconstructible unless an accessible zero-argument constructor has `SetsRequiredMembersAttribute`. Cover it with a compilation test that expects TWM006 and no CS9035.
- Source: general
- Disposition notes: Fixed on this walk. Required members on the handler and its bases block `new()`. Structs still count when they reach this check; the graph builder only binds class handlers, so the test covers a required property, an inherited required member, `SetsRequiredMembers` (Aot and Link), and Host (no TWM006).

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-generators-compilation-tests/aot-profile-tests.cs:56
- Description: `Behavior_DoesNotRunUnderAot` only asserts that `Send` returns `"pong"`. `LoggingBehavior.Handle` forwards `next()` unchanged, so the assertion passes whether or not the behavior runs.
- Suggestion: Record a side effect in the behavior (static counter, mutated response, or throw) and assert that it did not happen.
- Source: general
- Disposition notes: Fixed on this walk. The behavior throws `InvalidOperationException("behavior ran")` before `next()`. The runtime test still expects `"pong"`, so a woven behavior fails the test.

## Duplicates / conflicts

None. One reviewer, two suggestions.
