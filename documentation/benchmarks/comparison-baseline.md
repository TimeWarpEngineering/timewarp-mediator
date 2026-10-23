# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Commit: `2a2a10d2dca7ebbc5780bdf4899315e8c7190cfd`
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
| TimeWarpLegacyMakeGenericType        |  58.187 ns | 4.8960 ns | 2.5607 ns |  1.00 |    0.06 | 0.0041 |     208 B |        1.00 |
| TimeWarpGeneratedMonomorphicSend     |   5.264 ns | 0.5849 ns | 0.3059 ns |  0.09 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfaceSend       |   8.855 ns | 0.9539 ns | 0.4235 ns |  0.15 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedObjectSend          |  16.862 ns | 1.3415 ns | 0.7016 ns |  0.29 |    0.02 | 0.0019 |      96 B |        0.46 |
| TimeWarpGeneratedInterfaceSendUnit   |   9.321 ns | 0.0947 ns | 0.0495 ns |  0.16 |    0.01 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   3.566 ns | 0.4787 ns | 0.2126 ns |  0.06 |    0.00 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  18.811 ns | 0.3241 ns | 0.1695 ns |  0.32 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedScopedSend          |   8.406 ns | 0.2419 ns | 0.1074 ns |  0.14 |    0.01 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfacePublish    |  20.645 ns | 0.7831 ns | 0.4096 ns |  0.36 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  20.628 ns | 0.6913 ns | 0.3616 ns |  0.36 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 134.919 ns | 1.5022 ns | 0.7857 ns |  2.32 |    0.10 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  83.266 ns | 1.1969 ns | 0.5314 ns |  1.43 |    0.06 | 0.0005 |      24 B |        0.12 |
| MartinothamarGenerated               |   8.657 ns | 0.2759 ns | 0.1443 ns |  0.15 |    0.01 | 0.0005 |      24 B |        0.12 |

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

