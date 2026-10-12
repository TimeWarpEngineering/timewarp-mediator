# Round 1 — merged findings
**Date:** 2026-10-13
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-follow-up.md:93
- Description: Claimed notifications do not run on the generated mediator and must stay on `AddMediator()`; the migration matrix says `Publish` runs on the generated Host and Aot profiles. Stray "stream a song".
- Suggestion: List only streams, pre/post processors, exception cascade.
- Source: general
- Disposition notes: Rewrote the bullet to streams / pre-post processors / exception handlers and added that `Publish` runs on the generated mediator.

### M2 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/github-release-v14.0.0-draft.md:74; documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-release.md:80; source/timewarp-mediator/readme.md:512
- Description: `ServiceFactory` was removed, not internalized (per `release-notes-v14.0.0.md`).
- Suggestion: Split removed vs internal.
- Source: general
- Disposition notes: All three now say `ServiceFactory` is removed (resolve through `IServiceProvider`) and the wrappers / `ServiceRegistrar` are internal.

### M3 — Severity: suggestion — Status: fixed
- File: readme.md:72
- Description: Reflection quick start offered only the 13.0.0 install.
- Suggestion: Show 14.0.0 first, 13.0.0 as the non-net10.0 fallback.
- Source: general
- Disposition notes: Added the 14.0.0 install line; 13.0.0 kept as the fallback, matching the package readme.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/readme.md:23
- Description: "does not register `AddGeneratedMediator()`".
- Suggestion: "does not register the generated mediator".
- Source: general
- Disposition notes: Reworded.

## Duplicates / conflicts

- None. M2 collapses the same wording in three files into one finding.
