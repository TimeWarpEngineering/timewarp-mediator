# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** `master...902e5d5`: `source/timewarp-mediator-generators/{mediator-emitter,manifest-emitter}.cs`, generator tests, compilation tests, comparison benchmark, package smoke, `documentation/benchmarks/comparison-baseline.md`, `.editorconfig`.

## Summary

The change removes `SendCore` / `Box<T>` from typed `Send`, replaces per-call closures with static `MessageHandlerDelegate` fields, moves registration to concrete types in the host's namespace, and adds typed manifest entries and the `[GeneratedMediator]` attribute. The manifest, registration, attribute and typed `Publish` parts match the task. Two of the speed paths are unsound, though. The `Unsafe.As` reinterpretation of `ValueTask<TConcrete>` as `ValueTask<TResponse>` breaks covariant sends. The static chain reads its `IServiceProvider` from a `[ThreadStatic]` slot that is gone once a behavior awaits before it calls `next`. Neither case has a test. The rest are smaller semantic shifts (handler cache, synchronous throws) and polish.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator-generators/mediator-emitter.cs:319
- Description: Each `Send<TResponse>` arm returns `Unsafe.As<ValueTask<TConcrete>, ValueTask<TResponse>>(ref pending)`. The Design region says this is safe because covariant `IRequest<out T>` conversions "share one ValueTask layout". The layout is the same, but `ValueTask<T>` type-tests its `_obj` (`obj is Task<TResult>`) and otherwise calls `IValueTaskSource<TResult>` through `Unsafe.As`. A `Task<string>` is neither a `Task<object>` nor an `IValueTaskSource<object>`. Verified with a probe (`/tmp/rv021003/probe.cs`): an async-completing `ValueTask<string>` reinterpreted as `ValueTask<object>` throws `System.EntryPointNotFoundException` on await. The synchronous case happens to work, so the existing tests pass. A second failure mode: a request that implements both `IRequest<A>` and `IRequest<B>`, with a handler only for `A`, matches the same `case` arm on `Send<B>`. It returns an `A` typed as `B`, which is type confusion and, for value types of different sizes, a garbage read. The old `SendCore` path turned both into a working cast or an `InvalidCastException`.
- Suggestion: Guard the reinterpretation with `typeof(TResponse) == typeof(TConcrete)`. The JIT folds this for value types, and it is a cheap check for shared reference generics. When it fails, fall back to a small async adapter that awaits the concrete `ValueTask` and returns `(TResponse)(object)result`. That throws `InvalidCastException` in the mismatched case. Add dispatch tests that send `IRequest<object>` (or an interface response) for a handler that completes asynchronously (`await Task.Yield()`), and correct the Design region's claim.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-mediator-generators/mediator-emitter.cs:712
- Description: `Chain_*` keeps the per-call scope in a `[ThreadStatic] IServiceProvider? Scope`. `Invoke` sets it only for the synchronous descent and restores it in `finally`. Each `Step{n}` reads `Scope!` when `next` is invoked. A behavior that awaits anything incomplete before it calls `next` hits this. Examples are FluentValidation's `await validator.ValidateAsync(...)`, pre-processors, async logging and authorization checks, and a retry behavior that calls `next` again after a delay. When such a behavior resumes on a continuation, `Scope` is `null`, so `GetRequiredService(null)` throws `ArgumentNullException`. If the continuation runs inline inside another `Invoke` for the same request type on that thread, `Scope` is that other call's scope instead. The step then silently resolves the inner behaviors and handler from the wrong DI scope: another request's `DbContext`, user, or State store. The Design region documents this as a limitation, but `IPipelineBehavior.Handle` places no "call next synchronously" requirement on implementers. Neither the analyzer nor the runtime can detect a violation. Every test behavior calls `next` before its first await, so none of them exercises this.
- Suggestion: Bind the chain to the scope without ambient state. One option keeps 0 B per call once a scope is warm. Emit `Chain_*` as a class that is registered scoped, holds the `IServiceProvider` (or the resolved behaviors and handler), and creates its `Next{n}` instance-method delegates once in the constructor. `Dispatch_*` resolves the chain from `scope` and calls `Invoke`. If the operator accepts the synchronous-`next` restriction instead, that is a contract change: it needs an operator decision recorded on 021, a stated rule in the `IPipelineBehavior` XML docs, and a fail-fast rather than a wrong-scope resolution. Either way, add a test where a behavior does `await Task.Yield();` before `next`, and one where two concurrent sends of the same request run on different scopes.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-mediator-generators/mediator-emitter.cs:771
- Description: The no-behavior Host path adds a per-thread handler cache: `[ThreadStatic] CachedScope_X` / `CachedHandler_X`, keyed by `ReferenceEquals(scope)`. No requirement asked for it. It has three side effects. (a) Every thread-pool thread keeps the last scope it used for each request type alive, along with the cached handler and its dependency graph, even after that scope is disposed. On a server that is request types × threads worth of retained `DbContext` and other scoped graphs. (b) A send on a disposed scope returns the cached, possibly disposed, handler instead of the `ObjectDisposedException` the container would throw. (c) A consumer who registers a handler as transient, after `AddGeneratedMediator` or in place of it, silently gets a per-scope, per-thread instance.
- Suggestion: Drop the cache and resolve per send. The interface target was met mainly by removing `SendCore` / `Box`, so re-measure. If the cache stays, scope it to the DI scope (for example through the scoped chain object in Issue 2) rather than a thread-static. Then document in the Design region that handler lifetime is effectively scoped.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-mediator-generators/mediator-emitter.cs:829
- Description: `AdaptUnit` and `AsVoid` have an `IsCompleted` (faulted or canceled) branch that calls `GetResult()`. It turns a faulted or canceled `ValueTask` into a synchronous throw out of `Send`. More broadly, `Send<TResponse>`, `Send<TRequest>` and `Dispatch_*` are no longer `async`. A handler that throws synchronously, or a `NoHandlerException`, now escapes `Send` as a thrown exception rather than a faulted `ValueTask`. That breaks patterns such as `Task.WhenAll(items.Select(i => sender.Send(i).AsTask()))`, where one failure now aborts the `Select`. 14.x captured these in the task.
- Suggestion: Remove the `IsCompleted` branch so a non-successful `pending` goes to `AwaitUnit` / `AwaitVoid` and stays a faulted task. Decide whether synchronous throws from typed `Send` are intended for 15.0. If they are, note it for the 021-007 migration guide; if not, wrap the throw paths so they return `ValueTask.FromException`.
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-mediator-generators/mediator-emitter.cs:136
- Description: `[assembly: GeneratedMediatorAttribute(typeof(<ns>.Mediator))]` emits the namespace without `global::`. Every other emitted type reference is fully qualified. A consumer's global using alias or a type named like the first namespace segment can rebind it.
- Suggestion: Emit `typeof(global::<ns>.Mediator)`.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-mediator-generators/manifest-emitter.cs:214
- Description: `MediatorManifest.RequestEntry.Behaviors` is a `string[]` that shares the static backing array. `Requests` returns the static `RequestEntry[]` typed as `IReadOnlyList`, so a consumer can cast it back and mutate the process-wide manifest.
- Suggestion: Type `Behaviors` as `IReadOnlyList<string>`. Return a `ReadOnlyCollection` wrapper, or accept the array exposure on purpose and say so in the Design region.
- Status: open

