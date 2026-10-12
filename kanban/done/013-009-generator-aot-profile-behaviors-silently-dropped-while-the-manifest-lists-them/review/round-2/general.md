# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** review-fix delta for M1 and M2 (required members, behavior runtime test)

## Summary

M1 and M2 both hold. `HasRequiredMembers` walks the handler and its bases for `IFieldSymbol` / `IPropertySymbol` with `IsRequired`, value types stay constructible only when they have no required members, and an accessible zero-argument constructor (optional or `params` allowed) counts only when it carries `SetsRequiredMembersAttribute` if any required member exists. The new Aot/Link tests expect exactly two TWM006 diagnostics and no other errors for a required property plus an inherited required member, no TWM006 under Host, and a clean compile when that constructor is marked `[SetsRequiredMembers]`. M2 holds because `LoggingBehavior.Handle` throws `InvalidOperationException("behavior ran")` before `next`, so `Behavior_DoesNotRunUnderAot` can observe `"pong"` only when the behavior did not run.

## Resolved prior

- M1 — fixed. Required and inherited-required handlers are TWM006 instead of CS9035; `[SetsRequiredMembers]` on the zero-argument constructor stays constructible under Aot and Link, and Host reports no TWM006.
- M2 — fixed. The behavior throws instead of calling `next`, and the runtime test still expects `"pong"`, so a woven behavior would fail the test.

## Issues

No issues.
