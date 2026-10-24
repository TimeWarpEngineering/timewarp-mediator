# Deep code review: 15.0 breaking redesign for architectural purity and performance

## Description

14.0.0 stable shipped on 2026-10-13. 15.0 is a deliberate breaking release: no backward
compatibility, delete MediatR compatibility debt, move the codebase to TimeWarp coding standards,
and make the generated dispatcher the only dispatcher. All consumers (TimeWarp.State,
TimeWarp.Architecture, TimeWarp.Nuru) are TimeWarp repos and will be refactored to match.

This task holds the review itself. The redesign work is split into child tasks after the
decisions in the review are signed off.

Deliverable: `documentation/design/2026-10-13-15.0-breaking-redesign-review.md`.

## Requirements

- Big-picture review across Mediator, State, Architecture, and Nuru. Nuru is the primary consumer
  of the new scoped/generated interfaces and gets close attention.
- Identify what exists only for backward compatibility and what a faster, more functional design
  looks like. Purity and performance over compatibility.
- Every finding cites paths and, where possible, a measured number.

## Checklist

- [x] Create task 021 and worktree
- [x] Survey TimeWarp.State usage of Mediator (`research/timewarp-state-usage.md`)
- [x] Survey TimeWarp.Architecture usage (`research/timewarp-architecture-usage.md`)
- [x] Survey TimeWarp.Nuru usage (`research/timewarp-nuru-usage.md`)
- [x] Inventory the reflection runtime for compat debt (`research/reflection-runtime-inventory.md`)
- [x] Read contracts, generator emitter, generator pipeline, spec, benchmark baseline
- [x] Measure the `ISender` interface dispatch path (benchmark additions in `tests/timewarp-mediator-benchmarks-comparison`)
- [x] Write the review with ranked findings, target architecture, deletions, consumer impact, decisions, child-task plan
- [x] Maintainer sign-off on decisions D1–D6 (2026-10-23: yes to all)
- [x] Create child tasks 021-001 .. 021-007 via `ganda kanban reserve --parent 021` after sign-off (2026-10-23)

## Session

- Created: 2408358 (2026-10-13)
- 2026-10-13: research fan-out (four read-only surveys), review written; interface-path benchmark measured (65 ns / 240 B through `ISender` vs 30 ns / 96 B concrete) and added to the comparison project.
- 2026-10-23: maintainer signed off D1–D6 (yes to all). Child tasks 021-001 .. 021-007 created and published to the to-do column; see the child table below.
- 2026-10-23: PR #105 opened against master (review, kitchen, benchmarks, audit baseline); origin/master merged in so the child kitchens sit beside the parent.

## Child tasks

Decisions D1–D6 accepted as recommended on 2026-10-23. Children run in this order; 021-004,
021-005 and 021-006 may proceed in parallel once 021-003 has merged.

| Id | Title | Depends on | Findings |
|---|---|---|---|
| 021-001 | Benchmarks first: interface-path and pipeline baselines with a regression gate | none | F2, F17 |
| 021-002 | Contracts 15.0: ValueTask contracts, MessageHandlerDelegate, delete compat-only types | 021-001 | F3, F4, F5, F6, F8, F10, F18; D2, D3, D6 |
| 021-003 | Dispatcher emission: non-boxing interface Send, cached delegate chain, manifest | 021-002 | F2, F4, F7, F12, F13 |
| 021-004 | Delete the reflection runtime package, its tests, samples and signing | 021-003 | F1, F16; D1 |
| 021-005 | Profiles and packaging: one emission, two packages, SDK-10 Roslyn floor | 021-004 | F11, F14, F15; D4, D5 |
| 021-006 | Generator incrementality: syntax-provider candidates and a Cached assertion | 021-003 | F9 |
| 021-007 | Docs and release notes for 15.0: one architecture page, migration guide, skill update | 021-002..006 | F1 (docs), section 6 |

Consumer dogfood tasks (TimeWarp.State, TimeWarp.Architecture, TimeWarp.Nuru) are created in
their own repos once `15.0.0-beta.1` packs from a local feed; they gate the beta push.

## Notes

- 013-004 (2026-10-11) already established that no consumer references the `TimeWarp.Mediator`
  runtime package. This review confirms it against today's masters.
- The comparison benchmark measures the concrete `Mediator.Send(Ping)` overload; every consumer
  dispatches through `ISender` / `ISender<TScope>` / `IMediator`, whose generic `Send` is emitted
  as `SendCore` → `Send(object)` → `Box<T>`. See finding F2.
- The 2026-06-17 spec's "Phase D: Delete" is what 15.0 executes, plus `ValueTask` contracts and a
  closure-free pipeline delegate that the spec described but 14.0 did not ship.
