# Review framework — task 040

**Date:** 2026-11-14
**Host task:** kanban/in-progress/040-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-mediator/
**Diff scope:** branch `task/040-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (ahead 7). Product diff: `Directory.Packages.props`, `source/Directory.Build.props`, `agent.md`, `tools/dev-cli/endpoints/workflow-command.cs`, `.githooks/*.cs`.
**Plan / brief:** Refresh the outdated central NuGet pins in `Directory.Packages.props` and set `<Version>` in `source/Directory.Build.props` from `15.0.0-beta.7` to `15.0.0-beta.8`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok 4.7, session `01a1d949-8539-7c52-adfb-bda69301a4aa` (2026-11-14). Implementer session `b9fd91d6-173b-485b-9193-961f5ded1f3d`. Round 2 review oracle grok 4.7, session `01a1d954-30d9-7fa3-89d4-908710a1834f` (2026-11-14). Fix-pass session `9e801ac4-a2cd-47f5-b29c-fa52af1d004f`.

## Budget (by-diff)

- Lines changed: 117
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Round 2 scope

Re-review of `d6a7f9e..f68a938` (round-1 `base.sha` through HEAD). Product files: `.timewarp/suppression-baseline.json`, `tools/dev-cli/dev.cs`. Kitchen edits in that commit stay out of the product check except for the accuracy of `## Results`. Effort stays 1, roster general. Prior round files stay frozen.
