# Source-gen rewrite with ISender TScope named pipelines

## Description

Rewrite TimeWarp.Mediator as a source-generated dispatcher. Named pipelines are first-class via `ISender<TScope>` / `IPublisher<TScope>` (marker types, e.g. `ClientPipeline` vs `ServerPipeline`). This epic owns the rewrite. Consumers (TimeWarp.State, TimeWarp.Nuru) switch after M1 exists.

SSOT for design: `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` (supersedes the Composer/Opus/Grok brainstorms and `.agents/collaboration/2026-06-17-source-gen-aot-rewrite/`).

## Children

- **004-001** M1 — generated `Mediator`, analyzer, State golden-file
- **004-002** M2 — `ISender<TScope>` named pipelines (depends on 004-001)

## Requirements

- Handler-first discovery; missing/duplicate handler is a compile error
- Generated `sealed Mediator : IMediator`; reflection dispatch is deleted (staged: coexist, then remove)
- Unscoped `ISender` is the default pipeline; `ISender<TScope>` is a separate generated Sender per marker type
- Default DI for Host/State is MS.DI (scoped behaviors). TimeWarp.ServiceGen (Nuru 444) is the AOT/CLI profile — **not** on this epic's critical path
- TimeWarp.State is the primary consumer shape (ActionSet + nested `Handler`, re-entrant `Send`, `Send(object)` for JS interop)
- Native AOT / trim cleanliness is a gate (no `NoWarn` on IL2026/IL3050)

## Checklist

- [ ] 004-001 (M1) merged to origin-home
- [ ] 004-002 (M2) merged to origin-home
- [ ] Spec header points at this epic (fold-in on 004-001)
- [ ] GitHub issue #52 closed or pointed here
- [ ] Nuru 443 retargeted to consume (not rewrite)
- [ ] TimeWarp.State switch task created only after M1 exists — do not file it now

## Out of scope

- TimeWarp.ServiceGen (Nuru 444)
- TimeWarp.State package switch (file after 004-001)
- Nuru consuming this package (Nuru 443, after M1/M2)
- Call-site interceptors / pruning profiles (after M2)
- Preserving MediatR open-generic combinatorics / assembly scanning as the primary path

## Notes

- Cross-repo: Nuru 443/444, timewarp-state (no switch task yet), https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52
- 004-002 is the client vs server split for TimeWarp.State. Do not ship a single shared `IMediator` and filter inside behaviors.
- M1 proves one generated `Mediator`. M2 emits per-scope dispatch tables. Do not invert that order.
- Old PascalCase `Kanban/ToDo` items (007, 009, 010, 012) are unrelated rename/dotnet9 leftover.

## Session

- Created: 2438044 (2026-08-31)
