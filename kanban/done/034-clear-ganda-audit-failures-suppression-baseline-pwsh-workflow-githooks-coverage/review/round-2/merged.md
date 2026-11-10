# Round 2 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:172
- Description: The release-tag guard used case-insensitive `-ne`, so a mis-cased tag such as `V15.0.0-BETA.6` got past it (round 1).
- Suggestion: `-cne`.
- Source: general (round 1)
- Disposition notes: Fixed in `6842d32`. Line 172 is `$releaseTag -cne $expected`, and line 183 is `$tagSha -cne $sha`. Re-verified under pwsh: mis-cased tags refuse and the exact tag passes. `## Results` records the `V15.0.0-BETA.6` failing case.

### M2 — Severity: suggestion — Status: fixed
- File: .github/workflows/workflow.yml:159
- Description: The version read used case-insensitive `-match`, unlike the bash `sed` it replaced (round 1).
- Suggestion: `-cmatch`.
- Source: general (round 1)
- Disposition notes: Fixed in `6842d32`. Re-verified under pwsh: a lowercase `<version>` line does not match, and `<Version>15.0.0-beta.6</Version>` captures `15.0.0-beta.6`.

## New findings

None.

## Duplicates / conflicts

- None. There was one reviewer.
