# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Commit: `280a84289f7665ff9312fd607ad6d9894622be84`
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
| Method                               | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpLegacyMakeGenericType        |  70.29 ns |  6.088 ns |  2.703 ns |  1.00 |    0.05 | 0.0041 |     208 B |        1.00 |
| TimeWarpGeneratedMonomorphicSend     |  27.00 ns |  0.501 ns |  0.179 ns |  0.38 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfaceSend       |  66.89 ns | 14.227 ns |  6.317 ns |  0.95 |    0.09 | 0.0019 |      96 B |        0.46 |
| TimeWarpGeneratedObjectSend          |  36.86 ns |  1.230 ns |  0.546 ns |  0.53 |    0.02 | 0.0019 |      96 B |        0.46 |
| TimeWarpGeneratedInterfaceSendUnit   |  42.42 ns |  1.538 ns |  0.805 ns |  0.60 |    0.02 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedMonomorphicSendUnit |  25.39 ns |  0.680 ns |  0.302 ns |  0.36 |    0.01 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  25.06 ns |  0.360 ns |  0.188 ns |  0.36 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedScopedSend          |  50.78 ns |  6.424 ns |  2.852 ns |  0.72 |    0.05 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfacePublish    |  31.20 ns |  1.406 ns |  0.736 ns |  0.44 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  33.90 ns |  5.683 ns |  2.972 ns |  0.48 |    0.04 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 213.10 ns | 61.099 ns | 31.956 ns |  3.04 |    0.44 | 0.0052 |     272 B |        1.31 |
| TimeWarpGeneratedArchitectureSend    | 125.08 ns | 23.728 ns | 12.410 ns |  1.78 |    0.18 | 0.0029 |     144 B |        0.69 |
| MartinothamarGenerated               |  11.06 ns |  3.027 ns |  1.583 ns |  0.16 |    0.02 | 0.0005 |      24 B |        0.12 |

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

