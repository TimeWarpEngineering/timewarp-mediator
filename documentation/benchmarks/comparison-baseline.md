# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Commit: `d4784157057bced0a043b01422321a4583575f6a`
- Date (UTC): 2026-10-12

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                            | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| TimeWarpLegacyMakeGenericType     | 69.010 ns | 0.3917 ns | 0.2048 ns |  1.00 | 0.0044 |     224 B |        1.00 |
| TimeWarpGeneratedMonomorphicSend  | 29.904 ns | 0.3746 ns | 0.1663 ns |  0.43 | 0.0019 |      96 B |        0.43 |
| TimeWarpCallSiteInliningPrototype | 28.909 ns | 0.4389 ns | 0.2296 ns |  0.42 | 0.0019 |      96 B |        0.43 |
| MartinothamarGenerated            |  8.770 ns | 0.0885 ns | 0.0463 ns |  0.13 | 0.0005 |      24 B |        0.11 |

