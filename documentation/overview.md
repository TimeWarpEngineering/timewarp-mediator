# Versioning and release process

This document describes how the TimeWarp.Mediator NuGet packages are versioned, built and
published. The TimeWarp-wide convention is the `tw-release` skill
(`skills/tw-release/SKILL.md` in timewarp-flow). This page covers what this repository actually
does, including where it differs from that skill.

## Packages

The packages share one version and are packed together by `dev pack`:

| Package | Project | Target |
|---|---|---|
| `TimeWarp.Mediator` | `source/timewarp-mediator` (packs `TimeWarp.Mediator.Analyzers.dll` as an analyzer asset) | `net10.0;net11.0` |
| `TimeWarp.Mediator.Generators` | `source/timewarp-mediator-generators` | `netstandard2.0` |

`source/timewarp-mediator-analyzers` builds the TWM001–TWM004 analyzer. It is not a NuGet package.

`dev pack` writes the two nupkgs to `artifacts/packages` and fails if any other nupkg is there.
`TimeWarp.Mediator` must contain `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll` and exactly
the `lib/net10.0/` and `lib/net11.0/` folders. `TimeWarp.Mediator.Generators` must contain its generator DLL and
`buildTransitive/TimeWarp.Mediator.Generators.props`. Each nupkg must contain `notice` and
`unlicense`. Each package packs its own `source/<project>/readme.md` as the nuget.org readme;
`dev pack` fails when that readme's first heading is not the package id or it contains a relative
link (nuget.org cannot resolve those). The repository `readme.md` is the GitHub page only.

`dev pack` then runs the package smoke, `tests/timewarp-mediator-package-smoke`: a consumer
project outside the solution that restores `TimeWarp.Mediator` and
`TimeWarp.Mediator.Generators` from `artifacts/packages` (into a throwaway packages folder, never
the global cache), builds, and sends one request through the generated mediator. A broken
buildTransitive props or TFM folder fails here instead of after publishing.

## Versioning

- **One source of truth:** `<Version>` in `source/Directory.Build.props` (for example
  `14.0.0`). The root `Directory.Build.props` holds build settings only. Nothing derives
  the version from git tags.
- **Tags follow the version:** a release is tagged `v` + `<Version>` (for example
  `v14.0.0`). Nobody types the tag by hand; `dev release` creates it from the props file.
- **Prereleases are real releases.** Each prerelease (for example `14.0.0-beta.N`) goes through the same bump and release
  flow and is published to nuget.org as a prerelease. CI builds on `master` are not published
  anywhere; their packages are only uploaded as a `Packages-<run number>` workflow artifact.
- `dev check-version` compares `<Version>` with what nuget.org already serves for
  `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators`. It fails when that version is already
  published. `--strict` also fails when the source version is more than one prerelease increment
  ahead of the latest published version.

## CI: `.github/workflows/workflow.yml`

