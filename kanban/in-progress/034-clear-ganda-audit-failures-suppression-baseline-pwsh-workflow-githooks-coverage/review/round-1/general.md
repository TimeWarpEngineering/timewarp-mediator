# Round 1 — general
**Date:** 2026-11-11
**Scope reviewed:** `git diff master...HEAD`: `.github/workflows/workflow.yml`, `tools/dev-cli/endpoints/test-command.cs`, `Directory.Packages.props`, three test `.csproj` files, `.timewarp/suppression-baseline.json`, and `.githooks/*.cs`. The baseline and hooks were written by `audit --fix` and are policy.

## Summary

The change clears the four named audit failures. It adds a top-level `defaults: run: shell: pwsh`, rewrites the publish job's release guards in pwsh, adds `coverlet.collector` and `--collect "XPlat Code Coverage"` to `dev test` with a CI upload of `artifacts/coverage`, and commits the `audit --fix` baseline and hook refresh. The other `run:` bodies (`echo`, `exit 1`, `dotnet run … -- workflow`) are valid pwsh. pwsh 7.6 passes `--` through to native commands, which was checked locally. I compared the guards line by line. Four of the five match bash, and the green-CI guard is stricter than bash. bash `[ "" -lt 1 ]` errors inside `if` and falls through, while pwsh `[int]` fails closed on empty, `null`, or non-numeric output. The release-tag guard fails open on case: PowerShell `-ne` ignores case and bash `!=` does not.

Checks run by the reviewer:

- I pulled the release-guard body out of `workflow.yml` and parsed it with the PowerShell parser: 0 errors.
- I ran the guard under pwsh on this branch's HEAD (`01939c1`). It exits 1 with the tag-points-elsewhere `::error::`.
- I ran the green-CI guard against a fake `gh` that printed `""`, `0`, `null`, `abc`, and `2`, then against a `gh` that exits 4. All refuse except `2`. The failing `gh` makes the step exit 1, not 4. The exit is still non-zero, and the task only requires non-zero.
- `RELEASE_TAG=V15.0.0-BETA.6` got past the release-tag guard, and the run went on to the next guard. bash refuses it with `Release tag 'V15.0.0-BETA.6' does not match v+Version 'v15.0.0-beta.6'.`
- `ganda repo audit` on this branch: only `kanban-landed` fails, because the live kitchen differs from origin-home. The host land node owns that. Every check this task names passes.

## Issues

### Issue 1 — Severity: bug
- File: .github/workflows/workflow.yml:172
- Description: The release-tag guard compares `$releaseTag -ne $expected`. PowerShell `-ne` ignores case, but the bash original `[ "$RELEASE_TAG" != "$expected" ]` is case-sensitive. A release published for tag `V15.0.0-beta.6` or `v15.0.0-BETA.6` now passes this guard where bash refused it. Git refs are case-sensitive, so that tag is a different tag from `v15.0.0-beta.6`. This is a guard that fails open, which the task's `## Review` section says blocks the PR, and the task says the guard's behavior must not change. The next guard still requires the canonical `v<Version>` tag at HEAD, which limits the damage. The guard still no longer refuses the condition it is meant to refuse. I reproduced this locally: with `RELEASE_TAG=V15.0.0-BETA.6`, the run printed the tag-points-elsewhere error, not the release-tag mismatch.
- Suggestion: Use `-cne` (`$releaseTag -cne $expected`). Add a mis-cased `RELEASE_TAG` case to the `## Results` guard table. Use `-cne` in the `$tagSha -ne $sha` comparison too. git prints lowercase hex, so that one is harmless today, but it should be consistent.
- Status: open

### Issue 2 — Severity: suggestion
- File: .github/workflows/workflow.yml:159
- Description: The version read uses `-match '<Version>(.*)</Version>'`, which ignores case. The bash `sed` it replaces was case-sensitive, so pwsh would also read a `<version>` element that bash skipped. This does not fail open today, because `source/Directory.Build.props` has exactly one `<Version>` and the tag guards still check the value it reads. The task still asks for the same conditions as bash.
- Suggestion: Use `-cmatch`.
- Status: open
