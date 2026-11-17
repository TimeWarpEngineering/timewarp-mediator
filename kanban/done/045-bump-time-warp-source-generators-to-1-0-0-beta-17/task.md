# Bump TimeWarp.SourceGenerators to 1.0.0-beta.17

## Description

`ganda nuget outdated` (2026-11-17) reports `TimeWarp.SourceGenerators` 1.0.0-beta.16 -> 1.0.0-beta.17.
Update the pin in `Directory.Packages.props`.

The package is not a consumer dependency: `Directory.Build.props` references it with
`PrivateAssets="all"`, so no nupkg lists it as a dependency. It still runs as a source
generator when every project that does not remove it compiles, and that includes
`source/timewarp-mediator` (the shipped `TimeWarp.Mediator` assembly). A generator change can
change the shipped DLL. The generators project, the analyzers project, two test projects, and
one sample remove the reference.

## Goal (implementer restatement)

Move the central pin of `TimeWarp.SourceGenerators` from 1.0.0-beta.16 to 1.0.0-beta.17.
It is a build-only generator (`PrivateAssets="all"`), but it runs in `source/timewarp-mediator`,
so the shipped `TimeWarp.Mediator` DLL could change.

Acceptance:
- `Directory.Packages.props` pins 1.0.0-beta.17.
- Results say what changed between beta.16 and beta.17.
- Results say whether the generated source in `TimeWarp.Mediator` changed (diff of
  `EmitCompilerGeneratedFiles` output from both versions) and whether its public API changed.
- `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples`, `./bin/dev pack` pass, and
  `ganda repo audit` passes.
- No version bump unless the shipped change needs a release note.

## Checklist

- [x] Bump `TimeWarp.SourceGenerators` to `1.0.0-beta.17` in `Directory.Packages.props`
- [x] Read the beta.16 -> beta.17 changes (release notes or package diff) and note in Results what changed
- [x] Check whether the generated source in `TimeWarp.Mediator` changes (for example build both versions with `EmitCompilerGeneratedFiles=true` and diff), or whether its public API changes
- [x] `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples`, `./bin/dev pack` pass
- [x] If the shipped output changes, say so in Results; no version bump unless the change needs a release note

## Session

- Created: 3d261cef-4c88-4500-a89f-105b82c12ac0 (2026-11-17)
- Implemented 2026-11-17 by the implement oracle (Claude Opus 5.5) under `ganda task work`
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 4e297d87-c0c9-4656-bc7a-51368a603563, max-turns 500, Done — 2026-11-17T14:03:49Z
- Review oracle: grok session 01a1e9d2-5336-7e72-b155-d44611606a08 (2026-11-17), effort 1, roster general
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1e9d2-5336-7e72-b155-d44611606a08, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-17T14:08:31Z

## Review

- Round 1: Done

## Notes

Not a breaking change to a stable package (15.0 is beta), so a normal walk may merge.

Review trail: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Results

Bumped `TimeWarp.SourceGenerators` from 1.0.0-beta.16 to 1.0.0-beta.17 in `Directory.Packages.props`.
That is the only product change.

### What changed upstream (beta.16 -> beta.17)

- Upstream commits `addddba..b288fbd` (timewarp-source-generators PRs #52 and #53, release task 044).
  The only analyzer change is a TWA0038 fix ("file longer than 500 lines"): the rule now
  reports only files the consuming repository owns. It skips `.cs` under the NuGet package
  root, under `obj/` or `goldens/`, and outside both the project directory and
  `$(RepositoryRoot)`. A new `dotnet_diagnostic.TWA0038.exclude_paths` key takes globs.
- The package adds `build/TimeWarp.SourceGenerators.props` and
  `buildTransitive/TimeWarp.SourceGenerators.props`, which expose `NuGetPackageRoot`,
  `ProjectDir`, and `RepositoryRoot` to the analyzer. The rest is CI and kanban work.
- The package holds analyzers only (one `timewarp-source-generators.dll`). It does not generate source.

### Shipped output

- No change. Building `source/timewarp-mediator` with `EmitCompilerGeneratedFiles=true` writes
  zero generated files under both beta.16 and beta.17.
- `TimeWarp.Mediator.dll` (Debug build of `source/timewarp-mediator`) is byte-identical
  (`cmp`) between beta.16 and beta.17, so its public API is unchanged.
- Neither packed nuspec (`TimeWarp.Mediator`, `TimeWarp.Mediator.Generators` 15.0.0-beta.10)
  lists `TimeWarp.SourceGenerators` as a dependency.
- No version bump and no release note: nothing shipped changed.

### Gates

| Command | Result |
|---|---|
| `./bin/dev build` | Pass, 0 warnings, 0 errors |
| `./bin/dev test` | Pass: generators-tests 61/61, analyzers-tests 50/50, generators-compilation-tests 93/93 |
| `./bin/dev verify-samples` | Pass (aot NativeAOT publish and run, generated-requests, named-pipelines) |
| `./bin/dev pack` | Pass: layout verified for both nupkgs; package smoke printed `package-pong` |
| `ganda repo audit` | Pass ("Repository passes all audit checks"). The first run had one advisory warning, `vscode-window-icon` (`.vscode/settings.json` missing). `ganda repo audit --fix` wrote that gitignored local file, so no tracked change |

### How to validate

Smoke:

```bash
grep TimeWarp.SourceGenerators Directory.Packages.props
./bin/dev build && ./bin/dev test && ./bin/dev pack
```

Expect: the pin reads `Version="1.0.0-beta.17"`. Build has 0 warnings, all 204 tests pass, and
pack ends with `package-smoke: package-pong` and `Pack completed successfully!`.

### Review

Round 1, effort 1, reviewer `general`. Counts: bug 0 open / 0 fixed / 0 wontfix, suggestion 0/0/0, nit 0/0/0. Final open count 0. Disposition: clean. No wontfix and no escalation. The implementer restatement matches the original task, so there is no misread under Learnings.

Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

