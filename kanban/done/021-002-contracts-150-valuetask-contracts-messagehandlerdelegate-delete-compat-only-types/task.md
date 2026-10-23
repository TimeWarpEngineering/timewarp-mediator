# Contracts 15.0: ValueTask contracts, MessageHandlerDelegate, delete compat-only types

## Description

The contracts package is still MediatR-shaped: `Task`-returning members everywhere, a
`RequestHandlerDelegate<TResponse>` that forces a closure per behavior, stream types with no
consumer, and seven handler-kind interfaces that only exist so Nuru can string-match them.
Parent review task 021, findings F3, F4, F5, F6, F8, F10, F18; decisions D2, D3, D6 signed off
2026-10-23.

## Requirements

- Every member on `IRequestHandler<,>`, `IRequestHandler<>`, `INotificationHandler<>`,
  `IPipelineBehavior<,>`, `ISender`, `ISender<TScope>`, `IPublisher`, `IPublisher<TScope>` and
  `IMediator` returns `ValueTask` or `ValueTask<T>`.
- Replace `RequestHandlerDelegate<TResponse>` with
  `MessageHandlerDelegate<TRequest, TResponse>(TRequest request, CancellationToken ct)`;
  behaviors call `next(request, ct)`.
- Delete: `IBaseRequest`, `ICommandHandler<>`/`<,>`, `IQueryHandler<,>`, `IActionHandler<>`,
  `IIdempotentCommandHandler<>`/`<,>`, `NotificationHandler<T>`, `IStreamRequest<>`,
  `IStreamRequestHandler<,>`, `IStreamPipelineBehavior<,>`, `StreamHandlerDelegate<>`, both
  `ISender.CreateStream` overloads, `MediatorAssembliesAttribute`, `MediatorModuleAttribute`.
- Keep the message markers `IAction`, `ICommand(<T>)`, `IQuery<T>`, `IIdempotentCommand(<T>)`,
  `IIdempotent` as pure markers over `IRequest(<T>)`. Keep `ActionHandler<TAction>` as the base
  with a `ValueTask` shape and no `Task` shim.
- `Unit`: keep `Unit.Value`, add `Unit.ValueTask`, delete `Unit.Task` and the `IComparable`
  surface; the type must not hide `System.Threading.Tasks.Task` under `using static`.
- Keep `Send(object)` and `Publish(object)` on `ISender` / `IPublisher` (D6).
- Add `[assembly: GeneratedMediator(typeof(Mediator))]` attribute type in the contracts so
  consumers (Nuru) detect the generated mediator without reading build properties.
- Keep the `// Modified by Steven T. Cramer` notice on every surviving MediatR-derived file;
  do not add it to new files. Package license expression stays `Unlicense AND Apache-2.0`.
- Update the generator and its golden files so the emitted code compiles against the new
  contracts; the dispatcher redesign itself is 021-003. Update analyzers and their tests.
- Delete `migration.md` sections and XML docs that describe the removed members.

## Goal (implementer restatement)

Make the contracts package `ValueTask`-native and delete the MediatR-compat types that 15.0 does not dispatch. The generator must emit code that compiles against that surface. The non-boxing dispatcher, cached delegate chain, and manifest redesign stay on 021-003.

Round-1 review still requires two product fixes. A behavior must be able to call `next` with a different request instance, and that instance must be the one the handler runs, on the generated dispatcher and on the reflection dispatcher. The generated case includes a two-behavior chain so the nested delegate is on the path. `Unit.ValueTask` documentation must say that `using static TimeWarp.Mediator.Unit` does not hide `System.Threading.Tasks.Task`, does shadow `System.Threading.Tasks.ValueTask`, and that callers should use the qualified names `Unit.Value` and `Unit.ValueTask`. The same sentence belongs in 021-007's notes for the migration page.

Acceptance criteria:

