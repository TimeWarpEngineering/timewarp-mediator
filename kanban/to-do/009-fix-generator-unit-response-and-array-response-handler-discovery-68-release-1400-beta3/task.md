# Fix generator Unit-response and array-response handler discovery (#68), release 14.0.0-beta.3

## Description

Found by TimeWarp.Nuru task 443-002 while moving Nuru onto TimeWarp.Mediator 14.0.0-beta.2. The Mediator
generator runs in every Nuru host compilation, so every Nuru handler joins the message graph, and two
`MessageGraphBuilder` bugs break the build. Full write-up and a patch: GitHub issue
**TimeWarpEngineering/timewarp-mediator#68** (read it first: `gh issue view 68 --repo TimeWarpEngineering/timewarp-mediator`).

1. **`IRequestHandler<TReq, Unit>` emitted as the void `IRequestHandler<TReq>`** — `isUnit` is derived from
   `responseType == Unit`, so an explicit `ICommand<Unit>` + `IRequestHandler<TCmd, Unit>` is registered and
   dispatched as void and the generated code fails with **CS0311**. Fix: derive void-ness from which handler
   interface matched (`IRequestHandler<T>` vs `IRequestHandler<T,R>`), not from the response type.
2. **Non-`INamedTypeSymbol` responses skipped** — `TryGetRequestHandler` requires
   `TypeArguments[1] is INamedTypeSymbol`, so `IQuery<SearchResult[]>` handlers are never discovered
   (**TWM001**). Fix: widen to `ITypeSymbol` through `RequestBinding.ResponseType`,
   `CloseBehaviors`/`TryCloseBehavior`/`ImplementsPipeline`, and `MediatorEmitter.Fq`.

The #68 patch was validated locally as 14.0.0-beta.3-local against Nuru (Release build 0 warnings,
1731 CI tests, 64/64 samples).

## Requirements

- Apply the #68 fixes in `source/timewarp-mediator-analyzers/` (and the emitter). Review the patch; do not
  paste blindly.
- Generator tests: `ICommand<Unit>` with `IRequestHandler<TCmd, Unit>` compiles and dispatches as a
  response-returning request; `ICommand` with `IRequestHandler<TCmd>` stays void; `IQuery<T[]>` and
  `IQuery<List<T>>` handlers are discovered and dispatch; a pipeline behavior closes over an array response.
- Bump `<Version>` to `14.0.0-beta.3` in both Directory.Build.props files; changelog entry referencing #68.
- Close #68 from the PR (`Fixes #68` in the commit or PR body).
- Release after merge + green master CI is done by the cockpit (`dev release`; public repo, NuGet.org).

## Checklist

- [ ] Unit-response handler fix + tests
- [ ] Array/non-named response fix + tests
- [ ] Version 14.0.0-beta.3, changelog
- [ ] Released (cockpit, after merge)

## Notes

- Consumer: TimeWarp.Nuru 443-002 on `feature/443-mediator` (commit 20c368fd) waits on this release, then bumps its pin.
- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground.
