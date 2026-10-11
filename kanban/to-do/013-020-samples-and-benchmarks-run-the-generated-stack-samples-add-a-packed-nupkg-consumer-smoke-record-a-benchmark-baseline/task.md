# Samples and benchmarks: run the generated-stack samples, add a packed-nupkg consumer smoke, record a benchmark baseline

## Description

Samples compile but never run, all use ProjectReference, and benchmarks produce no stored results. Parent review task 013, findings B-19, B-20, D-11, D-12, D-13, D-14, D-22, D-23.

- D-13 / B-20 (debt): `verify-samples` rebuilds what the slnx already compiles; no sample is executed, so the AOT and named-pipelines samples prove nothing in CI. Nothing restores the packed nupkgs into a clean consumer project, so a broken buildTransitive props or TFM folder is only seen after publishing.
- D-11 / D-12 (debt): eight DI-container samples exercise only legacy `AddMediator`; Windsor targets net6.0; the shared library targets netstandard2.1.
- D-14 (debt): samples and tests disable warnings-as-errors and Nullable; RS0030 banned-API warnings (Console, ProcessStartInfo) are noise there; the test csproj re-enables Nullable in a way the props may override.
- D-22 / D-23 (debt): benchmarks reference `BenchmarkDotNet.Diagnostics.Windows` unconditionally, set `Configuration` in a PropertyGroup, have no `dev benchmark` command, and the only results live in `m1-benchmark-gap.md` with hard-coded host details and no commit SHA.

## Requirements

- Make `verify-samples` (or a test) run the aot and named-pipelines samples and assert exit code 0; wire it into `dev workflow`.
- Add a post-pack smoke project that references Contracts and Generators from the local artifacts feed and builds; extend the layout asserts to lib/ TFM folders for Mediator and Contracts.
- Label container samples as legacy in a `samples/readme.md`; decide which to keep; bump TFMs per the framework-floor decision.
- Explicit `NoWarn` for RS0030 in samples and benchmarks, or use ITerminal; fix the Nullable conflict in the tests props.
- `dev benchmark` command; condition the Windows diagnoser; store a results baseline with commit SHA and date; link `m1-benchmark-gap.md` from `generated-vs-legacy.md`.

## Checklist

- [ ] Run-samples step in workflow
- [ ] Packed-nupkg consumer smoke
- [ ] Samples readme and TFM alignment
- [ ] Warning noise removed
- [ ] Benchmark command and baseline
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Depends on the target-framework decision for TFM bumps; the rest can proceed.

## Session

- Created: 686431 (2026-10-11)
