# Round 2 — general
**Date:** 2026-11-11
**Scope reviewed:** fix delta `01939c1..ec6e59a`. The only product file is `.github/workflows/workflow.yml` (three operator changes in the publish release-guard step). The kitchen updates to `task.md` (goal restatement, Results guard table, Review disposition, Learnings) were also read.

## Summary

The fix pass changes three operators in the release-guard `run:` body. The version read now uses `-cmatch` (line 159). The release-tag comparison (line 172) and the tag-SHA comparison (line 183) now use `-cne`. Together these give back the case-sensitivity of the bash `sed` and `!=` they replaced. That closes round-1 M1 (bug) and M2 (suggestion). Nothing else in the product tree changed. The only `-ne` and `-eq` operators left in the step compare integers (`$LASTEXITCODE`, `$tagExit`), where case does not apply. No new defects.

Checks run by the reviewer:

- `git diff 01939c1..HEAD -- . ':!kanban'`: only the three operator lines in `workflow.yml` changed.
- Ran the operators under pwsh. With `$expected = 'v15.0.0-beta.6'`, `V15.0.0-BETA.6` and `v15.0.0-BETA.6` give `-cne` = True (refuse), and the exact tag gives False (pass). `'<version>1</version>' -cmatch '<Version>(.*)</Version>'` is False. `'    <Version>15.0.0-beta.6</Version>'` matches and captures `15.0.0-beta.6`.
- `## Results` has a new `Tag case` row (`RELEASE_TAG=V15.0.0-BETA.6` → exit 1 with the mismatch `::error::`), as round-1 M1 asked. The fix-pass paragraph records the mis-cased tag and lowercase `<version>` runs.
- The restatement now requires the comparisons to be case-sensitive. That follows from the task's "its behavior must not change" for the guard step, so it is not a misread. The earlier coverage-route misread is already under `## Learnings`.

## Issues

<!-- None. -->
