# Task 028: Update TimeWarp.Nuru pins to 4.0.0-beta.11 and bump to 15.0.0-beta.4

## Description

The v15.0.0-beta.3 release run (41184701087) failed in `Check Version`: `TimeWarp.Nuru.DevCli`
4.0.0-beta.10 read the org GitHub Packages feed unconditionally, had no token in the release step,
and returned `Unknown`, so the pipeline refused to push. Nuru task 564 (shipped in Nuru 4.0.0-beta.11,
on nuget.org as of 2026-10-31) makes `check-version` read only the feeds a repo declares in
`checkVersionConfig.feeds` (default `nuget.org`). Mediator publishes to nuget.org only, so with the
default no GitHub token is needed. `ganda nuget outdated` names exactly the two Nuru pins.

This task is the pin bump only. It does not change `<Version>` and does not release; the
beta.3-vs-beta.4 decision and the release are a follow-up.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to `4.0.0-beta.11`.
- Rebuild the dev CLI: `dotnet run --file tools/dev-cli/dev.cs -- self-install`. If DevCli beta.11
  changed visibility or signatures of the source-shipped services (`content/any/services/*.cs`,
  `content/any/endpoints/*.cs`), fix call sites in `tools/dev-cli/` (boy-scout). Do not pin back.
- `.timewarp/dev.jsonc`: leave `checkVersionConfig` as is unless beta.11 requires an explicit
  `feeds` key. The default is `nuget.org`, which is the only feed Mediator publishes to. If you do
  add `feeds`, it is `"nuget.org"` and the reason goes in a comment.
- Run `./bin/dev check-version` locally with `GITHUB_TOKEN` and `GH_TOKEN` unset and no `gh`
  login available to the process (for example `env -u GITHUB_TOKEN -u GH_TOKEN GH_CONFIG_DIR=/nonexistent ./bin/dev check-version`).
  It must report both packages at 15.0.0-beta.3 as not published on nuget.org and must not
  mention GitHub Packages, a token, or `Unknown`. Paste the output under `## Results`.
- `TimeWarp.Nuru` 4.0.0-beta.11 ships `lib/net10.0` only (verified from the nupkg): the dev CLI
  stays `#:property TargetFramework=net10.0`. Update the beta number in `agent.md` (Target
  Frameworks bullet) and in the comment at the top of `tools/dev-cli/endpoints/workflow-command.cs`.
  If beta.11 made `WorkflowCommand.Handler`'s dependencies public again, you may make the
  handler public again and drop that comment; otherwise leave it internal.
- `.github/workflows/workflow.yml`: do not add `GH_TOKEN` to the release step. Mediator does not
  depend on a feed it does not publish to (Steven, 2026-10-31).
- `ganda repo audit --fix`, commit whatever it changes.
- `./bin/dev workflow` green locally. `ganda nuget outdated` clean (no outdated rows).

## Checklist

- [x] Both Nuru pins at 4.0.0-beta.11
- [x] `self-install` succeeds; dev CLI call sites compile against DevCli beta.11
- [x] `check-version` with no GitHub credential reads nuget.org only (output in Results)
- [x] agent.md and workflow-command.cs comment name beta.11
- [x] No GH_TOKEN added to the release step
- [x] audit --fix committed; `./bin/dev workflow` green; `ganda nuget outdated` clean

## Notes

- Release run failure text, for reference:
  `GitHub Packages lookup for 'TimeWarp.Mediator' failed (no HTTP response): no GitHub token ... Pipeline FAILED — Check Version failed`.
- Tag `v15.0.0-beta.3` and its GitHub Release exist at 1bcba1d with nothing on nuget.org.
  Do not touch the tag or release in this task.
- DevCli beta.11 `package-feed-lookup.cs` header: "only the declared feeds are read"; feeds with
  nuget.org default come from `checkVersionConfig.feeds` (`check-version-config.cs`).

## Goal (implementer restatement)

