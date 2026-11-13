# Benchmark baseline: comparison (Native AOT)

Recorded by `./bin/dev benchmark --project comparison --aot`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net11.0`
- RID: `linux-x64`
- Commit: `eafa5617877c27af529d7ba313faa43515a23e23` (working tree had uncommitted changes)
- Date (UTC): 2026-11-13

BenchmarkDotNet's NativeAotToolchain is not used. The BenchmarkDotNet package is omitted from this publish (`BenchmarkAot=true`), and a small runner calls the `TimeWarp*` methods. Each iteration invokes the row through a delegate, so the mean includes that call. The result is stored in a generic static field and is not boxed. These numbers are not gated.

## MediatorBenchmarks.DispatchBenchmarks

PublishAot runner. Not BenchmarkDotNet. Not gated.
Each iteration invokes the row through a delegate, so the mean includes that call.
The result is stored in a generic static field and is not boxed.
Warmup 20000, iterations 200000.

| Method | Mean | Allocated |
| --- | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 21.428 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 14.641 ns | 24 B |
| TimeWarpGeneratedObjectSend | 61.309 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 9.314 ns | 0 B |
| TimeWarpGeneratedMonomorphicSendUnit | 9.646 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 43.566 ns | 24 B |
| TimeWarpGeneratedScopedSend | 13.192 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 37.927 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 40.340 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 278.996 ns | 0 B |
| TimeWarpGeneratedArchitectureSend | 145.562 ns | 24 B |

