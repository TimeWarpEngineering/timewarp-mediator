# Clear ganda audit failures: suppression baseline, pwsh workflow, githooks, coverage

## Goal (implementer restatement)

`ganda repo audit` on this tree fails four checks that landed after master last passed. Two errors block attestation: there is no `.timewarp/suppression-baseline.json`, and `.github/workflows/workflow.yml` still runs the publish release-guard step as bash. Two warnings are outdated `.githooks/*.cs` scaffolds and a CI test run that neither collects coverage nor uploads it. `vscode-window-icon` also fails and is in scope because a clean audit is the gate.

Done means:

- `ganda repo audit --fix` has seeded the suppression baseline and refreshed the five git hooks, and those files are committed.
- `workflow.yml` defaults `run:` to `pwsh`, and every `run:` body is valid pwsh. The release-guard step still refuses, with the same `::error::` text and a non-zero exit, when the version cannot be read, the release tag is not `v` + `<Version>`, the tag is missing or does not point at the commit being published, the commit is not on `origin/master`, or that commit has no successful push run of `workflow.yml`. A passing run against tag `v15.0.0-beta.6` at `39f9e22` is recorded, plus one failing case per guard.
- Coverage is collected on the VSTest `dotnet test` invocation (xunit 2.9 cannot switch to Microsoft.Testing.Platform without a large runner migration) and `artifacts/coverage` is uploaded from the CI job. `<Version>` stays `15.0.0-beta.6`.
- `ganda repo audit` passes and `./bin/dev workflow` passes. The pull request is left for the host open-pr node.

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

- [x] `ganda repo audit --fix` applied and committed (suppression baseline, githooks)
- [x] `workflow.yml` has a top-level pwsh default and no bash `run:` steps
- [x] Release guards converted, every guard tested locally for pass and fail, results recorded
- [x] Coverage collected and uploaded in CI (or check turned off with a reason)
- [x] `ganda repo audit` clean
- [x] `./bin/dev workflow` green
- [ ] PR opened

## Review

Reviewer: compare the bash and pwsh release guards line by line. For each guard, confirm the failure condition, exit code, and `::error::` text match. A guard that fails open, meaning it passes when it should refuse, blocks this PR.

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-11)
- Implementation: 2026-11-11. Cleared the four named audit failures and `vscode-window-icon` (`--fix` wrote the gitignored `.vscode/settings.json`). The task branch and origin/master both moved this kitchen to in-progress from `d830944`; this commit folderizes that kitchen.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c691-b89e-7b13-a535-e1a9ab60442b, max-turns 500, Done — 2026-11-10T18:06:28Z

## Notes

- Audit output from master `744c313`, 2026-11-11.
- The release guards can only be fully exercised by a real release run. The next release (beta.7) is the end-to-end check.
- Coverage stays on VSTest. The three suites are xunit 2.9.3 with `Microsoft.NET.Test.Sdk`. Switching `global.json` to `"runner": "Microsoft.Testing.Platform"` does not make those projects host Microsoft.Testing.Platform, and moving them to xunit v3 is a large runner migration. `coverlet.collector` 6.0.4 plus `--collect:"XPlat Code Coverage"` is the collection signal the audit accepts for VSTest. `ci-code-coverage` passes, so the check was not turned off.
- `vscode-window-icon --fix` reported that the Windows-side taskbar icon could not be applied from this machine. The audit check itself passes.

## Results

`ganda repo audit --fix` seeded `.timewarp/suppression-baseline.json` (39 suppressions, none above the baseline) and refreshed `.githooks/post-commit.cs`, `post-merge.cs`, `post-checkout.cs`, `pre-commit.cs`, and `pre-push.cs` to the attest baseline. It also set the top-level `defaults: run: shell: pwsh`. The release-guard body was rewritten by hand. `<Version>` is still `15.0.0-beta.6`.

