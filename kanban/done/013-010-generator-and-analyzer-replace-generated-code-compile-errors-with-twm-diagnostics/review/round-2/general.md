# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** M1 fix on task 013-010 (pipeline scope accessibility) and the surrounding binding loops. Prior round-1 findings re-checked. No new defects on the fix delta.

## Summary

`PipelineScopeAllowed` now runs for the scope that a request or notification binding would emit. An inaccessible marker is TWM007 and the binding is skipped, so `ISender<TScope>` / `IPublisher<TScope>` no longer name it. `InternalsVisibleTo` still binds. The two new compilation tests pass, and the existing scoped-pipeline, host-visibility, generator-diagnostic, and analyzer suites stayed green.

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:211
- Description: Re-checked. Request bindings (`handlerScope ?? requestScope`) and notification bindings (`handlerScope ?? notificationScope`) call `PipelineScopeAllowed` before they are added. `AccessGuard` reports TWM007 once. Unscoped bindings (null scope) are unchanged. Public scope markers in `HostVisibilityTests` and `ScopedPipelineTests` still generate.
- Suggestion: done
- Status: fixed