- `IRequestHandler<,>`, `IRequestHandler<>`, `INotificationHandler<>`, `IPipelineBehavior<,>`, `ISender`, `ISender<TScope>`, `IPublisher`, `IPublisher<TScope>`, and `IMediator` expose only `ValueTask` or `ValueTask<T>` members.
- `RequestHandlerDelegate<TResponse>` is replaced by `MessageHandlerDelegate<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)`. Behaviors call `next(request, cancellationToken)`.
- Deleted: `IBaseRequest`, both `ICommandHandler` arities, `IQueryHandler<,>`, `IActionHandler<>`, both `IIdempotentCommandHandler` arities, `NotificationHandler<T>`, `IStreamRequest<>`, `IStreamRequestHandler<,>`, `IStreamPipelineBehavior<,>`, `StreamHandlerDelegate<>`, both `ISender.CreateStream` overloads, `MediatorAssembliesAttribute`, and `MediatorModuleAttribute`.
- Message markers stay: `IAction`, `ICommand`, `ICommand<T>`, `IQuery<T>`, `IIdempotentCommand`, `IIdempotentCommand<T>`, and `IIdempotent`, as markers over `IRequest` or `IRequest<T>`. `ActionHandler<TAction>` stays, returns `ValueTask`, and has no `Task` shim.
- `Unit` keeps `Unit.Value`, adds a cached `Unit.ValueTask`, drops `Unit.Task` and `IComparable`, and does not hide `System.Threading.Tasks.Task` under `using static`.
- `ISender.Send(object)` and `IPublisher.Publish(object)` stay.
- Contracts define `GeneratedMediatorAttribute` for `[assembly: GeneratedMediator(typeof(Mediator))]`. Emission of that attribute stays on 021-003.
- Surviving MediatR-derived files keep the Steven T. Cramer notice. New files do not get it. The package license expression stays `Unlicense AND Apache-2.0`.
- Generator emission, analyzers, golden/dispatch tests, and in-repo handlers compile and pass against the new contracts. `migration.md` and XML docs no longer describe the removed members.
- `./bin/dev workflow` is green.
- A generated `[assembly: MediatorBehavior]` behavior can call `next(replacement, cancellationToken)` and the handler receives `replacement`. A second behavior inside that one receives `replacement` too, so the nested delegate is covered.
- A reflection behavior registered with `AddBehavior` in `pipeline-tests.cs` can do the same, including through a two-behavior chain.
- `Unit.ValueTask` XML docs describe the `using static` shadow, and 021-007 notes tell the migration page to say the same.

## Checklist

- [x] ValueTask on every contract member
- [x] `MessageHandlerDelegate<TRequest, TResponse>` replaces `RequestHandlerDelegate<T>`
- [x] Compat-only interfaces, streams, `MediatorAssemblies`, `MediatorModule` deleted
- [x] `Unit` cleanup
- [x] `[GeneratedMediator]` attribute added
- [x] Generator, analyzers, golden files and tests updated and green
- [x] Apache notices preserved on derived files
- [x] `./bin/dev workflow` green
- [x] M1: replacement request reaches the handler on both dispatchers
- [x] M2: `Unit.ValueTask` comment and 021-007 migration note
- [ ] PR with merge commit

## Review

- Round 1: open M1, M2 implemented in the fix pass. M3 stays wontfix on 021-007.
- Round 2: Done
## Notes

Parent: 021. Depends on: 021-001 (baseline recorded before contracts change). Blocks 021-003.
Review sections 3 (F3-F6, F8, F10, F18), 4 (contracts table), 5, 7 (D2, D3, D6).
Consumer impact is in review section 6: State's seven behaviors, Architecture's 62 handlers
and Nuru's endpoint handlers all change signature; those are consumer-repo tasks after
`15.0.0-beta.1`.

## Results

Contract members on `IRequestHandler<,>`, `IRequestHandler<>`, `INotificationHandler<>`, `IPipelineBehavior<,>`, `ISender`, `ISender<TScope>`, `IPublisher`, `IPublisher<TScope>`, and `IMediator` return `ValueTask` or `ValueTask<T>`. `RequestHandlerDelegate<TResponse>` is `MessageHandlerDelegate<TRequest, TResponse>`; behaviors call `next(request, cancellationToken)`. Compat-only handler interfaces, stream types, both `CreateStream` overloads, `MediatorAssembliesAttribute`, and `MediatorModuleAttribute` are deleted. Message markers and `ActionHandler<TAction>` stay. `Unit` keeps `Unit.Value`, adds a cached `Unit.ValueTask`, and drops `Unit.Task` and `IComparable`. Contracts define `GeneratedMediatorAttribute`; the generator does not emit it.

