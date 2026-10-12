# Runtime test suite: deterministic publisher tests, variance coverage, remove permanent skips

## Description

The runtime suite is the ported MediatR suite with timing-based assertions and gaps versus advertised features. Parent review task 013, findings D-16, D-17, D-18, D-19, D-20, D-21.

- D-16 (risk): `notification-publisher-tests.cs` compares `Stopwatch` elapsed for sequential vs `TaskWhenAllPublisher` runs (500 ms and 250 ms delays). Cold-start and CI load can flip it, and it never asserts order.
- D-20 (risk): no test for contravariant notification or request dispatch although the readme advertises generic variance; publish strategies have one timing test.
- D-17 (debt): timeout tests use real 2 s delays.
- D-19 (debt): two Lamar open-generic tests are permanently skipped; Lamar is a primary test container although MS.DI is the shipped integration.
- D-18, D-21 (nit): static mutable counters in `pipe-line-multi-call-to-constructor-test.cs`; duplicate file names at two folder levels (`pipeline-tests.cs`, `stream-pipeline-tests.cs`, `notification-publisher-tests.cs`).

## Requirements

- Replace delay-based publisher tests with `TaskCompletionSource`-gated handlers that assert call order (sequential) and concurrency (parallel).
- Add variance dispatch tests.
- Use `Task.Delay(Timeout.Infinite, ct)` plus explicit cancellation in timeout tests.
- Delete or port the two skipped Lamar tests; decide whether Lamar stays a test dependency (it is also the only outdated pin, held at 15.0.1 for net8.0).
- Instance-scope the static counters; rename duplicate files.

## Checklist

- [x] Deterministic publisher tests
- [x] Variance tests
- [x] Timeout tests without real delays
- [x] Skips removed or ported; Lamar decision recorded
- [x] Nits
- [ ] `./bin/dev test` green; PR with merge commit (tests green; PR opened by the host)

## Notes

Parent: 013.

**Lamar decision:** Lamar stays a test dependency. About 40 runtime tests (publish, pipeline,
exception, stream, create-stream) use it as a second container next to the MS.DI tests under
`microsoft-extensions-di/`, so it still covers non-MS.DI resolution. The pin is no longer outdated:
`Directory.Packages.props` already has Lamar 16.0.0 since the net10.0 retarget. The two skipped
"concrete and open generics" tests are ported to MS.DI, which mixes closed and open generic
registrations in registration order. Lamar does not.

## Results

- **D-16:** `tests/timewarp-mediator-tests/notification-publisher-tests.cs` no longer uses
  `Stopwatch`. The handlers wait on a `TaskCompletionSource` gate (`HandlerGate`). The sequential
  tests (`ForeachAwaitPublisher` directly, and the mediator's default publisher) assert that only
  one handler has started while the gate is closed, and that events come in start/end/start/end
  order. The concurrent tests (`TaskWhenAllPublisher` directly, and through
  `NotificationPublisherType`) assert that both handlers start before the gate opens.
- **D-20:** new `variance-tests.cs` covers contravariant notification dispatch (a derived
  notification reaches the derived, base-class and interface handlers, but a base notification
  does not reach the derived handler), `Publish(object)` dispatch on the runtime type, and a
  contravariant request handler (a `Greeting` handler serving `FormalGreeting`).
- **D-17:** in `send-tests.cs` the handlers await `Task.Delay(Timeout.Infinite, ct)`.
  `TimeoutBehavior` (constrained to `ITimeoutRequest`) starts the handler, then cancels its linked
  token explicitly. No 500 ms timer or 2 s delays remain.
- **D-19:** both `[Fact(Skip = ...)]` tests in `pipeline-tests.cs` and `stream-pipeline-tests.cs`
  are ported to MS.DI and now run. The runtime suite has 0 skips.
- **D-18:** `ConstructorTestHandler` uses an injected `ConstructorCallCounter` (one instance per
  test) instead of a static counter and lock.
- **D-21:** the duplicate files under `microsoft-extensions-di/` are renamed to
  `service-collection-{pipeline,stream-pipeline,notification-publisher}-tests.cs`, with classes
  renamed to match. `pipe-line-multi-call-to-constructor-test.cs` is renamed to
  `pipeline-multi-call-to-constructor-tests.cs`. The validate-on-build tests register the new
  handler dependencies.
- `./bin/dev build --clean` + `./bin/dev test`: runtime 211 passed / 0 skipped; generators 40,
  analyzers 43, compilation 85, roslyn-floor 86, all passing. The new and changed tests passed
  5 runs in a row at about 200 ms each. `ganda repo audit` passes.
- Environment note: one local build failed with `TypeLoadException ... MembershipOptions`
  (a stale VBCSCompiler server holding an old Analyzers DLL). `dotnet build-server shutdown`
  fixed it. This is not a code issue.

### How to validate

**Smoke:**

```bash
./bin/dev build --clean
./bin/dev test --no-build
dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj \
  --filter "FullyQualifiedName~NotificationPublisherTests|FullyQualifiedName~VarianceTests|FullyQualifiedName~SendTests|FullyQualifiedName~Should_handle_concrete_and_open_generics"
grep -rn "Skip =\|Stopwatch\|Task.Delay(2000\|Task.Delay(500\|Task.Delay(250" tests/timewarp-mediator-tests
```

**Expect:**

- The build succeeds, and every test project reports `Failed: 0`. `timewarp-mediator-tests`
  shows `Passed: 211, Skipped: 0`.
- The filtered run passes 30 tests in well under a second (no real delays).
- The `grep` finds nothing.

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implemented D-16..D-21 (implement oracle, Claude).
