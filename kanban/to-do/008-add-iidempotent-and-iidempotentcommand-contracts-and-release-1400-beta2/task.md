# Add IIdempotent and IIdempotentCommand contracts and release 14.0.0-beta.2

## Description

TimeWarp.Mediator 14.0.0-beta.1 ships `IRequest`, `IQuery<T>`, `ICommand`, `ICommand<T>`, their handlers,
and `Unit`, but no idempotency contracts. The design note
`analysis/2026-06-17-source-gen-aot-rewrite.md` planned them ("The Nuru extraction plan: `ICommand<T>`,
`IQuery<T>`, `IIdempotentCommand<T>`" and "CQRS/idempotency semantics"); the 005 subtasks that shipped
beta.1 did not pick them up. Verified 2026-09-25 against master, tag v14.0.0-beta.1, and the published
TimeWarp.Mediator.Contracts assembly: no `IIdempotent`, no `IIdempotentCommand`.

TimeWarp.Nuru epic 443 replaces Nuru's local message types with Mediator's. Nuru today has:

- `public interface IIdempotent : IMessage;` — marker: safe to retry
- `public interface IQuery<TResult> : IIdempotent;` — queries are read-only and idempotent
- `public interface IIdempotentCommand<TResult> : IIdempotent;` — mutates state, safe to retry
- `IIdempotentCommandHandler<TCommand, TResult> where TCommand : IIdempotentCommand<TResult>`

Nuru's help and capabilities output classify routes as Query / Command / IdempotentCommand by the
interface's short name, so Mediator must provide `IIdempotentCommand` for Nuru to keep that category.

## Requirements

1. Add to `source/timewarp-mediator-contracts/` (namespace `TimeWarp.Mediator`, kebab-case file names):
   - `IIdempotent` — marker interface, XML doc: safe to retry / repeat without additional side effects.
   - `IIdempotentCommand<out TResponse> : ICommand<TResponse>, IIdempotent` (and a void
     `IIdempotentCommand : ICommand, IIdempotent` for symmetry with `ICommand`).
   - `IIdempotentCommandHandler<in TCommand, TResponse> : ICommandHandler<TCommand, TResponse>`
     `where TCommand : IIdempotentCommand<TResponse>`, plus the void handler.
2. Make `IQuery<out TResponse>` also implement `IIdempotent` (queries are idempotent by definition).
   Additive; confirm no existing consumer breaks.
3. The source generator dispatches idempotent commands exactly like commands (they are `IRequest`s).
   Add generator and runtime tests: an `IIdempotentCommand<T>` handler is registered by
   `AddGeneratedMediator()` and `ISender.Send` reaches it; same for the void form.
4. Out of scope: the design note's idempotency-key enforcement and dedup store. Types only.
5. Bump `<Version>` to `14.0.0-beta.2` (both Directory.Build.props files that carry it today) and update
   the changelog/docs that list the contracts.
6. Release: after merge and green master CI, the cockpit runs `dev release` (tw-release). TimeWarp.Mediator
   is a **public** repo and already publishes to NuGet.org; confirm the workflow still does before release.

## Checklist

- [ ] IIdempotent, IIdempotentCommand (void + generic), handlers
- [ ] IQuery implements IIdempotent
- [ ] Generator + runtime dispatch tests
- [ ] Version 14.0.0-beta.2, docs
- [ ] Released to NuGet.org (cockpit, after merge)

## Notes

- Consumer: TimeWarp.Nuru 443 / 443-001 / 443-002 will target 14.0.0-beta.2.
- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground. You are one-shot and never receive background notifications.
