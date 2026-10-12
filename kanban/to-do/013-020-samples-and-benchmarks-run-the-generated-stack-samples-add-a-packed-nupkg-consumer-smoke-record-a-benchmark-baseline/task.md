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

- [x] Run-samples step in workflow
- [x] Packed-nupkg consumer smoke
- [x] Samples readme and TFM alignment
- [x] Warning noise removed
- [x] Benchmark command and baseline
- [ ] `./bin/dev workflow` green; PR with merge commit (workflow green locally; PR is opened by the host)

## Notes

Parent: 013. Depends on the target-framework decision for TFM bumps; the rest can proceed.

Review trail: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md` (clean, one round, general, effort 2).

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implement oracle (Claude Opus 5.5) — all product work landed on the task branch.
- Review: Grok `01a1303d-a90a-7872-a205-de2b354d3226` (2026-10-12)

## Results

- **Samples run in CI.** `dev verify-samples [--no-build]` builds every `samples/**/*.csproj`, then
  runs each Exe sample (`dotnet run --no-build`, 2 min timeout) and fails on a non-zero exit code;
  aot must print `aot-pong` and named-pipelines `server:hello`. `dev workflow` (pr/merge) now runs
  clean → build → test → verify-samples (`--no-build`, so nothing is rebuilt) → pack.
- **Four container samples were broken at runtime** (Autofac, LightInject, SimpleInjector, Windsor
  crashed on startup: post-split `IMediator` assembly, two `Mediator` ctors, unregistered
  `IServiceProvider`). Fixed all four. Decision: keep all eight container samples, labelled legacy in
  the new `samples/readme.md` (they prove the reflection `Mediator` still resolves via third-party
  containers; kept, not extended). TFMs: every sample, the shared library and Windsor were already
  `net10.0`, so no bump was needed.
- **Packed-nupkg smoke.** New `tests/timewarp-mediator-package-smoke` (not in the slnx; own
  `Directory.Build.props` that does not import the repo one; `nuget.config` maps `TimeWarp.Mediator.*`
  to `artifacts/packages` only; versions come from a conditioned "Package smoke" group in the root
  `Directory.Packages.props`). `dev pack` restores it into `artifacts/package-smoke/packages`
  (never the global cache, since the version is not bumped per build), builds and runs it:
  Contracts + Generators, `AddGeneratedMediator`, one `Send`. Layout asserts now also require exactly
  one `lib/net10.0` folder in Mediator and Contracts and `buildTransitive/TimeWarp.Mediator.Generators.props`
  in Generators. Pack in release mode runs the same gate before push.
- **Warning noise.** `NoWarn RS0030` in `samples/Directory.Build.props` and both benchmark csprojs
  (the build now has zero warnings, down from 66 RS0030). `tests/Directory.Build.props` no longer sets
  `Nullable=disable` only to be overridden by every csproj; the redundant per-csproj `enable` lines are
  gone and the legacy benchmark project opts out explicitly.
- **Benchmarks.** `dev benchmark [--project comparison|legacy] [--filter] [--record]`; results go to
  `artifacts/benchmarks/<name>`. `--record` writes `documentation/benchmarks/<name>-baseline.md` with
  commit SHA, dirty flag and UTC date plus BenchmarkDotNet's host block. Baseline recorded at
  `d478415` (generated Send 29.9 ns / 96 B vs legacy 69.0 ns / 224 B; martinothamar 8.8 ns).
  `BenchmarkDotNet.Diagnostics.Windows` only on Windows; `Configuration` removed from both csprojs;
  comparison job no longer pins `RuntimeMoniker.Net80` on a net10.0 project and uses
  `BenchmarkSwitcher` so `--filter` works. `m1-benchmark-gap.md` points at the baseline and is linked
  from `generated-vs-legacy.md`.
- Not done: NativeAOT *publish* of the aot sample in CI (it runs JIT; the AOT/trim analyzers still
  gate the build). Benchmarks stay out of `dev workflow` (CI runners too noisy for a stored number).

### Review

- Rounds: 1. Roster: general. Effort: 2 (by-diff, 717 lines).
- Counts: bug 0, suggestion 0, nit 0. No open findings. No wontfix.
- Disposition: clean.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev workflow --mode pr
./bin/dev benchmark --filter '*Generated*'
ganda repo audit
```

Expect:

- `workflow` ends with `Pipeline SUCCEEDED`, prints `Running 11 executable sample(s)...`,
  `Samples verified successfully!`, four `Package layout verified:` lines and
  `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`; the build has 0 warnings.
- `benchmark` exits 0 and writes `artifacts/benchmarks/comparison/results/*-report-github.md`
  (without `--record` the tracked baseline is unchanged).
- `ganda repo audit` reports no blocking failures.
- Negative check: change `aot-pong` in `tools/dev-cli/services/repo-layout.cs` to anything else,
  self-install, run `./bin/dev verify-samples --no-build`: exit code 1 and
  `Sample output is missing ...`.
