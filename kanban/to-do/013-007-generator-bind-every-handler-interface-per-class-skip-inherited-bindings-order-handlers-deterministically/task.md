# Generator: bind every handler interface per class, skip inherited bindings, order handlers deterministically

## Description

`source/timewarp-mediator-analyzers/message-graph-builder.cs` discovers at most one handler binding per class and treats inherited interfaces as new bindings. Parent review task 013, findings G-3, G-4, G-10.

- G-3 (bug): `TryGetRequestHandler` and `TryGetNotificationHandler` return on the first matching interface in `AllInterfaces`. A class implementing `INotificationHandler<A>` and `INotificationHandler<B>` loses one; the other request gets a false TWM001.
- G-4 (risk): `AllInterfaces` includes interfaces inherited from a concrete base handler, so `class B : A` is reported as a TWM002 duplicate and notifications run twice.
- G-10 (risk): member assemblies come from an `ImmutableHashSet`, and handlers are emitted in discovery order; notification handler order and the TWM002 message can differ between builds with several member assemblies.

## Requirements

- Return all matching bindings per type; register each.
- Count only interfaces the type itself introduces (or exclude types whose base already implements the handler interface).
- Sort member assemblies by name and handlers by display name; document the ordering rule in `documentation/m1-generated-mediator.md`.
- Tests in `tests/timewarp-mediator-generators-tests`: multi-interface handler, derived handler, two member assemblies with fixed order.

## Checklist

- [x] Multi-binding discovery with tests
- [x] Inherited-handler exclusion with tests
- [x] Deterministic ordering with test and doc
- [x] Implementation review disposition (clean, round 1, general)
- [ ] `./bin/dev workflow` green; PR with merge commit (workflow green; PR is opened by the host)

## Notes

Parent: 013.

Review: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 2, roster general, 1 round, disposition clean (no findings).

## Results

- `message-graph-builder.cs`: `GetRequestHandlers` / `GetNotificationHandlers` return every matching closed interface on a class (G-3). A two-arity `IRequestHandler<T, R>` still wins over `IRequestHandler<T>` for the same `T` on one class.
- `IsBoundByBaseHandler` skips an interface that a concrete, non-generic, discoverable base class in a member assembly already implements (G-4). The derived class still binds interfaces it adds. Abstract and generic bases are unchanged.
- Member assemblies are walked by ordinal assembly name, and each handler list is sorted by display name (G-10). This fixes notification handler order and the TWM002 handler list and location.
- `mediator-emitter.cs`: `EmitHandlerRegistrations` used to skip every interface forward after a handler type's first one. It now registers the concrete type once and adds one `IRequestHandler<…>` / `INotificationHandler<…>` forward per binding. Without this, the second interface never resolved from DI.
- Docs: new "Handler discovery and ordering" section in `documentation/m1-generated-mediator.md`.
- Tests:
  - `tests/timewarp-mediator-generators-tests/handler-binding-tests.cs` + `bindings/binding-messages.cs`: one class with 2 notifications + 3 requests; a concrete base handler with a derived handler (no TWM002, publish runs once, the derived class binds its own new interface); an abstract generic base.
  - `tests/timewarp-mediator-generators-compilation-tests/member-assembly-order-tests.cs`: two in-memory member assemblies (`Zeta`, `Alpha`) referenced in both orders. Generated source is byte-identical, notification handlers come out Alpha → Host → Zeta, and the TWM002 message is fixed. This test lives in the compilation-tests project because only that project can build several member assemblies in memory.
- Behavior note: notification handlers inside one assembly now run in display-name order instead of declaration/namespace walk order.

### Review

- Rounds: 1. Roster: general. Effort: 2 (by-diff; no specialist axes).
- Final counts: bug 0, suggestion 0, nit 0, open 0, fixed 0, wontfix 0.
- Disposition: clean. No exceptions and no escalations.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
dotnet build-server shutdown   # the compiler server can keep a stale analyzer DLL loaded
./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-tests -c Release --no-build --filter "FullyQualifiedName~HandlerBindingTests"
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --no-build --filter "FullyQualifiedName~MemberAssemblyOrderTests"
ganda repo audit
```

Expect: `Pipeline SUCCEEDED`. HandlerBindingTests: 6 passed. MemberAssemblyOrderTests: 3 passed. The generators-tests project builds with no TWM001 for `GetOrderLabel` / `ArchiveOrder` and no TWM002 for `GetInvoiceTotal`. `ganda repo audit` reports "Repository passes all audit checks."

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12 implement (claude-opus-5-5, ganda task work): discovery, emitter registration, ordering, docs, tests; `./bin/dev workflow` green; `ganda repo audit` clean.
- Review: grok session `01a12f11-0cf1-7942-ae9e-83adf1bc56cb` (2026-10-12)
