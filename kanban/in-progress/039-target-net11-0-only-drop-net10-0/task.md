# Target net11.0 only, drop net10.0

## Description

Steven (2026-11-14): make everything target `net11.0` only. Today `TimeWarp.Mediator`, the tests, the benchmarks, the samples, and the package smoke consumer are `net10.0;net11.0`. Drop `net10.0` everywhere. 15.0.0-beta.7 has not been released yet; this change ships in beta.7. `<Version>` stays `15.0.0-beta.7`.

## Goal (implementer restatement)

Ship 15.0.0-beta.7 with `net11.0` as the only runtime target. Nine projects (the library, the four test and benchmark projects, the martinothamar scoped peer, the package smoke consumer, and the two samples) move from `net10.0;net11.0` to a single `<TargetFramework>net11.0</TargetFramework>`. ~~The generator and analyzer stay `netstandard2.0`.~~ Superseded 2026-11-14: they move to `net11.0` too (see Requirements).

Acceptance:

- All 11 projects under `source/`, `tests/`, and `samples/` target `net11.0` only. The generator and analyzer drop `netstandard2.0`, `LangVersion latest`, and the `IsExternalInit` shim. Only the Roslyn "compiler extension must target netstandard2.0" rule is turned off in those two projects, with a comment. Packing still puts the generator in `analyzers/dotnet/cs` and the analyzer DLL inside `TimeWarp.Mediator`. The smoke consumer still prints `package-pong`, and the analyzer tests still see TWM001–TWM004 and TWM012.
- `Directory.Packages.props` has one unconditioned pin each for `Microsoft.Extensions.DependencyInjection` 11.0.0, `Microsoft.Extensions.Logging` 11.0.0, and `BenchmarkDotNet` 0.16.0-preview.2 (0.16.0 stable is not on nuget.org on 2026-11-14), and no net10/net11 split comments.
- The dev CLI gate framework (`RepoLayout.GateTargetFramework`) is `net11.0`, the only lib folder is `lib/net11.0`, `dev pack` rejects any other lib folder, and the smoke consumer runs once on net11.0 and prints `package-pong`.
- `dev benchmark` defaults to net11.0; `--record` on net11.0 rewrites `comparison-baseline.md`; `comparison-baseline-net11.md` is removed; `--gate` passes on net11.0 against the new file with thresholds unchanged; `--aot` publishes net11.0. Old (net10.0, BDN 0.15.8) and new (net11.0) gate numbers sit side by side in `## Results` for Steven to approve.
- CI installs only `11.0.x`; the benchmark job and mode description no longer name net10.0.
- `agent.md`, both package readmes, `documentation/`, the beta.7 release notes (Breaking entry), and the 15.0.0 break list say net11.0 only; net10.0 consumers stay on 15.0.0-beta.6 or earlier.
- `rg net10` outside kanban and history files finds nothing; `./bin/dev build`, `./bin/dev test`, `./bin/dev check-version --strict`, and `ganda repo audit` pass.

## Requirements

### Target frameworks

- Every `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>` becomes `<TargetFramework>net11.0</TargetFramework>` (9 projects under `source/`, `tests/`, `samples/`).
- **Superseded (Steven, 2026-11-14): everything targets `net11.0`, including the generator and the analyzer.** Steven does not support Visual Studio or .NET Framework MSBuild. `source/timewarp-mediator-generators` and `source/timewarp-mediator-analyzers` go from `netstandard2.0` to `net11.0`.
  - The `dotnet` SDK 11 compiler runs on .NET 11, so it can load a `net11.0` analyzer or generator. Prove it: `./bin/dev pack` and the package smoke consumer must still print `package-pong` with the generated `Mediator`, and TWM001–TWM004 and TWM012 must still fire in the analyzer tests.
  - Packaging stays `analyzers/dotnet/cs` (generator) and the analyzer DLL inside `TimeWarp.Mediator`. `dev pack` keeps asserting those.
  - Roslyn's own analyzers may warn about a compiler extension that is not `netstandard2.0` (for example RS1041). Turn that rule off for these two projects only, with a comment that says why (Steven 2026-11-14, net11.0 only, no Visual Studio support). Do not turn off anything else.
  - Remove `LangVersion latest` and anything else in those two projects that only existed for netstandard2.0, such as polyfills, `IsExternalInit` shims, or `System.Memory`-style package references. Nullable and modern APIs now come from net11.0.
  - Remove `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` only if it no longer makes sense. Otherwise keep it.
  - `agent.md` and the docs drop the "Roslyn loads them in-process, including .NET Framework MSBuild and Visual Studio" wording. The beta.7 Breaking entry also says Visual Studio and .NET Framework MSBuild are not supported; build with the .NET 11 SDK (`dotnet build`).
