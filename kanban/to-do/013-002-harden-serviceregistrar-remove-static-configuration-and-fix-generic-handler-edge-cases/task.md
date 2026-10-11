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

- [ ] Replace static configuration with passed configuration
- [ ] Fix non-generic request guard (C-5) with test
- [ ] Fix ambiguous interface lookup (C-6) with test
- [ ] Clean swallowed exception and dead branch (C-7)
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Making `ServiceRegistrar` internal is tracked with the public-surface task; keep this one behavior-only unless both land in the same beta.

## Session

- Created: 686431 (2026-10-11)
