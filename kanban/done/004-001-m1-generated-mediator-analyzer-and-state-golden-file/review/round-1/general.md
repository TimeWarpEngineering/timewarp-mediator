# Round 1 — general
**Date:** 2026-08-31
**Scope reviewed:** commit b42ce9b vs origin/master (analyzer, generator, Contracts, golden-file, AOT sample, benchmarks, spec, docs)

## Summary

M1 lands a coherent compile-time path: membership-filtered handler discovery, analyzer-only TWM001/TWM002, a sealed Host/Aot `Mediator` with monomorphic `ValueTask` `Send` plus `Send(object)` switch, State-shaped golden coverage, and an honest benchmark gap doc. Risk is moderate for a greenfield generator (pipeline closing and multi-assembly behavior order are the sharp edges) but the claimed exit criteria hold under re-check. Dominant themes: graph emit looks right for the golden file; a few correctness/polish defects in behavior closing and manifest emission remain.

## Issues

### Issue 1 — Severity: bug
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:361
- Description: `TryCloseBehavior` ends with `return ImplementsPipeline(...) ? constructed : constructed;`, so a failed pipeline-shape check still returns the constructed type. An arity-1 behavior such as `Foo<TRequest> : IPipelineBehavior<TRequest, Unit>` can therefore be closed onto a non-`Unit` request after `CanConstruct` succeeds, producing a `ClosedBehaviors` entry that does not implement `IPipelineBehavior<TRequest, TResponse>` and can break generated `Dispatch_*` compilation or dispatch.
- Suggestion: Return `null` when `ImplementsPipeline` is false (`? constructed : null`), and add a regression test with an arity-1 `Unit`-only behavior plus a non-`Unit` request in the same member assembly.
- Status: open

### Issue 2 — Severity: suggestion
- File: src/TimeWarp.Mediator.Generators/ManifestEmitter.cs:1
- Description: Purpose/docs/csproj describe emitting `mediator.manifest.json` (and Membership exposes `IntermediateOutputPath` / `ProjectDir` for that), but `Emit` only adds `MediatorManifest.g.cs` with an embedded JSON string. The golden test writes `mediator.manifest.json` itself at runtime. M1’s Version=1 constant is satisfied; the named build artifact is not produced by the generator.
- Suggestion: Either write `mediator.manifest.json` under `IntermediateOutputPath` (or via an MSBuild target) and stop manufacturing it in the test, or narrow the Purpose/docs to “embedded `MediatorManifest` v1” and drop unused IntermediateOutputPath wiring until a file emit exists.
- Status: open

### Issue 3 — Severity: suggestion
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:266
- Description: `DiscoverBehaviors` walks `membership.MemberAssemblies` (`ImmutableHashSet`) with no name sort before assigning `SourceIndex`. When multiple member assemblies register `[assembly: MediatorBehavior(..., order: 0)]` (or omit `order`), tie-breaking follows hash-set iteration and can yield non-deterministic pipeline order across builds.
- Suggestion: Enumerate member assemblies ordered by `assembly.Name` (then attribute order within each assembly) before assigning `SourceIndex`, matching the “attribute order, then optional order” contract in a stable way.
- Status: open

### Issue 4 — Severity: suggestion
- File: src/TimeWarp.Mediator.Generators/MediatorEmitter.cs:352
- Description: In the Aot profile, `EmitDispatchMethods` resolves only `ServiceGen` handler fields and never closes or invokes `ClosedBehaviors`. `[assembly: MediatorBehavior]` on an Aot host is silently ignored, which is fine for the current parameterless Ping sample but surprising if someone reuses Aot with a State-like pipeline.
- Suggestion: Document that Aot/ServiceGen M1 drops behaviors, or emit a diagnostic when behaviors are registered under the Aot profile until an inline/static weave exists.
- Status: open