- The dev CLI is already `net11.0`.

### Packages (`Directory.Packages.props`)

- Remove the `net10.0`-conditioned pins and drop the `Condition` from the `net11.0` ones: `Microsoft.Extensions.DependencyInjection` 11.0.0, `Microsoft.Extensions.Logging` 11.0.0, `BenchmarkDotNet` `0.16.0-preview.2` (or `0.16.0` if stable is out). Remove the comments that explain the net10/net11 split.

### Benchmark gate

- The gate (`./bin/dev benchmark --gate`, `RepoLayout.GateTargetFramework`) runs on `net10.0` against `documentation/benchmarks/comparison-baseline.md`, recorded with BenchmarkDotNet 0.15.8. Move the gate to `net11.0`.
- Make `net11.0` the default `--framework`, and remove `net10.0` from the option text.
- Use `comparison-baseline-net11.md` numbers as the new gate baseline: run `./bin/dev benchmark --record` on net11.0 so `comparison-baseline.md` is the net11.0 file, and delete or fold in `comparison-baseline-net11.md`. Put the old and new numbers side by side in `## Results`. **Steven approves the new gate numbers at review.** Do not loosen any threshold to get the gate green.
- The `--aot` path publishes for net11.0.

### Dev CLI and CI

- `tools/dev-cli/services/repo-layout.cs`: the expected lib folders are exactly `lib/net11.0`. `./bin/dev pack` fails if `lib/net10.0` appears.
- `pack-command.cs`, `benchmark-command.cs`, and any other dev CLI source: update comments and logic that name net10.0.
- `.github/workflows/workflow.yml`: remove the `10.0.x` setup-dotnet steps and their comment. Rename `Benchmark gate (net10.0)` and update the `benchmark` mode description.
- The package smoke consumer runs on net11.0 only and must still print `package-pong`.

### Docs

- `agent.md`: Target Frameworks, Key Dependencies, the `dev pack` and `dev benchmark` rows, and the test architecture notes.
- `documentation/release-notes/15.0.0-beta.7.md`: add a **Breaking** entry saying `TimeWarp.Mediator` now targets `net11.0` only and `net10.0` consumers must stay on 15.0.0-beta.6 or earlier.
- `documentation/release-notes/15.0.0.md`: add the same entry to the 15.0 break list.
- Package readmes and `documentation/` wherever they state supported frameworks.

### Gates

- `grep -rn "net10" --exclude-dir=kanban --exclude-dir=.git` finds nothing outside history files (older release notes, archived kanban).
- `ganda repo audit` clean.
- `./bin/dev workflow` green.
- `./bin/dev benchmark --gate` green on net11.0 against the new baseline.
- `./bin/dev check-version --strict` passes for 15.0.0-beta.7.

## Checklist