The workflow is a thin trigger; the pipeline itself is `dev workflow` (C#, in `tools/dev-cli`).
Every step runs `dotnet run --file tools/dev-cli/dev.cs -- …`, so CI does not need a
pre-installed `./bin/dev`.

`global.json` pins the SDK to `11.0.100-rc.3` with `rollForward: latestFeature` and
`allowPrerelease: true`. Each job's `setup-dotnet` steps install `10.0.x` on the GA channel
and `11.0.x` with `dotnet-quality: preview`. The `10.0.x` install stays so the `net10.0`
targeting pack and runtime restore. `dotnet-quality: preview` is not applied to `10.0.x`.
When .NET 11 ships, replace the RC pins with `11.0.100` / `11.0.0` and drop the preview quality.

| Trigger | Job | What runs |
|---|---|---|
| push to `master`, pull request to `master` | `ci` | `dev workflow`: clean → build → test → verify-samples (runs every sample) → pack (layout gate and package smoke). Uploads `Packages-<run number>`. |
| `workflow_dispatch` with `mode=merge` (default) | `ci` | Same as above. Never publishes. |
| `workflow_dispatch` with `mode=benchmark`, and a weekly Monday schedule (`17 6 * * 1` UTC) | `benchmark` | `dev benchmark --gate` on `net10.0`. Not part of pull requests. The gate tolerance is 50% on mean time and no extra bytes. Three consecutive hosted-runner results inside that tolerance were not collected, so the job stays off the pull-request path. |
| GitHub Release published | `publish` | Release guards → OIDC login → `dev workflow --mode release`: clean → build → pack → check-version → push. |
| `workflow_dispatch` with `mode=release` and `confirm=release` | `publish` | Break-glass. Same as a release; see below. |
| `workflow_dispatch` with `mode=probe` | `publish` | OIDC login only. No checkout, build or push. |

There are no path filters: every `master` commit gets a CI run, and the release guards depend on
that run.

### The `publish` job

Only `publish` holds `id-token: write`. Before it builds anything it checks that:

1. the release tag equals `v` + `<Version>` from `source/Directory.Build.props`;
2. that tag exists and points at the commit being published;
3. the commit is on `master`;
4. `workflow.yml` has a successful `push` run for that commit.

Tests are not re-run in `publish`; the green push CI run on that commit is the test gate. The job
then rebuilds and packs from the tagged commit. (The `tw-release` skill describes a pipeline that
promotes the CI artifact unchanged. This repository does not do that yet.)

### Trusted publishing (OIDC)

There is no stored NuGet API key. `NuGet/login` exchanges the GitHub OIDC token for a short-lived
key (owner `TimeWarp.Enterprises`) and passes it to `dev workflow` in the `NUGET_API_KEY`
environment variable, never on the command line. The nuget.org policy binds this repository and
the workflow file name `workflow.yml`, not job names, so do not rename the file.

`dev workflow` pushes `artifacts/packages/*.nupkg` with `--skip-duplicate`. Without a key (for
example a local `dev workflow --mode release`) it stops after pack and check-version.

## Cutting a release

1. **Bump.** On a task branch, change `<Version>` in `source/Directory.Build.props`. In the same
   PR, move every row in `source/timewarp-mediator-analyzers/AnalyzerReleases.Unshipped.md` to
   `AnalyzerReleases.Shipped.md` (see [Analyzer release tracking](#analyzer-release-tracking)),
   and update the release notes under [release-notes/](./release-notes/). Merge the PR with a
   merge commit.
2. **Wait for green.** The push CI run on the merge commit must succeed.
3. **Cut.** From a clean `master` checkout that is in sync with `origin/master`:

   ```bash
   ./bin/dev release --dry-run   # runs every guard, creates nothing
   ./bin/dev release             # tags v<Version> and creates the GitHub Release
   ```

   `dev release` refuses when the tree is dirty, `master` is not in sync, the tag already exists,
   the version is already on nuget.org, or HEAD has no successful CI run. The `tw-release` skill
   lists each refusal and its remedy. If `release` is missing from `./bin/dev --help`, the AOT
   binary is stale: run `./bin/dev self-install`, or use
   `dotnet run --file tools/dev-cli/dev.cs -- release`.
4. **Watch.** The published Release starts the `publish` job. Confirm the packages on
   nuget.org.

Do not run `gh release create` with a hand-typed tag; the tag guard rejects any tag that does not
match `<Version>`.

### Break-glass

Use break-glass only to recover, for example to finish a partial publish. It must run on the
release tag, so the tag guard sees the same commit:

```bash
gh workflow run workflow.yml --ref v<Version> -f mode=release -f confirm=release
```

Without `confirm=release` the job fails before it checks anything out. A plain dispatch
(`mode=merge`) never publishes.

### Probe

To check the nuget.org trusted-publishing policy without building or publishing:

```bash
gh workflow run workflow.yml -f mode=probe
```

If the `NuGet login` step fails, the policy is missing or misconfigured on nuget.org.

## Analyzer release tracking

The analyzer project `source/timewarp-mediator-analyzers` (shipped inside `TimeWarp.Mediator`, not as its own package) uses Roslyn analyzer release tracking (RS2000–RS2008). New or changed
TWM rules go in `AnalyzerReleases.Unshipped.md`. Every release that publishes packages, including
prereleases, moves those rows to `AnalyzerReleases.Shipped.md` and leaves Unshipped with only its
header.

Release headers must parse as `System.Version` (RS2007 rejects `## Release 14.0.0-beta.1`), so a
prerelease files its rows under its target version: every 14.0.0 beta adds to `## Release 14.0.0`
and notes the beta in the Notes column (`RequestHasNoHandler, shipped in 14.0.0-beta.1`). Create the
section if it does not exist yet. TWM001–TWM004 shipped in 14.0.0-beta.1; TWM005–TWM011 shipped in 14.0.0-beta.5.
15.0 retires TWM005, TWM006, and TWM009. The live set is TWM001–TWM004, TWM007, TWM008, TWM010, and TWM011.

## Local commands

Bootstrap the AOT CLI once per clone with `dotnet run --file tools/dev-cli/dev.cs -- self-install`.

| Command | Purpose |
|---|---|
| `./bin/dev workflow` | The CI pipeline locally: clean → build → test → pack. |
| `./bin/dev workflow --mode release` | Release pipeline; pushes only when `NUGET_API_KEY` or `--api-key` is set. |
| `./bin/dev pack` | Pack the packages and run the nupkg layout checks. |
| `./bin/dev check-version` | Fail if `<Version>` is already on nuget.org. |
| `./bin/dev release [--dry-run]` | Tag `v<Version>` and create the GitHub Release. |
