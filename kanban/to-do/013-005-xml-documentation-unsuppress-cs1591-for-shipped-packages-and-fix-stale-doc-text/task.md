# XML documentation: unsuppress CS1591 for shipped packages and fix stale doc text

## Description

The root props enable documentation files but add CS1591 to NoWarn, so a large public surface ships undocumented and new gaps are never flagged. Parent review task 013, findings C-18, C-19, C-28, C-29.

- C-19: undocumented public members in `Mediator`, `NotificationHandlerExecutor`, `INotificationPublisher`, both publishers, all wrappers, `ServiceRegistrar`, `RequestExceptionActionProcessorStrategy`, `MediatorServiceConfiguration` class, and the constructors and `Handle` methods of all five pipeline behaviors.
- C-18: broken word "med" (from a mediator find-and-replace) in `i-mediator.cs`, `i-sender.cs`, `i-publisher.cs`, `service-collection-extensions.cs`; duplicated `RequestPreProcessorBehavior` cref; "transient" claim although `Lifetime` is configurable; typos "controlls", "containg"; `<code>true</code>` instead of `<c>`; missing `<returns>` on `Unit` members; empty param/typeparam docs on `ISender`/`IPublisher`.
- C-28: `IIdempotent` and `IIdempotentCommand` public docs say "safe to retry" but the "types only; no enforcement or dedup store" caveat is only in a `#region Purpose` comment. `IQuery<T>` now inherits `IIdempotent` without a doc note.
- C-29: `t` parameter name in `i-pipeline-behavior.cs`; default-token fallback behavior undocumented; `MediatorAssembliesAttribute` accepts null while siblings throw; `NoHandlerException` builds its message before the null check; `IStreamPipelineBehavior` returns text says "Awaitable task".

## Checklist

- [x] Remove CS1591 from NoWarn for `timewarp-mediator` and `timewarp-mediator-contracts` (keep for tests/samples)
- [x] Document or `<inheritdoc/>` every public member until the build is clean
- [x] Fix C-18 text defects
- [x] Add `<remarks>` to `IIdempotent`, `IIdempotentCommand`, and `IQuery<T>` stating nothing is enforced
- [x] C-29 nits
- [x] `./bin/dev workflow` green
- [ ] PR with merge commit (host open-pr)

## Notes

Parent: 013. Docs only plus the NoWarn change; no behavior changes.

Review: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`. Effort 1, roster general, 2 rounds, disposition clean (M1 bug and M2 nit fixed on this task).

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implementer (ganda task work) — unsuppressed CS1591 for shipped packages, documented gaps, fixed C-18/C-28/C-29; `./bin/dev workflow` green.
- Review: grok session `01a12ee2-b471-7a10-b903-045be5854734` (2026-10-12)

## Results

- `timewarp-mediator.csproj` and `timewarp-mediator-contracts.csproj` now strip CS1591 from the inherited `NoWarn` (`$(NoWarn.Replace('CS1591', ''))`); tests, samples, analyzers, and generators keep the root suppression. With `TreatWarningsAsErrors`, any new undocumented public member fails the build.
- C-19: documented `INotificationPublisher`, `NotificationHandlerExecutor` (record params), `MediatorServiceConfiguration` class, `RequestExceptionActionProcessorStrategy` (+ members, semantics taken from `ServiceRegistrar.AddRequiredServices` ordering); `<inheritdoc/>` on `Mediator` Send/Publish/CreateStream, both publishers' `Publish`, and the four pipeline behaviors' `Handle`; constructor docs for the four behaviors. Wrappers and `ServiceRegistrar` are internal since 013-004, so nothing to document there; the repo ships four (not five) pipeline behaviors.
- C-18: "med" → "mediator" (`i-mediator.cs`, `i-sender.cs`, `i-publisher.cs`, `service-collection-extensions.cs`); rewrote the `ServiceCollectionExtensions` summary (dropped the duplicate `RequestPreProcessorBehavior` cref; scanned handlers stay `ServiceLifetime.Transient`; `IMediator`, `ISender`, and `IPublisher` use `MediatorServiceConfiguration.Lifetime`; pre- and post-processor behaviors are registered when processors are configured); fixed "controlls"/"containg"; `<code>true</code>` → `<c>true</c>`; added `<returns>` to `Unit` `==`/`!=`; filled empty param/typeparam/returns on `ISender.CreateStream` overloads and added missing typeparams on `ISender.Send<TRequest>` and `IPublisher.Publish<TNotification>`; empty typeparams on `RequestPreProcessorBehavior`; "Defined a" → "Defines a" on `IRequestPreProcessor`.
- C-28: `<remarks>` on `IIdempotent`, both `IIdempotentCommand` interfaces, and `IQuery<T>` stating idempotency is not enforced (no key, dedup store, or retry).
- C-29: `RequestHandlerDelegate` parameter `t` → `cancellationToken`, with the default-token fallback documented; `IStreamPipelineBehavior.Handle` returns text corrected; `MediatorAssembliesAttribute` documents null-as-empty (kept, no behavior change per task notes); `NoHandlerException` validates `requestType` before building the message.

### Review

- Rounds: 2. Roster: general. Effort: 1.
- Final counts: bug 1 fixed, nit 1 fixed, suggestion 0, open 0, wontfix 0.
- Disposition: clean. No escalations.
- M1: `ServiceCollectionExtensions` claimed scanned handlers used `MediatorServiceConfiguration.Lifetime`. The registrar uses `AddTransient` / `TryAddTransient`. The bullet now says `ServiceLifetime.Transient`.
- M2: `IRequestPreProcessor` summary "Defined a" is now "Defines a".
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
./bin/dev workflow
grep -n "CS1591" source/timewarp-mediator/timewarp-mediator.csproj source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj
```

Expect: `Pipeline SUCCEEDED` (all tests pass, packages pack, analyzer layout verified) and both csproj files show the `NoWarn.Replace('CS1591', '')` line. Removing any `/// <summary>` from a public member in either shipped project makes `dotnet build` fail with `error CS1591`.

Doc check:

```bash
grep -n "ServiceLifetime.Transient" source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs
grep -n "Defines a request pre-processor" source/timewarp-mediator/pipeline/i-request-pre-processor.cs
```

Expect: the handler bullet names `ServiceLifetime.Transient`, and the pre-processor summary says "Defines".

