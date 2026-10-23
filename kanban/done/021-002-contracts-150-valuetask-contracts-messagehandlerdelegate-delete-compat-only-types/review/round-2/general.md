# Round 2 — general
**Date:** 2026-10-23
**Scope reviewed:** fix delta `03fa275..20a559e`: `source/timewarp-mediator-contracts/unit.cs`,
`tests/timewarp-mediator-generators-tests/replacement-request-tests.cs`,
`tests/timewarp-mediator-tests/pipeline-tests.cs`, and the 021-002 and 021-007 `task.md` edits.

## Summary

The fix pass adds the tests M1 asked for and the doc comment M2 asked for. It changes no product
behavior. The new tests do guard the regression M1 described. If the generated nested delegate or
the reflection wrapper forwarded the captured outer `request` and not the delegate's own argument,
either the inner behavior's `Seen` or the handler's `Received` would be the original instance and
the `ShouldBeSameAs` checks would fail. The four new tests were run for this review and all pass
(generators-tests 2/2, mediator-tests 2/2). The fix-pass goal paragraph in `task.md` restates
M1 and M2 accurately, so there is no misread.

## Prior findings re-verified

- **M1 (suggestion): fixed.** Generated side: `ReplacementPipeline` has a single-behavior leaf
  case and a two-behavior chain. `ChainOuterBehavior` has the default order and `ChainInnerBehavior`
  has order 1, and the passing test confirms outer replaces and inner observes the replacement.
  Reflection side: `AddBehavior<ReplacingBehavior>()` then `AddBehavior<ObservingBehavior>()` gives
  the same shape. Static capture fields are per test class, and xUnit runs tests in one class
  sequentially, so the tests cannot race each other.
- **M2 (nit): fixed.** `unit.cs:17-23` says the name does not hide `Task`, does shadow `ValueTask`
  under `using static`, and recommends `Unit.Value` and `Unit.ValueTask`. 021-007 `task.md` Notes
  carry the same text for the migration page.
- **M3 (nit): still wontfix**, deferred to 021-007. The delta does not touch it.

## Issues

<!-- None. The delta raises no new findings. -->