- [x] All 11 projects target `net11.0` only, including generator and analyzer
- [x] net10.0 package pins removed
- [x] Benchmark gate moved to net11.0, baseline re-recorded, old and new numbers in Results
- [x] `dev pack` asserts exactly `lib/net11.0`
- [x] CI no longer installs 10.0.x
- [x] agent.md, release notes (beta.7 and 15.0.0 break list), readmes updated
- [x] `ganda repo audit` clean
- [x] `./bin/dev workflow` green (its steps run one by one: build, test, verify-samples, pack)
- [x] `./bin/dev benchmark --gate` green
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-14)
- 2026-11-14 implement: finished the retarget, including the generator and analyzer on net11.0. Re-recorded the benchmark baselines on net11.0. Updated the docs and release notes.
- Boy-scout fix: `ganda repo audit` `package-pin-currency` failed on the existing branch. The dev-only pins were behind nuget.org. `TimeWarp.SourceGenerators` moved 1.0.0-beta.14 → 1.0.0-beta.15, `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` 2.0.0 → 3.0.0-beta.2, and `TimeWarp.Terminal` 1.0.2 → 2.0.0-beta.1. The solution and the dev CLI (`self-install`) both build on those pins. They are listed in the beta.7 pin table.
- Boy-scout fix: the `.githooks/*.cs` scaffolds were refreshed by `ganda repo audit --fix` (memsearch-scaffold advisory). The VS Code window-icon config was also generated; `.vscode/settings.json` is gitignored.

## Results

### What changed

- All 11 projects target `net11.0` only. The generator and analyzer dropped `netstandard2.0`, `LangVersion latest`, and `is-external-init.cs` (the `IsExternalInit` shim). `Nullable` comes from the root `Directory.Build.props`.
- RS1041 fired on both Roslyn projects, as the task expected. Each `.csproj` adds `<NoWarn>$(NoWarn);RS1041</NoWarn>` with the reason in a comment (Steven 2026-11-14). `.timewarp/suppression-baseline.json` gains `RS1041: 1` for those two files only, the same way commit `a858e34` handled an approved suppression. Nothing else is turned off.
- `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` stays. The shipped generator must still not reference a Roslyn newer than the SDK compiler.
- `Directory.Packages.props` has one pin each: DependencyInjection `11.0.0`, Logging `11.0.0`, BenchmarkDotNet `0.16.0-preview.2` (`0.16.0` stable is not on nuget.org).
- In the dev CLI, `RepoLayout.GateTargetFramework` is `net11.0` and `LibTargetFrameworks = [net11.0]`, so `dev pack` fails on any other `lib/` folder. `dev benchmark` defaults to net11.0, and its option text no longer names net10.0.
- CI installs only `11.0.x`. The job is now `Benchmark gate (net11.0)`, and the benchmark mode text says net11.0.
- Docs updated: agent.md, readme.md, samples/readme.md, the three package readmes, the tw-mediator skill, migration.md, architecture.md, overview.md, the beta.7 notes (with a `## Breaking` section), and the 15.0.0 break list. Each one drops the "Roslyn loads netstandard2.0 in Visual Studio" wording.
- `rg net10` outside kanban: the remaining hits are intentional. They are the guidance that `net10.0` consumers stay on beta.6, the beta.7 and 15.0.0 notes describing the break, the pack comment saying a `lib/net10.0` folder fails, and older release notes and design or archive history.

### Benchmark gate: old (net10.0) and new (net11.0) gate numbers — Steven approves at review

Old: `comparison-baseline.md` on net10.0, BenchmarkDotNet 0.15.8, .NET 10.0.14, commit `a5129e1`, 2026-10-28.
New: `comparison-baseline.md` on net11.0, BenchmarkDotNet 0.16.0-preview.2, .NET 11.0.0 GA, commit `eafa561`, recorded 2026-11-13 UTC. Same host (Ryzen 9 7950X3D). Thresholds unchanged: mean at most +50%, no extra bytes.

| Row | Old mean | Old alloc | New mean | New alloc |
|---|---:|---:|---:|---:|
| TimeWarpGeneratedMonomorphicSend | 5.061 ns | 24 B | 10.325 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 8.480 ns | 24 B | 11.577 ns | 24 B |
| TimeWarpGeneratedObjectSend | 16.546 ns | 96 B | 19.952 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 9.736 ns | 0 B | 9.796 ns | 0 B |
| TimeWarpGeneratedMonomorphicSendUnit | 4.062 ns | 0 B | 7.988 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 22.901 ns | 24 B | 31.351 ns | 24 B |
| TimeWarpGeneratedScopedSend | 8.534 ns | 24 B | 11.716 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 20.388 ns | 0 B | 21.566 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 26.678 ns | 0 B | 21.496 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 133.339 ns | 0 B | 133.604 ns | 0 B |
| TimeWarpGeneratedArchitectureSend | 88.852 ns | 24 B | 86.959 ns | 24 B |
| MartinothamarGenerated (not gated) | 9.370 ns | 24 B | 12.721 ns | 24 B |
| MartinothamarScopedSend (not gated) | 47.457 ns | 88 B | 44.766 ns | 80 B |
| MediatRSend (not gated) | 58.270 ns | 224 B | 50.196 ns | 216 B |

