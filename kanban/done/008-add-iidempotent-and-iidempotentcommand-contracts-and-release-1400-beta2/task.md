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

- [x] IIdempotent, IIdempotentCommand (void + generic), handlers
- [x] IQuery implements IIdempotent
- [x] Generator + runtime dispatch tests
- [x] Version 14.0.0-beta.2, docs
- [ ] Released to NuGet.org (cockpit, after merge)

## Notes

- Consumer: TimeWarp.Nuru 443 / 443-001 / 443-002 will target 14.0.0-beta.2.
- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground. You are one-shot and never receive background notifications.
- Review (2026-09-25): implementation review under `review/`, effort 1, reviewer `general`, 1 round,
  disposition `accepted-exceptions` (see Results).

## Results

### Contracts (`source/timewarp-mediator-contracts/`)

- `i-idempotent.cs`: `IIdempotent` marker (safe to retry, no additional side effects).
- `i-idempotent-command.cs`: `IIdempotentCommand : ICommand, IIdempotent`,
  `IIdempotentCommand<out TResponse> : ICommand<TResponse>, IIdempotent`,
  `IIdempotentCommandHandler<in TCommand>` and `IIdempotentCommandHandler<in TCommand, TResponse>`
  (constrained to the idempotent command types, deriving from `ICommandHandler`).
- `i-query.cs`: `IQuery<out TResponse> : IRequest<TResponse>, IIdempotent` (additive; full test suite
  and samples unchanged and green).
- `source/timewarp-mediator/type-forwardings.cs`: forwards the five new types like the other contracts.
- No generator or analyzer change was needed: both discover handlers through `IRequestHandler<>` /
  `IRequestHandler<,>` in `AllInterfaces`, so idempotent commands dispatch exactly like commands.
  Idempotency-key enforcement and a dedup store remain out of scope.

### Tests

- `tests/timewarp-mediator-generators-tests/idempotent-command-tests.cs` (+ `idempotency/idempotent-messages.cs`):
  `AddGeneratedMediator()` registers `IRequestHandler<UpsertSetting, string>` and
  `IRequestHandler<ResetSetting>`; `ISender.Send` reaches the generic and void idempotent handlers;
  `Send(object)` switch covers both; `IQuery` still dispatches; manifest records both commands.
- `tests/timewarp-mediator-generators-tests/pipeline-log-collection.cs`: xunit collection shared with
  `IncrementActionSetTests`, because unscoped sends run the global tracking behaviors that write to the
  static `PipelineLog`.
- `tests/timewarp-mediator-tests/send-idempotent-command-tests.cs`: reflection `Mediator` resolves
  generic and void idempotent command handlers.

### Version and docs

- `<Version>` `14.0.0-beta.2` in `Directory.Build.props` and `source/Directory.Build.props`.
- `readme.md` (install snippets, contracts list), `documentation/generated-vs-legacy.md`
  ("Changes in 14.0.0-beta.2"), `documentation/m1-generated-mediator.md`, `documentation/m2-named-pipelines.md`.
- Release workflow: `.github/workflows/workflow.yml` still pushes to NuGet.org via `nuget/login`
  trusted publishing (`workflow --mode release`). Release itself is the cockpit's `dev release` after merge.

### Gate fixes (pre-existing, needed for a green build and audit)

- `Microsoft.SourceLink.GitHub` 8.0.0 -> 10.0.401: 8.0.0 pulls `Microsoft.Build.Tasks.Git` 8.0.0
  (GHSA-23fw-v26w-5fgq, NU1902), fatal under `TreatWarningsAsErrors`.
- `tests/timewarp-mediator-tests` gets `RollForward=LatestMajor` like the other test projects
  (testhost aborted without a .NET 8 runtime).
- `ganda repo audit --fix`: `kanban/in-progress/.gitkeep`, TimeWarp.SourceGenerators pin + reference +
  `.editorconfig` TW0007 entry, refreshed memsearch git hooks, peacock colors. The analyzer, generator,
  and analyzer-test projects `Remove` TimeWarp.SourceGenerators because it depends on Roslyn 4.11 and
  they pin Roslyn 4.8 (NU1605/NU1107).

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- workflow
dotnet test tests/timewarp-mediator-generators-tests -c Release --no-build --filter "FullyQualifiedName~IdempotentCommandTests"
dotnet test tests/timewarp-mediator-tests -c Release --no-build --filter "FullyQualifiedName~SendIdempotentCommandTests"
ganda repo audit
```

Expect:

- Workflow prints `Pipeline SUCCEEDED`; generators tests 26 passed, analyzers tests 6 passed,
  mediator tests 165 passed / 2 skipped; `artifacts/packages/*.14.0.0-beta.2.nupkg` for all four packages.
- `IdempotentCommandTests`: 7 passed. `SendIdempotentCommandTests`: 2 passed.
- `ganda repo audit`: "Repository passes all audit checks."

### Review disposition

- **Outcome:** `accepted-exceptions`, 0 open. 1 round, effort 1, roster `general`
  (ganda task-work review oracle, cursor).
- **Final counts:** bug 0; suggestion 0; nit 1 wontfix (0 open, 0 fixed).
- **M1 (nit, wontfix):** SourceLink 10.0.401 pulls in `System.IO.Hashing` 10.0.12 as a build-time dependency,
  which warns "doesn't support net6.0" when packing `source/timewarp-mediator`. The warning is not fatal,
  and the published nuspec dependency groups are unchanged. The bump is required for NU1902, and dropping
  `net6.0` is out of scope.
- **Reviewer re-verification:** `./bin/dev workflow` Pipeline SUCCEEDED (26 / 6 / 165 passed, 2 skipped;
  four beta.2 nupkgs). `ganda repo audit` passes. The generator and analyzer do not classify messages by
  interface name.
- **Artifacts:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`,
  `review/disposition.md`.
