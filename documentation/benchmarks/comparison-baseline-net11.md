# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --framework net11.0 --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net11.0`
- Commit: `d84a8549902bcd8504b4dc2068e6d50a34edd3a6`
- Date (UTC): 2026-10-28

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.16.0-preview.2, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 62.34 GB Total, 1.17 GB Available
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.0-rc.3.26522.148, 11.0.26.52348), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 11.0.0 (11.0.0-rc.3.26522.148, 11.0.26.52348), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                               | Mean       | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpGeneratedMonomorphicSend     |   9.919 ns |  0.7704 ns | 0.4030 ns |  0.81 |    0.04 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfaceSend       |  12.218 ns |  0.5919 ns | 0.3096 ns |  1.00 |    0.00 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedObjectSend          |  21.290 ns |  1.4764 ns | 0.7722 ns |  1.74 |    0.07 | 0.0019 |      96 B |        4.00 |
| TimeWarpGeneratedInterfaceSendUnit   |  10.191 ns |  0.2672 ns | 0.1398 ns |  0.83 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   8.091 ns |  0.0591 ns | 0.0262 ns |  0.66 |    0.02 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  22.965 ns |  1.0874 ns | 0.4828 ns |  1.88 |    0.06 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedScopedSend          |  12.874 ns |  0.3374 ns | 0.1764 ns |  1.05 |    0.03 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfacePublish    |  22.708 ns |  1.3302 ns | 0.6957 ns |  1.86 |    0.07 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  22.102 ns |  1.5918 ns | 0.7068 ns |  1.81 |    0.07 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 140.910 ns | 14.1799 ns | 5.0567 ns | 11.54 |    0.47 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  91.508 ns |  8.1796 ns | 3.6318 ns |  7.49 |    0.33 | 0.0005 |      24 B |        1.00 |
| MartinothamarGenerated               |  12.000 ns |  1.1110 ns | 0.4933 ns |  0.98 |    0.04 | 0.0005 |      24 B |        1.00 |

This file records `net11.0`. The regression gate reads `documentation/benchmarks/comparison-baseline.md` (`net10.0`) and does not use these numbers.

