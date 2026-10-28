# Round 2 — general
**Date:** 2026-10-28
**Scope reviewed:** fix delta `6362884..f42f012` (commits a5129e1, 3c5c08c, f42f012), checked against the round-1 findings M1–M5, `## Goal (implementer restatement)`, and `## Requirements`
**Reviewer:** reviewer-claude (claude-opus-5-5), headless review oracle, effort 2

## Summary

The fix pass commits the martinothamar Singleton default (a5129e1). It re-records `comparison-baseline.md` from that commit on a clean tree (3c5c08c). It replaces the boxing AOT sink with a generic static field and states the delegate overhead in both headers. It records `comparison-baseline-aot.md`, assigns the AOT exit codes directly, and puts the paired singleton numbers in `## Results`, `architecture.md`, and the emitter Design comment. The only product-source change is a comment in `mediator-emitter.cs`. No generated output changes. All five round-1 findings are resolved, and I found no new defects.

## Verification

- `./bin/dev build --quiet` and `./bin/dev test --no-build --quiet` both passed in this worktree.
- `ganda repo audit` passes. The only output is the `kanban-landed` warning that task 021 has no usable worktree, which matches `## Results` and belongs to another id.
- `git diff a5129e1..HEAD -- source tests tools` shows only the emitter comment. The baseline commit `a5129e1` therefore holds the benchmark code that is in HEAD, and the header has no dirty-tree marker.
- The AOT runner was unchanged after `a5129e1`, so the AOT record at `3c5c08c` used the fixed sink.
- I published the AOT lane to `/tmp` and ran it independently. Allocations match the committed file on every row: MonomorphicSendUnit 0 B, ArchitectureSend 24 B, ObjectSend 96 B, and the other rows the same. Means moved by about 2× because the host was under load (load average 10). Those numbers are documented as not gated.
- `benchmark-command.cs` now assigns `ExitCode` only inside `if (X.ExitCode != 0)`, so the assignment cannot be 0.

## Round-1 findings re-verified

| ID | Status | Evidence |
|----|--------|----------|
| M1 | fixed | Lifetime fix committed (a5129e1). Baseline recorded from that commit on a clean tree (3c5c08c). InterfaceSend 8.480 ns ±0.261 (was 10.116 ±2.31). `--gate` passed, as recorded in Results |
| M2 | fixed | `documentation/benchmarks/comparison-baseline-aot.md` committed and reproducible |
| M3 | fixed | `## Results` has the paired singleton numbers, the recommendation, gate, AOT, CI, workflow, audit, and version. `architecture.md` cites them. Checklist ticked |
| M4 | fixed | `Consumed<T>.Value` generic sink with `NoInlining` `Keep<T>`. The delegate overhead is stated in the runner Design region, the table header, and the dev-cli baseline preamble |
| M5 | fixed | Exit code assigned directly at both sites |

## Observations (not findings)

- Some re-recorded gate rows moved up from the c5e4a3a record: ScopedPublish 20.2 → 26.7 ns, CallSiteInliningPrototype 18.4 → 22.9 ns. No product dispatch code changed, so this is machine variance. It slightly raises the bar for those rows. Tolerances are unchanged, which is what the task requires.
- In both AOT runs, MonomorphicSend measures slower than InterfaceSend. The JIT run shows the opposite. That is AOT codegen, not a defect in this change, and the row is not gated.

## Issues

None.

## Plan alignment

The restatement matches the task. The requirement's escape clause ("if singleton mode is not measurably faster … do not ship an API that buys nothing") covers dropping the `ServiceLifetime` parameter. The paired numbers support it: ratio 0.95, ratio SD 0.05, both rows 24 B. No misread to record.
