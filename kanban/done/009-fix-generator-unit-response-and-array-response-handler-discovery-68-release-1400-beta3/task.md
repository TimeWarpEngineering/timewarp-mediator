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

- [x] Unit-response handler fix + tests
- [x] Array/non-named response fix + tests
- [x] Version 14.0.0-beta.3, changelog
- [ ] Released (cockpit, after merge)

## Notes

- Consumer: TimeWarp.Nuru 443-002 on `feature/443-mediator` (commit 20c368fd) waits on this release, then bumps its pin.
- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground.

## Results

Applied the #68 patch after review; it matched the current `MessageGraphBuilder` shape exactly.

- `message-graph-builder.cs`: `TryGetRequestHandler` now reports `isVoid` only when
  `IRequestHandler<T>` matched; bindings take `IsUnitResponse` from a `voidRequests` set instead of
  `responseType == Unit`. Response type is `ITypeSymbol` through discovery, `CloseBehaviors`,
  `TryCloseBehavior`, and `ImplementsPipeline`. Design region notes both constraints.
- `message-graph.cs`: `RequestBinding.ResponseType` is `ITypeSymbol`.
- `mediator-emitter.cs`: `Fq(ITypeSymbol)`. `manifest-emitter.cs` needed no change.
- Reviewed but unchanged: `request-handler-analyzer.cs` only inspects request type arguments
  (`TypeArguments[0]`), so it has neither bug.
- New generator tests (`tests/timewarp-mediator-generators-tests/response-shape-tests.cs`, messages in
  `responses/response-shape-messages.cs`, closed `SearchWidgetsAuditBehavior` registered at order 5):
  explicit `ICommand<Unit>` + `IRequestHandler<T, Unit>` registers the two-arity interface and dispatches
  via monomorphic `Send`, `ISender.Send<T>`, and `Send(object)`; `ICommand` + `IRequestHandler<T>` stays
  void; `IQuery<string[]>` and `IQuery<List<string>>` are discovered and dispatch; generic tracking
  behaviors and the closed `IPipelineBehavior<SearchWidgets, string[]>` wrap the array query in order;
  `UnitOnlyBehavior<>` still closes over the explicit-Unit command; manifest records the response shapes.
- Negative check: with `source/` reverted, the test project fails to build with exactly the #68 errors
  (CS0311 on `CreateWidget`, TWM001 on `SearchWidgets`).
- `<Version>` 14.0.0-beta.3 in `Directory.Build.props` and `source/Directory.Build.props`; readme and
  documentation version references bumped; changelog section "Changes in 14.0.0-beta.3" in
  `documentation/generated-vs-legacy.md` and a bullet in `documentation/m1-generated-mediator.md`, both
  linking #68. Commit body carries `Fixes #68`.
- Gates: `./bin/dev workflow` succeeded (generators 34/34, analyzers 6/6, runtime 165 passed / 2 skipped
  pre-existing; beta.3 nupkgs packed and analyzer/generator layout verified). `ganda repo audit`: 28 passed,
  0 failed. The `System.IO.Hashing` net6.0 NuGet warning is pre-existing and not an error.
- Not done here (by design): release to NuGet.org after merge + green master CI (cockpit, `dev release`).

### Implementation review

- Rounds: 1. Effort 1, roster: `general` (cursor implementer-cursor review oracle, ganda task work, headless).
- Final counts: bug 0, suggestion 0, nit 1 (0 open, 0 fixed, 1 wontfix).
- Disposition: **accepted-exceptions**. M1 (nit): `TypeArguments[1] is ITypeSymbol response2` is always true.
  Kept because it has no behavioral effect and mirrors the `request2` binding and the validated #68 patch.
- Review re-ran the gates: `./bin/dev workflow` shows `Pipeline SUCCEEDED` (generators 34/34, analyzers 6/6,
  runtime 165 passed / 2 skipped), and `ganda repo audit` passes all checks.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`,
  `review/disposition.md`.

### How to validate

Smoke:

```bash
./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-tests -c Release --no-build --filter "FullyQualifiedName~ResponseShapeTests"
ganda repo audit
```

Expect:

- Workflow ends with `Pipeline SUCCEEDED`; packages are `*.14.0.0-beta.3.nupkg`.
- `ResponseShapeTests`: 8 passed, 0 failed (generator project 34/34 total).
- `ganda repo audit`: `Failed: 0`.
- Reverting the `source/` changes (`git stash push -- source/`) makes the generator test
  project fail to build with CS0311 (`CreateWidget`) and TWM001 (`SearchWidgets`).
