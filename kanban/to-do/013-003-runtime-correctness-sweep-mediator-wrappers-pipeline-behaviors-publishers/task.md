# Runtime correctness sweep: Mediator, wrappers, pipeline behaviors, publishers

## Description

Small, verified defects in the shipped reflection runtime. Parent review task 013, findings C-8, C-9, C-10, C-11, C-12, C-13, C-15, C-16, C-17, C-22, C-26, C-27, C-30.

- C-9 (bug): `mediator.cs` `Publish(object)` message has a stray `$` ("does not implement $INotification") and no `paramName`.
- C-22 (risk): `type-forwardings.cs` forwards every moved contract except `IStreamRequest<>`.
- C-13 (risk): `RequestExceptionProcessorBehavior` rethrows when `Handled` is true but `Response` is null, so a handler that legitimately returns null sees its exception rethrown.
- C-15 (risk): `TaskWhenAllPublisher` invokes handlers synchronously inside `Select(...).ToArray()`; a synchronous throw skips remaining handlers and leaves earlier tasks unobserved. Its XML sample does not match the code.
- C-12 (risk): the four `AddOpenBehavior`-family methods check `IsGenericType` instead of `IsGenericTypeDefinition`, so a closed generic passes and fails later in the container.
- C-8 (risk): wrapper caches are keyed by request type only; `Send(object)` then `Send<object>` on the same request type can `InvalidCastException`. Fix or document.
- C-10, C-11 (nit): missing null guards in `Mediator` constructor, `AddMediator`, configuration methods; `open-behavior.cs` passes a sentence as `paramName`.
- C-16, C-17 (nit): stream handler resolved lazily on first `MoveNextAsync`; one missing `ConfigureAwait(false)`.
- C-26, C-27, C-30 (nit): stale "dynamic dispatch" comment; `Compare(null, null)` returns 1 in `object-details.cs`; `RegisterBehaviorIfImplementationsExist` only sees registrations present at `AddMediator` time (document it).

## Checklist

- [x] Fix C-9 message and paramName
- [x] Add `IStreamRequest<>` type forward (C-22) and a test that enumerates public contract types against the forwarding list
- [x] Decide and fix C-13 null-Handled semantics
- [x] Make `TaskWhenAllPublisher` convert synchronous throws to faulted tasks; fix its doc sample (C-15)
- [x] `IsGenericTypeDefinition` in open-behavior registration; extract the four duplicate blocks (C-12)
- [x] Fix or document C-8 cache key
- [x] Null guards and nits (C-10, C-11, C-16, C-17, C-26, C-27, C-30)
- [x] Tests for each behavior change; `./bin/dev workflow` green
- [ ] PR with merge commit (host open-pr node)

## Notes

Parent: 013. No public API additions. Keep each fix a separate commit for reviewability.

## Session

- Created: 686431 (2026-10-11)
- Implemented 2026-10-12 (ganda task work implement oracle): one commit per finding group, `./bin/dev workflow` green, `ganda repo audit` clean.

## Results

One commit per finding group on `task/013-003-runtime-correctness-sweep-mediator-wrappers-pipeli`. No public API additions.

