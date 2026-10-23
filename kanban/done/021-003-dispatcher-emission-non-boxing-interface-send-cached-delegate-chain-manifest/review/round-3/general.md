# Round 3 — general
**Date:** 2026-10-24
**Scope reviewed:** fix-pass delta `885e8d2..e69c39d` (product commit c5e4a3a, baseline commit e69c39d): `source/timewarp-mediator-generators/mediator-emitter.cs`, `tests/timewarp-mediator-generators-compilation-tests/host-dispatch-tests.cs`, `documentation/benchmarks/comparison-baseline.md`, task.md

## Summary

The pass resolves round-2 M8. `HandlerCache` now keeps the scope and exposes each handler as `X => XCache ??= GetRequiredService<X>(Scope)`. The constructor only stores the scope, so a handler is constructed on the first send of its request. A handler that cannot be resolved fails only its own send. `HandlerSlots` names the backing fields `<property>Cache` and adds a numeric suffix if that name is already a property name. Property names come from `MemberNames`. They are sanitized fully qualified names that start with `global__`, so neither the properties nor the fields can collide with `Scope`, `Disposed`, `EnsureAlive`, or `Dispose`. `LiveCache()` still calls `EnsureAlive()` before any slot is read, so a disposed scope still throws `ObjectDisposedException`. The generated file is `#nullable enable`, so the `X?` field annotations are valid.

The new test `UnresolvableHandler_DoesNotBlockTheOtherRequest` asserts the four properties M8 asked for: the good request dispatches, the bad handler is never constructed before its own send, the bad send throws, and the good handler is constructed once and reused. The cache-shape test asserts that no `GetRequiredService` call appears in the constructor. The baseline was re-recorded from clean commit c5e4a3a. Interface send is 8.243 ns / 24 B against a ceiling of 12.471 ns. Void send and the State pipeline are 0 B.

Verified locally: compilation tests 88/88 and generator tests 49/49 pass.

## Issues

None.

## Notes (not findings)

- `??=` on a slot is not synchronized. If two threads send the same request at once on one scope, a transient handler could be constructed twice and one instance discarded. A scope is not a concurrency boundary that callers share, and scoped or singleton registrations return the same instance anyway. This is not worth a finding.
- The goal restatement and the acceptance criteria were updated in this pass to say each slot is lazy. That matches the M8 suggestion and the task intent, so it is not a misread.
