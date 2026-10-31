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
- [ ] `<Version>` is `15.0.0-beta.3`
- [ ] Readmes and `skills/tw-mediator/SKILL.md` pin `15.0.0-beta.3`
- [ ] Release-notes entry confirmed, no-runtime-change line added
- [ ] `./bin/dev check-version` safe to release
- [ ] `./bin/dev workflow` green on net10.0 and net11.0
- [ ] `ganda repo audit --fix` applied and committed
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Notes

- Release path: `tw-release` from mediator master after this PR merges and master CI is
  green (`./bin/dev release --dry-run`, then `./bin/dev release`). beta.1 was cut at 5de6bf5
  on 2026-10-26 and beta.2 at b8dca08 on 2026-10-28 the same way.
- After beta.3 is on nuget.org, timewarp-state task 128 can re-pin to it.

## Implementation Notes

-

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-31)