| Finding | Change | Test |
|---------|--------|------|
| C-9 | `Publish(object)` message is `"<Type> does not implement INotification"` with `paramName: "notification"` | `ExceptionTests.Should_throw_argument_exception_with_param_name_for_publish_of_non_notification` |
| C-22 | Added `[TypeForwardedTo(typeof(IStreamRequest<>))]` | `TypeForwardingTests.Should_forward_every_public_contract_type` diffs every exported contracts type against `GetForwardedTypes()` (confirmed it fails without the forward) |
| C-13 | **Decision:** `Handled` is authoritative. `RequestExceptionProcessorBehavior` returns `state.Response` even when it is null; rethrows only when no handler set `Handled`. The XML doc on `RequestExceptionHandlerState.Response` says so | `RequestExceptionHandlerTests.Should_return_null_response_when_exception_handled_with_null` |
| C-15 | `TaskWhenAllPublisher` wraps each callback; a synchronous throw becomes `Task.FromException`, so the remaining handlers still run and every failure lands in the aggregate. Doc sample now matches the code | `NotificationPublisherTests.Task_when_all_publisher_should_run_remaining_handlers_when_a_handler_throws_synchronously` |
| C-12 | The four `AddOpen*` methods now delegate to one private `AddOpenGeneric` helper that requires `IsGenericTypeDefinition` (message: "must be an open generic type definition") and null-guards `openBehaviorType` | `OpenGenericRegistrationTests` (closed generic rejected, null rejected, open registered once) |
| C-8 | **Fixed:** typed `Send<TResponse>` / void `Send<TRequest>` / `CreateStream<TResponse>` caches are keyed by `(requestType, responseType)` (`typeof(void)` marks the void wrapper). `Send(object)` and `CreateStream(object)` have their own request-type-keyed caches | `WrapperCacheTests` (3 tests; all 3 threw `InvalidCastException` before the fix) |
| C-16 | Stream wrapper resolves `IStreamRequestHandler<,>` and builds the pipeline when `CreateStream` is called, so a missing handler throws from `CreateStream` and not on the first `MoveNextAsync`. Pipeline behaviors' `Handle` is now also invoked at `CreateStream` time; behaviors written as async iterators still defer their bodies | `CreateStreamTests.Should_resolve_stream_handler_when_stream_is_created` |
| C-17 | Added `ConfigureAwait(false)` to the void request handler await and to the boxed stream enumeration | covered by the existing suite |
| C-10 | Null guards: `Mediator` constructors, both `AddMediator` overloads, `RegisterServicesFrom*`, single-type `AddBehavior` / `AddStreamBehavior` / `AddRequestPre/PostProcessor`, both `AddOpenBehaviors` | `ArgumentGuardTests` |
| C-11 | `OpenBehavior` passes `nameof(openBehaviorType)` as `paramName` | `ArgumentGuardTests.OpenBehavior_should_use_parameter_name_for_null_type` |
| C-26 | Replaced the stale "dynamic dispatch" comment in `Mediator.Send(object)` | n/a |
| C-27 | `ObjectDetails.Compare(null, null)` (and the same instance) returns 0 | `ArgumentGuardTests.ObjectDetails_should_compare_two_nulls_as_equal` |
| C-30 | XML doc on `RegisterBehaviorIfImplementationsExist` explains that only registrations present at `AddMediator` time are seen | n/a (doc only) |

Behavior changes for consumers:

- Closed generic types passed to `AddOpen*` now throw `InvalidOperationException` at configuration time. Before, they failed later in the container.
- A handled exception whose response is null now returns null instead of rethrowing.
- `CreateStream` throws right away when no stream handler is registered.

### How to validate

**Smoke:**

```bash
./bin/dev workflow
dotnet test tests/timewarp-mediator-tests -c Release --no-build --filter "FullyQualifiedName~WrapperCacheTests|FullyQualifiedName~TypeForwardingTests|FullyQualifiedName~OpenGenericRegistrationTests|FullyQualifiedName~ArgumentGuardTests|FullyQualifiedName~RequestExceptionHandlerTests|FullyQualifiedName~NotificationPublisherTests|FullyQualifiedName~CreateStreamTests|FullyQualifiedName~ExceptionTests"
ganda repo audit
```

**Expect:**

- `./bin/dev workflow` ends with `Pipeline SUCCEEDED`. `timewarp-mediator-tests` reports 195 passed and 2 skipped (pre-existing skips), and the generators, analyzers and compilation-test suites all pass.
- The filtered run shows `Failed: 0`.
- `ganda repo audit` prints `Repository passes all audit checks.`
- To confirm a regression guard, remove the `IStreamRequest<>` forward from `source/timewarp-mediator/type-forwardings.cs`: `TypeForwardingTests` then fails and lists ``TimeWarp.Mediator.IStreamRequest`1``.
