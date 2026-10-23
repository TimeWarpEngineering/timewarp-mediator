# M1 dispatch gap

> Historical M1 snapshot. The current numbers, with commit SHA and date, are in
> [benchmarks/comparison-baseline.md](./benchmarks/comparison-baseline.md); re-record them with
> `./bin/dev benchmark --record`.

Measured on AMD Ryzen 9 7950X3D, Ubuntu, BenchmarkDotNet 0.13.10, `.NET 10.0.11` host, 8 iterations. Project: `tests/timewarp-mediator-benchmarks-comparison`. Ping/Pong, no pipeline behaviors.

| Method | Mean | vs legacy | Allocated |
|--------|------|-----------|-----------|
| TimeWarp legacy `MakeGenericType` + wrapper cache | 50.0 ns | 1.00× | 224 B |
| Generated monomorphic `Send(Ping)` | 29.2 ns | 0.58× | 96 B |
| CallSiteInlining prototype (`Dispatch_*` static) | 28.5 ns | 0.57× | 96 B |
| martinothamar Mediator 2.1.7 | 8.3 ns | 0.17× | 24 B |

## Honest reading

- M1 generated dispatch is about **1.7× faster** and **2.3× less allocation** than this fork's reflection `Mediator`.
- **CallSiteInlining is not a product win yet.** The prototype still resolves the handler from `IServiceProvider` on every send (Host profile). Skipping the `IMediator` instance hop saves ~0.7 ns. Interceptors stay out of product until that gap is real.
- **martinothamar remains ~3.5× faster** on this microbenchmark. Their default path is closer to a static call with less per-send DI. Closing that gap is later work (inline weave for singleton/pure behaviors, ServiceGen for AOT, interceptors after measurement) — not asserted as done in M1.

Re-run:

```bash
./bin/dev benchmark            # run only; results in artifacts/benchmarks/comparison
./bin/dev benchmark --record   # also rewrite documentation/benchmarks/comparison-baseline.md
./bin/dev benchmark --gate     # fail when a TimeWarp* row exceeds that baseline
```

Tolerance, the rows the gate checks, and the 15.0 targets that are recorded but not enforced live in [benchmarks/comparison-baseline.md](./benchmarks/comparison-baseline.md). `--record` writes them. Do not edit that file by hand.
