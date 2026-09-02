# Round 2 — general
**Date:** 2026-09-02
**Scope reviewed:** post-fix task.md How to validate / nuget.org Results (M1, M2)

## Summary

Re-checked Results and How to validate after the M1/M2 kitchen wording fixes. Smoke no longer uses gallery HEAD; it uses GET status (`-w '%{http_code}'`) plus a GET-body prerelease grep, and warns that nuget.org HEAD 404s live pages. Proof of “no stable 14” is flatcontainer/nupkg GET 404, with gallery `/14.0.0` GET correctly documented as a 200 fallback to 13.0.0 rather than a publish verdict. No new defects in the fix delta.

## Issues

### M1 — Severity: nit — Status: fixed
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md (How to validate smoke + Expect)
- Description: Prior smoke used `curl -sSI` (HEAD) and Expect claimed gallery HTTP 200, which nuget.org HEAD never returns for live pages.
- Verification: Smoke commands are GET (`curl -sS -o /dev/null -w '%{http_code}\n'`) for the four beta gallery URLs and 13.0.0; comment notes HEAD 404s; body check is `curl -sS … | grep -F 'This is a prerelease version'`. Expect says Gallery **GET** HTTP **200**. Results also warn not to use `curl -sSI`.
- Status: fixed

### M2 — Severity: nit — Status: fixed
- File: kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/task.md (nuget.org Results + Expect + smoke)
- Description: Prior Results/Expect treated gallery `/14.0.0` status (HEAD/GET 404) as proof of no stable 14; gallery GET is actually 200 fallback to 13.0.0.
- Verification: Results prove no stable 14 via flatcontainer/nupkg GET **404**, and document gallery GET `/14.0.0` as HTTP 200 with fallback title `TimeWarp.Mediator 13.0.0`. Smoke curls the nupkg URL for 404 and comments that gallery `/14.0.0` GET is 200 fallback. Expect matches (nupkg 404; gallery GET 200 fallback, not a 14.0.0 package); flatcontainer Expect still requires no `14.0.0` in `versions`.
- Status: fixed

No new issues.
