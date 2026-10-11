# Runtime test suite: deterministic publisher tests, variance coverage, remove permanent skips

## Description

The runtime suite is the ported MediatR suite with timing-based assertions and gaps versus advertised features. Parent review task 013, findings D-16, D-17, D-18, D-19, D-20, D-21.

- D-16 (risk): `notification-publisher-tests.cs` compares `Stopwatch` elapsed for sequential vs `TaskWhenAllPublisher` runs (500 ms and 250 ms delays). Cold-start and CI load can flip it, and it never asserts order.
- D-20 (risk): no test for contravariant notification or request dispatch although the readme advertises generic variance; publish strategies have one timing test.
- D-17 (debt): timeout tests use real 2 s delays.
- D-19 (debt): two Lamar open-generic tests are permanently skipped; Lamar is a primary test container although MS.DI is the shipped integration.
- D-18, D-21 (nit): static mutable counters in `pipe-line-multi-call-to-constructor-test.cs`; duplicate file names at two folder levels (`pipeline-tests.cs`, `stream-pipeline-tests.cs`, `notification-publisher-tests.cs`).

## Requirements

- Replace delay-based publisher tests with `TaskCompletionSource`-gated handlers that assert call order (sequential) and concurrency (parallel).
- Add variance dispatch tests.
- Use `Task.Delay(Timeout.Infinite, ct)` plus explicit cancellation in timeout tests.
- Delete or port the two skipped Lamar tests; decide whether Lamar stays a test dependency (it is also the only outdated pin, held at 15.0.1 for net8.0).
- Instance-scope the static counters; rename duplicate files.

## Checklist

- [ ] Deterministic publisher tests
- [ ] Variance tests
- [ ] Timeout tests without real delays
- [ ] Skips removed or ported; Lamar decision recorded
- [ ] Nits
- [ ] `./bin/dev test` green; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
