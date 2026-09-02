# Round 1 — merged findings
**Date:** 2026-09-02
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: documentation/m1-generated-mediator.md:3,12; documentation/m2-named-pipelines.md:3,12
- Description: This commit kebab-renamed samples and `Analysis/`→`analysis/`, and updated the adjacent benchmarks path to `tests/timewarp-mediator-benchmarks-comparison`, but left the AOT sample as `samples/TimeWarp.Mediator.Examples.Aot` (missing on disk; real path `samples/timewarp-mediator-examples-aot`) and the Design SSOT as `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` (missing; real path `analysis/...`). Same pattern in `documentation/m2-named-pipelines.md:3` (`Analysis/...`) and `:12` (`samples/TimeWarp.Mediator.Examples.NamedPipelines` vs `samples/timewarp-mediator-examples-named-pipelines`). Pre-existing wrong `ci-cd.yml`/MinVer prose in `documentation/overview.md` was not re-litigated.
- Suggestion: Point those live doc paths at the kebab folders (`samples/timewarp-mediator-examples-aot`, `samples/timewarp-mediator-examples-named-pipelines`, `analysis/2026-06-17-source-gen-aot-rewrite-spec.md`).
- Source: general
- Disposition notes: Fixed on this task id. Live docs now point at `analysis/2026-06-17-source-gen-aot-rewrite-spec.md`, `samples/timewarp-mediator-examples-aot`, and `samples/timewarp-mediator-examples-named-pipelines`. Historical kanban done/archived snapshots left as written.

## Duplicates / conflicts

- None (single reviewer).
