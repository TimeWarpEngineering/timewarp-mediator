# Benchmark baseline: comparison

Recorded by `./bin/dev benchmark --project comparison --record`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net11.0`
- Commit: `eafa5617877c27af529d7ba313faa43515a23e23`
- Date (UTC): 2026-11-13

## MediatorBenchmarks.DispatchBenchmarks

```

BenchmarkDotNet v0.16.0-preview.2, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 94.29 GB Total, 2.46 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

```
| Method                               | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpGeneratedMonomorphicSend     |  10.325 ns | 0.0954 ns | 0.0424 ns |  0.89 |    0.01 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfaceSend       |  11.577 ns | 0.2100 ns | 0.1098 ns |  1.00 |    0.00 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedObjectSend          |  19.952 ns | 0.3501 ns | 0.1831 ns |  1.72 |    0.02 | 0.0019 |      96 B |        4.00 |
| TimeWarpGeneratedInterfaceSendUnit   |   9.796 ns | 0.0988 ns | 0.0439 ns |  0.85 |    0.01 |      - |         - |        0.00 |
| TimeWarpGeneratedMonomorphicSendUnit |   7.988 ns | 0.0702 ns | 0.0367 ns |  0.69 |    0.01 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    |  31.351 ns | 4.3194 ns | 2.2592 ns |  2.71 |    0.19 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedScopedSend          |  11.716 ns | 0.0946 ns | 0.0495 ns |  1.01 |    0.01 | 0.0005 |      24 B |        1.00 |
| TimeWarpGeneratedInterfacePublish    |  21.566 ns | 0.2233 ns | 0.1168 ns |  1.86 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedScopedPublish       |  21.496 ns | 0.3740 ns | 0.1956 ns |  1.86 |    0.02 |      - |         - |        0.00 |
| TimeWarpGeneratedStatePipeline       | 133.604 ns | 1.0329 ns | 0.5402 ns | 11.54 |    0.11 |      - |         - |        0.00 |
| TimeWarpGeneratedArchitectureSend    |  86.959 ns | 1.3456 ns | 0.7038 ns |  7.51 |    0.09 | 0.0005 |      24 B |        1.00 |
| MartinothamarGenerated               |  12.721 ns | 0.1033 ns | 0.0459 ns |  1.10 |    0.01 | 0.0005 |      24 B |        1.00 |
| MartinothamarScopedSend              |  44.766 ns | 0.3532 ns | 0.1260 ns |  3.87 |    0.04 | 0.0015 |      80 B |        3.33 |
| MediatRSend                          |  50.196 ns | 0.9208 ns | 0.4088 ns |  4.34 |    0.05 | 0.0043 |     216 B |        9.00 |

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

