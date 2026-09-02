# M1 dispatch gap (004-001)

Measured on AMD Ryzen 9 7950X3D, Ubuntu, BenchmarkDotNet 0.13.10, `.NET 10.0.11` host, 8 iterations. Project: `tests/timewarp-mediator-benchmarks-comparison`. Ping/Pong, no pipeline behaviors.

| Method | Mean | vs legacy | Allocated |
|--------|------|-----------|-----------|
| TimeWarp legacy `MakeGenericType` + wrapper cache | 50.0 ns | 1.00× | 224 B |
| Generated monomorphic `Send(Ping)` | 29.2 ns | 0.58× | 96 B |
| CallSiteInlining prototype (`Dispatch_*` static) | 28.5 ns | 0.57× | 96 B |
| martinothamar Mediator 2.1.7 | 8.3 ns | 0.17× | 24 B |

## Honest reading

- M1 generated dispatch is about **1.7× faster** and **2.3× less allocation** than this fork's reflection `Mediator`.
- **CallSiteInlining is not a product win yet.** The prototype still resolves the handler from `IServiceProvider` on every send (OQ-B / Host profile). Skipping the `IMediator` instance hop saves ~0.7 ns. Interceptors stay out of product until that gap is real.
- **martinothamar remains ~3.5× faster** on this microbenchmark. Their default path is closer to a static call with less per-send DI. Closing that gap is later work (inline weave for singleton/pure behaviors, ServiceGen for AOT, interceptors after measurement) — not asserted as done in M1.

Re-run:

```bash
DOTNET_ROLL_FORWARD=LatestMajor dotnet run --project tests/timewarp-mediator-benchmarks-comparison -c Release
```
