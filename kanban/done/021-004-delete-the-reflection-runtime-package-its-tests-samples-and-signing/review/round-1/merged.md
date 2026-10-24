# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 3 | 0 | 2 |

Open for the fix pass: M1, M2, M3, M5.

## Issues

### M1 — Severity: suggestion — Status: open
- File: agent.md:79
- Description: Key Dependencies names a `Microsoft.Extensions.DependencyInjection.Abstractions` pin in `Directory.Packages.props`. This branch removed that pin.
- Suggestion: Name the full `Microsoft.Extensions.DependencyInjection` pin (test, benchmark, sample, and smoke hosts). Note that a Host-profile consumer needs Abstractions (TWM008).
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: readme.md:16
- Description: The Contracts row still says "Needed by both stacks."
- Suggestion: "Needed by every host and message library." or drop the phrase.
- Source: general
- Disposition notes:

### M3 — Severity: nit — Status: open
- File: .githooks/Directory.Build.props:10
- Description: A dead `<SignAssembly>false</SignAssembly>` override survives. The acceptance list says `SignAssembly` is gone.
- Suggestion: Delete the line.
- Source: general
- Disposition notes:

### M4 — Severity: nit — Status: wontfix
- File: agent.md:57 (also the Contracts and Generators readmes, migration.md:18-20, diagnostics.md:85-91, skills/tw-mediator/SKILL.md)
- Description: The TWM009 and stream text describes types that no shipped package contains, so a consumer cannot get the warning it describes.
- Suggestion: Defer to 021-005 (deletes TWM009) and 021-007 (rewrites agent.md, the skill, the package readmes, and migration).
- Source: general
- Disposition notes: wontfix on this id; the review oracle decided this in round 1. 021-005's Requirements delete TWM009, and 021-007's Requirements rewrite every page listed. This is the same deferral as 021-002 M3. Fixing the text here would be rewritten again within the same 15.0 train.

### M5 — Severity: nit — Status: open
- File: documentation/release-notes/release-notes-v14.0.0.md:16, :62; documentation/release-notes/github-release-v14.0.0-draft.md:97
- Description: These links point at the deleted `generated-vs-legacy.md` and break on merge.
- Suggestion: Repoint them to `https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/documentation/generated-vs-legacy.md` (tag v14.0.0 exists).
- Source: general
- Disposition notes:

### M6 — Severity: nit — Status: wontfix
- File: documentation/benchmarks/comparison-baseline.md:5
- Description: The hand-edited baseline's Ratio column is still relative to the removed legacy row. Code moved `Baseline = true` to `TimeWarpGeneratedInterfaceSend`.
- Suggestion: Re-record on a quiet machine.
- Source: general
- Disposition notes: wontfix on this id; the review oracle decided this in round 1. `BenchmarkGate` reads only Mean and Allocated, so the gate is correct. The file and the task Results both disclose the stale ratio. A trustworthy re-record needs a quiet machine and is not part of `workflow`. 021-007 already plans to link the re-recorded baseline.

## Duplicates / conflicts

- None. Single reviewer.
- Not raised: the whitespace-only line at `Directory.Packages.props:37` came from `ganda repo audit --fix`. Audit-fix output is policy, and that line breaks no build or test.
