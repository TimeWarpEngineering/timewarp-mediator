# Round 1 — general
**Date:** 2026-10-23
**Scope reviewed:** `master...6d7892e`. Contracts, reflection runtime, generator emitter and csproj, analyzers, tests, samples, `migration.md`.

## Summary

The change does what the task asks. Every member on the nine named contract interfaces returns
`ValueTask` or `ValueTask<T>`. `MessageHandlerDelegate<TRequest, TResponse>` replaces
`RequestHandlerDelegate<T>`. The listed compat-only types and both `CreateStream` overloads are
deleted. `Unit` and `ActionHandler<TAction>` have the required shape, and
`GeneratedMediatorAttribute` exists. The Apache notice is still the first line of every
modified MediatR-derived file, and neither new file carries it.

`./bin/dev workflow` was re-run for this review and printed `Pipeline SUCCEEDED`. Test counts were
generators 40, analyzers 42, compilation 82, roslyn-floor 83 and mediator 181, all with 0 failed.
Samples verified and the package smoke passed.

Overall risk is low. The main gap is test coverage for the one new capability of the delegate:
a behavior can now hand the next step a different request. The goal restatement matches the
task, so there is no misread.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-mediator-generators/mediator-emitter.cs:511
- Description: The point of `MessageHandlerDelegate<TRequest, TResponse>` is that the request
  travels through `next(request, cancellationToken)`. A behavior can then pass a replacement
  request, and the call needs no closure. `EmitNextDelegate` now forwards the lambda's `message`
  parameter to `b{n}.Handle` and `handler.Handle`. `RequestHandlerWrapperImpl` in
  `source/timewarp-mediator/wrappers/request-handler-wrapper.cs:796-806` does the same.
  No test anywhere calls `next` with a request other than the one the behavior received.
  `git grep "next(" -- tests` shows only `next(request, cancellationToken)` and one
  `next(request, cts.Token)`. If either dispatcher regressed to pass the captured outer `request`
  (which is what the old emitter did), every test would still pass.
- Suggestion: Add one test per dispatcher. Generated: a `[assembly: MediatorBehavior]` behavior in
  `tests/timewarp-mediator-generators-tests`. Reflection: an `AddBehavior` behavior in
  `tests/timewarp-mediator-tests/pipeline-tests.cs`. The behavior calls `next` with a new request
  instance and the handler asserts it received that instance. Include a two-behavior chain on the
  generated side so the nested `message` lambda emission is covered.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-mediator-contracts/unit.cs:18-21
- Description: The XML comment says `ValueTask` is "Named so `using static TimeWarp.Mediator.Unit`
  does not hide `System.Threading.Tasks.Task`". That is true, but the same `using static` now
  imports a static member named `ValueTask`. Parent research
  (`021-deep…/research/timewarp-nuru-usage.md:68,111`) records the problem Nuru hit:
  `using static Unit` hid `Task`. By the same name-lookup rule, the new member collides with
  `System.Threading.Tasks.ValueTask`, which is now the return type of every handler. The task
  requires the name `Unit.ValueTask`, so renaming it is an operator call and outside this
  finding. The comment, however, implies the `using static` problem is solved.
- Suggestion: Reword the comment to say what is true: it no longer hides `Task`, and a file that
  uses `using static TimeWarp.Mediator.Unit` should write `Unit.ValueTask`/`Unit.Value`
  qualified, or drop the `using static`. Leave a note for 021-007 so the 15.0 migration page
  says the same.
- Status: open

### Issue 3 — Severity: nit
- File: documentation/diagnostics.md:88-90
- Description: The TWM009 section still lists `IStreamRequestHandler` and
  `IStreamPipelineBehavior` and says "`CreateStream` throws `NotSupportedException`". The
  descriptor's `helpLinkUri` points to this page, and its description was updated in this
  change. `documentation/generated-vs-legacy.md:150-153`, `documentation/m2-named-pipelines.md:6,22`
  and `documentation/m1-generated-mediator.md:14-15,49` still describe the deleted
  `MediatorAssemblies` and `MediatorModule` attributes and the handler interfaces.
- Suggestion: 021-007 owns the docs rewrite. Its Requirements cover the architecture page
  (including diagnostics), the migration page and the package readmes, and it replaces
  `generated-vs-legacy.md`. Defer this there instead of editing pages that page deletes. At
  minimum, 021-007 should fix the TWM009 help text.
- Status: open

## Checked, no finding

- Contracts surface: there are no `Task`-returning members left in `source/timewarp-mediator-contracts/*.cs`.
  `IBaseRequest` and `IComparable` are gone from `source/`.
- `in TRequest` removal on `IPipelineBehavior`: this is forced. `TRequest` appears as an input to
  the `next` delegate parameter, so it cannot stay contravariant. The removal is noted in Results.
- The cancellation-token fallback (`t == default ? cancellationToken : t`) is removed. That is
  consistent with a non-optional delegate parameter, and `send-tests.cs:171` still covers token
  substitution through a behavior. 021-007's migration page should mention it.
- Generator csproj `AddMediatorAnalyzerDependency`: `TimeWarp.Mediator.Analyzers.dll` is now
  passed as an analyzer from both the generator and analyzer bin folders in projects that
  reference both. Builds of the named-pipelines, aot, benchmarks-comparison and generators-tests
  projects showed no warnings, and the nupkg layout check passes, so there is no finding.
- Nested `message`/`token` lambda parameters in emitted chains shadow the outer lambda's
  parameters. That needs C# 8, and the emitted code already needs C# 8 for `object?`.
- Type forwards: removed types are dropped. The new `MessageHandlerDelegate<,>` and
  `GeneratedMediatorAttribute` are forwarded, which is harmless.
