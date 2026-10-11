# Fix legacy registrar generic-registration limits and timeout

## Description

The reflection registrar (`source/timewarp-mediator/registration/service-registrar.cs`) does not honor its documented limit settings. Parent review task 013, findings C-1, C-2, C-3.

- C-1 (bug): `RegistrationTimeout = 0` is documented as "disable", but `new CancellationTokenSource(0)` cancels at once, so `RegisterGenericHandlers` throws a spurious `TimeoutException`.
- C-3 (bug): the combination guard reads `MaxGenericTypeParameters > 0 && totalCombinations > MaxGenericTypeRegistrations`. It should guard on `MaxGenericTypeRegistrations > 0`. The multiply is unchecked `long`.
- C-2 (risk): the cancellation token reaches only two of the six scan passes; notification, stream, exception-handler and processor scans ignore the timeout.

## Requirements

- Zero (or negative) `RegistrationTimeout` disables the timeout. Use `Timeout.InfiniteTimeSpan` or skip the CTS.
- Pass the inner `OperationCanceledException` to the `TimeoutException`.
- Guard on `MaxGenericTypeRegistrations > 0`; use a checked or saturating multiply.
- Thread the cancellation token through every `ConnectImplementationsToTypesClosing` / `AddAllConcretionsThatClose` call.
- Tests in `tests/timewarp-mediator-tests` for: timeout 0 does not throw; `MaxGenericTypeRegistrations = 0` disables the cap; a cap of N throws past N.

## Checklist

- [x] Fix timeout-zero handling
- [x] Fix registration-count guard and overflow
- [x] Thread cancellation token to all scan passes
- [x] Add regression tests
- [x] `./bin/dev workflow` green
- [x] Implementation review disposition (`review/disposition.md`, clean)
- [ ] PR with merge commit (host open-pr node)

## Notes

Parent: 013. Legacy `AddMediator` path only; the generated mediator is unaffected.

Review trail: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 1, roster general, one round, disposition clean (0 open).

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implementer (ganda task work) landed C-1/C-2/C-3 fixes and regression tests; `./bin/dev workflow` and `ganda repo audit` green.
- Review: opencode session ses_ed3da47abffeSdWFr7Ixl4aJro (2026-10-12). Effort 1, general only.

## Results

`ServiceRegistrar` (`source/timewarp-mediator/registration/service-registrar.cs`) now honors its documented limits:

- **C-1:** `RegistrationTimeout <= 0` skips the `CancellationTokenSource` entirely, so a zero timeout no longer cancels at once. A timeout wraps the inner `OperationCanceledException` as the `TimeoutException` inner exception, and only when the registrar's own CTS fired.
- **C-3:** The combination cap guards on `MaxGenericTypeRegistrations > 0` (it used to read `MaxGenericTypeParameters`). The running product saturates at `long.MaxValue` instead of overflowing.
- **C-2:** The cancellation token now reaches all eight `ConnectImplementationsToTypesClosing` passes (notification, stream, exception handler/action, pre/post processors). Each scan loop calls `ThrowIfCancellationRequested`, so the timeout covers the whole scan and not only combination generation.

New tests in `tests/timewarp-mediator-tests/generic-request-handler-tests.cs`:

- `ShouldNotTimeOutWhenRegistrationTimeoutIsZero`
- `ShouldNotCapGenericRegistrationsWhenMaxGenericTypeRegistrationsIsZero` (fails on the old guard: default `MaxGenericTypeParameters = 10` plus cap 0 threw)
- `ShouldThrowWhenGenericRegistrationsExceedConfiguredCap` (theory over `MaxGenericTypeParameters` 10 and 0; a cap of 27 allows 27 and a cap of 26 throws)
- `ShouldNotOverflowWhenCountingGenericRegistrations` (10 lists of 100 types; 10^20 would overflow `long`)

### How to validate

**Smoke:**

```bash
dotnet test tests/timewarp-mediator-tests -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests"
./bin/dev workflow
```

**Expect:** All 22 `GenericRequestHandlerTests` pass, including the four new ones. `./bin/dev workflow` ends with `Pipeline SUCCEEDED` (timewarp-mediator-tests: 170 passed, 2 skipped). The existing `ShouldThrowExceptionWhenTimeoutOccurs` still throws `TimeoutException` with the same message.

### Review

- Rounds: 1. Roster: general. Effort: 1.
- Counts (final, `review/round-1/merged.md`): bug 0/0/0, suggestion 0/0/0, nit 0/0/0 (open/fixed/wontfix).
- Disposition: clean (`review/disposition.md`). No wontfix, no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Re-verified during review: `dotnet test tests/timewarp-mediator-tests -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests"` — Passed 22, Failed 0, Skipped 0.

