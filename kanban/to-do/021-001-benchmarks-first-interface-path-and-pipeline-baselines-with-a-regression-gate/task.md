# Benchmarks first: interface-path and pipeline baselines with a regression gate

## Goal (implementer restatement)

Record what this tree costs on the paths consumers call, and fail a later change that is slower or allocates more than that record.

Every consumer dispatches through `ISender`, `ISender<TScope>`, or `IPublisher`. The stored comparison baseline measured the concrete `Mediator.Send(Ping)` overload. This task keeps the interface Ping/Pong and void rows already in the comparison project, and adds a scoped `ISender<ClientPipeline>` send, unscoped and scoped `IPublisher` publishes, a State-shaped send (one `IAction`, three open-generic behaviors registered with `[assembly: MediatorBehavior]` and scoped to `ClientPipeline`, dispatched through `ISender<ClientPipeline>`), and an Architecture-shaped send (`IRequest<OneOf<...>>`-style response through `ISender` with one behavior).

`./bin/dev benchmark --project comparison --record` rewrites `documentation/benchmarks/comparison-baseline.md` with those rows, the commit SHA, and the date. `./bin/dev benchmark --project comparison --gate` re-runs that project and fails when a `TimeWarp*` row's mean time or allocated bytes exceed the recorded numbers by more than the tolerance written in that file. The 15.0 goals (interface Ping/Pong ≤ 1.5× martinothamar and ≤ 24 B, void send 0 B, three-behavior pipeline allocation-free apart from the handler) are a table in the same file and are not enforced.

Done when those rows exist, the baseline file is a fresh `--record` of this tree, the gate is documented and rejects an overage, and `./bin/dev workflow` is green.

## Description

Every consumer dispatches through `ISender`, `ISender<TScope>` or `IMediator`, but the recorded
comparison baseline measures only the concrete `Mediator.Send(Ping)` overload. Through the
interface the 14.0 generated mediator costs about 65 ns / 240 B, the same as the reflection
dispatcher, because `Send<TResponse>` is emitted as `SendCore` → `Send(object)` → `Box<T>`.
Parent review task 021, findings F2 and F17, gates in section 9.

This task lands before any redesign work so that 021-003 has numbers to beat and later tasks
cannot regress silently.

## Requirements

- Keep the interface-path benchmarks already added to
  `tests/timewarp-mediator-benchmarks-comparison` (`TimeWarpGeneratedInterfaceSend`,
  `TimeWarpGeneratedObjectSend`, `TimeWarpGeneratedInterfaceSendUnit`,
  `TimeWarpGeneratedMonomorphicSendUnit`) and add `ISender<TScope>` and `IPublisher` variants.
- Add a State-shaped pipeline benchmark: one `IAction` request, three open-generic behaviors
  woven through `[assembly: MediatorBehavior]`, dispatched through `ISender<ClientPipeline>`.
- Add an Architecture-shaped benchmark: `IRequest<OneOf<...>>`-style response through `ISender`
  with one behavior.
- Record the 14.0 baseline with `./bin/dev benchmark --project comparison --record` so
  `documentation/benchmarks/comparison-baseline.md` carries interface, scoped, publish and
  pipeline rows with commit SHA and date.
- Add a `--gate` mode (or a separate `dev benchmark-gate` command) that runs the comparison
  project and fails when mean time or allocated bytes for a gated row exceed the recorded
  baseline by more than a documented tolerance. Document the tolerance and how to re-record.
- Targets for 15.0 are recorded in the baseline file as a table, not enforced yet: interface
  Ping/Pong ≤ 1.5× martinothamar and ≤ 24 B; void send 0 B; pipeline with three behaviors
  allocation-free apart from the handler.

## Checklist

