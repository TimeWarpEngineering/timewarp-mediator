# Round 1 — merged findings
**Date:** 2026-11-19
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: tools/dev-cli/endpoints/workflow-command.cs:19
- Description: The Design comment still says TimeWarp.Nuru.DevCli 4.0.0-beta.15 ships `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` as internal. The pin is now 4.0.0-beta.16. Those three types are still `internal` in the beta.16 package content, so the handler stays internal for the same reason. The version token is the stale fact. The restatement says doc and comment references that name the old Nuru version as a fact stay accurate. `agent.md` was updated. This comment was not.
- Suggestion: Change `4.0.0-beta.15` to `4.0.0-beta.16` in that comment.
- Source: general
- Disposition notes: Fixed in fix pass 2 (2026-11-19). The Design comment now names 4.0.0-beta.16.

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
