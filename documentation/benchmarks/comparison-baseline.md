# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net10.0`
- Commit: `a5129e19c5fb2a6bed40f183db40020111db8856`
- Date (UTC): 2026-10-28

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                               | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpGeneratedMonomorphicSend     |   5.061 ns | 2.7254 ns | 1.2101 ns |  0.60 |    0.13 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfaceSend       |   8.480 ns | 0.2613 ns | 0.1160 ns |  1.00 |    0.02 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedObjectSend          |  16.546 ns | 1.2480 ns | 0.5541 ns |  1.95 |    0.07 | 0.0019 |      96 B |        4.00 |
| TimeWarpGeneratedInterfaceSendUnit   |   9.736 ns | 0.2145 ns | 0.0952 ns |  1.15 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   4.062 ns | 0.2018 ns | 0.0896 ns |  0.48 |    0.01 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  22.901 ns | 2.0739 ns | 1.0847 ns |  2.70 |    0.13 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedScopedSend          |   8.534 ns | 0.2300 ns | 0.1203 ns |  1.01 |    0.02 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfacePublish    |  20.388 ns | 0.5363 ns | 0.2381 ns |  2.40 |    0.04 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  26.678 ns | 2.4140 ns | 0.8609 ns |  3.15 |    0.10 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 133.339 ns | 3.0012 ns | 1.3326 ns | 15.73 |    0.25 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  88.852 ns | 3.8866 ns | 1.7257 ns | 10.48 |    0.23 | 0.0005 |      24 B |        1.00 |
| MartinothamarGenerated               |   9.370 ns | 1.4288 ns | 0.7473 ns |  1.11 |    0.08 | 0.0005 |      24 B |        1.00 |
| MartinothamarScopedSend              |  47.457 ns | 5.9249 ns | 2.6307 ns |  5.60 |    0.30 | 0.0018 |      88 B |        3.67 |
| MediatRSend                          |  58.270 ns | 7.6685 ns | 4.0108 ns |  6.87 |    0.46 | 0.0045 |     224 B |        9.33 |

## Regression gate

`./bin/dev benchmark --project comparison --gate` re-runs the comparison project and fails when a row whose name starts with `TimeWarp` exceeds this file.

- Mean time may be at most 50% above the recorded mean.
- Allocated bytes may not exceed the recorded bytes.
- Rows that do not start with `TimeWarp` are external references and are not gated. That includes `MartinothamarGenerated`, `MartinothamarScopedSend`, and `MediatRSend`.
- `--filter` cannot be combined with `--gate`. `--record` and `--gate` cannot be combined. `--aot` writes `comparison-baseline-aot.md` and is not gated.
- Re-record after an intentional change: `./bin/dev benchmark --project comparison --record`.
- The gate is not part of `./bin/dev workflow`. `.github/workflows/workflow.yml` runs it on a weekly schedule and on `workflow_dispatch` with mode `benchmark`. It does not run on pull requests.

## 15.0 targets (not enforced)

The gate above does not apply this table. It is the dispatcher goal for 15.0.

| Row | Time target | Allocation target |
| --- | --- | --- |
| TimeWarpGeneratedInterfaceSend | ≤ 1.5× the MartinothamarGenerated mean | ≤ 24 B |
| TimeWarpGeneratedInterfaceSendUnit | — | 0 B |
| TimeWarpGeneratedStatePipeline | — | 0 B apart from the handler (the handler returns a completed ValueTask, so the row target is 0 B) |

