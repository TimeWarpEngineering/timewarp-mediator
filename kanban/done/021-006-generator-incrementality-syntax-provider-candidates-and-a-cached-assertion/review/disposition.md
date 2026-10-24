# Disposition — task 021-006

**Date:** 2026-10-24
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 raised 12 findings, M1–M12: 6 bugs, 1 suggestion, and 5 nits. The fix pass `a1979f8` fixed all of them, and round 2 re-verified each one against the symbol walk in `MessageGraphBuilder` and against new compilation tests. Round 2 raised two new cosmetic nits. M13 is a dead `HostAssemblyName` option, and M14 is an unused helper parameter. Both are accepted as wontfix. The incrementality goal holds: the model step is `Cached` on an unrelated edit and `Modified` on a new handler. Build, test, and pack are green, including the package smoke checks for host membership and the host behavior weave.

Final counts: bug 6 fixed; suggestion 1 fixed; nit 5 fixed and 2 wontfix; 0 open.

## Exception log

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M13 | nit | The analyzer option is dead, but it has no behavior effect and the Design region already documents it. The tests use it. Removing it changes the Analyzers public surface, so it is deferred to later cleanup. | review oracle (round 2) |
| M14 | nit | The private helper has an unused parameter. It has no runtime effect, and the build is clean. | review oracle (round 2) |

## Escalations

- None.

## Follow-up candidates (not blocking)

- `ExtractBehavior`'s `GetAttributes` index can go stale across files during IDE incremental runs. This affects only the same-`Order` tie-break, and command-line builds are unaffected. See `round-2/general.md`.
- M13 and M14 cleanup.
