# Round 2 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 5 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- Disposition notes: The `using static Unit` text now says signatures still bind and a bare `ValueTask` in an expression is CS0229. It is fixed in migration.md (both places), the 15.0.0 F8 row, and the SKILL. Fixed in `7b8074f`.

### M2 — Severity: bug — Status: fixed
- Disposition notes: Every 15.0 install line pins `--version 15.0.0-beta.1` and explains the 14.0.0 fallback. Fixed in `7b8074f`.

### M3 — Severity: bug — Status: fixed
- Disposition notes: The id-reuse sentence (the reflection runtime through 14.0.0, the contracts and analyzer in 15.0) is on the migration page, the contracts readme, and the SKILL. Fixed in `7b8074f`.

### M4 — Severity: suggestion — Status: fixed
- Disposition notes: The baseline header matches `dev benchmark --record`, and the Ratio note moved to architecture.md. Fixed in `7b8074f`.

### M5 — Severity: bug — Status: fixed
- Disposition notes: The docs now say that `TimeWarpMediatorAssembly` is analyzer membership only and that the generator always emits. That matches the source (`membership.cs:23`, `candidate-graph.cs:22`). The Nuru bullet tells the library not to reference Generators. This settles the maintainer question round 1 raised: the docs follow the generator shipped in 021-006. Fixed in `7b8074f`.

### M6 — Severity: bug — Status: fixed
- Disposition notes: A lower `Order` is outermost and declaration order breaks ties, matching `CompareOrdered` (`candidate-graph.cs:857–866`). A handler is bound per interface and registered once. Fixed in `7b8074f`.

### M7 — Severity: suggestion — Status: fixed
- Disposition notes: The typed-`Send` row cites F2, F3, and F7. The `[GeneratedMediator]` row cites F10 and F11. Fixed in `7b8074f`.

### M8 — Severity: suggestion — Status: fixed
- Disposition notes: In the quick starts, the statements come first and `ISender` is resolved from a scope. In m2, the assembly attributes come before the types. Fixed in `7b8074f`.

### M9 — Severity: nit — Status: fixed
- Disposition notes: The stale phrases are removed or corrected. Fixed in `7b8074f`.

## New findings

None.

## Duplicates / conflicts

- None. A single reviewer covered this round. The four items under "Considered and not raised" in general.md are consistent with the docs or out of scope, so none became a finding.
