# Generator: make the pipeline incremental and stop double-reporting diagnostics

## Description

The generator rebuilds the whole message graph on every compilation and reports the same diagnostics as the bundled analyzer. Parent review task 013, findings G-1, G-2, G-19.

- G-1 (risk): `mediator-generator.cs` combines `CompilationProvider` with options and does all discovery and emission inside one `RegisterSourceOutput`. It captures `Compilation` and `ISymbol`s, so nothing is cacheable and every IDE keystroke re-emits everything.
- G-2 (risk): the Generators nupkg packs Analyzers.dll and its props set `TimeWarpMediatorAssembly=true`, so the analyzer also reports `graph.Diagnostics`. TWM001-003 are likely reported twice in generator hosts. No test asserts the count.
- G-19 (debt): the analyzer rebuilds the graph at `CompilationStart`; `Membership.ReadProperty` loops over every syntax tree for global `build_property.*` keys; no cancellation tokens.

## Requirements

- Split into syntax/semantic providers producing equatable models (strings, `ImmutableArray` of records); emit from those; add `WithTrackingName` steps and a caching test.
- Single reporter for TWM001-003: either drop the analyzer from the generator package or have the generator skip reporting when the analyzer is present. Test that compiles with both and asserts one TWM001.
- Read only `GlobalOptions`; defer graph build to compilation end; pass cancellation tokens.

## Checklist

- [ ] Equatable model layer with tracking names
- [ ] Incrementality test (edit unrelated file, assert cached steps)
- [ ] Single-reporter decision implemented with test
- [ ] Analyzer perf fixes (G-19)
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Largest generator task; consider landing after the correctness tasks so tests exist first.

## Session

- Created: 686431 (2026-10-11)
