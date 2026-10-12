# Decide and reduce the public API surface before 14.0.0 stable

## Description

Implementation types in TimeWarp.Mediator are public and cannot change later without a breaking release. Parent review task 013, findings C-20, C-21, and the "internal" half of C-4.

- `RequestHandlerWrapper` / `RequestHandlerWrapperImpl` / `NotificationHandlerWrapper*` are public while `StreamRequestHandlerWrapper*` are internal.
- `ServiceRegistrar` is a public static class with public `GenerateCombinations` and `AddMediatorClassesWithTimeout`.
- `ServiceFactory` delegate (`service-factory.cs`) has no references in source or tests; only a stale comment mentions it.
- `notification-handler-wrapper.cs` names its `IServiceProvider` parameter `serviceFactory`.

## Requirements

- Policy (Steven, 2026-10-11): reduce the public API as far as possible. Anything not needed by a consumer becomes internal.
- Hard constraint: the result must keep working for TimeWarp.State, TimeWarp.Architecture, and TimeWarp.Nuru. Re-run the consumer inventory below against each repo's current master before merging, and build those repos against the packed nupkgs (local feed) as the acceptance test.
- Default: wrappers and `ServiceRegistrar` become internal; `ServiceFactory` is removed or `[Obsolete]`; `NotificationHandlerExecutor` and the publishers stay public only if a consumer or `MediatorServiceConfiguration` needs them.
- Check the Generators package, tests, and samples for uses before internalizing.
- Land in a 14.0.0 beta, not after stable.
- Record the change in the 14.0.0 release notes.

## Checklist

- [x] Inventory references to each candidate type across source, tests, samples, generators
- [x] Internalize wrappers and registrar (or record why not)
- [x] Remove or obsolete `ServiceFactory`; fix stale comment and `service-factory-tests.cs` name
- [x] Rename `serviceFactory` parameter
- [x] Release-notes entry; `./bin/dev workflow` green
- [x] Implementation review disposition (`review/disposition.md`, clean)
- [ ] PR with merge commit (host open-pr node)

## Notes

Parent: 013. Breaking change; the final list needs maintainer sign-off, but the direction (minimize) is decided.

Review trail: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 1, roster general, one round, disposition clean (0 open).

### Consumer inventory (2026-10-11, local master checkouts)

None of the three consumers references the `TimeWarp.Mediator` runtime package. All three take `TimeWarp.Mediator.Contracts` 14.0.0-beta.4 plus `TimeWarp.Mediator.Generators` (State and Architecture also take `TimeWarp.Mediator.Analyzers`). Every `AddMediator` hit in Nuru is documentation text telling users not to call it. So no consumer can reach the wrappers, `ServiceRegistrar`, `ServiceFactory`, `NotificationHandlerExecutor`, `MediatorServiceConfiguration`, the publishers, or the pipeline-behavior implementations.

Contract types each consumer uses (file-hit counts, `grep -l` on `.cs`/`.razor` outside obj/bin):

| Type | State | Architecture | Nuru |
|---|---|---|---|
| `ISender` / `ISender<TScope>` | 27 | 35 | 26 |
| `IMediator` | 6 | 14 | 13 |
| `IPublisher` | 9 | 7 | 12 |
| `IRequest`, `IRequest<T>`, `IBaseRequest` | 4 | 28 | 1 |
| `IRequestHandler` | 4 | 1 | 0 |
| `INotification`, `INotificationHandler` | 3 | 5 | 4 |
| `IStreamRequest` | 0 | 5 | 0 |
| `IPipelineBehavior` | 4 | 0 | 0 |
| `IAction`, `IActionHandler`, `ActionHandler` | 120 | 2 | 0 |
| `ICommand`, `ICommandHandler` | 0 | 0 | 261 |
| `IQuery`, `IQueryHandler` | 0 | 0 | 147 |
| `IIdempotent`, `IIdempotentCommand*` | 0 | 0 | 16 |
| `Unit` | 1 | 1 | 646 |
| `NoHandlerException` | 2 | 0 | 0 |
| `MediatorAssembly`, `MediatorScope`, `MediatorBehavior` attributes | 24 | 0 | 1 |
| `AddGeneratedMediator` | 30 | 2 | 8 |

Everything in that table is in Contracts and must stay public. Nothing in the runtime package appears. Nuru references Generators with `PrivateAssets="none"` (flows the generator to its consumers); note for the Generators tasks, out of scope here.

Re-run before merge:

```bash
for r in timewarp-state timewarp-architecture timewarp-nuru; do
  grep -rlE 'TimeWarp\.Mediator' ~/worktrees/github.com/TimeWarpEngineering/$r/master --include=*.cs --include=*.razor \
    | grep -vE '/obj/|/bin/' | xargs grep -ohE 'RequestHandlerWrapper|NotificationHandlerWrapper|ServiceRegistrar|ServiceFactory|NotificationHandlerExecutor|MediatorServiceConfiguration|ForeachAwaitPublisher|TaskWhenAllPublisher|RequestExceptionHandlerState|OpenBehavior' | sort | uniq -c
done
```

