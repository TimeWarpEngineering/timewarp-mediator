# Benchmark baseline: comparison (Native AOT)

Recorded by `./bin/dev benchmark --project comparison --aot`. Do not edit by hand; re-record instead.

- Project: `tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj`
- Framework: `net10.0`
- RID: `linux-x64`
- Commit: `3c5c08c47a408b1907e4f358bc4a4e5ad297e315`
- Date (UTC): 2026-10-28

BenchmarkDotNet's NativeAotToolchain is not used. The BenchmarkDotNet package is omitted from this publish (`BenchmarkAot=true`), and a small runner calls the `TimeWarp*` methods. Each iteration invokes the row through a delegate, so the mean includes that call. The result is stored in a generic static field and is not boxed. These numbers are not gated.

## MediatorBenchmarks.DispatchBenchmarks

PublishAot runner. Not BenchmarkDotNet. Not gated.
Each iteration invokes the row through a delegate, so the mean includes that call.
The result is stored in a generic static field and is not boxed.
Warmup 20000, iterations 200000.

| Method | Mean | Allocated |
| --- | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 54.746 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 35.295 ns | 24 B |
| TimeWarpGeneratedObjectSend | 173.630 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 18.946 ns | 0 B |
| TimeWarpGeneratedMonomorphicSendUnit | 13.357 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 86.548 ns | 24 B |
| TimeWarpGeneratedScopedSend | 24.283 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 62.419 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 62.426 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 373.966 ns | 0 B |
| TimeWarpGeneratedArchitectureSend | 289.744 ns | 24 B |

