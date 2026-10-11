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

- [ ] Multi-binding discovery with tests
- [ ] Inherited-handler exclusion with tests
- [ ] Deterministic ordering with test and doc
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
