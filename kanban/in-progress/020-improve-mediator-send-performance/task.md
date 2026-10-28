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

The 15.0 dispatcher already hits the interface-send target (about 8.2 ns and 24 B, within 1.5× of martinothamar). This task does not bring back the costs 15.0 removed: `Task` handlers, an async wrapper, per-send pipeline closures, or a scoped `GetRequiredService` on every send.

Done means:

- `AddGeneratedMediator` and `AddGeneratedMediator<TScope>` take one optional `ServiceLifetime` argument, default `Transient`, so today's calls stay transient. `Scoped` is one mediator per scope. `Singleton` is one mediator for the container, and the `HandlerCache` that instance already resolves lives with it. Handlers, behaviors, and chains stay scoped. Architecture docs say singleton is only correct when every handler and behavior is singleton or transient and has no scoped dependency (`DbContext`, per-request services), and that the generator cannot check that. Nothing on the send path validates it.
- A `TimeWarpGeneratedSingletonSend` row and a compilation test that singleton resolves the same `Mediator` across scopes and transient does not. Measure that row against the cached transient send on net10.0. If it is not faster, do not keep the API; record the numbers and a recommendation.
- Like-for-like rows: `MartinothamarScopedSend` whose handler takes the same scoped dependency as `TimeWarpGeneratedScopedSend`, a martinothamar singleton row when that library can do it and the TimeWarp singleton row exists, and a MediatR `Send` on the same `Ping`. Handler bodies and lifetimes match inside each pair. The interface-send target does not regress.
- `./bin/dev benchmark --aot` publishes the comparison project with `PublishAot` and runs the `TimeWarp*` rows. Numbers go in `documentation/benchmarks/comparison-baseline-aot.md` and are not gated.
- `comparison-baseline.md` is re-recorded on net10.0 with BenchmarkDotNet 0.15.8, then `./bin/dev benchmark --gate` is green. Tolerances stay 50% on time and no extra bytes.
- `.github/workflows/workflow.yml` has a `benchmark` job that runs `./bin/dev benchmark --gate` on net10.0 on a weekly schedule and `workflow_dispatch`, not on every pull request, unless three consecutive runner results sit inside the tolerance. The decision is in `documentation/overview.md` and `## Results`.
- `ganda repo audit --fix` is committed, `./bin/dev workflow` is green on net10.0 and net11.0, and `<Version>` stays `15.0.0-beta.2`.

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
- [ ] `AddGeneratedMediator(ServiceLifetime)` with transient default; singleton documented
- [ ] Singleton row, compilation test, measured gain recorded (or recommendation to drop)
- [ ] MartinothamarScopedSend, MediatR and (if possible) martinothamar singleton rows
- [ ] Native AOT lane and `comparison-baseline-aot.md`
- [ ] `comparison-baseline.md` re-recorded on 0.15.8; `--gate` green
- [ ] `benchmark` CI job on dispatch and schedule (or per-PR with evidence)
- [ ] `./bin/dev workflow` green on net10.0 and net11.0
### Documentation
- [ ] `documentation/architecture.md` singleton-mode section
- [ ] `documentation/overview.md` benchmark CI note; `agent.md` benchmark row
### Review
- [ ] Code Review
- [ ] PR with merge commit

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

## Implementation Notes

-
