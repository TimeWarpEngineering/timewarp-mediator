# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net11.0`
- Commit: `9815fa71fec6ce990965f33c808e6d4d03d91510`
- Date (UTC): 2026-11-13

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.16.0-preview.2, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 94.29 GB Total, 0.75 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                               | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpGeneratedMonomorphicSend     |  10.483 ns | 0.1072 ns | 0.0476 ns |  0.81 |    0.01 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfaceSend       |  12.932 ns | 0.1765 ns | 0.0784 ns |  1.00 |    0.00 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedObjectSend          |  20.540 ns | 0.4085 ns | 0.2136 ns |  1.59 |    0.02 | 0.0019 |      96 B |        4.00 |
| TimeWarpGeneratedInterfaceSendUnit   |   9.942 ns | 0.1132 ns | 0.0503 ns |  0.77 |    0.01 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   8.087 ns | 0.1178 ns | 0.0616 ns |  0.63 |    0.01 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  22.850 ns | 0.3075 ns | 0.1608 ns |  1.77 |    0.02 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedScopedSend          |  11.884 ns | 0.1107 ns | 0.0579 ns |  0.92 |    0.01 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfacePublish    |  21.592 ns | 0.4218 ns | 0.2206 ns |  1.67 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  21.472 ns | 0.2687 ns | 0.1405 ns |  1.66 |    0.01 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 133.462 ns | 1.3768 ns | 0.7201 ns | 10.32 |    0.08 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  87.036 ns | 0.5959 ns | 0.2646 ns |  6.73 |    0.04 | 0.0005 |      24 B |        1.00 |
| MartinothamarGenerated               |  11.701 ns | 0.0718 ns | 0.0319 ns |  0.90 |    0.01 | 0.0005 |      24 B |        1.00 |
| MartinothamarScopedSend              |  47.342 ns | 4.3808 ns | 2.2912 ns |  3.66 |    0.17 | 0.0015 |      80 B |        3.33 |
| MediatRSend                          |  48.821 ns | 1.0364 ns | 0.4602 ns |  3.78 |    0.04 | 0.0043 |     216 B |        9.00 |

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

