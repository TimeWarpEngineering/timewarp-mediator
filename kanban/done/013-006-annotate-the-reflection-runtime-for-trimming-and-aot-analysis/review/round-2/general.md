# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** fix delta for M2 (`trimming-messages.cs`) and re-check of M1 wontfix

## Summary

The only product diff since round 1 is the two `TrimmingMessages` sentences. Both now tell the caller to reference `TimeWarp.Mediator.Contracts` and the `TimeWarp.Mediator.Generators` source generator, which emits `AddGeneratedMediator()` into the host. That matches `mediator-emitter.cs`: the default Host profile writes an internal `GeneratedMediatorServiceCollectionExtensions.AddGeneratedMediator` into `Microsoft.Extensions.DependencyInjection`, and generated registration uses Contracts types that the host must reference directly. `Send`, `Publish`, `CreateStream`, and behavior `Handle` are still unannotated, and `ISender`, `IPublisher`, and `IPipelineBehavior` still have no trim attributes, so the IL2046 wontfix stands.

## Resolved prior

- M1 — suggestion — wontfix (confirmed): interface members in Contracts are still unannotated, and the reflection `Send` / `Publish` / `CreateStream` / `Handle` methods were not given `[RequiresUnreferencedCode]` or `[RequiresDynamicCode]`; no call-site warning reaches those interface dispatches without IL2046 or annotating the contracts.
- M2 — nit — fixed (confirmed): the new sentences no longer attribute `AddGeneratedMediator()` to the Generators assembly, and they correctly describe host emission plus a Contracts reference.

## Issues

No issues.
