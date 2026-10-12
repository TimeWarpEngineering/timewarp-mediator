# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `8063f41` vs `f2a5a3f` (product files listed in `review/review-framework.md`)

## Summary

The suite changes match D-16, D-17, D-18, D-19, and D-21. Publisher tests gate handlers on a `TaskCompletionSource` and assert non-overlapping order for `ForeachAwaitPublisher` and overlap for `TaskWhenAllPublisher`. Both publishers run each handler body on the calling thread until its first await, so those checks are deterministic for these implementations. Timeout handlers wait on `Task.Delay(Timeout.Infinite, ct)` and `TimeoutBehavior` is constrained to `ITimeoutRequest`, so the immediate cancel does not wrap the other `SendTests`. The two skipped Lamar tests now run on MS.DI, the runtime tree has no `[Fact(Skip)]` and no `Stopwatch`, the constructor counter is per test, and the duplicate file names under `microsoft-extensions-di/` are renamed. Lamar stays at 16.0.0, as the task notes. The new variance file describes a stronger rule than the reflection registrar implements.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-mediator-tests/variance-tests.cs:3
- Description: The file summary says a base handler also handles a derived message. That is not what `AddMediator` does. `ServiceRegistrar.ConnectImplementationsToTypesClosing` only learns closed handler interfaces that scanned handlers implement. For notifications (`addIfAlreadyExists: true`) it then registers every scanned handler assignable to that closed interface, so `Publish(DogBarked)` reaches `AnimalEventHandler` and `AnimalEventInterfaceHandler` only because `DogBarkedHandler` is scanned. Removing `DogBarkedHandler` from `ScannedTypes` and publishing `DogBarked` yields an empty log. For requests the registrar keeps one exact match (`IsMatchingWithInterface`) and never registers `IRequestHandler<FormalGreeting, string>`. `Send_derived_request_should_use_contravariant_base_handler` passes only because line 103 adds that closed service by hand. Removing that line throws `InvalidOperationException`: no service for `IRequestHandler<FormalGreeting, string>`. The explicit registration is legal because `TRequest` is `in`, and the handler does see the runtime type, but the test name reads as automatic contravariant lookup.
- Suggestion: State those two rules in the file summary. Rename the request test so the explicit closed registration is the scenario, and say why the registrar will not supply it.
- Status: open
