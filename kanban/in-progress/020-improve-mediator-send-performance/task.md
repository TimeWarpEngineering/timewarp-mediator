# Task 020: Improve Mediator send performance

## Description

Refreshed 2026-10-28 against the 15.0 line (15.0.0-beta.2 released today). The original body
(2026-10-13) measured the 14.0 generated mediator at 29.9 ns / 96 B per send against
martinothamar/Mediator at 8.8 ns / 24 B and listed the causes: Task-returning handlers, an async
wrapper, pipeline closures built per call, scoped `GetRequiredService` per send. The 15.0 train
(task 021, children 021-001 and 021-003) removed those causes. Recorded net10.0 baseline
(`documentation/benchmarks/comparison-baseline.md`, commit c5e4a3a):

| Row | Mean | Allocated |
| --- | --- | --- |
| TimeWarpGeneratedInterfaceSend | 8.243 ns | 24 B |
| TimeWarpGeneratedMonomorphicSend | 5.016 ns | 24 B |
| TimeWarpGeneratedInterfaceSendUnit | 9.360 ns | 0 B |
| TimeWarpGeneratedObjectSend | 15.447 ns | 96 B |
| TimeWarpGeneratedScopedSend | 10.753 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 20.024 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 131.117 ns | 0 B |
| MartinothamarGenerated | 8.314 ns | 24 B |

Delivered by 15.0 and no longer in scope here: `ValueTask` on every handler and behavior, no
async wrapper, the handler and behavior chain cached per mediator instance (`HandlerCache`,
`??=` fields in `mediator-emitter.cs`), `bin/dev benchmark` with `--record` and `--gate`,
publish and behavior-pipeline rows, and the net11.0 record
(`documentation/benchmarks/comparison-baseline-net11.md`, task 023). Streams are removed, so
the stream row from the original body is dropped.

What remains is the opt-in singleton mode and the benchmark-suite gaps. The interface send
target (≤ 1.5× martinothamar, ≤ 24 B) is already met; do not regress it.

## Goal (implementer restatement)

The 15.0 send path already meets the interface-send target: within 1.5× of martinothamar and at most 24 B. This task records that path honestly and fills the benchmark gaps around it. It does not put back `Task` handlers, an async wrapper, per-send pipeline closures, or a scoped resolve on every send.

Done means:

- Singleton is not a public registration. A paired net10.0 run (BenchmarkDotNet 0.15.8) of a singleton dispatcher against a transient dispatcher that the benchmark resolves once sits inside the noise at the same 24 B. `AddGeneratedMediator` stays parameterless and transient. `## Results` holds the numbers and the recommendation to Steven to leave that API out. `documentation/architecture.md` cites those numbers.
- External rows match the path users run. `MartinothamarGenerated` keeps that generator's Singleton default. Forcing Transient made that row about five times slower and is not what this gate should store. `MartinothamarScopedSend` uses the same scoped dependency as `TimeWarpGeneratedScopedSend`, in its own compilation because that generator emits one lifetime. `MediatRSend` sends the same `Ping`. Handler bodies match inside each pair. The interface-send gate does not regress.
- `comparison-baseline.md` is re-recorded on net10.0 from the committed tree after that lifetime fix, with BenchmarkDotNet 0.15.8. `./bin/dev benchmark --gate` is green on that file. Tolerances stay 50% on mean time and no extra bytes.
- `./bin/dev benchmark --aot` publishes the comparison project with `PublishAot` and writes `documentation/benchmarks/comparison-baseline-aot.md`. The runner stores each result in a generic static field so a value-type result is not boxed, and the file header says the mean includes a delegate call. Those numbers are not gated.
- The `benchmark` job in `.github/workflows/workflow.yml` runs `./bin/dev benchmark --gate` on net10.0 on a weekly schedule and on `workflow_dispatch`, not on pull requests. Three consecutive hosted-runner results inside the tolerance were not collected, so the job stays off the pull-request path. That decision is in `documentation/overview.md` and `## Results`.
- A failed AOT publish or AOT run assigns that process exit code directly.
- `ganda repo audit` is clean, `./bin/dev workflow` is green on net10.0 and net11.0, and `<Version>` stays `15.0.0-beta.2`.

## Requirements

### Opt-in singleton mediator (never the default)

- `AddGeneratedMediator()` keeps registering `Mediator`, `IMediator`, `ISender`, `IPublisher`
  (and the `<TScope>` forms) as transient. Add one optional parameter,
  `ServiceLifetime lifetime = ServiceLifetime.Transient`, reusing the BCL enum. No new type,
  no options class, no MSBuild property. `ServiceLifetime.Scoped` is also accepted and behaves
  like transient per scope.
