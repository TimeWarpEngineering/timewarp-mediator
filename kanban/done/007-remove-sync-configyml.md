# Remove sync-config.yml

## Description

Stop template sync noise by removing sync-config.yml (and related sync workflow/config copies if present). These files drive "Sync configurable files from parent repository" PRs.

## Requirements

- Delete `.github/sync-config.yml`
- Remove or disable any related sync-configurable-files workflow if present
- Close open sync PRs after merge if appropriate
- Do not reintroduce sync-config

## Checklist

- [x] Find all sync-config copies
- [x] Delete them
- [x] Remove related workflow if any
- [x] Commit
- [x] Verify no new sync PRs
- [x] Close stale sync PRs

## Notes

Created to stop recurring template-sync PRs from an active `.github/sync-config.yml`.

## Results

Cleanup PR left open (draft, not merged): https://github.com/TimeWarpEngineering/timewarp-mediator/pull/66

### What was deleted

On this branch:

- `.github/sync-config.yml` — last remaining parent-template sync driver (`branch_prefix: sync-configurable-files`, title `Sync configurable files from parent repository`)

Already absent on `master` (task 002; confirmed not reintroduced):

- `.github/workflows/sync-configurable-files.yml`
- `.github/workflows/sync-configurable-files.md`
- `.github/scripts/sync-configurable-files.ps1`
- `workflows-temp/`, `copy-workflows.ps1`

Repo-wide search after the delete found no remaining `sync-config.yml` or `sync-configurable-files` script/workflow in the tree (historical mentions remain only in archived/done kanban notes).

### Stale sync PRs

No open PRs titled "Sync configurable files from parent repository" (or equivalent deploy-sync titles) were open at cleanup time. None were closed in this session because they were already closed and unmerged.

Already-closed historical sync PRs (left closed; not merged):

- Sync configurable files: #47, #46, #45, #44, #43, #42, #41, #40, #39, #38, #37, #35, #32, #27, #23, #21, #19
- Deploy sync workflow: #18, #16, #15, #10, #8, #3, #1

### Leftover remote branches (coordinator deletes AFTER merge)

Do not delete from this task. Names as of 2026-09-21:

`sync-configurable-files-*`:

- `sync-configurable-files-1751637643`
- `sync-configurable-files-1751647459`
- `sync-configurable-files-1751647863`
- `sync-configurable-files-1751706475`
- `sync-configurable-files-1751792876`
- `sync-configurable-files-1751879540`
- `sync-configurable-files-1751965894`
- `sync-configurable-files-1752052296`
- `sync-configurable-files-1752138738`
- `sync-configurable-files-1752225045`
- `sync-configurable-files-1752311299`
- `sync-configurable-files-1752397758`
- `sync-configurable-files-1752484566`
- `sync-configurable-files-1752571091`
- `sync-configurable-files-1752657256`
- `sync-configurable-files-1752743616`
- `sync-configurable-files-1752830033`

`sync-deployment-*`:

- `sync-deployment-20250704-103921`
- `sync-deployment-20250704-103924`
- `sync-deployment-20250704-103928`
- `sync-deployment-20250704-103932`
- `sync-deployment-20250704-103936`
- `sync-deployment-20250704-103943`
- `sync-deployment-20250704-115836`
- `sync-deployment-20250704-115844`
- `sync-deployment-20250704-201016`
- `sync-deployment-20250704-205642`
