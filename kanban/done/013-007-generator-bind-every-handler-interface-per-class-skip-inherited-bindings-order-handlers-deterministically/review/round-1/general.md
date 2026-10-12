# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `9bc7518` (generator handler binding, inherited-binding skip, deterministic order)

## Summary

Discovery now returns every closed `IRequestHandler` and `INotificationHandler` on a concrete class, and the emitter registers the concrete type once plus one interface forward per binding. A two-arity `IRequestHandler<T, R>` still wins over `IRequestHandler<T>` for the same `T` on one class, which matches the separate `IRequest` / `IRequest<TResponse>` contracts. `IsBoundByBaseHandler` skips an interface only when a concrete, non-generic, discoverable base in a member assembly already implements that same closed interface, so a derived class still binds interfaces it adds and an abstract or open generic base does not swallow the closed derived handler. Member assemblies are walked by ordinal name and each handler list is sorted by display name before TWM002 and publish emission. Re-checked the registration, void-request flag, and publish call sites against that graph. `HandlerBindingTests` passed 6/6 and `MemberAssemblyOrderTests` passed 3/3 (byte-identical source either reference order, Alpha → Host → Zeta, one TWM002 whose message lists Alpha then Zeta).

## Issues

No issues.
