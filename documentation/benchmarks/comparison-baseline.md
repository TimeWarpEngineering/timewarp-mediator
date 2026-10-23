# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Commit: `1c7f2b0d3a30403fa04b6c6721c736a8c67fa99c` (working tree had uncommitted changes)
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
| Method                               | Mean       | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpLegacyMakeGenericType        |  57.157 ns |  2.1646 ns | 0.9611 ns |  1.00 |    0.02 | 0.0041 |     208 B |        1.00 |
| TimeWarpGeneratedMonomorphicSend     |   6.030 ns |  0.2216 ns | 0.1159 ns |  0.11 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfaceSend       |   9.731 ns |  0.3028 ns | 0.1584 ns |  0.17 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedObjectSend          |  16.940 ns |  0.4986 ns | 0.2214 ns |  0.30 |    0.01 | 0.0019 |      96 B |        0.46 |
| TimeWarpGeneratedInterfaceSendUnit   |  11.765 ns |  0.1140 ns | 0.0596 ns |  0.21 |    0.00 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   4.876 ns |  0.0866 ns | 0.0385 ns |  0.09 |    0.00 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |   5.779 ns |  0.0541 ns | 0.0240 ns |  0.10 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedScopedSend          |  14.403 ns |  0.1544 ns | 0.0808 ns |  0.25 |    0.00 | 0.0005 |      24 B |        0.12 |
| TimeWarpGeneratedInterfacePublish    |  20.570 ns |  0.2405 ns | 0.1258 ns |  0.36 |    0.01 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  21.440 ns |  2.9941 ns | 1.3294 ns |  0.38 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 141.016 ns | 14.2245 ns | 7.4397 ns |  2.47 |    0.13 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  67.039 ns |  0.7598 ns | 0.3374 ns |  1.17 |    0.02 | 0.0005 |      24 B |        0.12 |
| MartinothamarGenerated               |   8.670 ns |  0.0924 ns | 0.0483 ns |  0.15 |    0.00 | 0.0005 |      24 B |        0.12 |

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

