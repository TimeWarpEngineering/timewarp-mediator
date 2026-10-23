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

## Checklist

- [ ] ValueTask on every contract member
- [ ] `MessageHandlerDelegate<TRequest, TResponse>` replaces `RequestHandlerDelegate<T>`
- [ ] Compat-only interfaces, streams, `MediatorAssemblies`, `MediatorModule` deleted
- [ ] `Unit` cleanup
- [ ] `[GeneratedMediator]` attribute added
- [ ] Generator, analyzers, golden files and tests updated and green
- [ ] Apache notices preserved on derived files
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-001 (baseline recorded before contracts change). Blocks 021-003.
Review sections 3 (F3-F6, F8, F10, F18), 4 (contracts table), 5, 7 (D2, D3, D6).
Consumer impact is in review section 6: State's seven behaviors, Architecture's 62 handlers
and Nuru's endpoint handlers all change signature; those are consumer-repo tasks after
`15.0.0-beta.1`.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
