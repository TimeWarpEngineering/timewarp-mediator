# Round 2 — general
**Date:** 2026-10-24
**Scope reviewed:** fix-pass delta `8fe815f..e1d0eb4` (round-1 `base.sha` to HEAD): `04bdae6` (product prose plus one MSBuild line) and `e1d0eb4` (kanban). Five product files, 7 insertions and 7 deletions.

## Summary

The fix pass closes all four open round-1 findings. Each change is the smallest edit that does the job, and each one matches the round-1 suggestion. `agent.md` names the full DI pin that `Directory.Packages.props:36` still carries. It also points Host-profile consumers at Abstractions, which matches TWM008 in `documentation/diagnostics.md:77-83`. The dead `SignAssembly` override is gone from `.githooks/Directory.Build.props`. `git grep SignAssembly` outside `kanban/` and `documentation/archive` now prints nothing. The three release-note links point at the `v14.0.0` blob, and `git cat-file -e v14.0.0:documentation/generated-vs-legacy.md` confirms that the file exists at that tag. Nothing still links to the deleted page with a relative link. `github-release-v14.0.0-draft.md:108` still names `documentation/generated-vs-legacy.md`, but as historical inline-code text about the 14.0.0 bump, not as a link, so it is fine. The `## Goal (implementer restatement)` now covers the DI requirement and the friend-assembly narrowing, and it matches the task Requirements. The fix pass introduces no new defects.

## Issues

None.

## Prior findings re-verified

| ID | Round-1 status | Round-2 status | Evidence |
|----|----------------|----------------|----------|
| M1 | open | fixed | `agent.md:79` names the full `Microsoft.Extensions.DependencyInjection` pin and the TWM008 Abstractions note |
| M2 | open | fixed | `readme.md:16` says "Needed by every host and message library."; `both stacks` greps empty |
| M3 | open | fixed | `.githooks/Directory.Build.props` line removed; tree-wide `SignAssembly` grep is empty |
| M4 | wontfix | wontfix | Deferred to 021-005 / 021-007; unchanged |
| M5 | open | fixed | `release-notes-v14.0.0.md:16,62` and `github-release-v14.0.0-draft.md:97` use the `blob/v14.0.0` URL; the tag holds the file |
| M6 | wontfix | wontfix | Deferred re-record (021-007); unchanged |