Move the dev CLI onto TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.11 so `check-version` reads only the feeds this repo declares. Mediator publishes to nuget.org, the default when `checkVersionConfig.feeds` is absent, so a release no longer needs a GitHub Packages token. Also advance the packed version from the unpublished `15.0.0-beta.3` tag to `15.0.0-beta.4`. This walk does not tag and does not create a GitHub Release.

Acceptance:

- `Directory.Packages.props` pins both Nuru packages at `4.0.0-beta.11`. `dotnet run --file tools/dev-cli/dev.cs -- self-install` succeeds, and dev CLI call sites compile against that pin. The dev CLI stays `net10.0`. `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` stay internal in beta.11, so `WorkflowCommand.Handler` stays internal and the comment names beta.11. `agent.md` names beta.11.
- `.timewarp/dev.jsonc` keeps `checkVersionConfig` without a `feeds` key.
- With `GITHUB_TOKEN` and `GH_TOKEN` unset and no `gh` login, `./bin/dev check-version` prints `Version in source: 15.0.0-beta.4`, `Latest NuGet version: 15.0.0-beta.2`, and `safe to release`. The output does not mention GitHub Packages, a token, or `Unknown`.
- `<Version>` is `15.0.0-beta.4`. The root readme, the three package readmes, and `skills/tw-mediator/SKILL.md` pin that version. `git grep '15\.0\.0-beta\.3' -- ':!kanban'` hits only historical text under `documentation/release-notes/`.
- `documentation/release-notes/15.0.0-beta.4.md` comes from `git mv` of the beta.3 note, is retitled, and explains why beta.3 was tagged and never published. The root readme and the F14 row in `15.0.0.md` link to the beta.4 note.
- The release step in `.github/workflows/workflow.yml` does not gain `GH_TOKEN`.
- `ganda repo audit --fix` is committed. `./bin/dev workflow` is green, packs both `15.0.0-beta.4` nupkgs, and the smoke prints `package-pong` on both frameworks. `ganda nuget outdated` has no outdated rows.

## Results

`Directory.Packages.props` pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.11`. `source/Directory.Build.props` is `<Version>15.0.0-beta.4</Version>`. Install lines in the root readme, the three package readmes, and `skills/tw-mediator/SKILL.md` match. `.timewarp/dev.jsonc` still has no `feeds` key. The release step in `.github/workflows/workflow.yml` was not given `GH_TOKEN` (the existing `GH_TOKEN` on the release-guards step is unchanged; it calls `gh api` for the green-CI guard).

`TimeWarp.Nuru` 4.0.0-beta.11 contains `lib/net10.0` only. The dev CLI stays `#:property TargetFramework=net10.0`. In DevCli 4.0.0-beta.11, `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` are still `internal`, so `WorkflowCommand.Handler` stays internal. The comment at the top of `tools/dev-cli/endpoints/workflow-command.cs` and the Target Frameworks bullet in `agent.md` name beta.11. `dotnet run --file tools/dev-cli/dev.cs -- self-install` succeeded with no call-site changes.

`documentation/release-notes/15.0.0-beta.4.md` is the `git mv` of the beta.3 note, retitled, with the unpublished-beta.3 paragraph at the top. `readme.md` and the F14 row in `documentation/release-notes/15.0.0.md` link to it. `git grep '15\.0\.0-beta\.3' -- ':!kanban'` hits only that historical paragraph.

`env -u GITHUB_TOKEN -u GH_TOKEN GH_CONFIG_DIR=/nonexistent ./bin/dev check-version` exited 0:

```
Using configured package override; delete checkVersionConfig.packages to derive the set from IsPackable.
Version in source: 15.0.0-beta.4
Latest NuGet version: 15.0.0-beta.2
Source is 2 prerelease increments ahead of v15.0.0-beta.2
1 version(s) were bumped but never released — was a release step skipped?
Packages checked: TimeWarp.Mediator, TimeWarp.Mediator.Generators

✓ Version in source is new — safe to release.
```

The skipped-increment line is beta.3, which was tagged and never published. The output does not mention GitHub Packages, a token, or `Unknown`.

`./bin/dev workflow` (mode Pr) exited 0. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 42, compilation 69, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.4.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.4.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0. The log ends with `Pipeline SUCCEEDED`.