The reflection runtime, samples, analyzers, and generator compile against that surface. Project references register `TimeWarp.Mediator.Analyzers.dll` beside the generator so Roslyn can load `MediatorGenerator`. Pre-processors, post-processors, exception handlers and actions, and `INotificationPublisher` stay `Task`. Package version stays `14.0.0`. Root `readme.md`, `agent.md`, and `skills/tw-mediator/SKILL.md` still describe the removed APIs; 021-007 owns that rewrite. `migration.md` no longer describes them.

`in TRequest` is gone from `IPipelineBehavior` because `MessageHandlerDelegate` is invariant. There is no implicit conversion from `Task` to `ValueTask`, so handlers return `ValueTask.FromResult`, `default`, or `new ValueTask(...)`, and call sites that still need a `Task` use `.AsTask()`.

### How to validate

**Smoke**

```bash
./bin/dev workflow
ganda repo audit
```

**Expect**

- `./bin/dev workflow` runs `clean -> build -> test -> verify-samples -> pack`, exits 0, and prints `Pipeline SUCCEEDED`.
- Tests pass with failed 0: generators 42, analyzers 42, roslyn-floor 83, compilation 82, mediator 183.
- `Samples verified successfully!` for the 11 executable samples.
- Pack produces `TimeWarp.Mediator.14.0.0.nupkg`, `TimeWarp.Mediator.Contracts.14.0.0.nupkg`, `TimeWarp.Mediator.Analyzers.14.0.0.nupkg`, and `TimeWarp.Mediator.Generators.14.0.0.nupkg`. Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.
- `ganda repo audit` exits 0.

**Not in scope:** emitting `[assembly: GeneratedMediator(typeof(Mediator))]`, the non-boxing dispatcher, and deleting the reflection runtime (021-003 and 021-004). State, Architecture, and Nuru signature updates wait on `15.0.0-beta.1`.

The fix pass covers round-1 M1 and M2. Generated `ReplacementPipeline` has one behavior that calls `next` with a different request, and a two-behavior chain whose inner behavior and handler both receive that instance. Reflection tests in `pipeline-tests.cs` register the same shape with `AddBehavior`. `Unit.ValueTask` docs say `using static TimeWarp.Mediator.Unit` does not hide `Task`, does shadow `ValueTask`, and that callers should use `Unit.Value` and `Unit.ValueTask`. The same sentence is in 021-007's notes. M3 stays deferred to 021-007.

### Review

- Round 1 (2026-10-23): one general reviewer at effort 3. `./bin/dev workflow` was re-run and printed `Pipeline SUCCEEDED`.
- Counts at merge: bug 0. Suggestion: 1 open (M1). Nit: 1 open (M2), 1 wontfix (M3, deferred to 021-007).
- Fix pass (20a559e) implemented M1 and M2.
- Round 2 (2026-10-23): one general reviewer at effort 2 on the fix delta `03fa275..20a559e`. The four new replacement-request tests were run and pass. M1 and M2 are fixed, and the delta raised no new findings.
- Final counts: bug 0. Suggestion: 1 fixed (M1). Nit: 1 fixed (M2), 1 wontfix (M3, deferred to 021-007, which owns the docs rewrite).
- Disposition: **accepted-exceptions**. 0 open after 2 rounds.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: grok 01a16902-d58d-7201-b302-322c8e278bd4 (2026-10-23)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-23T14:28:49Z
- Review oracle round 1: general reviewer (claude-opus-5-5), effort 3, session id not exposed. Result: fix pass needed for M1, M2 (2026-10-23)
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-23T14:35:37Z
- Implement-fix-2: implemented by implementer-grok (grok, model grok-4.7), session 01a16930-56db-70c3-8fa2-210ae4899e92 (2026-10-23). M1 and M2 addressed. `./bin/dev workflow` printed `Pipeline SUCCEEDED`. `ganda repo audit` passed.
- Review oracle round 2: general reviewer (claude-opus-5-5), effort 2, session id not exposed. Result: M1 and M2 fixed, disposition accepted-exceptions (2026-10-23)
