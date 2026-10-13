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

- [ ] Confirm no code change landed on master since v14.0.0-beta.5 that needs a release-note line
- [ ] Version bump to 14.0.0
- [ ] Release notes: `## 14.0.0` section added, intro flipped from "in beta" to stable; prerelease wording elsewhere flipped
- [ ] Analyzer release tracking unchanged and validating (RS2000-RS2008)
- [ ] `./bin/dev workflow` green locally
- [ ] PR merged (merge commit), push CI green
- [ ] `dev release --dry-run` clean; `dev release` tags v14.0.0 and the publish job pushes all packages
- [ ] nuget.org shows 14.0.0 for Contracts, Mediator, Generators, Analyzers
- [ ] timewarp-nuru 489 notified
- [ ] Results recorded (PR, merge commit, CI runs, Release URL, publish run)

## Session

- Created: 1736118 (2026-10-13)

## Notes

- Downstream: timewarp-nuru 489 → Nuru 3.0.0. timewarp-state 119 and timewarp-architecture 342 are done on beta.5.
- Marketing: task 015 launch pack (blog, posts, GitHub Release body draft). Publishing it is Steven's call, after this task.
- `dev release` refuses on dirty tree, out-of-sync master, existing tag, version already on nuget.org, or no green CI on HEAD.
- Releases: https://github.com/TimeWarpEngineering/timewarp-mediator/releases
