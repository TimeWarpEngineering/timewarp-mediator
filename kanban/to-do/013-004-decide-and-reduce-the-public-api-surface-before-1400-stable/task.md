# Decide and reduce the public API surface before 14.0.0 stable

## Description

Implementation types in TimeWarp.Mediator are public and cannot change later without a breaking release. Parent review task 013, findings C-20, C-21, and the "internal" half of C-4.

- `RequestHandlerWrapper` / `RequestHandlerWrapperImpl` / `NotificationHandlerWrapper*` are public while `StreamRequestHandlerWrapper*` are internal.
- `ServiceRegistrar` is a public static class with public `GenerateCombinations` and `AddMediatorClassesWithTimeout`.
- `ServiceFactory` delegate (`service-factory.cs`) has no references in source or tests; only a stale comment mentions it.
- `notification-handler-wrapper.cs` names its `IServiceProvider` parameter `serviceFactory`.

## Requirements

- Decide which types stay public (document the decision in Notes). Default: wrappers and `ServiceRegistrar` become internal; `ServiceFactory` is removed or `[Obsolete]`.
- Check the Generators package, tests, and samples for uses before internalizing.
- Land in a 14.0.0 beta, not after stable.
- Record the change in the 14.0.0 release notes.

## Checklist

- [ ] Inventory references to each candidate type across source, tests, samples, generators
- [ ] Internalize wrappers and registrar (or record why not)
- [ ] Remove or obsolete `ServiceFactory`; fix stale comment and `service-factory-tests.cs` name
- [ ] Rename `serviceFactory` parameter
- [ ] Release-notes entry; `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Breaking change; needs maintainer sign-off on the final list.

## Session

- Created: 686431 (2026-10-11)
