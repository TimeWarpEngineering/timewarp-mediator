# Round 2 — merged findings
**Date:** 2026-09-02
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md (How to validate smoke + Expect)
- Description: How to validate used `curl -sSI` (HEAD) on gallery package URLs and Expect claimed Gallery HTTP **200**. nuget.org returns HEAD 404 for live pages too.
- Suggestion: Switch the smoke to GET status checks; keep the “prerelease version” copy check on GET body.
- Source: general
- Disposition notes: Verified in round 2. Smoke is GET `-w '%{http_code}'` plus GET-body prerelease grep; Results warn not to use `curl -sSI`.

### M2 — Severity: nit — Status: fixed
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md (nuget.org Results + Expect + smoke)
- Description: Results/Expect treated gallery `/14.0.0` status 404 as proof of no stable 14. Gallery GET is 200 fallback to 13.0.0; real proof is nupkg/flatcontainer 404.
- Suggestion: Document proof of “no stable 14” as nupkg/flatcontainer 404; note gallery GET fallback.
- Source: general
- Disposition notes: Verified in round 2. Results, smoke, and Expect use nupkg GET 404; gallery `/14.0.0` GET documented as 200 fallback to 13.0.0.

## Duplicates / conflicts

- None. No new findings. Prior M1/M2 carried with updated status.

## Resolved prior

- M1, M2 fixed on this task id (kitchen How to validate / Results only). No product re-cut.
