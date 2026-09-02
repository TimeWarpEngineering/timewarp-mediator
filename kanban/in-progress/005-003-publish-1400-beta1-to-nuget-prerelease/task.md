# Publish 14.0.0-beta.1 to NuGet prerelease

## Description

Parent: **005**. After **005-001** is merged and master CI is green, cut the **prerelease** with the repo’s trusted-publishing path (`dev release` / workflow). This is how State and Nuru consume the rewrite. It is **not** a stable 14.0.0.

## Depends on

- 005-001

## Requirements

- Tag/version is `14.0.0-beta.1` (prerelease)
- All four packages on nuget.org as **prerelease**
- Do **not** unlist or overwrite 13.0.0
- Do **not** ship 14.0.0 without `-beta`

## Checklist

- [x] 005-001 merged; master CI green; Packages artifact exists
- [x] `dev release --dry-run` then `dev release` (or this repo’s equivalent)
- [x] Confirm nuget.org versions include `14.0.0-beta.1` for all four ids

## Out of scope

- Stable 14.0.0
- State/Nuru code changes

## Notes

- tw-release: bump is 005-001; this task is the cut. Never hand-type tags.
- Parent **005** also gated this cut on **006-005** (`ganda repo audit` error-green). That child is already merged; this kitchen still lists **005-001** as the serial depends-on.

## Session

- Created: 150754 (2026-09-01)
- Implementer: grok (2026-09-02)

## Results

Cut **14.0.0-beta.1** as a NuGet **prerelease** from origin/master via `dev release` (tag derived from `source/Directory.Build.props`; never hand-typed). Trusted-publishing `release:published` workflow pushed all four package ids. GitHub **Latest** remains **v13.0.0**. No `v14.0.0` tag and no stable 14.0.0 nupkg.

### Preconditions

