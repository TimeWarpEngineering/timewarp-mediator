# Round 2 — general
**Date:** 2026-08-31
**Scope reviewed:** post-fix delta after round-1 M1–M4, plus re-verify prior IDs against current code

## Summary

Round-1 Issues 1–4 (M1–M4) are all resolved in the current working tree. `TryCloseBehavior` now returns null when `ImplementsPipeline` fails, with `UnitOnlyBehavior_ClosesOntoUnitRequestsOnly` green; the manifest path is documented and tested as an embedded `MediatorManifest` const with `ProjectDir`/`IntermediateOutputPath` removed; `DiscoverBehaviors` sorts member assemblies by `Name` (Ordinal) before `SourceIndex`; and Aot’s behavior skip is documented in `MediatorEmitter` Design and `Documentation/m1-generated-mediator.md`. No new issues found on the post-fix delta.

## Issues

<!-- none — M1–M4 fixed; no new findings -->
