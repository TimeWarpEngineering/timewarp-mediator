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

- [ ] Inventory references to each candidate type across source, tests, samples, generators
- [ ] Internalize wrappers and registrar (or record why not)
- [ ] Remove or obsolete `ServiceFactory`; fix stale comment and `service-factory-tests.cs` name
- [ ] Rename `serviceFactory` parameter
- [ ] Release-notes entry; `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Breaking change; the final list needs maintainer sign-off, but the direction (minimize) is decided.

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
