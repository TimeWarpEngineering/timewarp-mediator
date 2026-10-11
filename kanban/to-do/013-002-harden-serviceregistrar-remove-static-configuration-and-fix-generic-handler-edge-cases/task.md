# Harden ServiceRegistrar: remove static configuration and fix generic handler edge cases

## Description

`ServiceRegistrar` keeps its limits in mutable public static fields and has several reflection edge cases. Parent review task 013, findings C-4, C-5, C-6, C-7.

- C-4 (risk): `MaxGenericTypeParameters`, `MaxTypesClosing`, `MaxGenericTypeRegistrations`, `RegistrationTimeout` are non-readonly statics written by `SetGenericRequestHandlerRegistrationLimitations`. Two `AddMediator` calls on different threads overwrite each other. The public `GenerateCombinations` reads zeros when called directly.
- C-5 (risk): a generic handler whose first interface argument is a closed non-generic request (`class H<T> : IRequestHandler<Ping, Pong>`) skips the early return and `GetGenericTypeDefinition()` throws `InvalidOperationException`.
- C-6 (risk): `handlerType.GetInterface(handlerInterface.Name)` throws `AmbiguousMatchException` when a class implements the handler interface for several requests.
- C-7 (debt): empty `catch (Exception)` around `MakeGenericType`; unreachable `else if` branch for non-interface templates; `GetTypes()` used where the rest uses `DefinedTypes`; assemblies rescanned once per generic parameter.

## Requirements

- Pass the configuration object into `GenerateCombinations` and friends instead of reading statics.
- Check `requestType.IsGenericType` before `GetGenericTypeDefinition()`; move early returns above the type scan.
- Replace name-based `GetInterface` with a `GetInterfaces()` match on generic definition and arguments.
- Catch only `ArgumentException` around `MakeGenericType` with a comment; delete the dead branch; compute the type list once.
- Tests for C-5 and C-6 shapes.

## Checklist

- [x] Replace static configuration with passed configuration
- [x] Fix non-generic request guard (C-5) with test
- [x] Fix ambiguous interface lookup (C-6) with test
- [x] Clean swallowed exception and dead branch (C-7)
- [x] `./bin/dev workflow` green (PR with merge commit is a host step)

## Notes

Parent: 013. Making `ServiceRegistrar` internal is tracked with the public-surface task; keep this one behavior-only unless both land in the same beta.

## Session

- Created: 686431 (2026-10-11)
- Implemented: 2026-10-12 (implement oracle)

## Results

- **C-4**: removed the mutable statics and `SetGenericRequestHandlerRegistrationLimitations`. `MediatorServiceConfiguration` is now passed down through `AddMediatorClassesWithTimeout` (reads `configuration.RegistrationTimeout`), `ConnectImplementationsToTypesClosing`, `AddAllConcretionsThatClose`, `GetConcreteRequestTypes`, and `GenerateCombinations(requestType, lists, configuration, depth, ct)`. `AddMediator` no longer calls the setter. Public API change: the setter is gone and `GenerateCombinations` now requires a configuration argument (beta, behavior-only; making `ServiceRegistrar` internal is still tracked with the public-surface task).
- **C-5**: `GetConcreteRequestTypes` returns early before scanning when the request argument is a generic parameter, is not generic, or is already closed. This means `class H<T> : IRequestHandler<Ping, Pong>` is now skipped instead of throwing `InvalidOperationException`.
- **C-6**: `IsMatchingWithInterface` now matches `GetInterfaces()` on generic definition and arguments, so a handler for several requests no longer throws `AmbiguousMatchException`.
- **C-7**: `AddConcretionsThatCouldBeClosed` now catches only `ArgumentException` (constraint violation) and has a comment saying why. Removed the unreachable non-interface branch in `FindInterfacesThatClosesCore`. The candidate closing-type list is built once per `AddAllConcretionsThatClose` call, using `DefinedTypes`, and each generic parameter filters that list.
- Tests (`tests/timewarp-mediator-tests/generic-request-handler-tests.cs`): rewrote the cap and overflow tests to pass configuration directly with no static reset in `finally`. Added `ShouldApplyEachConfigurationIndependently` (C-4), `ShouldSkipGenericHandlerOfClosedNonGenericRequest` (C-5), and `ShouldRegisterHandlerImplementingSameInterfaceForSeveralRequests` (C-6). The `assembly-info.cs` comment now gives the real reason the suite stays serial (the timing-sensitive timeout test).

### How to validate

**Smoke:**

```bash
./bin/dev workflow
dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release --filter "FullyQualifiedName~GenericRequestHandlerTests"
grep -n "private static int" source/timewarp-mediator/registration/service-registrar.cs
```

**Expect:** the workflow ends with `Pipeline SUCCEEDED` (173 passed, 2 skipped). The filtered run passes, including the three new tests. The grep prints nothing because no static limit fields remain.

