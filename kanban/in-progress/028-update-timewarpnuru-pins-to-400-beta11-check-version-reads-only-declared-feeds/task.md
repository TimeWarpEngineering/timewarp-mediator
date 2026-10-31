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

- [ ] Both Nuru pins at 4.0.0-beta.11
- [ ] `self-install` succeeds; dev CLI call sites compile against DevCli beta.11
- [ ] `check-version` with no GitHub credential reads nuget.org only (output in Results)
- [ ] agent.md and workflow-command.cs comment name beta.11
- [ ] No GH_TOKEN added to the release step
- [ ] audit --fix committed; `./bin/dev workflow` green; `ganda nuget outdated` clean

## Notes

- Release run failure text, for reference:
  `GitHub Packages lookup for 'TimeWarp.Mediator' failed (no HTTP response): no GitHub token ... Pipeline FAILED — Check Version failed`.
- Tag `v15.0.0-beta.3` and its GitHub Release exist at 1bcba1d with nothing on nuget.org.
  Do not touch the tag or release in this task.
- DevCli beta.11 `package-feed-lookup.cs` header: "only the declared feeds are read"; feeds with
  nuget.org default come from `checkVersionConfig.feeds` (`check-version-config.cs`).

## Results

(implementer fills in)

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

- [ ] `<Version>` is `15.0.0-beta.4`; readmes and SKILL.md pin it; no stray `beta.3` pins outside release notes
- [ ] `15.0.0-beta.4.md` exists via `git mv`, explains the unpublished beta.3, and both links point at it
- [ ] `check-version` says `15.0.0-beta.4` is safe to release
