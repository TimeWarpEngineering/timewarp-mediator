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
- [ ] Maintainer sign-off on decisions D1–D6
- [ ] Create child tasks 021-001 .. 021-007 via `ganda kanban reserve --parent 021` after sign-off

## Session

- Created: 2408358 (2026-10-13)
- 2026-10-13: research fan-out (four read-only surveys), review written; interface-path benchmark measured (65 ns / 240 B through `ISender` vs 30 ns / 96 B concrete) and added to the comparison project.

## Notes

- 013-004 (2026-10-11) already established that no consumer references the `TimeWarp.Mediator`
  runtime package. This review confirms it against today's masters.
- The comparison benchmark measures the concrete `Mediator.Send(Ping)` overload; every consumer
  dispatches through `ISender` / `ISender<TScope>` / `IMediator`, whose generic `Send` is emitted
  as `SendCore` → `Send(object)` → `Box<T>`. See finding F2.
- The 2026-06-17 spec's "Phase D: Delete" is what 15.0 executes, plus `ValueTask` contracts and a
  closure-free pipeline delegate that the spec described but 14.0 did not ship.
