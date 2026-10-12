# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** git diff 01e74ea...HEAD (013-004 public-API reduction, commit 3504496)

## Summary

The commit internalizes the request and notification handler wrappers and `ServiceRegistrar`, deletes the unused `ServiceFactory` delegate, and gives the Windsor sample its own internal resolver delegate in the same assembly. `InternalsVisibleTo` names `timewarp-mediator-tests`, which matches the test assembly name, and the embedded public key matches `timewarp-mediator.snk`. Types left public sit on consumer-facing signatures (`INotificationPublisher.Publish`, `MediatorServiceConfiguration.NotificationPublisher`, `AddOpenBehaviors`, `IRequestExceptionHandler`, and the four behaviors Windsor registers by type). The release-note names match the code, the relative link to `documentation/generated-vs-legacy.md` resolves, and no `serviceFactory` identifier remains under `source/`. The runtime test project and the Windsor sample build, and the filtered registrar plus missing-handler tests passed (26/26).
