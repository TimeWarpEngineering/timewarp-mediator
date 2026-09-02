# Round 1 — merged findings
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
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md:128
- Description: How to validate uses `curl -sSI` (HEAD) on gallery package URLs and Expect claims Gallery HTTP **200**. nuget.org returns `HTTP/1.1 404 Not Found` on HEAD for live pages too (verified for `/14.0.0-beta.1`, `/13.0.0`, and the other three beta ids). A stranger running the smoke would think the gallery publish failed even though GET is 200 and flatcontainer/nupkg prove the cut.
- Suggestion: Switch the smoke to GET status checks (e.g. `curl -sS -o /dev/null -w '%{http_code}\n' …`) or rely on flatcontainer/nupkg GET; keep the “prerelease version” copy check on GET body.
- Source: general
- Disposition notes: Smoke now uses gallery GET `-w '%{http_code}'` plus a GET-body prerelease grep. Orchestrator, same task id, 2026-09-02.

### M2 — Severity: nit — Status: fixed
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md:93
- Description: Results says `HEAD` of `/packages/TimeWarp.Mediator/14.0.0` is **404**, and Expect says Gallery HTTP **404** for TimeWarp.Mediator **14.0.0**. HEAD 404 is uninformative (see M1). Gallery **GET** of `/packages/TimeWarp.Mediator/14.0.0` is HTTP **200** with fallback title `TimeWarp.Mediator 13.0.0`. That is not a stable 14.0.0 nupkg—flatcontainer/nupkg GET for `14.0.0` correctly 404—but the gallery status claim as written is false and could confuse validation.
- Suggestion: Document proof of “no stable 14” as nupkg/flatcontainer 404 (and optionally gallery GET body/title fallback), not gallery HEAD/GET status 404.
- Source: general
- Disposition notes: Results and Expect now prove no stable 14 via nupkg/flatcontainer 404; gallery `/14.0.0` GET documented as 200 fallback to 13.0.0. Orchestrator, same task id, 2026-09-02.

## Duplicates / conflicts

- None. M1 and M2 overlap on HEAD vs GET but address different claims (live-page 200 vs no-stable-14 404). Keep both.
