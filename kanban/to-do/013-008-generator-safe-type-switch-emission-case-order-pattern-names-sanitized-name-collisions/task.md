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

- [ ] Case ordering fix with tests
- [ ] Local-name fix with tests
- [ ] Collision-safe member names with test
- [ ] Doc the variance decision in `documentation/m1-generated-mediator.md`
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
