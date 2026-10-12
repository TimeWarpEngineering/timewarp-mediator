# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit 0bc5cac vs dad2483 (TWM007–TWM011, TWM003/TWM004 descriptor and analyzer changes, diagnostics docs, generator and analyzer tests)

## Summary

The change replaces generated-code compile failures and silent drops with TWM007–TWM011, and it aligns TWM003 and TWM004 with the task brief. Accessibility checks cover handlers, requests, responses, notifications, and behavior definitions, including generic arguments and arrays. TWM008 emits nothing when Contracts or Host MS.DI types are missing. TWM009–TWM011, help links, CompilationEnd on TWM001–TWM003, and the CreateStream message match the requirements and the tests that lock them. One remaining path still names an inaccessible type in generated code.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:209
- Description: Pipeline scope markers are not passed through `AccessGuard`. `ResolveScope` can return an `internal` marker from a referenced member assembly (type attribute or `[assembly: MediatorScope]`). The binding is still added, and `MediatorEmitter` names that marker on `ISender<TScope>` / `IPublisher<TScope>` (`mediator-emitter.cs` `Fq(scope)`). That is CS0122, which TWM007 is supposed to replace. Handlers, requests, responses, notifications, and behavior definitions are checked; the scope marker is the type the generator also emits and does not check. `InternalsVisibleTo` would make the same marker accessible, and that path is not covered.
- Suggestion: If the scope that will be emitted (`handlerScope ?? messageScope`) is not accessible, report TWM007 and do not add the request or notification binding. Add a compilation test for an internal assembly-level marker (request and notification) that expects one TWM007, no CS0122, and no handler in the manifest, plus the `InternalsVisibleTo` case that binds.
- Status: open
