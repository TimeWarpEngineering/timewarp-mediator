# Round 2 — merged findings
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
- Description: Live docs still pointed at PascalCase sample folders and `Analysis/` after this slice kebab-renamed those trees.
- Suggestion: Point those live doc paths at the kebab folders.
- Source: general (round 1)
- Disposition notes: Re-verified. Both files use `analysis/2026-06-17-source-gen-aot-rewrite-spec.md`, `samples/timewarp-mediator-examples-aot`, and `samples/timewarp-mediator-examples-named-pipelines`. Those paths exist. `documentation/` has no remaining `samples/TimeWarp.Mediator.Examples.*` or `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` references. Fix delta is four path strings; no new defects.

## Duplicates / conflicts

- None. No new findings. Prior M# ID carried forward as fixed.
