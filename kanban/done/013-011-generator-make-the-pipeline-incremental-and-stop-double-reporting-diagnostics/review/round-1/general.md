# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit b9387f4 vs its parent (incremental equatable pipeline, single TWM001–TWM003 reporter, analyzer GlobalOptions and lazy graph)

## Summary

The generator now renders an equatable `MediatorModel` (source text plus `DiagnosticInfo`) and emits from that model. `WithTrackingName` steps are in place. An unrelated edit leaves the options step cached, the model unchanged, and the AddSource output cached; a new handler modifies the model and is emitted. The generator package sets `TimeWarpMediatorGenerator=true`. In that host the analyzer does not report `graph.Diagnostics` (TWM001–TWM003); the generator does, once, with the span reattached to the syntax tree. Library compilations without that property still report TWM001–TWM003 from the analyzer. TWM004 still runs in a generator host. Build properties come from `GlobalOptions` only. Membership and the graph are built on first `ISender.Send` or at compilation end, and the type walk takes a cancellation token. The graph is still built from the `Compilation` inside the model step, so discovery re-executes when the compilation changes and then compares equal. That matches the Design note on `mediator-generator.cs`: whole-program symbol data is what the graph needs, and the cacheable win is no re-emit. Checked against the source, the descriptors, and the tests below. No defect found.

## Issues