- `ServiceLifetime.Singleton` registers the mediator once; the `HandlerCache` it already
  resolves then lives for the container lifetime, so handlers and behaviors are resolved once.
  Document in `documentation/architecture.md` that singleton mode is only correct when every
  handler and behavior is singleton or transient with no scoped dependencies (`DbContext`,
  per-request services), and that the generator cannot verify that. No runtime check in the hot
  path; a startup check in `AddGeneratedMediator` is acceptable if it costs nothing per send.
- Add a `TimeWarpGeneratedSingletonSend` benchmark row and a compilation test that the singleton
  registration resolves the same `Mediator` instance across scopes while transient does not.
- Record the measured gain in `## Results`. If singleton mode is not measurably faster than
  the cached transient path on net10.0, say so and recommend to Steven whether to keep it;
  do not ship an API that buys nothing.

### Benchmark suite

- Like-for-like martinothamar rows: a `MartinothamarScopedSend` row whose handler takes the same
  scoped dependency as `TimeWarpGeneratedScopedSend` (`scoped-ping.cs`), and, if martinothamar
  supports a singleton mediator, a row matching the new singleton row. Align handler bodies and
  lifetimes across every compared pair and state the pairing in a comment on each row.
- A `MediatR` reference row (`MediatR` package, latest stable) for `Send` on the same `Ping`.
- A Native AOT lane: a `dev benchmark --aot` option (or a documented manual procedure if
  BenchmarkDotNet's `NativeAotToolchain` cannot run in this repo) that publishes the comparison
  project with `PublishAot` and runs the `TimeWarp*` rows. Record the numbers in
  `documentation/benchmarks/comparison-baseline-aot.md`; the AOT numbers are not gated.
- Re-record `comparison-baseline.md` on net10.0 with BenchmarkDotNet 0.15.8 (the gate pin) so
  the new rows have a baseline, then confirm `./bin/dev benchmark --gate` is green. Do not
  loosen the gate tolerances.
- CI tracking: add a `benchmark` job to `.github/workflows/workflow.yml` that runs
  `./bin/dev benchmark --gate` on net10.0. Run it on `workflow_dispatch` and a weekly schedule,
  not on every PR, unless three consecutive runner results stay inside the 50% tolerance; record
  the runner numbers in `## Results` and the decision in `documentation/overview.md`.

### Hygiene

- `ganda repo audit --fix` applied and committed.
- `./bin/dev workflow` green on both TFMs. `<Version>` stays `15.0.0-beta.2`; not a release.
- `agent.md` benchmark row and `documentation/benchmarks/` readme (if any) list the new
  options and files.

## Checklist

### Implementation
- [x] `AddGeneratedMediator` stays parameterless and transient. Singleton was measured and is not part of the API
- [x] Paired singleton measurement recorded, with the recommendation to leave the parameter off
- [x] `MartinothamarGenerated` stays that generator's Singleton default; `MartinothamarScopedSend` and `MediatRSend` are in the baseline
- [x] Native AOT lane and `comparison-baseline-aot.md` (generic sink, delegate overhead in the header)
- [x] `comparison-baseline.md` re-recorded on 0.15.8 from commit `a5129e1`; `--gate` green
- [x] `benchmark` CI job on dispatch and schedule. Three hosted-runner results were not collected, so it stays off pull requests
- [x] `./bin/dev workflow` green on net10.0 and net11.0
### Documentation
- [x] `documentation/architecture.md` singleton-mode section cites the paired numbers
- [x] `documentation/overview.md` benchmark CI note; `agent.md` benchmark row
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5. Fix pass below addresses them. Round 2 is the host's next node.

## Results

Singleton stays off the public API. A paired net10.0 run (BenchmarkDotNet 0.15.8, IterationCount=8, WarmupCount=3, same job) measured a transient dispatcher that the benchmark resolves once against a singleton dispatcher:

| Row | Mean | Error | Allocated |
| --- | ---: | ---: | ---: |
| TimeWarpGeneratedInterfaceSend (reused transient) | 10.320 ns | 0.578 ns | 24 B |
| TimeWarpGeneratedSingletonSend | 9.776 ns | 0.884 ns | 24 B |

The ratio is 0.95 and the ratio standard deviation is 0.05. The 0.544 ns gap sits inside the singleton error bar. Both rows allocate 24 B. `LiveCache` already keeps the handler for the life of the instance the benchmark holds, so a singleton registration does not change `Send`. Recommendation to Steven: do not add a `ServiceLifetime` parameter. It buys nothing on this path, and disposing the scope disposes that cache (`ObjectDisposedException` on the next send). `documentation/architecture.md` cites this table.

`MartinothamarGenerated` is that generator's Singleton default again. Forcing Transient had recorded 42.790 ns and 112 B, which is not the path their users run. The re-record from commit `a5129e19c5fb2a6bed40f183db40020111db8856` (clean tree, BenchmarkDotNet 0.15.8, net10.0) stores 9.370 ns (error 1.429 ns) and 24 B. `TimeWarpGeneratedInterfaceSend` in that same file is 8.480 ns (error 0.261 ns) and 24 B, inside 1.5× of martinothamar and at the 24 B cap. The earlier uncommitted record had raised that row to 10.116 ns with a ±2.31 ns error. The host was sharing the CPU (load above 4). `TimeWarpGeneratedMonomorphicSend` in the stored file is 5.061 ns, matching the previous 5.016 ns mean, with a wider ±2.725 ns error.

`./bin/dev benchmark --gate` on that file printed `Benchmark gate passed.` (2026-10-28, net10.0, 14 benchmarks, run time 00:02:51). Tolerances are unchanged: 50% on mean time, no extra bytes. External rows are not gated.

`./bin/dev benchmark --aot` wrote `documentation/benchmarks/comparison-baseline-aot.md` at commit `3c5c08c47a408b1907e4f358bc4a4e5ad297e315` (clean tree, `linux-x64`, net10.0). The header states that each iteration goes through a delegate and that the result is stored in a generic static field. Allocations that the old boxing sink had inflated now match BenchmarkDotNet: `TimeWarpGeneratedMonomorphicSendUnit` 0 B (was 24 B) and `TimeWarpGeneratedArchitectureSend` 24 B (was 56 B). The AOT numbers are not gated.

The `benchmark` job stays on the weekly schedule (`17 6 * * 1` UTC) and on `workflow_dispatch` with `mode=benchmark`. Three consecutive hosted-runner results inside the tolerance were not collected, so the job stays off pull requests. That decision is in `documentation/overview.md`.

`<Version>` stays `15.0.0-beta.2`.

`./bin/dev workflow` (mode Pr: clean, build, test, verify-samples, pack) exited 0 on 2026-10-28. Tests passed on net10.0 and net11.0 (generators 61, analyzers 42, compilation 69 on each TFM). Both sample frameworks ran. Pack produced `TimeWarp.Mediator.15.0.0-beta.2`, and the package smoke printed `package-pong` on net10.0 and net11.0.

`ganda repo audit --fix` landed kitchen 020 onto origin-home. A following `ganda repo audit` passes. The remaining `kanban-landed` line is an advisory that task 021 has no usable worktree. That claim is outside this id.

### How to validate

Smoke: from this worktree, run `./bin/dev benchmark --gate`, then `./bin/dev benchmark --aot`, then `./bin/dev workflow`.

Expect: `--gate` prints `Benchmark gate passed.` `documentation/benchmarks/comparison-baseline-aot.md` lists `TimeWarpGeneratedMonomorphicSendUnit` at 0 B and `TimeWarpGeneratedArchitectureSend` at 24 B, and its header says the mean includes a delegate call. `./bin/dev workflow` exits 0 on net10.0 and net11.0.

## Notes

- Original findings (2026-10-13) for history: 14.0 send 29.9 ns / 96 B, 72 of the 96 B from
  the Task-returning handler, scoped `GetRequiredService` per send, closures per call.
  References then: `mediator-emitter.cs` ~461, 485-548, 632-668; martinothamar
  `Mediator.sbn-cs` Init ~596-640. Those line numbers are stale after 021-003.
- Benchmark tooling: BenchmarkDotNet is pinned per TFM (0.15.8 on net10.0, 0.16.0-preview.2 on
  net11.0, task 023). Gate on net10.0 only; the 0.16 preview runs about 2× slower and is a
  tooling artifact.
- Related: TimeWarpEngineering/timewarp-nuru task 501 "Improve Nuru generated startup
  performance".
- Policy: minimal public API (one optional `ServiceLifetime` parameter is the whole surface).

## Session

- Created: 1509100 (2026-10-13)
- Refreshed: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-28)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a1828a-fa21-7a71-b50c-41a406199c83, max-turns 500, Done — 2026-10-28T13:21:44Z
- Review oracle round 1: reviewer-claude (claude-opus-5-5), effort 2, roster general — 2026-10-28.
  Five findings are open: M1, M2, and M3 are bugs, M4 is a suggestion, and M5 is a nit. A fix pass is needed. See `review/round-1/merged.md`.
  The implementer reported Done but left the baseline re-record and the martinothamar lifetime fix uncommitted.
  Reviewer checks: `dev build` and `dev test` pass, `ganda repo audit` passes, and an AOT publish and run to `/tmp` succeeds.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session b80304cc-f4e2-47c9-abcb-60204f9acb8b, max-turns 120 — 2026-10-28T13:25:41Z
- Fix pass (implement-fix-2): closed M1–M5. Re-recorded the net10.0 baseline from the committed singleton martinothamar row, `./bin/dev benchmark --gate` passed, and `./bin/dev benchmark --aot` wrote `comparison-baseline-aot.md` after the non-boxing sink. `./bin/dev workflow` passed on net10.0 and net11.0. Recommendation: leave `AddGeneratedMediator` transient. `ganda repo audit --fix` landed this kitchen; the only leftover audit line is the task 021 worktree advisory.

## Implementation Notes

-
