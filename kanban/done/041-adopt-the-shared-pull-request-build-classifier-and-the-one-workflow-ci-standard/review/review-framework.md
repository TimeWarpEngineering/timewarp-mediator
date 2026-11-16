# Review framework — task 041

**Date:** 2026-11-16
**Host task:** kanban/in-progress/041-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/041-adopt-the-shared-pull-request-build-classifier-and` vs `master` (221 lines: `.github/workflows/workflow.yml`, `.timewarp/dev.jsonc`, `Directory.Packages.props`, `agent.md`, `tools/dev-cli/endpoints/workflow-command.cs`, and this task's `task.md`)
**Plan / brief:** Adopt `PullRequestBuildClassifier` from TimeWarp.Nuru.DevCli 4.0.0-beta.15 so every pull request reports `CI`, and a pull request that changes only non-build paths skips build and test. Declare packed `assets/logo.png` through `pullRequestBuild`. Leave `pull_request` without a paths filter.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** review Grok `01a1e51d-461b-7301-961e-5ee5f4af70d5`; implementer Claude `c956032c-0330-4502-8d82-8603e32af74f`

## Budget (by-diff)

- Lines changed: 221
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
