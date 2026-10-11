# XML documentation: unsuppress CS1591 for shipped packages and fix stale doc text

## Description

The root props enable documentation files but add CS1591 to NoWarn, so a large public surface ships undocumented and new gaps are never flagged. Parent review task 013, findings C-18, C-19, C-28, C-29.

- C-19: undocumented public members in `Mediator`, `NotificationHandlerExecutor`, `INotificationPublisher`, both publishers, all wrappers, `ServiceRegistrar`, `RequestExceptionActionProcessorStrategy`, `MediatorServiceConfiguration` class, and the constructors and `Handle` methods of all five pipeline behaviors.
- C-18: broken word "med" (from a mediator find-and-replace) in `i-mediator.cs`, `i-sender.cs`, `i-publisher.cs`, `service-collection-extensions.cs`; duplicated `RequestPreProcessorBehavior` cref; "transient" claim although `Lifetime` is configurable; typos "controlls", "containg"; `<code>true</code>` instead of `<c>`; missing `<returns>` on `Unit` members; empty param/typeparam docs on `ISender`/`IPublisher`.
- C-28: `IIdempotent` and `IIdempotentCommand` public docs say "safe to retry" but the "types only; no enforcement or dedup store" caveat is only in a `#region Purpose` comment. `IQuery<T>` now inherits `IIdempotent` without a doc note.
- C-29: `t` parameter name in `i-pipeline-behavior.cs`; default-token fallback behavior undocumented; `MediatorAssembliesAttribute` accepts null while siblings throw; `NoHandlerException` builds its message before the null check; `IStreamPipelineBehavior` returns text says "Awaitable task".

## Checklist

- [ ] Remove CS1591 from NoWarn for `timewarp-mediator` and `timewarp-mediator-contracts` (keep for tests/samples)
- [ ] Document or `<inheritdoc/>` every public member until the build is clean
- [ ] Fix C-18 text defects
- [ ] Add `<remarks>` to `IIdempotent`, `IIdempotentCommand`, and `IQuery<T>` stating nothing is enforced
- [ ] C-29 nits
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Docs only plus the NoWarn change; no behavior changes.

## Session

- Created: 686431 (2026-10-11)