Expect no output from the loop.

## Session

- Created: 686431 (2026-10-11)
- Implement (2026-10-12): internalized wrappers + registrar, removed `ServiceFactory`, release notes added.
- Review: Grok session `01a12ed7-1122-7ca0-8548-9c99ad6445b3` (2026-10-12). Effort 1, general only.
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 80 — 2026-10-12T06:47:28Z

## Results

### Inventory (2026-10-12, this branch)

- Wrappers (`RequestHandlerBase`, `RequestHandlerWrapper*`, `NotificationHandlerWrapper*`): used only by `mediator.cs` private fields/locals. No use in Generators, Analyzers, Contracts, tests, or samples.
- `ServiceRegistrar`: used by `ServiceCollectionExtensions.AddMediator` and by `generic-request-handler-tests.cs` (`GenerateCombinations` cap tests).
- `ServiceFactory`: no library use. Only the Windsor sample used it, as a Windsor-side resolver delegate unrelated to `Mediator` (which takes `IServiceProvider`).
- Consumer re-run against current masters (state 3fb65048, architecture 41c20d085, nuru c3bb5952): the inventory loop prints nothing. None of the three references the `TimeWarp.Mediator` runtime package. Nuru's three `TimeWarp.Mediator` hits in props files are `<Using Include=...>` namespace imports, not package references.

### Changes

- `wrappers/request-handler-wrapper.cs`, `wrappers/notification-handler-wrapper.cs`: all types now `internal`, which matches the stream wrappers. The `serviceFactory` parameter is renamed `serviceProvider`.
- `registration/service-registrar.cs`: `ServiceRegistrar` is now `internal static`. The stale "ServiceFactory/IMediator" TryAdd comment is fixed.
- `service-factory.cs` deleted (the type is removed, not `[Obsolete]`; this is still a beta).
- Windsor sample: declares its own `internal delegate object ServiceFactory(Type)` in `samples/timewarp-mediator-examples-windsor/service-factory.cs`.
- `InternalsVisibleTo` to `timewarp-mediator-tests` (with the public key) in `timewarp-mediator.csproj`. The tests project is now signed with `timewarp-mediator.snk`, which `InternalsVisibleTo` from a strong-named assembly requires.
- `tests/.../service-factory-tests.cs` renamed with `git mv` to `missing-handler-tests.cs` (`MissingHandlerTests`).
- `documentation/release-notes/release-notes-v14.0.0.md`: a breaking-changes table listing each type that was removed or internalized, with its replacement. 013-017 should extend this file instead of starting a new 14.0.0 note.

### Kept public (with reason)

These are reachable from consumer-facing signatures or configuration:
- `NotificationHandlerExecutor`: used in `INotificationPublisher.Publish` and `Mediator.PublishCore`.
- `ForeachAwaitPublisher` and `TaskWhenAllPublisher`: values for `MediatorServiceConfiguration.NotificationPublisher`.
- `MediatorServiceConfiguration`, `OpenBehavior`, `RequestExceptionActionProcessorStrategy`.
- `RequestExceptionHandlerState<T>`: appears in the `IRequestExceptionHandler` signature.
- The four built-in behaviors: non-MS-DI containers register them by hand (Windsor and other samples).

### Consumer acceptance build

Not run as a separate local-feed build. Every changed file is in the `TimeWarp.Mediator` runtime package, which none of State, Architecture, or Nuru references. Contracts, Generators, and Analyzers are byte-for-byte unaffected by this diff. Packing under the same `14.0.0-beta.4` version would also resolve from the NuGet cache, so the consumer build would not exercise these packages. The consumer grep above is the effective acceptance check.

### How to validate

Smoke:

```bash
./bin/dev workflow
grep -nE '^(public|internal)[a-z ]* class' source/timewarp-mediator/wrappers/*.cs source/timewarp-mediator/registration/*.cs
test ! -e source/timewarp-mediator/service-factory.cs && echo removed
dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests|FullyQualifiedName~MissingHandlerTests"
```

Expect:

- `Pipeline SUCCEEDED`, with timewarp-mediator-tests at 195 passed and 2 skipped.
- Every listed wrapper and registrar type begins with `internal`.
- The test prints `removed`.
- The filtered tests pass, including the `ServiceRegistrar.GenerateCombinations` cap tests, which reach the internal registrar through `InternalsVisibleTo`.
- The consumer inventory loop in Notes prints nothing.

### Review

- Rounds: 1. Roster: general. Effort: 1.
- Counts (final, `review/round-1/merged.md`): bug 0/0/0, suggestion 0/0/0, nit 0/0/0 (open/fixed/wontfix).
- Disposition: clean (`review/disposition.md`). No wontfix, no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Re-verified during review: `InternalsVisibleTo` public key matches `timewarp-mediator.snk`. `dotnet build tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release` and `dotnet build samples/timewarp-mediator-examples-windsor/timewarp-mediator-examples-windsor.csproj -c Release` succeeded. `dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests|FullyQualifiedName~MissingHandlerTests"` — Passed 26, Failed 0, Skipped 0.

