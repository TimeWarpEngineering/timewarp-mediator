# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** branch `task/013-001-fix-legacy-registrar-generic-registration-limits-a` vs `origin/master` (`source/timewarp-mediator/registration/service-registrar.cs`, `tests/timewarp-mediator-tests/generic-request-handler-tests.cs`, `tests/timewarp-mediator-tests/global-usings.cs`)

## Summary

The change makes the legacy registrar honor the three documented limit bugs from parent task 013 (C-1, C-2, C-3) without widening scope into static-configuration hardening (C-4, task 013-002). Timeout disable, inner-exception wrapping, token threading, and the registration-cap guard match the task requirements. Required regression tests are present and the filtered suite passed (22/22).

## Issues

No issues.

## Verification

- **C-1:** `AddMediatorClassesWithTimeout` skips the `CancellationTokenSource` when `RegistrationTimeout <= 0` (`service-registrar.cs:22-26`). A positive timeout still uses `new CancellationTokenSource(RegistrationTimeout)` and wraps `OperationCanceledException` only when that source has fired (`service-registrar.cs:28-35`), so an unrelated cancellation is not reported as a timeout. Message text is unchanged, so `ShouldThrowExceptionWhenTimeoutOccurs` still matches.
- **C-2:** All eight `ConnectImplementationsToTypesClosing` call sites pass `cancellationToken` (`service-registrar.cs:44-54`), including notification, stream, exception handler, exception action, and the pre/post processor pair. `AddAllConcretionsThatClose`, `GetConcreteRequestTypes`, and `GenerateCombinations` receive the same token. `ThrowIfCancellationRequested` runs in each of the three scan loops plus combination generation and concrete registration. The method is private; there are no other call sites.
- **C-3:** The combination cap is gated on `MaxGenericTypeRegistrations > 0` (`service-registrar.cs:270`). The running product saturates at `long.MaxValue` instead of wrapping, and a zero `list.Count` short-circuits the division. Equal-to-cap is allowed (`>`), which matches "throws past N".
- **Tests:** `ShouldNotTimeOutWhenRegistrationTimeoutIsZero`, `ShouldNotCapGenericRegistrationsWhenMaxGenericTypeRegistrationsIsZero`, and `ShouldThrowWhenGenericRegistrationsExceedConfiguredCap` cover the required cases. `ShouldNotOverflowWhenCountingGenericRegistrations` covers a product that would wrap an unchecked `long` on the old guard. Direct `GenerateCombinations` tests restore static limits in `finally`. Assembly parallelization is already disabled (`assembly-info.cs`).
- **Re-run:** `dotnet test tests/timewarp-mediator-tests -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests"` — Passed 22, Failed 0, Skipped 0.
