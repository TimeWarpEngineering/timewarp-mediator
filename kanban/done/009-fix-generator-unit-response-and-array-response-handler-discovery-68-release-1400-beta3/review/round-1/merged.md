# Round 1 — merged findings
**Date:** 2026-09-25
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:269
- Description: `iface.TypeArguments[1] is ITypeSymbol response2` is always true for the non-null elements of
  `ImmutableArray<ITypeSymbol>`. The pattern only binds a variable.
- Suggestion: Assign `iface.TypeArguments[1]` directly.
- Source: general
- Disposition notes: wontfix (review oracle). The pattern has no behavioral effect, mirrors the `request2`
  binding in the same condition, and matches the #68 patch that was validated against Nuru. Changing it
  would add churn and a re-review round for no benefit.

## Duplicates / conflicts

- None (single reviewer).
