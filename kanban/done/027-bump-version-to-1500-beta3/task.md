# Task 027: Bump version to 15.0.0-beta.3

## Description

Steven's direction (2026-10-31): release 15.0.0-beta.3. Master is ahead of nuget.org: task 026
(PR #120) compiled the shipped analyzer and generator against Roslyn 5.9.0 and dropped the
5.0.0 floor; tasks 020 and 025 changed nothing that ships. `<Version>` is still
`15.0.0-beta.2`, which is already published, so `dev release` refuses until it moves.

This task is the bump PR only. The cut (`./bin/dev release`) runs from master after merge.

## Requirements

- `source/Directory.Build.props`: `<Version>15.0.0-beta.3</Version>`.
- Install lines and pins that name the packed version move to `15.0.0-beta.3`:
  `readme.md` (two `dotnet add package` lines), `source/timewarp-mediator/readme.md`,
  `source/timewarp-mediator-generators/readme.md` (two lines),
  `source/timewarp-mediator-analyzers/readme.md`, and `skills/tw-mediator/SKILL.md` (the two
  `PackageVersion` lines and the "currently" sentence). Do not touch `kanban/` history or the
  older release-notes files.
- `documentation/release-notes/15.0.0-beta.3.md` already exists (task 026). Confirm it reads
  as the entry for this release; add a one-line pointer that the dispatcher is unchanged since
  beta.2 (task 020 measured singleton mode and dropped it) so a reader knows there is no
  runtime change in this beta. Keep it short.
- `./bin/dev check-version` reports `15.0.0-beta.3` not yet published and safe to release.
- `./bin/dev workflow` green on both TFMs; pack produces the two `15.0.0-beta.3` nupkgs and
  the smoke prints `package-pong` on net10.0 and net11.0.
- `ganda repo audit --fix` applied and committed.
- Do not tag or create a GitHub Release here.

## Checklist

### Implementation
- [x] `<Version>` is `15.0.0-beta.3`
- [x] Readmes and `skills/tw-mediator/SKILL.md` pin `15.0.0-beta.3`
- [x] Release-notes entry confirmed, no-runtime-change line added
- [x] `./bin/dev check-version` safe to release
- [x] `./bin/dev workflow` green on net10.0 and net11.0
- [x] `ganda repo audit --fix` applied and committed
### Review
- [x] Code Review
- [ ] PR with merge commit

## Review

- Round 1: Done

## Notes

- Release path: `tw-release` from mediator master after this PR merges and master CI is
  green (`./bin/dev release --dry-run`, then `./bin/dev release`). beta.1 was cut at 5de6bf5
  on 2026-10-26 and beta.2 at b8dca08 on 2026-10-28 the same way.
- After beta.3 is on nuget.org, timewarp-state task 128 can re-pin to it.

## Goal (implementer restatement)

`15.0.0-beta.2` is already on nuget.org, so `dev release` will not cut again until `<Version>` moves. This walk only prepares that bump. It sets `source/Directory.Build.props` to `15.0.0-beta.3` and moves the install lines and skill pins that name the packed version to the same string: the root readme, the three package readmes, and the two `PackageVersion` lines plus the "currently" sentence in `skills/tw-mediator/SKILL.md`. Kanban history and the older release-note files stay as they are.

`documentation/release-notes/15.0.0-beta.3.md` already describes the Roslyn 5.9.0 compiler rule from task 026. It stays that entry, with one added line: the dispatcher is unchanged since beta.2, because task 020 measured singleton mode and dropped it, so this beta has no runtime change.

Acceptance: `./bin/dev check-version` says `15.0.0-beta.3` is not published and is safe to release. `./bin/dev workflow` is green on net10.0 and net11.0, packs both `15.0.0-beta.3` nupkgs, and the smoke prints `package-pong` on both frameworks. `ganda repo audit --fix` is applied and committed. This walk does not tag or create a GitHub Release. The cut is `./bin/dev release` from master after merge.

## Implementation Notes

- `<Version>` and the install pins are `15.0.0-beta.3`. The beta.3 release note keeps the Roslyn compiler sentence from task 026 and adds one line: the dispatcher is unchanged since beta.2 because task 020 measured singleton mode and dropped it.
- `ganda repo audit --fix` changed no files. The only audit result is the existing advisory that task 021 has no usable worktree.

## Results

`<Version>` in `source/Directory.Build.props` is `15.0.0-beta.3`. The root readme, `source/timewarp-mediator/readme.md`, `source/timewarp-mediator-generators/readme.md`, `source/timewarp-mediator-analyzers/readme.md`, and `skills/tw-mediator/SKILL.md` pin that version. `documentation/release-notes/15.0.0-beta.3.md` still records the SDK-compiler rule and now says the dispatcher is unchanged since beta.2. This walk does not tag or create a GitHub Release. The cut stays `./bin/dev release` from master after merge.

`./bin/dev check-version` printed `Version in source: 15.0.0-beta.3`, `Latest NuGet version: 15.0.0-beta.2`, `Source is 1 prerelease increment ahead of v15.0.0-beta.2`, and `safe to release`. Packages checked: `TimeWarp.Mediator`, `TimeWarp.Mediator.Generators`.

`ganda repo audit --fix` exited 0. 33 checks passed, 3 skipped, and one advisory warning: `kanban-landed` says task 021 has no usable worktree. That claim is outside this task. `audit --fix` changed no files.

`./bin/dev workflow` (mode Pr) exited 0. Clean, then build with 0 warnings and 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 42, compilation 69, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.3.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.3.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0. The log ends with `Pipeline SUCCEEDED`.

### How to validate

Smoke: from this worktree, `./bin/dev check-version`, then `./bin/dev workflow`.

Expect: check-version prints `Version in source: 15.0.0-beta.3`, `Latest NuGet version: 15.0.0-beta.2`, and `safe to release`. Workflow prints `0 Warning(s)`, tests 61 + 42 + 69 passed on net10.0 and net11.0, `Samples verified successfully!`, both `15.0.0-beta.3` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`.

### Review disposition

- Rounds: 1. Effort 1, roster `general`.
- Final counts: bug 0, suggestion 0, nit 0. All are 0 open, 0 fixed, and 0 wontfix.
- Disposition: **clean**. The implementation matches the task and the implementer restatement, so there is no misread.
- The reviewer reran `./bin/dev check-version`, which printed `safe to release`. `dotnet msbuild -getProperty:Version` gives `15.0.0-beta.3` on both product projects. `git grep beta.2` outside `kanban/` finds only historical references. Workflow was not rerun; the implementer's run above is the record.
- Observation, not a finding: the prerelease history in `documentation/release-notes/15.0.0.md` does not link beta.3 yet. That file was out of scope for this task.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
- Implementation: 01a190fb-a798-7731-8139-b96699b4f637 (2026-10-31)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a190fb-a798-7731-8139-b96699b4f637, max-turns 500, Done — 2026-10-31
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a190fb-a798-7731-8139-b96699b4f637, max-turns 500, Done — 2026-10-31T08:11:35Z
- Review oracle: reviewer (claude, model claude-opus-5-5), effort 1, roster general, max-turns 80, round 1, disposition clean — 2026-10-31
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 558fb079-82db-4e0a-bf63-6958bf1d3a3d, max-turns 80 — 2026-10-31T08:13:44Z
