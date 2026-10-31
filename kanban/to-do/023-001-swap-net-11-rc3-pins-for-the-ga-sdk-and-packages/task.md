# Task 023-001: Swap .NET 11 RC3 pins for the GA SDK and packages

## Description

Follow-up to 023. When .NET 11 ships (expected November 2026), replace the RC3 SDK pin, the net11.0 DependencyInjection pin, and the preview CI quality with the GA builds. 023 leaves those pins on RC3 so the net11.0 asset can be built before GA.

## Depends on

- 023

## Requirements

- `global.json`: pin SDK `11.0.100`. Drop the `rc.3` band. Keep `rollForward: latestFeature`. Drop `allowPrerelease` once the pin is a GA SDK.
- `Directory.Packages.props`: the `net11.0` `Microsoft.Extensions.DependencyInjection` pin becomes `11.0.0`. Leave the `net10.0` pin on the 10.0 line. A net10.0 host still must not restore an 11 package.
- `.github/workflows/workflow.yml`: remove `dotnet-quality: preview` from the `11.0.x` setup-dotnet step. Keep the `10.0.x` install so the net10.0 targeting pack and runtime still restore.
- BenchmarkDotNet is pinned per target framework: `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`. `0.15.8` recorded `documentation/benchmarks/comparison-baseline.md`. `0.15.8` throws on a net11.0 host before it measures, so the net11.0 leg stays on the preview until `0.16.0` stable is on NuGet. Move only the `net11.0` pin off the preview. Leave the `net10.0` pin on `0.15.8`.
- Keep `./bin/dev benchmark --gate` green. That gate is the net10.0 check against `comparison-baseline.md`. On 2026-10-28 it passed on BenchmarkDotNet `0.15.8` and .NET 10.0.14, and it had failed on `0.16.0-preview.2` for the two monomorphic rows. Do not re-record `comparison-baseline.md` as part of the stable bump. A later move of the `net10.0` pin has to re-run the gate and leave it green: stay on `0.15.8`, or re-record that file only after Steven approves the new numbers.
- `<Version>` is unchanged by this swap. This is not a release.
- `./bin/dev workflow` is green on the GA SDK after the swap.

## Checklist

- [ ] SDK pin is `11.0.100` and CI no longer asks for preview quality
- [ ] net11.0 DependencyInjection pin is `11.0.0`; net10.0 pin stays on 10.0
- [ ] net11.0 BenchmarkDotNet pin is off the preview when 0.16.0 stable exists; net10.0 stays on 0.15.8
- [ ] `./bin/dev benchmark --gate` is green. Re-record `comparison-baseline.md` only after Steven approves the new numbers
- [ ] `./bin/dev workflow` green

## Notes

- Parent: 023.
- 023 recorded the net11.0 comparison numbers in `documentation/benchmarks/comparison-baseline-net11.md`. That file is not the regression gate.
- Dependabot's NuGet updater cannot install the SDK that `global.json` pins (`11.0.100-rc.3`): run 41181970758 on 2026-10-31 failed with `dotnet_install: Error: Could not find .NET Core SDK with version = 11.0.100-rc.3` and then `Missing SDK, check global.json locations vs. job directories`. It had not run on NuGet since the pin landed in task 023 on 2026-10-28, so this is the first failure, not a regression from task 026. Weekly NuGet Dependabot PRs are off until this task moves `global.json` to the GA `11.0.100`. After the swap, confirm the next `Dependabot Updates` run for the `nuget` ecosystem succeeds and record its run id in Results.

## Session

- Created: 3691297 (2026-10-28)
- Spec: task 023 implementer (2026-10-28)
- Gate item: task 023 implement-fix-4 (2026-10-28). The net10.0 gate passed on BenchmarkDotNet 0.15.8. This task keeps that gate green when the net11.0 pin leaves the preview.
