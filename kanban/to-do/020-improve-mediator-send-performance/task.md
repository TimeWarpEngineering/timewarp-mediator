# Improve Mediator send performance

## Goal
Bring generated Send closer to martinothamar/Mediator's dispatch cost without breaking scoped-dependency support by default.

## Findings
- comparison-baseline.md (2026-10-12): martinothamar/Mediator takes 8.8 ns and 24 B per send; TimeWarp generated Send takes 29.9 ns and 96 B.
- A Task-returning handler, compared with a ValueTask one, accounts for 72 of the 96 B.
- TimeWarp calls scoped GetRequiredService on every send. martinothamar uses a singleton mediator with an eagerly pre-built pipeline.
- TimeWarp adds an async wrapper and builds pipeline closures on every call.
- References: mediator-emitter.cs ~461, 485-548, 632-668; martinothamar Mediator.sbn-cs Init ~596-640.

## Improvements
- Benchmarks: add a ValueTask (IActionHandler) row and a Scoped-mode martinothamar row for like-for-like comparison.
- Skip the async wrapper for ValueTask handlers.
- Add an opt-in singleton mode, never the default, because it can't take scoped dependencies such as DbContext.
- Build the pipeline once per mediator instead of on every call.
- Longer term: ValueTask on IRequestHandler in a major version.

## Benchmark suite improvements
- Align handlers and lifetimes across compared rows.
- Add MediatR, behavior, publish, stream and Native AOT rows.
- Track benchmarks in CI.
- Add the missing bin/dev script.

## Related
- TimeWarpEngineering/timewarp-nuru: "Improve Nuru generated startup performance" task 501: https://github.com/TimeWarpEngineering/timewarp-nuru/blob/master/kanban/to-do/501-improve-nuru-generated-startup-performance/task.md

## Session

- Created: 1509100 (2026-10-13)
