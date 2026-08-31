# M1 generated Mediator analyzer and State golden-file

## Description

Prove the rewrite core: a source generator plus analyzer that treats handlers + behaviors as a compile-time graph, emits a real `sealed Mediator : IMediator`, and matches TimeWarp.State's current pipeline semantics on one ActionSet.

Parent: **004**. Do not implement `ISender<TScope>` here — that is **004-002**.

Design SSOT: `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14 (M1 table).

## Requirements

- Handler-first discovery with explicit assembly membership (`MediatorOptions.Assemblies`, `[assembly: MediatorAssembly]`, or equivalent)
- `TimeWarp.Mediator.Analyzers`: TWM001 (request with no handler), TWM002 (duplicate handler)
- Generated `sealed Mediator : IMediator` with monomorphic `Send`; `Send(object)` is a generated switch (JsonRequestHandler / JS interop path)
- `ValueTask` contracts, including void/`Unit` actions (`IAction` / nested `Handler`)
- MS.DI scope-resolved handlers and behaviors (Host/State default). ServiceGen static fields may appear as an AOT sample, not the State path
- One real State-shaped golden file: `IncrementActionSet` + `StateTransactionBehavior` (scoped) matching today's `Reverse().Aggregate` order (short-circuit, clone/restore, exception notification)
- `mediator.manifest.json` v1
- AOT sample publishes trim/AOT-analyzer-clean with **no** `NoWarn` on IL2026/IL3050

## Checklist

### Design
- [ ] Fold spec header: this epic is 004, this task is 004-001
- [ ] Confirm membership rule so multi-project solutions do not cross-link by accident

### Implementation
- [ ] Analyzer package (TWM001, TWM002) usable on a library that does not run the generator
- [ ] Generator emits `Mediator` + `Send(object)` switch
- [ ] State golden-file: IncrementActionSet + StateTransactionBehavior
- [ ] AOT sample: EnableTrimAnalyzer + EnableAotAnalyzer + IsAotCompatible, warning-clean
- [ ] Benchmarks vs current MakeGenericType fork **and** martinothamar (document the gap; include a CallSiteInlining prototype number even if interceptors stay out of product)

### Documentation
- [ ] Point GitHub issue #52 at 004 / 004-001
- [ ] Notes on what is deferred to 004-002 (scoped senders) and later (interceptors, pruning, streams)

## Out of scope

- `ISender<TScope>` / `IPublisher<TScope>` emit (004-002)
- Call-site interceptors as the default dispatch
- Call-graph pruning
- TimeWarp.State NuGet switch
- TimeWarp.ServiceGen (Nuru 444)

## Notes

- Re-entrancy is mandatory: handlers and behaviors inject `ISender` and `Send` while a dispatch is active. Keep a real injectable `ISender` object; static-only dispatch is not the default.
- Pipeline default: compile-time-fixed order, scope-resolved instances at send (OQ-B in the spec).
- Consumers of this package: TimeWarp.State (primary), then Nuru 443. Do not wait on 444.

## Session

- Created: 2438044 (2026-08-31)
