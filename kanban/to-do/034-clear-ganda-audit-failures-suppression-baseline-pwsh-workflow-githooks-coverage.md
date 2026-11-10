# Clear ganda audit failures: suppression baseline, pwsh workflow, githooks, coverage

## Description

`ganda repo audit` on master (2026-11-11) reports 4 failures that newer ganda checks added. Master passed every check on 2026-11-09. Two are Errors and block attestation of master: `analyzer-suppressions` and `workflow-pwsh-shell`. Two are Warnings: `memsearch-scaffold` and `ci-code-coverage`. Clear all four in one PR.

## Requirements

### Automatic (`ganda repo audit --fix`)

- `analyzer-suppressions`: `--fix` seeds `.timewarp/suppression-baseline.json` from the current tree. Commit it.
- `memsearch-scaffold`: `--fix` refreshes the outdated `.githooks/post-commit.cs`, `post-merge.cs`, `post-checkout.cs`, `pre-commit.cs`, and `pre-push.cs`. Commit what it changes.

### By hand: `workflow-pwsh-shell` (Error)

- `.github/workflows/workflow.yml`: add a top-level `defaults: run: shell: pwsh`.
- Rewrite every `run:` step that is bash today in pwsh. The audit names the `publish` job step `Release guards (tag == v+Version, on master, green CI)`, which uses `set -euo pipefail`. Check every other `run:` step too: once the top-level default is pwsh, any step without an explicit `shell:` runs under pwsh.
- **The release guards step is the gate before every NuGet publish. Its behavior must not change.** For each guard, the pwsh version must fail (non-zero exit, same `::error::` message) under the same condition as the bash version:
  - `<Version>` cannot be read from `source/Directory.Build.props`
  - the release tag does not equal `v` + Version
  - tag `v<Version>` is missing or does not point at the commit being published (break-glass dispatch must run with `--ref v<Version>`)
  - the commit is not on master
  - there is no successful push CI run of `workflow.yml` for that commit
- Fail fast like `set -e`: set `$ErrorActionPreference = 'Stop'` and check `$LASTEXITCODE` after every native command (`git`, `gh`). pwsh does not stop on a non-zero native exit by default.
- Keep every step output (`$GITHUB_OUTPUT`) and env var name the later steps read.
- Run the converted guard logic locally with pwsh against real inputs: one passing case (tag `v15.0.0-beta.6` at `39f9e22`) and at least one failing case per guard. Record the commands and results under `## Results`.

### By hand: `ci-code-coverage` (Warning)

- Follow the audit details: set `"test": { "runner": "Microsoft.Testing.Platform" }` in `global.json` if `dotnet test` runs the suite. Add `--results-directory artifacts/coverage --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml` to the test invocation (the dev CLI `test` command if CI goes through it). Add an `actions/upload-artifact` step after the test step for `artifacts/coverage`, pinned to a commit SHA like the other actions in `workflow.yml`.
- If switching the runner breaks the test projects and the fix gets large, stop. Set `ci-code-coverage.severity = off` under `[ganda.audit]` in `.editorconfig` with a reason and this task id, and record why under `## Notes`.

### Gates

- `ganda repo audit` is clean.
- `./bin/dev workflow` is green.
- `<Version>` stays `15.0.0-beta.6`. No release.

## Checklist

- [ ] `ganda repo audit --fix` applied and committed (suppression baseline, githooks)
- [ ] `workflow.yml` has a top-level pwsh default and no bash `run:` steps
- [ ] Release guards converted, every guard tested locally for pass and fail, results recorded
- [ ] Coverage collected and uploaded in CI (or check turned off with a reason)
- [ ] `ganda repo audit` clean
- [ ] `./bin/dev workflow` green
- [ ] PR opened

## Review

Reviewer: compare the bash and pwsh release guards line by line. For each guard, confirm the failure condition, exit code, and `::error::` text match. A guard that fails open, meaning it passes when it should refuse, blocks this PR.

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-11)

## Notes

- Audit output from master `744c313`, 2026-11-11.
- The release guards can only be fully exercised by a real release run. The next release (beta.7) is the end-to-end check.