- [x] Scoped-sender and publisher interface benchmarks added
- [x] State-shaped pipeline benchmark added
- [x] Architecture-shaped response benchmark added
- [x] 14.0 baseline recorded with `--record`
- [x] Regression gate implemented and documented
- [x] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. First child; nothing depends on it except the gate in 021-003. Review:
`documentation/design/2026-10-13-15.0-breaking-redesign-review.md` sections 3 (F2, F17), 9.
Measured on 2026-10-13: interface 64.8 ns / 240 B, reflection 65.2 ns / 224 B, concrete
29.9 ns / 96 B, martinothamar 8.6 ns / 24 B; void concrete 24.1 ns / 0 B, void interface
43.9 ns / 96 B (`research/interface-path-benchmark.md` in the parent kitchen).

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: 01a169b4-3bff-75c0-82a3-d7ebd381caf0 (2026-10-24)

## Results

Recorded `documentation/benchmarks/comparison-baseline.md` with `./bin/dev benchmark --project comparison --record` on commit `280a84289f7665ff9312fd607ad6d9894622be84` (2026-10-23, BenchmarkDotNet 0.15.8, iteration count 8, warmup count 3, .NET 10.0.13). Package version is still 14.0.0. This tree already contains the merged 021-002 ValueTask contracts, so these numbers are the baseline for 021-003. They are not the 2026-10-13 Task-shaped research numbers (interface was 64.8 ns / 240 B then).

| Method | Mean | Allocated |
| --- | ---: | ---: |
| TimeWarpLegacyMakeGenericType | 70.29 ns | 208 B |
| TimeWarpGeneratedMonomorphicSend | 27.00 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 66.89 ns | 96 B |
| TimeWarpGeneratedObjectSend | 36.86 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 42.42 ns | 24 B |
| TimeWarpGeneratedMonomorphicSendUnit | 25.39 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 25.06 ns | 24 B |
| TimeWarpGeneratedScopedSend | 50.78 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 31.20 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 33.90 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 213.10 ns | 272 B |
| TimeWarpGeneratedArchitectureSend | 125.08 ns | 144 B |
| MartinothamarGenerated | 11.06 ns | 24 B |

`MartinothamarGenerated` is the external reference and is not gated. The interface Ping/Pong row is about 6× that reference and allocates 96 B. The 15.0 table in the same file (≤ 1.5× and ≤ 24 B, void send 0 B, three-behavior pipeline 0 B apart from the handler) is not enforced.

The gate allows a mean at most 50% above the recorded mean and does not allow allocated bytes to rise. BenchmarkDotNet writes `-` for 0 B; the parser treats that dash as zero. The recorded 99.9% error bars on the interface, architecture, and State rows were about 19–29% of the mean, so a 20% limit would fail a quiet re-run on this host. The measurement table is the `--record` output. The tolerance sentence was aligned to `BenchmarkGate.MeanToleranceRatio` after that record, because the ratio was widened once those error bars were visible. A later `--record` rewrites the table and that sentence together.

A second comparison run on 2026-10-24 (`./bin/dev benchmark --project comparison --gate`) printed `Benchmark gate passed`. The noisiest gated mean was scoped send at 61.56 ns against the 50.78 ns baseline (limit 76.17 ns). Allocations matched, including the three 0 B rows.

`./bin/dev workflow` printed `Pipeline SUCCEEDED`: generators 42, analyzers 42, Roslyn-floor 83, compilation 82, mediator tests 191, 11 samples, pack of 14.0.0 with the package smoke. `ganda repo audit` passed 31, failed 0. The pull request is the host open-pr node.

### How to validate

**Smoke**

```bash
dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj --filter FullyQualifiedName~BenchmarkGateTests --nologo
```

**Expect**

- BenchmarkGateTests: 8 passed, 0 failed. The recorded baseline parses as 13 rows, dash cells are 0 B, the State pipeline row is 272 B, and the file contains the same percent as `BenchmarkGate.MeanTolerancePercent` (50).
- A mean more than 50% above its baseline row fails. Allocated bytes above the recorded bytes fail. `MartinothamarGenerated` is not gated.

**Gate**

```bash
./bin/dev benchmark --project comparison --gate
```

Expect: exit 0 and the line `Benchmark gate passed`. The run takes about three minutes. Re-record after an intentional change with `./bin/dev benchmark --project comparison --record`. Do not combine `--gate` with `--record` or with `--filter`. The gate is not part of `./bin/dev workflow`.