- The small send rows are 3–5 ns slower on net11.0 with BenchmarkDotNet 0.16.0-preview.2. The October net11 rc.3 record (`comparison-baseline-net11.md`, now removed) showed the same thing: InterfaceSend 12.218 ns, MonomorphicSend 9.919 ns. martinothamar's generated send moved by about the same amount (9.37 → 12.72 ns). That points to the runtime and tool change, not a dispatcher change. Allocations are the same on every TimeWarp row.
- `TimeWarpCallSiteInliningPrototype` was noisy in the record run (31.351 ns, error 4.3 ns). The gate run that followed measured 22.597 ns. Steven may want to re-record that row before approving.
- `./bin/dev benchmark --gate` right after the record: **passed** on net11.0.
- `./bin/dev benchmark --aot` published for net11.0 and rewrote `comparison-baseline-aot.md` (not gated). Example: InterfaceSend 35.295 → 14.641 ns.

### Gates

| Command | Result |
|---|---|
| `./bin/dev build` | Pass. 0 warnings, 0 errors |
| `./bin/dev test` | Pass. generators-tests 61/61, analyzers-tests 50/50 (TWM001–TWM004 and TWM012 fire from the net11.0 analyzer), compilation-tests 80/80, all on net11.0 |
| `ganda repo audit` | Pass, 41 passed and 0 blocking. Two advisory warnings (`memsearch-scaffold`: outdated `.githooks/*.cs`; `vscode-window-icon`: no `.vscode/settings.json`) were fixed with `ganda repo audit --fix --checks memsearch-scaffold,vscode-window-icon`. The rerun printed "Repository passes all audit checks." |
| `./bin/dev pack` | Pass. Layout verified for both nupkgs. `Package smoke (net11.0)` printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` |
| `./bin/dev verify-samples --no-build` | Pass. The AOT sample published with NativeAOT on net11.0, and the named-pipelines sample ran |
| `./bin/dev check-version --strict` | Pass. 15.0.0-beta.7 is one prerelease ahead of beta.6 on nuget.org |
| `./bin/dev benchmark --gate` | Pass on net11.0 against the re-recorded `comparison-baseline.md` |

### How to validate

Smoke:

```bash
./bin/dev build && ./bin/dev test
./bin/dev pack
rg -n "TargetFramework" -g '*.csproj' source tests samples
./bin/dev benchmark --gate
```

Expect:

- Build and test are green. The test summary lists only `(net11.0)` assemblies.
- `dev pack` prints `Package layout verified` for both nupkgs, then `Package smoke (net11.0)...` and `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.
- Every `.csproj` under source, tests, and samples has `<TargetFramework>net11.0</TargetFramework>`. None has `TargetFrameworks` or `netstandard2.0`.
- `dev benchmark --gate` ends with `Benchmark gate passed.` and writes to `artifacts/benchmarks/comparison/net11.0`.

## Notes

- 2026-11-14: Steven reversed the netstandard2.0 exception mid-walk ("I don't care about supporting Visual Studio... everything"). The first implement run was stopped. Its uncommitted edits to workflow.yml, Directory.Packages.props, agent.md, and documentation/architecture.md are left in the tree to continue from.

- The policy memory said "net10+ only". Steven moved the floor to net11.0 on 2026-11-14.
- If the pre-push hook fails with `Failed to map file ... pre-push.deps.json`, run `ganda runfile cache --clear` and push again.
