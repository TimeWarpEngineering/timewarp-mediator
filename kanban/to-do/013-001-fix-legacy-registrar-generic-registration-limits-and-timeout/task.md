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

- [ ] Fix timeout-zero handling
- [ ] Fix registration-count guard and overflow
- [ ] Thread cancellation token to all scan passes
- [ ] Add regression tests
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Legacy `AddMediator` path only; the generated mediator is unaffected.

## Session

- Created: 686431 (2026-10-11)
