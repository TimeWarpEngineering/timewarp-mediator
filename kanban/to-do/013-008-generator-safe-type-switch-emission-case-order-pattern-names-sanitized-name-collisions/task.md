# Generator: safe type-switch emission (case order, pattern names, sanitized-name collisions)

## Description

`source/timewarp-mediator-generators/mediator-emitter.cs` emits `case <Type> local:` in alphabetical order with the type name as the local. Parent review task 013, findings G-5, G-6, G-15.

- G-5 (bug): when a base type or interface sorts before its derived type, generated code fails with CS8120 (subsumed case); when it compiles, a derived notification is not delivered to handlers of its base, so MediatR contravariance is lost silently.
- G-6 (bug): `LocalName` lower-cases the type name; types named `Request`, `Notification`, `Event`, `Object`, `Lock`, `Params`, `Base` produce CS0136/CS1001 in a `.g.cs` file.
- G-15 (nit): sanitizing non-alphanumerics to `_` lets `A.B_C` and `A_B.C` share `Dispatch_`/`Publish_`/`ServiceGen` member names.

## Requirements

- Order cases most-derived first (topological by assignability) or use exact-type checks, and either implement base-type delivery for notifications or emit a diagnostic that it is unsupported.
- Use fixed local names (`r0`, `r1`) or `@`-escape.
- Append a stable hash or index on sanitized-name collision.
- Compilation tests for: base-before-derived, interface-typed notification, keyword-named request, colliding sanitized names.

## Checklist

- [x] Case ordering fix with tests
- [x] Local-name fix with tests
- [x] Collision-safe member names with test
- [x] Doc the variance decision in `documentation/m1-generated-mediator.md`
- [x] Implementation review disposition (clean, round 1, general)
- [x] `./bin/dev workflow` green (PR with merge commit is the host open-pr node)

## Notes

Parent: 013.

Review: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 2, roster general, 1 round, disposition clean (no findings).

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implement oracle (Claude Opus 5.5) — G-5/G-6/G-15 fixed, tests added, workflow green.
- Review: grok session `01a12f23-5fd4-73c0-899b-f5666f67b7c1` (2026-10-12)

## Results

- **G-5 (case order + variance):** new `TypeOrder.MostDerivedFirst` (`source/timewarp-mediator-generators/type-order.cs`) orders types so a derived type precedes every base class / interface it implements (ordinal display name breaks ties). `Send(object)` keeps its `switch`, now in that order, so CS8120 cannot occur and `ZDerived : ABase` reaches its own handler. `Publish(object)` now calls a `PublishCore` with one `if (notification is T nN)` per bound type, so a notification reaches the handlers of **every** bound type it is assignable to (its own type, base classes, interfaces). That is MediatR's contravariant delivery, and we chose it over emitting a diagnostic. Generic variance (`IEvent<out T>`) is not modelled.
- **G-6 (locals):** pattern locals are fixed (`r0…`, `n0…`). `LocalName` is removed.
- **G-15 (collisions):** new `MemberNames` (`member-names.cs`) assigns sanitized names once per graph. Unique names stay as they were, so `Dispatch_global__MediatorBenchmarks_Ping` is unchanged. Every type in a colliding group gets `_` plus the FNV-1a hex of its FQ name, with an index fallback. This covers `Dispatch_*`, `Publish_*`, `ServiceGen` fields, and duplicate-name scope suffixes.
- **Docs:** new section "Type switch and notification variance" in `documentation/m1-generated-mediator.md`. The emitter Design region is updated.
- **Tests:** `tests/timewarp-mediator-generators-compilation-tests/type-switch-tests.cs` has 10 cases (Host + Aot compile, plus Aot runtime via a collectible ALC): base-before-derived request, derived/interface/unbound notification delivery, keyword-named messages (`Request`, `Object`, `Lock`, `Params`, `Base`, `Notification`, `Event`), and colliding `A.B_C` / `A_B.C` requests, notifications and handlers. All 10 fail on the previous emitter and pass now.
- **Environment note:** a long-lived shared VBCSCompiler server on this machine kept an older `TimeWarp.Mediator.Analyzers` loaded, which caused spurious TWM001/TWM002 errors in `timewarp-mediator-generators-tests`. `dotnet build-server shutdown` did not clear it. `UseSharedCompilation=false ./bin/dev workflow` is green: 20 + 40 + 6 + 201 tests passed and the package layout was verified. CI runs clean.

### Review

- Rounds: 1. Roster: general. Effort: 2 (by-diff; no specialist axes).
- Final counts: bug 0, suggestion 0, nit 0, open 0, fixed 0, wontfix 0.
- Disposition: clean. No exceptions and no escalations.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

**Smoke:**

```bash
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --filter "FullyQualifiedName~TypeSwitchTests"
UseSharedCompilation=false ./bin/dev workflow
```

**Expect:**

- `TypeSwitchTests`: 10 passed, 0 failed.
- Workflow ends with `Pipeline SUCCEEDED`. Generated `Mediator.g.cs` has `case … r0:` in most-derived-first order and a `PublishCore` with `if (notification is … n0)` blocks. Colliding types produce two distinct `Dispatch_global__A_B_C_<hash>` members.
