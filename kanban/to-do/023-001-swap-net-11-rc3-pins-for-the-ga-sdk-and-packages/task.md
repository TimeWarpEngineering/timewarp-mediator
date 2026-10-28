# Task 023-001: Swap .NET 11 RC3 pins for the GA SDK and packages

## Description

Follow-up to 023. When .NET 11 ships (expected November 2026), replace the RC3 SDK pin, the net11.0 DependencyInjection pin, and the preview CI quality with the GA builds. 023 leaves those pins on RC3 so the net11.0 asset can be built before GA.

## Depends on

- 023

## Requirements

- `global.json`: pin SDK `11.0.100`. Drop the `rc.3` band. Keep `rollForward: latestFeature`. Drop `allowPrerelease` once the pin is a GA SDK.
- `Directory.Packages.props`: the `net11.0` `Microsoft.Extensions.DependencyInjection` pin becomes `11.0.0`. Leave the `net10.0` pin on the 10.0 line. A net10.0 host still must not restore an 11 package.
- `.github/workflows/workflow.yml`: remove `dotnet-quality: preview` from the `11.0.x` setup-dotnet step. Keep the `10.0.x` install so the net10.0 targeting pack and runtime still restore.
- BenchmarkDotNet is `0.16.0-preview.2` on the comparison project because 0.15.8 throws on a net11.0 host. When 0.16.0 stable is on NuGet, move that pin off the preview. Do not re-record `documentation/benchmarks/comparison-baseline.md` (the net10.0 gate from 021-003) as part of that bump.
- `<Version>` is unchanged by this swap. This is not a release.
- `./bin/dev workflow` is green on the GA SDK after the swap.

## Checklist

- [ ] SDK pin is `11.0.100` and CI no longer asks for preview quality
- [ ] net11.0 DependencyInjection pin is `11.0.0`; net10.0 pin stays on 10.0
- [ ] BenchmarkDotNet preview pin replaced when 0.16.0 stable exists, without rewriting the net10.0 gate file
- [ ] `./bin/dev workflow` green

## Notes

- Parent: 023.
- 023 recorded the net11.0 comparison numbers in `documentation/benchmarks/comparison-baseline-net11.md`. That file is not the regression gate.

## Session

- Created: 3691297 (2026-10-28)
- Spec: task 023 implementer (2026-10-28)
