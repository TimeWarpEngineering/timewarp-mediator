# Round 1 — general
**Date:** 2026-09-02
**Scope reviewed:** branch `task/005-003-publish-1400-beta1-to-nuget-prerelease` vs `origin/master` (docs + kitchen Results); external cut at tag `v14.0.0-beta.1` → `40a9841270ab14d6694c0373bf6d83bb3dde9d6e`

## Summary

The prerelease cut succeeded: annotated tag `v14.0.0-beta.1` peels to origin/master `40a9841`, GitHub Release is Pre-release with Latest still `v13.0.0`, release workflow `33640437708` pushed all four package ids, and nuget.org flatcontainer/nupkg GET confirm `14.0.0-beta.1` without a stable `14.0.0` nupkg or unlisted `13.0.0`. Branch docs correctly record that nuget.org now serves the beta as prerelease. Residual risk is only kitchen smoke wording that can mislead a stranger; it does not hide a failed publish.

## Issues

### Issue 1 — Severity: nit
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md:128
- Description: How to validate uses `curl -sSI` (HEAD) on gallery package URLs and Expect claims Gallery HTTP **200**. nuget.org returns `HTTP/1.1 404 Not Found` on HEAD for live pages too (verified for `/14.0.0-beta.1`, `/13.0.0`, and the other three beta ids). A stranger running the smoke would think the gallery publish failed even though GET is 200 and flatcontainer/nupkg prove the cut.
- Suggestion: Switch the smoke to GET status checks (e.g. `curl -sS -o /dev/null -w '%{http_code}\n' …`) or rely on flatcontainer/nupkg GET; keep the “prerelease version” copy check on GET body.
- Status: open

### Issue 2 — Severity: nit
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md:93
- Description: Results says ``HEAD`` of `/packages/TimeWarp.Mediator/14.0.0` is **404**, and Expect says Gallery HTTP **404** for TimeWarp.Mediator **14.0.0**. HEAD 404 is uninformative (see Issue 1). Gallery **GET** of `/packages/TimeWarp.Mediator/14.0.0` is HTTP **200** with fallback title `TimeWarp.Mediator 13.0.0` and copy “The specified version 14.0.0 was not found. You have been taken to version 13.0.0.” That is not a stable 14.0.0 nupkg—flatcontainer/nupkg GET for `14.0.0` correctly 404—but the gallery status claim as written is false and could confuse validation.
- Suggestion: Document proof of “no stable 14” as nupkg/flatcontainer 404 (and optionally gallery GET body/title fallback), not gallery HEAD/GET status 404.
- Status: open
