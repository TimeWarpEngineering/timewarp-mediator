# Round 2 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 3 | 2 |

Fixed: M1 (suggestion), M2, M3, M5 (nits). Wontfix: M4, M6 (nits). No new findings in round 2. This is the final ledger.

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: agent.md:79
- Description: Key Dependencies named the removed Abstractions pin.
- Source: general (round 1)
- Disposition notes: fixed in 04bdae6. Names the full `Microsoft.Extensions.DependencyInjection` pin (`Directory.Packages.props:36`) and the Host-profile Abstractions requirement (TWM008). Round 2 verified this.

### M2 — Severity: nit — Status: fixed
- File: readme.md:16
- Description: The Contracts row said "Needed by both stacks."
- Source: general (round 1)
- Disposition notes: fixed in 04bdae6. The row now says "Needed by every host and message library." Round 2 verified this.

### M3 — Severity: nit — Status: fixed
- File: .githooks/Directory.Build.props:10
- Description: A dead `<SignAssembly>false</SignAssembly>` override.
- Source: general (round 1)
- Disposition notes: fixed in 04bdae6. The line is deleted, and the tree-wide grep is empty. Round 2 verified this.

### M4 — Severity: nit — Status: wontfix
- File: agent.md:57 (and package readmes, migration.md, diagnostics.md, skills/tw-mediator/SKILL.md)
- Description: The TWM009 and stream text describes types that no shipped package contains.
- Source: general (round 1)
- Disposition notes: wontfix. Deferred to 021-005 (deletes TWM009) and 021-007 (rewrites these pages). This is the round-1 decision of the review oracle, unchanged.

### M5 — Severity: nit — Status: fixed
- File: documentation/release-notes/release-notes-v14.0.0.md:16, :62; documentation/release-notes/github-release-v14.0.0-draft.md:97
- Description: Links pointed at the deleted `generated-vs-legacy.md`.
- Source: general (round 1)
- Disposition notes: fixed in 04bdae6. All three use `https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/documentation/generated-vs-legacy.md`, and tag `v14.0.0` contains the file. Round 2 verified this.

### M6 — Severity: nit — Status: wontfix
- File: documentation/benchmarks/comparison-baseline.md:5
- Description: The Ratio column is still relative to the removed legacy row.
- Source: general (round 1)
- Disposition notes: wontfix. `BenchmarkGate` reads only Mean and Allocated, and the stale ratio is disclosed. Re-recording needs a quiet machine, and 021-007 links the re-recorded baseline. This is the round-1 decision of the review oracle, unchanged.

## Duplicates / conflicts

- None. Single reviewer, no new findings in round 2.