- **005-001** merged as PR [#60](https://github.com/TimeWarpEngineering/timewarp-mediator/pull/60) (`chore: bump Version to 14.0.0-beta.1`).
- **006-005** already merged (audit gate).
- HEAD after **005-002** docs PR [#61](https://github.com/TimeWarpEngineering/timewarp-mediator/pull/61) is `40a9841270ab14d6694c0373bf6d83bb3dde9d6e`. Path filters skipped CI on that docs-only push, so `dev release` would refuse at the CI-at-HEAD guard. Dispatched `gh workflow run workflow.yml --ref master -f mode=merge` → run [33639894185](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/runs/33639894185) success, artifact `Packages-26`.
- Earlier 005-001 merge CI [33611279312](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/runs/33611279312) also green (`Packages-25`) at the bump commit.

### Cut

From the **master** worktree (clean, synced, on `master`), with `CLICOLOR_FORCE` / `FORCE_COLOR` unset so `gh --json` is parseable:

```text
dotnet run --file tools/dev-cli/dev.cs -- release --dry-run
dotnet run --file tools/dev-cli/dev.cs -- release
```

Dry-run: all guards passed (gh auth, clean tree, on master, in sync, tag `v14.0.0-beta.1` available, version not published, CI run 33639894185). Then tag + GitHub Release:

- Tag `v14.0.0-beta.1` at `40a9841`
- Release: https://github.com/TimeWarpEngineering/timewarp-mediator/releases/tag/v14.0.0-beta.1

`dev release` does not pass `--prerelease`. Edited afterward so 13.0.0 stays GitHub Latest:

```bash
gh release edit v14.0.0-beta.1 --prerelease
```

### Publish

`release:published` run [33640437708](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/runs/33640437708) success.

- OIDC `nuget/login@v1` (user `TimeWarp.Enterprises`) exchanged a token.
- Pipeline: clean → build → pack → assert-version-ssot → check-version → push (this repo rebuilds on release rather than promoting the CI artifact).
- Packed and pushed (HTTP **Created**):
  - `TimeWarp.Mediator.14.0.0-beta.1.nupkg` (+ snupkg)
  - `TimeWarp.Mediator.Contracts.14.0.0-beta.1.nupkg` (+ snupkg)
  - `TimeWarp.Mediator.Analyzers.14.0.0-beta.1.nupkg`
  - `TimeWarp.Mediator.Generators.14.0.0-beta.1.nupkg`

### nuget.org

Gallery HTTP 200 for all four, each marked **prerelease**, still indexing at cut time:

| Id | Gallery |
|----|---------|
| TimeWarp.Mediator | https://www.nuget.org/packages/TimeWarp.Mediator/14.0.0-beta.1 |
| TimeWarp.Mediator.Contracts | https://www.nuget.org/packages/TimeWarp.Mediator.Contracts/14.0.0-beta.1 |
| TimeWarp.Mediator.Analyzers | https://www.nuget.org/packages/TimeWarp.Mediator.Analyzers/14.0.0-beta.1 |
| TimeWarp.Mediator.Generators | https://www.nuget.org/packages/TimeWarp.Mediator.Generators/14.0.0-beta.1 |

**13.0.0** still listed (gallery 200; version table still shows 13.0.0). `HEAD` of `/packages/TimeWarp.Mediator/14.0.0` is **404**. No `v14.0.0` tag.

### Docs on this branch

Replaced the stale “nuget.org still serves 13.0.0 until 005-003” sentences in `readme.md` and `documentation/generated-vs-legacy.md`.

### Key decisions / deviations

- Cut from the **master worktree**, not the task branch (required by `dev release` branch guard).
- Agent env `CLICOLOR_FORCE=1` made `gh run list --json` emit ANSI; `dev release` JSON-parsed that and crashed. Reran with `CLICOLOR_FORCE` / `FORCE_COLOR` unset. Not a product change.
- Marked the GitHub Release prerelease after create so **v13.0.0** stays Latest.
- This repo’s release pipeline **rebuilds** nupkgs on the release event (does not download `Packages-*`). That is the current `workflow-command.cs` path; OIDC push succeeded.

### Files changed

- `readme.md`, `documentation/generated-vs-legacy.md` — nuget.org now has 14.0.0-beta.1 prerelease; 13.0.0 remains last stable
- `kanban/in-progress/005-003-…/task.md` — this Results block

### Test outcomes

- Master merge-mode CI at HEAD: success (33639894185)
- Release workflow: success (33640437708); all four `Your package was pushed.`
- No local `dotnet test` gate on the release path (by convention)

### How to validate

**Smoke**

```bash
# GitHub tag + prerelease; 13.0.0 remains Latest
git ls-remote --tags origin 'refs/tags/v14.0.0-beta.1' 'refs/tags/v14.0.0'
gh release list --limit 3
gh release view v14.0.0-beta.1 --json isPrerelease,tagName,url

# Gallery (prerelease pages; 13.0.0 still there; no stable 14.0.0)
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator/14.0.0-beta.1 | head -1
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator.Contracts/14.0.0-beta.1 | head -1
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator.Analyzers/14.0.0-beta.1 | head -1
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator.Generators/14.0.0-beta.1 | head -1
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator/13.0.0 | head -1
curl -sSI https://www.nuget.org/packages/TimeWarp.Mediator/14.0.0 | head -1

# Flatcontainer (may lag the gallery by minutes)
curl -sS https://api.nuget.org/v3-flatcontainer/timewarp.mediator/index.json
curl -sS https://api.nuget.org/v3-flatcontainer/timewarp.mediator.contracts/index.json
curl -sS https://api.nuget.org/v3-flatcontainer/timewarp.mediator.analyzers/index.json
curl -sS https://api.nuget.org/v3-flatcontainer/timewarp.mediator.generators/index.json
```

**Expect**

- Remote tag `v14.0.0-beta.1` present; **no** `v14.0.0`
- `gh release list`: `v14.0.0-beta.1` is **Pre-release**; `v13.0.0` is **Latest**
- `isPrerelease: true` for `v14.0.0-beta.1`
- Gallery HTTP **200** for all four `/14.0.0-beta.1` URLs; page copy includes “This is a prerelease version”
- Gallery HTTP **200** for TimeWarp.Mediator **13.0.0**
- Gallery HTTP **404** for TimeWarp.Mediator **14.0.0** (no stable 14)
- Flatcontainer `versions` arrays include `14.0.0-beta.1` for all four ids; Mediator/Contracts still include `13.0.0`
- Restore (after index): `dotnet add package TimeWarp.Mediator --version 14.0.0-beta.1` and the other three ids at the same version

**Automated gate**

Release already ran on GitHub. Re-prove the pack surface locally (does not push):

```bash
dotnet run --file tools/dev-cli/dev.cs -- pack
# expect: Pack completed successfully! and four 14.0.0-beta.1 nupkgs under artifacts/packages
```

**Not in scope:** stable 14.0.0; State/Nuru consume; unlisting 13.0.0; NuGet search index (lags gallery/flatcontainer).
