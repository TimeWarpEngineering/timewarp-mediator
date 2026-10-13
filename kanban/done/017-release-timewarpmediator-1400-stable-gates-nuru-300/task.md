# Release TimeWarp.Mediator 14.0.0 stable (gates Nuru 3.0.0)

## Description

Ship stable `14.0.0` for all four packages (Contracts, Mediator, Generators, Analyzers). Replaces archived task 014,
whose gates are now met:

- Repository review 013 (22 children) merged in PR #95; 013-004 public API reduction is in.
- `14.0.0-beta.5` released 2026-10-13 (task 016) as the dogfood build.
- Dogfood verdicts, all "ready for 14.0.0 stable", zero TWM005-TWM011, nothing filed back:
  timewarp-state 119, timewarp-architecture 342, timewarp-nuru 498.
- Issues #52, #63, #64, #65 closed 2026-10-13; the tracker has no open issues.

Why stable matters now: timewarp-nuru 489 chose to wait for Mediator 14.0.0 rather than ship Nuru 3.0.0 with a
prerelease dependency (NU5104). Nuru 3.0.0 is blocked on this release.

Follows `documentation/overview.md` "Cutting a release". Marketing launch pack is task 015 (drafted, unpublished,
waits for Steven's OK; nothing in that pack is part of this task's gate).

## Depends on

- 018 (tw-mediator agent skill: `skills/tw-mediator/SKILL.md` + readme pointer must be on master before the version bump PR, so the launch pack and GitHub Release can link it)

## Requirements

- `<Version>` in `source/Directory.Build.props` is `14.0.0`.
- Analyzer release tracking: `AnalyzerReleases.Unshipped.md` is already header-only and every TWM row sits under
  `## Release 14.0.0` in `Shipped.md`. No rows move; keep the per-row "shipped in beta.N" notes. RS2000-RS2008 validate.
- Release notes `documentation/release-notes/release-notes-v14.0.0.md`: add a `## 14.0.0` section at the top (changes
  since beta.5; if none, say the stable is beta.5 unchanged) and rewrite the intro so it no longer says 14.0.0 is in
  beta and 13.0.0 is the last stable. Keep the per-beta sections as history.
- GitHub Release body: `dev release` generates notes; the hand-written body is
  `documentation/release-notes/github-release-v14.0.0-draft.md` and is pasted over the generated body only after the
  packages are indexed and Steven approves. Leave the Release published with generated notes until then.
- Any readme / overview text that describes 14.0.0 as prerelease is flipped in the same PR.
- `./bin/dev workflow` green locally; PR merged with a merge commit; push CI green on the merge commit.
- `./bin/dev release --dry-run` then `./bin/dev release` from a clean, synced origin-home master. Publish job pushes four
  nupkgs and two snupkgs; nuget.org flat index shows `14.0.0` for all four.
- Afterwards: comment on timewarp-nuru 489 that 14.0.0 is on nuget.org so Nuru bumps `Directory.Packages.props` and
  drops the NU5104 question; timewarp-state and timewarp-architecture bump on their next NuGet refresh.

## Checklist

- [x] 018 merged: `skills/tw-mediator/SKILL.md` on master and linked from the readme
- [x] Confirm no code change landed on master since v14.0.0-beta.5 that needs a release-note line
- [x] Version bump to 14.0.0
- [x] Release notes: `## 14.0.0` section added, intro flipped from "in beta" to stable; prerelease wording elsewhere flipped
- [x] Analyzer release tracking unchanged and validating (RS2000-RS2008)
- [x] `./bin/dev workflow` green locally
- [x] PR merged (merge commit), push CI green
- [x] `dev release --dry-run` clean; `dev release` tags v14.0.0 and the publish job pushes all packages
- [ ] nuget.org shows 14.0.0 for Contracts, Mediator, Generators, Analyzers
- [ ] timewarp-nuru 489 notified
- [x] Results recorded (PR, merge commit, CI runs, Release URL, publish run)

## Session

- Created: 1736118 (2026-10-13)
- Release cut: Claude (timewarp-mediator cockpit session 2142087), 2026-10-13
- 2026-10-13 implement (ganda task work, Claude Opus 5.5): version bump, release notes, prerelease wording
  flipped, `dev workflow --mode pr` green, `dev check-version --strict` and `ganda repo audit` pass.
  Remaining (post-merge, operator): merge PR, push CI, `dev release`, nuget.org check, Nuru 489 comment.
- Implement oracle: implemented by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-10-13T13:34:41Z
- Review oracle: Grok session `01a13578-f54a-74c0-8ed8-3e1569bd4336` (2026-10-13), effort 1, roster general, disposition clean
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 80 — 2026-10-13T13:40:10Z

