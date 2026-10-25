# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Commit: `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`
- Date (UTC): 2026-10-23

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                               | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpGeneratedMonomorphicSend     |   5.016 ns | 0.5836 ns | 0.3053 ns |  0.09 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfaceSend       |   8.243 ns | 0.0840 ns | 0.0373 ns |  0.15 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedObjectSend          |  15.447 ns | 0.2685 ns | 0.1405 ns |  0.28 |    0.00 | 0.0019 |      96 B |        0.46 |
| TimeWarpGeneratedInterfaceSendUnit   |   9.360 ns | 0.0754 ns | 0.0394 ns |  0.17 |    0.00 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   3.885 ns | 0.0332 ns | 0.0174 ns |  0.07 |    0.00 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  18.439 ns | 0.2035 ns | 0.1064 ns |  0.33 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedScopedSend          |  10.753 ns | 3.5615 ns | 1.8627 ns |  0.19 |    0.03 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfacePublish    |  20.024 ns | 0.3203 ns | 0.1422 ns |  0.36 |    0.00 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  20.210 ns | 0.0833 ns | 0.0436 ns |  0.36 |    0.00 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 131.117 ns | 0.8922 ns | 0.4666 ns |  2.36 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  81.432 ns | 0.2973 ns | 0.1060 ns |  1.46 |    0.01 | 0.0005 |      24 B |        0.12 |
| MartinothamarGenerated               |   8.314 ns | 0.0837 ns | 0.0372 ns |  0.15 |    0.00 | 0.0005 |      24 B |        0.12 |

## Regression gate

`./bin/dev benchmark --project comparison --gate` re-runs the comparison project and fails when a row whose name starts with `TimeWarp` exceeds this file.

- Mean time may be at most 50% above the recorded mean.
- Allocated bytes may not exceed the recorded bytes.
- `MartinothamarGenerated` is the external reference and is not gated.
- `--filter` cannot be combined with `--gate`. `--record` and `--gate` cannot be combined.
- Re-record after an intentional change: `./bin/dev benchmark --project comparison --record`.
- The gate is not part of `./bin/dev workflow`. A shared runner is too noisy for this number.

## 15.0 targets (not enforced)

The gate above does not apply this table. It is the dispatcher goal for 15.0.

| Row | Time target | Allocation target |
| --- | --- | --- |
| TimeWarpGeneratedInterfaceSend | ≤ 1.5× the MartinothamarGenerated mean | ≤ 24 B |
| TimeWarpGeneratedInterfaceSendUnit | — | 0 B |
| TimeWarpGeneratedStatePipeline | — | 0 B apart from the handler (the handler returns a completed ValueTask, so the row target is 0 B) |

