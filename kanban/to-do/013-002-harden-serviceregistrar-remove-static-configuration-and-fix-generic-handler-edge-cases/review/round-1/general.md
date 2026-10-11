# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `b9017cc` vs parent `2a4dd6c` (`service-registrar.cs`, `service-collection-extensions.cs`, `generic-request-handler-tests.cs`, `assembly-info.cs`)

## Summary

The change removes the mutable static limits and threads `MediatorServiceConfiguration` through registration and `GenerateCombinations`. Closed non-generic requests no longer call `GetGenericTypeDefinition`. Multi-request handlers are matched on generic definition and arguments instead of `GetInterface(name)`. The `MakeGenericType` catch is narrowed, the unreachable non-interface branch is gone, and the candidate closing-type list is built once per open interface. Required C-5 and C-6 tests are present. Overall risk is low; no defects found.

## Issues

No issues.

## Verification

- **C-4:** No static limit fields remain in `service-registrar.cs`. `SetGenericRequestHandlerRegistrationLimitations` is gone, and `AddMediator` no longer calls it (`service-collection-extensions.cs`). `GenerateCombinations` takes `MediatorServiceConfiguration` and reads `MaxGenericTypeParameters`, `MaxTypesClosing`, and `MaxGenericTypeRegistrations` from that instance (`service-registrar.cs:243-249`). `AddMediatorClassesWithTimeout` reads `configuration.RegistrationTimeout` directly. The 013-001 timeout, token, and saturating-cap behavior is preserved. A repo search finds no remaining setter. Making `ServiceRegistrar` internal stays on 013-004, as the task brief requires.
- **C-5:** `GetConcreteRequestTypes` returns before the type-parameter scan and before `GetGenericTypeDefinition` when the request argument is a generic parameter, is not generic, or is already closed (`service-registrar.cs:223-224`). `class H<T> : IRequestHandler<Ping, Pong>` is skipped instead of throwing `InvalidOperationException`.
- **C-6:** `IsMatchingWithInterface` matches `GetInterfaces()` on generic definition and arguments (`service-registrar.cs:176-179`). It no longer calls `GetInterface(name)`, so a handler that implements `IRequestHandler<,>` for several requests does not throw `AmbiguousMatchException`. Callers still pass concrete classes from the non-open concretion list, which is the only path that used the old name lookup.
- **C-7:** `AddConcretionsThatCouldBeClosed` catches only `ArgumentException`, with a comment (`service-registrar.cs:191-194`). `FindInterfacesThatClosesCore` no longer has the unreachable non-interface `else if`. `AddAllConcretionsThatClose` builds the candidate list once with `DefinedTypes` and reuses it for each concretion (`service-registrar.cs:307-311`).
- **Tests:** `ShouldApplyEachConfigurationIndependently` covers independent configs. `ShouldSkipGenericHandlerOfClosedNonGenericRequest` covers C-5. `ShouldRegisterHandlerImplementingSameInterfaceForSeveralRequests` includes a second handler so the ambiguous-match path runs, and asserts both request interfaces. The 013-001 direct `GenerateCombinations` tests now pass configuration instead of the removed setter. `assembly-info.cs` still serializes the suite, now for the timing-sensitive timeout test.
- **Call sites:** Every `ConnectImplementationsToTypesClosing` call passes `configuration` and `cancellationToken`. `AddAllConcretionsThatClose` and `GetConcreteRequestTypes` receive both.
- **Diagnostics:** Roslynk reports no errors or warnings on the changed files.
- **Re-run:** `dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests"` — Passed 25, Failed 0, Skipped 0.