`dev test` collects Cobertura into `artifacts/coverage` and fails if no `coverage.cobertura.xml` is written. The CI job uploads that directory with `actions/upload-artifact` pinned to `cf430e030ddbb5b0abf93d22962f4752f3646cd9` (v7.0.2), `if: success()`, `if-no-files-found: warn`.

Release guards were the workflow `run:` body, executed with pwsh in a shared clone (`GITHUB_REPOSITORY=TimeWarpEngineering/timewarp-mediator`). The clone's tag and `origin/master` edits were not pushed.

| Case | Setup | Exit | Output |
| --- | --- | --- | --- |
| Pass | detached `39f9e22`, tag `v15.0.0-beta.6`, `RELEASE_TAG` unset, real `gh` | 0 | `Release guards passed (1 successful CI run(s) for 39f9e22e04e3ebc4f66c32e8251b158422558c12).` |
| Version unreadable | `<Version>` element removed from `source/Directory.Build.props` | 1 | `::error::Could not read <Version> from source/Directory.Build.props.` |
| Tag mismatch | `RELEASE_TAG=v0.0.0` at `39f9e22` | 1 | `::error::Release tag 'v0.0.0' does not match v+Version 'v15.0.0-beta.6'.` |
| Tag points elsewhere | detached `origin/master` `f4025a0`, tag still at `39f9e22` | 1 | `::error::Tag v15.0.0-beta.6 must exist and point at the commit being published (f4025a0dee9995f65d9c5663343800ef11fd28f7); found '39f9e22e04e3ebc4f66c32e8251b158422558c12'. Break-glass dispatch must run with --ref v15.0.0-beta.6.` |
| Tag missing | tag deleted, detached `39f9e22` | 1 | `::error::Tag v15.0.0-beta.6 must exist and point at the commit being published (39f9e22e04e3ebc4f66c32e8251b158422558c12); found 'none'. Break-glass dispatch must run with --ref v15.0.0-beta.6.` |
| Not on master | empty commit `6d2d369` tagged `v15.0.0-beta.6`, `origin/master` left at `f4025a0` | 1 | `::error::Commit 6d2d369e1ac299bc18b327619276bd79410bb582 is not on master.` |
| No green CI | same commit, clone `origin/master` moved to `6d2d369`, real `gh api` | 1 | `::error::No successful push CI run of workflow.yml for 6d2d369e1ac299bc18b327619276bd79410bb582. Wait for CI or re-run it, then re-run this release.` |

`ganda repo audit`: 43 passed, 0 failed, 3 skipped. `./bin/dev workflow` (PR mode): Pipeline SUCCEEDED. The test step wrote 6 `coverage.cobertura.xml` files under `artifacts/coverage`. The host open-pr node opens the pull request.

This machine's `DOTNET_ROOT` had no .NET 10 runtime, so the first workflow run aborted the net10 testhosts. Installing runtime 10.0.15 into that directory (not a repo change) let the second run finish. CI already installs 10.0.x.

### How to validate

**Smoke:** From a detached checkout of `39f9e22` with `GITHUB_REPOSITORY=TimeWarpEngineering/timewarp-mediator` and `RELEASE_TAG` unset, run the publish job's release-guard `run:` body with `pwsh`. Repeat with `RELEASE_TAG=v0.0.0`, with `<Version>` removed, with HEAD not equal to the tag, with the tag deleted, with a commit that is not an ancestor of `origin/master`, and with a commit that is that ancestor but has no successful push run. Then, on this branch, run `ganda repo audit` and `./bin/dev workflow`.

**Expect:** The `39f9e22` run exits 0 and prints `Release guards passed (1 successful CI run(s) for 39f9e22e04e3ebc4f66c32e8251b158422558c12).` Each failing case exits 1 and prints the `::error::` line in the table above. Audit prints `Repository passes all audit checks.` Workflow prints `Pipeline SUCCEEDED`, leaves `<Version>` at `15.0.0-beta.6`, and writes `coverage.cobertura.xml` under `artifacts/coverage`.
