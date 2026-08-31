# Round 1 — general
**Date:** 2026-09-01
**Scope reviewed:** commit 5f94fb5 vs origin/master (Contracts ISender/IPublisher TScope, MediatorScope, analyzer TWM003/TWM004, generator per-scope emit, scoped tests, NamedPipelines sample, spec §9.1, docs)

## Summary

This change ships marker-type named pipelines: `ISender<TScope>` / `IPublisher<TScope>` with per-scope generated `Sender_{TScope}` / `Publisher_{TScope}` dispatch tables, `[MediatorScope]` membership (closest type then assembly), scoped `MediatorBehavior.Scope`, and independent `AddGeneratedMediator` / `AddGeneratedMediator<TScope>` MS.DI registration. Risk is low for the M2 surface: emitted client/server tables are disjoint from each other and from unscoped `Mediator`, behaviors are filtered at close time, TWM003/TWM004 and `NoHandlerException` cover wrong-scope, and smoke tests plus the NamedPipelines sample pass. Out-of-scope work (State switch, interceptors, streams) was not started.
