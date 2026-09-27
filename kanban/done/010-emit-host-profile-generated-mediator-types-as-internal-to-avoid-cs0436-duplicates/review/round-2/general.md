# Round 2 — general
**Date:** 2026-09-26
**Scope reviewed:** round-1 fix delta (readme.md, documentation/generated-vs-legacy.md)

## Summary

M1 is fixed: both the readme visibility paragraph and the beta.4 changelog now state that
`InternalsVisibleTo` re-exposes the generated types. The delta is docs-only; no new defects.
`./bin/dev workflow` re-run after the fix: `Pipeline SUCCEEDED` (generators 34/34, analyzers 6/6,
compilation 7/7, runtime 165 passed / 2 skipped pre-existing, beta.4 packages packed and layout verified).

## Issues

<!-- None. -->
