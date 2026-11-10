# Round 1 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: .github/workflows/workflow.yml:172
- Description: The release-tag guard uses PowerShell `-ne`, which ignores case, where bash `!=` was case-sensitive. A release for tag `V15.0.0-beta.6` or `v15.0.0-BETA.6` passes this guard instead of being refused with `Release tag '…' does not match v+Version '…'.` This guard fails open, and the task's `## Review` section says that blocks the PR. Reproduced with `RELEASE_TAG=V15.0.0-BETA.6`.
- Suggestion: `$releaseTag -cne $expected`. Also use `-cne` for `$tagSha -ne $sha` (line 183) for consistency. Add a mis-cased `RELEASE_TAG` failing case to the `## Results` guard table.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: .github/workflows/workflow.yml:159
- Description: `-match '<Version>(.*)</Version>'` ignores case, and the `sed` it replaces did not. pwsh would also accept a `<version>` element that bash did not read. No guard fails open today.
- Suggestion: `-cmatch`.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. There was one reviewer.

## Verified without findings

- The other `run:` bodies are valid under the pwsh default. pwsh 7.6 passes the `--` in `dotnet run --file … -- workflow` through to the native command.
- Guards for unreadable version, missing or moved tag, not on master, and no green CI refuse under the same conditions as bash. They print the same `::error::` text and exit non-zero. The green-CI guard is stricter than bash: empty, `null`, or non-numeric `gh` output now refuses, where bash fell through.
- When `git` or `gh` itself fails, the step exits 1 instead of the native exit code, because of how pwsh `-Command` dot-sourcing exits. The exit is still non-zero, so this is not a finding.
- Coverage: `dev test` clears `artifacts/coverage`, collects coverlet Cobertura, and fails when no `coverage.cobertura.xml` is written. The CI upload is SHA-pinned like the other actions.
- `.timewarp/suppression-baseline.json` and `.githooks/*.cs` were written by `audit --fix` and are policy. The baseline records existing suppressions and adds no new ones.
- `ganda repo audit`: every check the task names passes. Only the host-owned `kanban-landed` check fails, and it fails because of the live kitchen state.