### Issue 7 — Severity: nit
- File: documentation/benchmarks/comparison-baseline.md:6
- Description: The re-recorded baseline header reads `Commit: 1c7f2b0… (working tree had uncommitted changes)`. That is the base commit, not the implementation commit `902e5d5`. Anyone reading the gate file cannot tell which code produced the numbers.
- Suggestion: Re-record with `./bin/dev benchmark --project comparison --record` from a clean tree after the fix pass. The Issue 1–3 fixes change the hot path anyway, so the gate has to be re-proven.
- Status: open

## Checked, no issue

- Kind precedence (Query > IdempotentCommand > Command > Action > Request) and the JSON `kind` field; tests cover Action, IdempotentCommand, Query, Request.
- Registration: concrete handlers and behaviors only, internal class in the host generated namespace, name derived from the namespace; tests assert no interface descriptors.
- Typed `Publish<TNotification>`: separate `if` tests, most-derived first, sequential awaits, single-type fast path keeps contravariant delivery.
- `IRequest` (void) does not derive `IRequest<Unit>`, so splitting the unit and response switches does not lose any reachable arm.
- Benchmark rows in Results match `comparison-baseline.md` and meet the stated targets (9.731 ns ≤ 13.005 ns, 24 B; void 0 B; State pipeline 0 B).

## Goal restatement check

The restatement matches the task. It adds kind precedence and "outermost first", which are consistent with the task and the parent review. No misread. The per-thread handler cache (Issue 3) goes beyond both the task and the restatement.
