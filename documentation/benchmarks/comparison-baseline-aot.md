# Benchmark baseline: comparison (Native AOT)

Recorded by `./bin/dev benchmark --project comparison --aot`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net11.0`
- RID: `linux-x64`
- Commit: `a0abd367a410b8ae80c24a6658796295e2e1a4bd`
- Date (UTC): 2026-11-13

BenchmarkDotNet's NativeAotToolchain is not used. The BenchmarkDotNet package is omitted from this publish (`BenchmarkAot=true`), and a small runner calls the `TimeWarp*` methods. Each iteration invokes the row through a delegate, so the mean includes that call. The result is stored in a generic static field and is not boxed. These numbers are not gated.

## MediatorBenchmarks.DispatchBenchmarks

PublishAot runner. Not BenchmarkDotNet. Not gated.
Each iteration invokes the row through a delegate, so the mean includes that call.
The result is stored in a generic static field and is not boxed.
Warmup 20000, iterations 200000.

| Method | Mean | Allocated |
| --- | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 20.066 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 14.517 ns | 24 B |
| TimeWarpGeneratedObjectSend | 69.553 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 9.201 ns | 0 B |
| TimeWarpGeneratedMonomorphicSendUnit | 9.676 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 45.279 ns | 24 B |
| TimeWarpGeneratedScopedSend | 13.219 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 40.128 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 39.094 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 272.895 ns | 0 B |
| TimeWarpGeneratedArchitectureSend | 147.684 ns | 24 B |

