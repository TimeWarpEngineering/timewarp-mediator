# Round 1 — general
**Date:** 2026-10-13
**Scope reviewed:** branch vs master, all 17 files (readmes, release draft, posts, card, task.md)

## Summary

Docs-only launch pack. The benchmark figures, diagnostic ids/severities, license expressions,
Aot/Link limits, card size, and blip lengths all match the repository. The NuGet readmes use
absolute links and the release draft is clearly marked unpublished. One factual error in the
follow-up blog (notifications) would mislead readers; the rest are accuracy and consistency
polish.

## Issues

### Issue 1 — Severity: bug
- File: documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-follow-up.md:93
- Description: Says the generated snippet "does not publish notifications" and that notifications "stay on `AddMediator()`". The migration matrix says the generated Host and Aot profiles both dispatch `Publish` (sequentially). "stream a song" is also a stray phrase.
- Suggestion: List only what generated dispatch actually drops: streams, pre/post processors, the exception cascade.
- Status: open

### Issue 2 — Severity: suggestion
- File: documentation/release-notes/github-release-v14.0.0-draft.md:74 (also blog release draft:80, source/timewarp-mediator/readme.md:512)
- Description: Groups `ServiceFactory` with types that "went internal". `release-notes-v14.0.0.md` says `ServiceFactory` was removed (remedy: resolve through `IServiceProvider`); only the wrappers and `ServiceRegistrar` became internal.
- Suggestion: Say `ServiceFactory` was removed and the wrappers / `ServiceRegistrar` are internal.
- Status: open

### Issue 3 — Severity: suggestion
- File: readme.md:72
- Description: The reflection quick start now shows only the 13.0.0 install and frames it as the net10.0-fallback, so a net10.0 reader of the GitHub readme gets no 14.0.0 reflection install line. `source/timewarp-mediator/readme.md` shows 14.0.0 first, then 13.0.0 as the fallback.
- Suggestion: Add the 14.0.0 install line, keep 13.0.0 as the fallback, matching the package readme.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-mediator/readme.md:23
- Description: "It does not register `AddGeneratedMediator()`" — a method is not registered; the intended object is the generated mediator.
- Suggestion: "It does not register the generated mediator."
- Status: open