## Notes

- Upstream: 018 (agent skill). Maintainer decision 2026-10-13: the skill gates stable so agents have a concise entry point on launch day.
- Downstream: timewarp-nuru 489 → Nuru 3.0.0. timewarp-state 119 and timewarp-architecture 342 are done on beta.5.
- Marketing: task 015 launch pack (blog, posts, GitHub Release body draft). Publishing it is Steven's call, after this task.
- `dev release` refuses on dirty tree, out-of-sync master, existing tag, version already on nuget.org, or no green CI on HEAD.
- Releases: https://github.com/TimeWarpEngineering/timewarp-mediator/releases
- Review trail: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md` (clean, 0 open).

## Results

### Implementation (PR stage)

- `source/Directory.Build.props`: `<Version>14.0.0</Version>`.
- `git diff v14.0.0-beta.5..master` outside `kanban/`: only `skills/tw-mediator/SKILL.md` and the readme
  pointer (task 018). No source, package or analyzer change, so the `## 14.0.0` release-notes section says the
  stable is beta.5 unchanged.
- `documentation/release-notes/release-notes-v14.0.0.md`: intro now says 14.0.0 is stable and supersedes 13.0.0;
  new `## 14.0.0` section at the top; per-beta sections kept. The GitHub Release draft is unchanged.
- Prerelease wording flipped in `readme.md`, `skills/tw-mediator/SKILL.md`, `documentation/generated-vs-legacy.md`
  (heading, #52-open paragraph, `--prerelease` installs → `--version 14.0.0`), `documentation/m1-generated-mediator.md`,
  `documentation/overview.md` (version/tag examples).
- Analyzer release tracking untouched: Unshipped is header-only, TWM rows under `## Release 14.0.0` in Shipped.
- `./bin/dev workflow --mode pr`: Pipeline SUCCEEDED (build, tests, verify-samples, pack + layout check +
  package smoke on 14.0.0 nupkgs). The first run failed with CS8784 (`TypeLoadException` for
  `MembershipOptions` in Analyzers 14.0.0.0): a stale analyzer assembly in the shared compiler server,
  because beta.5 and 14.0.0 both have assembly version 14.0.0.0. `dotnet build-server shutdown` fixed it.
  The same can happen on any machine that built an older 14.0.0 beta.
- `./bin/dev check-version --strict`: 14.0.0 is new (latest on nuget.org is 14.0.0-beta.5).
- `ganda repo audit`: passes.

Still open after this node (host/operator): PR merge with a merge commit, push CI on the merge commit,
`dev release --dry-run` / `dev release`, nuget.org flat-index check, timewarp-nuru 489 comment.

### Implementation review

- Rounds: 1. Roster / effort: general only, effort 1 (131 lines changed).
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 / 0 / 0.
- Disposition: **clean**. No wontfix and no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
dotnet build-server shutdown
./bin/dev workflow --mode pr
./bin/dev check-version --strict
ls artifacts/packages
grep -rn -- "--prerelease\|14.0.0-beta is not" readme.md skills documentation/*.md
```

Expect: `Pipeline SUCCEEDED` (package smoke prints `package-pong`); check-version reports
`Version in source: 14.0.0` and "safe to release"; `artifacts/packages` holds four
`*.14.0.0.nupkg` plus `TimeWarp.Mediator.14.0.0.snupkg` and `TimeWarp.Mediator.Contracts.14.0.0.snupkg`;
the grep prints nothing.

### Release outcome (2026-10-13, Claude from the mediator cockpit session)

- PR #101 merged to master as `96e3e42` (merge commit) via `ganda pr merge 101 --task-id 017`; push CI run 38408024637 green.
- `./bin/dev release --dry-run`: all seven guards passed. `./bin/dev release`: tag `v14.0.0` at `96e3e42`, GitHub Release
  https://github.com/TimeWarpEngineering/timewarp-mediator/releases/tag/v14.0.0 (generated notes; the task 015 draft body is
  still unpublished and waits for Steven).
- Publish run 38408409563 succeeded 13:53 UTC: `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator`, `TimeWarp.Mediator.Generators`,
  `TimeWarp.Mediator.Analyzers` 14.0.0 nupkgs plus the Contracts and Mediator snupkgs, six "Your package was pushed".
- nuget.org flat index: see the checklist tick above for when all four showed `14.0.0`.
- Downstream: timewarp-nuru 489 updated with the release (Nuru bumps `Directory.Packages.props` to 14.0.0 and ships 3.0.0 without NU5104).