`ganda nuget outdated` printed `All packages are up to date!` (both Nuru pins `up to date, nuget.org`). `ganda repo audit --fix` changed nothing. The audit exited 0 (`Repository passes`) with one advisory warning: `kanban-landed` says claim 021 has no usable worktree. That claim is not this task, and `--fix` did not land a kitchen for it.

This walk does not tag or create a GitHub Release. The cut is `./bin/dev release` from master after merge.

### How to validate

Smoke: `env -u GITHUB_TOKEN -u GH_TOKEN GH_CONFIG_DIR=/nonexistent ./bin/dev check-version` and `./bin/dev workflow`.

Expect: check-version prints `Version in source: 15.0.0-beta.4`, `Latest NuGet version: 15.0.0-beta.2`, and `safe to release`, and does not mention GitHub Packages, a token, or `Unknown`. Workflow prints `0 Warning(s)`, tests 61 + 42 + 69 passed on net10.0 and net11.0, `Samples verified successfully!`, both `15.0.0-beta.4` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`.

## Session

- Implementation: implementer-grok (2026-10-31). Pinned Nuru 4.0.0-beta.11, set `<Version>` to 15.0.0-beta.4, and recorded the unpublished beta.3 in the release note.

## Addendum: also bump to 15.0.0-beta.4 (Steven, 2026-10-31)

Steven chose beta.4 over re-cutting beta.3, so this task also carries the version bump. The
two paragraphs above that say "pin bump only" and "does not change `<Version>`" are superseded
by this section. Tag `v15.0.0-beta.3` and its GitHub Release still stay untouched.

- `source/Directory.Build.props`: `<Version>15.0.0-beta.4</Version>`.
- Every install line and pin that names the packed version moves to `15.0.0-beta.4`: `readme.md`
  (two `dotnet add package` lines and the release-notes link), `source/timewarp-mediator/readme.md`,
  `source/timewarp-mediator-generators/readme.md` (two lines), `source/timewarp-mediator-analyzers/readme.md`,
  and `skills/tw-mediator/SKILL.md` (two `PackageVersion` lines and the "currently" sentence).
  `git grep '15\.0\.0-beta\.3' -- ':!kanban'` afterwards must hit only `documentation/release-notes/`
  text that is deliberately historical (see next bullet).
- Release notes: `git mv documentation/release-notes/15.0.0-beta.3.md documentation/release-notes/15.0.0-beta.4.md`.
  Retitle it `# TimeWarp.Mediator 15.0.0-beta.4` and add one paragraph at the top: 15.0.0-beta.3 was
  tagged on 2026-10-31 but never published, because the release pipeline's version check
  (`TimeWarp.Nuru.DevCli` 4.0.0-beta.10) read the org GitHub Packages feed without a token and
  refused to push; beta.4 is that content with the dev tooling moved to DevCli 4.0.0-beta.11
  (`check-version` reads only nuget.org), and the shipped packages are unchanged since beta.2.
  Fix the two links that point at `15.0.0-beta.3.md`: `readme.md` release-notes list and
  `documentation/release-notes/15.0.0.md` (the F14 row). Do not edit older release-note files
  or kanban history.
- `./bin/dev check-version` (with the no-credential environment from the first section) must print
  `Version in source: 15.0.0-beta.4`, `Latest NuGet version: 15.0.0-beta.2`, and `safe to release`.
  Paste that output under `## Results` too.
- `./bin/dev workflow` packs both `15.0.0-beta.4` nupkgs and the smoke prints `package-pong` on
  both frameworks.
- This walk does not tag or create a GitHub Release. The cut is `./bin/dev release` from master
  after merge.

Extra checklist items:

- [x] `<Version>` is `15.0.0-beta.4`; readmes and SKILL.md pin it; no stray `beta.3` pins outside release notes
- [x] `15.0.0-beta.4.md` exists via `git mv`, explains the unpublished beta.3, and both links point at it
- [x] `check-version` says `15.0.0-beta.4` is safe to release
