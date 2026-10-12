# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: source/timewarp-mediator/mediator.cs:46
- Description: `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` are on both `AddMediator` overloads and on the public constructors of `Mediator` and the two exception behaviors. Those members do not themselves call `MakeGenericType`, `GetMethod`, `Activator.CreateInstance`, or `MethodInfo.Invoke`. `Send`, `Publish`, `CreateStream`, and both behavior `Handle` methods still do that work (directly or via `PublishNotification`, `GetHandlerReflection`, and `GetActionReflection`) outside a Requires scope. A call through `IMediator` or `IPipelineBehavior` does not warn at the call site. With default `TrimmerSingleWarn`, reached library reflection is reported as IL2104 on `TimeWarp.Mediator`; with `TrimmerSingleWarn=false` the IL2026/IL3050 warnings point at the library members. `AddMediator` and `new Mediator(...)` do warn at their call sites. The polyfill namespace, `#if` guards, and attribute shape match what ILLink/ILC look up.
- Suggestion: Also annotate the three `Send` overloads, both `Publish` overloads, both `CreateStream` overloads, both `Handle` methods, and the private reflection helpers. Do not annotate `IMediator` or `IPipelineBehavior`.
- Source: general
- Disposition notes: Wontfix (review oracle, 2026-10-12). The written entry-point list is met: `AddMediator` and the constructors carry both attributes, the exception behaviors cache `MethodInfo` per closed generic and exception type, and `documentation/generated-vs-legacy.md` states the split. Annotating `Send` / `Publish` / `CreateStream` / `Handle` without the same attribute on `ISender`, `IPublisher`, and `IPipelineBehavior` is IL2046 (annotations must match across interface implementations). Those contracts stay trim-safe so the generated mediator is not marked unsafe. Annotating the contracts would warn every generated `Send` / `Publish` / `CreateStream` / `Handle` too. Calls through the interfaces therefore cannot warn. Internal IL2026/IL3050/IL2104 on the reflection members stay; suppressing them would hide a real trim hazard. `AddMediator` and the constructors remain the call-site signal.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/internal/trimming-messages.cs:11
- Description: Both trim messages say to use `AddGeneratedMediator()` "from TimeWarp.Mediator.Generators" (line 15 as well). The generator does not ship that method. `mediator-emitter.cs` emits an internal `GeneratedMediatorServiceCollectionExtensions.AddGeneratedMediator` into the host, in `Microsoft.Extensions.DependencyInjection`. The AOT sample references the generator with `ReferenceOutputAssembly=false`. `documentation/generated-vs-legacy.md` already describes Contracts plus the generator.
- Suggestion: Say that the `TimeWarp.Mediator.Generators` source generator emits `AddGeneratedMediator()` into the host, and mention `TimeWarp.Mediator.Contracts`, instead of attributing the method to the Generators assembly.
- Source: general
- Disposition notes: Fixed on this task. Both messages now say to reference `TimeWarp.Mediator.Contracts` and the `TimeWarp.Mediator.Generators` source generator, which emits `AddGeneratedMediator()` into the host. Round 2 must re-check the wording.

## Duplicates / conflicts

- Single reviewer. No overlaps.
