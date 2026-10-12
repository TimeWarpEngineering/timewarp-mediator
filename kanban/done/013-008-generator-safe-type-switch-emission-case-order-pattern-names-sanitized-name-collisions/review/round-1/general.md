# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `3d1d2f5` (most-derived type switch, contravariant publish, safe generated names)

## Summary

`TypeOrder.MostDerivedFirst` is Kahn's algorithm over "derived before each base class and exact implemented interface," with an ordinal fully-qualified-name tie-break. That order is what `Send(object)` emits, so a `ZDerived : ABase` case cannot be subsumed (CS8120) and the derived request hits its own handler. `Publish(object)` no longer switches: `PublishCore` runs one `if (notification is T nN)` per bound type, most-derived first, so a notification is delivered to its own handlers and to every bound base and interface. Pattern locals are `r0…` / `n0…`, which removes the keyword and parameter clashes from `LocalName`. `MemberNames` keeps a unique sanitized name unchanged (`Dispatch_global__MediatorBenchmarks_Ping` stays valid: `Ping` and `PingHandler` do not sanitize to the same string) and suffixes each type in a colliding group with the FNV-1a of its fully qualified name, falling back to an ordinal index when that suffix is already taken. Scope suffixes use that map only when two scopes share a simple name. Generic variance (`IEvent<out T>`) is an exact-symbol gap and is stated in `type-order.cs` and in the new doc section. `TypeSwitchTests` passed 10/10 on this tree (Host and Aot compile, plus Aot runtime through a collectible load context).

## Issues

No issues.
