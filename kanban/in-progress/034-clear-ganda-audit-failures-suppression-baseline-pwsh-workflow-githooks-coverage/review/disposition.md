# Disposition — task 034

**Date:** 2026-11-11
**Outcome:** clean
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 (effort 2, general) found one bug and one suggestion. M1: the pwsh release-tag guard used case-insensitive `-ne`, so it would pass a mis-cased tag that bash refused. M2: the version read used case-insensitive `-match`. The fix pass (`6842d32`) switched to `-cne` and `-cmatch` and recorded a mis-cased-tag failing case in `## Results`. Round 2 (effort 1, general) re-verified both fixes under pwsh and found nothing new. Both findings are `fixed`.

## Exception log (if accepted-exceptions)

None.

## Escalations

- None.
