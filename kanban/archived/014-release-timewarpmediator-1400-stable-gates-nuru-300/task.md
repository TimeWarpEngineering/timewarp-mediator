# Release TimeWarp.Mediator 14.0.0 stable (gates Nuru 3.0.0)

## Description

Ship stable `TimeWarp.Mediator` `14.0.0` (Contracts, Generators, Analyzers, runtime) so TimeWarp.Nuru `3.0.0`
does not carry a prerelease dependency. A stable Nuru package that references `14.0.0-beta.4` raises NU5104 at
pack time and forces every Nuru consumer onto a Mediator beta. The maintainer chose to ship Mediator 14.0.0 first,
after dogfooding the current beta across its TimeWarp consumers (timewarp-nuru kanban 489, decision 2026-10-12).

Current state: newest release `v14.0.0-beta.4` (2026-09-27). The repository review (013) filed 22 child tasks;
013-004 (public API surface) is explicitly a before-stable gate.

## Depends on

- 013

## Dogfood status (consumers of 14.0.0-beta.4, checked 2026-10-12 on origin master)

| Repo | Reference | Version |
|------|-----------|---------|
| timewarp-nuru | `TimeWarp.Mediator.Contracts`, `.Generators` (direct) | 14.0.0-beta.4 |
| timewarp-state | `.Contracts`, `.Generators`, `.Analyzers` (direct) | 14.0.0-beta.4 |
| timewarp-architecture | `.Contracts`, `.Generators`, `.Analyzers` (direct) | 14.0.0-beta.4 |
| timewarp-intel | via `TimeWarp.State` 12.0.0-beta.10 | 14.0.0-beta.4 |
| amuru, bayline, flow, ganda, session-lake-tools, taratibu, terminal, mediator (dev CLI) | via `TimeWarp.Nuru` 3.0.0-beta.79 | 14.0.0-beta.4 |

Every consumer is already on beta.4, so no per-repo dogfood bump task was needed. Breakages found while
dogfooding are filed here or as 013 children (known: timewarp-mediator#64, internal request types fail with CS0051).

## Requirements

- The before-stable 013 children are merged or explicitly deferred past 14.0.0 (at least 013-004 public API).
- No open dogfood breakage that would change the 14.0.0 public contract types Nuru 3.0 exposes
  (`IMessage`, `IQuery<T>`, `ICommand<T>`, `IIdempotentCommand<T>`, `IIdempotent`, handler interfaces, `Unit`).
- Release cut via the convention (`tw-release`): version bump PR to `14.0.0`, green CI, `dev release` from synced master.
- After it ships, tell timewarp-nuru 489 so Nuru bumps `Directory.Packages.props` to `14.0.0`.

## Checklist

- [ ] Decide which 013 children gate 14.0.0 and record the list here
- [ ] Gating 013 children merged
- [ ] Dogfood breakages triaged (none blocking the contract types)
- [ ] Version bump PR to `14.0.0` merged, CI green
- [ ] `dev release` → `v14.0.0` on nuget.org (Contracts, Generators, Analyzers, runtime)
- [ ] timewarp-nuru 489 notified (Nuru bumps to 14.0.0)

## Notes

- Upstream gate for: timewarp-nuru kanban 489 (Mediator prerelease dependency) → Nuru 3.0.0.
- Releases: https://github.com/TimeWarpEngineering/timewarp-mediator/releases

## Session

- Created: claude (timewarp-nuru 489 implementer) (2026-10-12)
