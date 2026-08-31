# M2 ISender TScope named pipelines

## Description

Named pipelines: `ISender<TScope>` / `IPublisher<TScope>` as separate generated classes. Marker types name the pipeline (`ClientPipeline` vs `ServerPipeline`). Each scope gets its own dispatch table and behavior chain. No runtime "is this my message?" filtering.

Parent: **004**. This is the TimeWarp.State client vs server split.

## Depends on

- 004-001

## Requirements

- `ISender<TScope>` and `IPublisher<TScope>` plus unscoped `ISender` / `IPublisher` (unscoped = default pipeline)
- Generator emits a concrete Sender and Publisher **per** `TScope` with type-switched dispatch
- Handlers and behaviors belong to a scope via membership (`[MediatorModule]`, options, or equivalent — pick one and document it)
- `ISender<ClientPipeline>` never dispatches server handlers; `ISender<ServerPipeline>` never dispatches client handlers
- Behaviors registered for one scope do not run on the other
- Re-entrant `Send` stays in the same scope unless the caller injects a different `ISender<T>`
- MS.DI can resolve `ISender<TScope>` independently (two mediators in one host)

## Checklist

### Design
- [ ] Membership rule: how a handler/behavior is assigned to a scope (do not leave this implicit)
- [ ] Unscoped vs scoped coexistence (what happens if a host only registers the unscoped sender)

### Implementation
- [ ] Interfaces in Contracts
- [ ] Per-scope Sender/Publisher emit
- [ ] Sample: `ClientPipeline` + `ServerPipeline` in one host with disjoint handler sets
- [ ] Tests: wrong-scope send is a compile error or a hard runtime miss (prefer compile error)
- [ ] Re-entrancy within a scope still works (M1 golden-file behaviors)

### Documentation
- [ ] Spec § scoped-sender fold-in
- [ ] Note for the future TimeWarp.State switch task: inject `ISender<ClientPipeline>` on the Blazor client, `ISender<ServerPipeline>` on the server

## Out of scope

- TimeWarp.State package switch (file after 004-001, implement after this task)
- Call-site interceptors / pruning
- TimeWarp.ServiceGen

## Notes

- This is **not** string-named pipelines. `TScope` is a marker type.
- Impossible to retrofit onto martinothamar; this is the product reason to own Mediator.
- Nuru 443 may use scoped senders after this ships; it must not implement them.

## Session

- Created: 2438044 (2026-08-31)
