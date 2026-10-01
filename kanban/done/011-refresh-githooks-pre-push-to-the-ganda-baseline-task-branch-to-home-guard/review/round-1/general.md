# Round 1 — general
**Date:** 2026-10-01
**Scope reviewed:** branch vs master: `.githooks/pre-push.cs`, `.gitignore`, task.md

## Summary

The hook change is additive and matches the ganda baseline: it collects `refs/heads/task/*`
sources whose destination is `refs/heads/master|main` and refuses them before the
HEAD-is-home check, while raw-sha sources (kanban publish) pass. `IsHomeBranchDest` already
exists in the file, so the slicing is safe (both prefixes are checked before slicing).
`.gitignore` gains `.local/` and `.memsearch/.index.pid`, both local tool state. Re-verified:
`ganda repo audit` passes all checks; stdin smoke test refuses task/x→master (exit 1) and
allows raw sha→master (exit 0). No issues found.

## Issues

<!-- none -->
