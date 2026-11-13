# Target net11.0 only, drop net10.0

## Description

Steven (2026-11-14): make everything target `net11.0` only. Today `TimeWarp.Mediator`, the tests, the benchmarks, the samples, and the package smoke consumer are `net10.0;net11.0`. Drop `net10.0` everywhere. 15.0.0-beta.7 has not been released yet; this change ships in beta.7. `<Version>` stays `15.0.0-beta.7`.

## Requirements

### Target frameworks

- Every `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>` becomes `<TargetFramework>net11.0</TargetFramework>` (9 projects under `source/`, `tests/`, `samples/`).
- **Keep** `netstandard2.0` on `source/timewarp-mediator-generators` and `source/timewarp-mediator-analyzers`. Roslyn loads them in-process, including Visual Studio and .NET Framework MSBuild. Changing that is a separate decision Steven has not made.
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

- [ ] All 9 projects target `net11.0` only; generator and analyzer stay `netstandard2.0`
- [ ] net10.0 package pins removed
- [ ] Benchmark gate moved to net11.0, baseline re-recorded, old and new numbers in Results
- [ ] `dev pack` asserts exactly `lib/net11.0`
- [ ] CI no longer installs 10.0.x
- [ ] agent.md, release notes (beta.7 and 15.0.0 break list), readmes updated
- [ ] `ganda repo audit` clean
- [ ] `./bin/dev workflow` green
- [ ] `./bin/dev benchmark --gate` green
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-14)

## Notes

- The policy memory said "net10+ only". Steven moved the floor to net11.0 on 2026-11-14.
- If the pre-push hook fails with `Failed to map file ... pre-push.deps.json`, run `ganda runfile cache --clear` and push again.
