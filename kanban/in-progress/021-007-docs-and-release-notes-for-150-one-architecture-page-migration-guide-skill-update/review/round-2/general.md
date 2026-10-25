# Round 2 — general
**Date:** 2026-10-26
**Scope reviewed:** the round-1 fix pass, `835b203..7b8074f` (commit `7b8074f`, 11 product files plus `task.md`). The reviewer re-checked each round-1 id against that delta and the shipped source, and scanned the delta for new defects.

## Summary

The fix pass corrects all nine round-1 findings, and the corrected pages agree with each other.
Each claim below was checked against the code, not against the round-1 text:

- `CompareOrdered` sorts by `Order` first and then by `SourceIndex` (`source/timewarp-mediator-generators/candidate-graph.cs:857–866`).
- The generator never reads `TimeWarpMediatorAssembly`. Only the analyzer's `Membership` reads it (`source/timewarp-mediator-analyzers/membership.cs:23`).
- Emission is gated only on the missing-runtime check that reports TWM008 (`candidate-graph.cs:22`, `mediator-generator.cs:8`).

No new bug or suggestion turned up in the delta.

## Prior ids re-verified

| ID | Result | Evidence |
|----|--------|----------|
| M1 | fixed | migration.md:70–75 and :177–179, 15.0.0.md F8 row, and the SKILL gotcha all say that signatures still bind and that a bare `ValueTask` in an expression is CS0229. None says the property hides the type. Grep finds no remaining "hides" or "shadow" outside the parent design review, which is historical |
| M2 | fixed | Every 15.0 install line pins `--version 15.0.0-beta.1` and says why: the root readme, all three `source/*/readme.md`, and the SKILL |
| M3 | fixed | The id-reuse sentence is in migration.md:19–21, the `TimeWarp.Mediator` readme (:11 and the Migration bullet), and the SKILL's previous-surface list |
| M4 | fixed | The baseline header is the recorder's header again: "Recorded by …", Project, Commit, Date. The Ratio note moved to architecture.md:212–213 |
| M5 | fixed | architecture.md Membership and the property table, agent.md, migration.md (properties and Nuru), 15.0.0.md (the F10 row and Unchanged), and the SKILL all say that the analyzer reads the property, that `false` does not stop emission, and that a library does not reference Generators |
| M6 | fixed | architecture.md:171–173 says a lower `Order` is outermost and declaration order breaks ties, which matches `CompareOrdered`. :92–93 says bound per interface and registered once |
| M7 | fixed | The typed-`Send` row cites F2, F3, and F7. The `[GeneratedMediator]` row cites F10 and F11 |
| M8 | fixed | In the root and package readme quick starts, the statements come before the types and `ISender` comes from `CreateScope()`. In m2, the `[assembly: …]` lines sit above the type declarations, and the registration block declares `services` |
| M9 | fixed | The impossible clause is gone from the analyzer readme. The SKILL description lists TWM007, TWM008, TWM010, and TWM011. The release notes say "13.x/14.x reflection dispatcher" |

## Issues

None.

## Considered and not raised

- `documentation/diagnostics.md:49` says the analyzer treats a compilation as a generator host when `TimeWarpMediatorGenerator=true` or `TimeWarpMediatorAssembly=true`. That is analyzer behavior, so it agrees with M5. The fix delta did not touch that file.
- `documentation/migration.md:101` maps the removed membership attributes to "`[assembly: MediatorAssembly]` and `TimeWarpMediatorAssembly`". Both still do membership for the analyzer, and :119–123 explains the split. That is consistent.
- The m2 snippet has no `using TimeWarp.Mediator;`. That was true before this delta too. The snippet is an excerpt of the named-pipelines sample, so this is not raised.
- The root readme says to keep `--version` equal to `<Version>`, but `<Version>` is still `14.0.0`. Round 1 already left the bump out of scope; it belongs to the release PR. The pins name the version that 15.0 ships as.
