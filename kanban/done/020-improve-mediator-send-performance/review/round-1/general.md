# Round 1 — general
**Date:** 2026-10-28
**Scope reviewed:** branch vs `master` at HEAD `6362884`, plus the two uncommitted product edits

## Summary

The branch extends the comparison suite. It adds `MartinothamarScopedSend`, which uses a
satellite project because martinothamar emits one lifetime per compilation, and a `MediatRSend`
row. It also adds a `dev benchmark --aot` lane with a hand-written PublishAot runner and a weekly
`benchmark` CI job. The singleton `ServiceLifetime` API was dropped and the reason is documented
in `architecture.md`, the emitter Design comment, and the skill. The task allows dropping it.

The work stopped partway. The implementer's log ends on a plan ("commit the lifetime fix, then
run the full benchmark workflow and audit"), but the oracle reported Done. The re-recorded
baseline and a lifetime fix are uncommitted. The AOT record is missing, and `## Results` is
absent.

Verified by this reviewer:
- `./bin/dev build` passes
- `./bin/dev test --no-build` passes
- `ganda repo audit` passes with one advisory about task 021's worktree
- An AOT publish to `/tmp` succeeds and the runner exits 0

Not run: `verify-samples`, `pack`, `--gate`.

Goal restatement check: it matches the task. Dropping the API when it is not faster, and the
conditional martinothamar singleton row, both come from the original requirements. No misread.

## Issues

### Issue 1 — Severity: bug
- File: documentation/benchmarks/comparison-baseline.md:1 (uncommitted), tests/timewarp-mediator-benchmarks-comparison/dispatch-benchmarks.cs:66 (uncommitted)
- Description: The re-recorded gate baseline and the switch of `MartinothamarGenerated` back to martinothamar's default Singleton are only in the working tree. The committed branch keeps the 021-003 baseline, which has no rows for the new external benchmarks. The two uncommitted edits also contradict each other. The baseline was recorded at `a9714eb` with martinothamar forced to Transient (42.790 ns / 112 B), and the edited code then changed that row to Singleton. As written, the reference row in the gate file does not match the code that produces it. The TimeWarp rows in this record are also noisy and raise the baseline the gate compares against:
  - `TimeWarpGeneratedInterfaceSend`: 8.243 → 10.116 ns, error ±2.31 ns
  - `TimeWarpGeneratedArchitectureSend`: 81.4 → 89.7 ns, error ±19.8 ns

  The 50% tolerance stays the same, but these higher records let more regression pass, which goes against "do not loosen the gate". Nothing shows that `./bin/dev benchmark --gate` ran green.
- Suggestion: Commit the `dispatch-benchmarks.cs` lifetime fix. Re-record `comparison-baseline.md` from the clean committed tree on a quiet machine. Re-run if `TimeWarpGeneratedInterfaceSend` error is not near the previous ±0.08 ns. Run `./bin/dev benchmark --gate`, commit the baseline, and record the gate output in `## Results`.
- Status: open

### Issue 2 — Severity: bug
- File: documentation/benchmarks/comparison-baseline-aot.md (missing)
- Description: The requirement says to record the AOT numbers in `comparison-baseline-aot.md`. The tooling exists and works (`dev benchmark --aot`; the reviewer published to `/tmp` and the run exited 0), but the file was never produced or committed. `agent.md`, `readme.md`, and the baseline suffix all link to a file that does not exist.
- Suggestion: After Issue 4, run `./bin/dev benchmark --aot` from a clean committed tree and commit the generated file.
- Status: open

### Issue 3 — Severity: bug
- File: kanban/in-progress/020-improve-mediator-send-performance/task.md (no `## Results`; checklist unticked)
- Description: The task asks for the singleton result: the measured numbers and a recommendation to Steven. `architecture.md:52-57`, the emitter comment, and the skill say the singleton and the reused-transient send "allocated the same 24 B, and the means sat inside each other's noise". No numbers back that anywhere in the repo or the kitchen, and the implementer log has no singleton result table. The task also asks for these in `## Results`, and none of them are there:
  - the gate result
  - the CI-job decision with runner numbers, or a statement that none were collected
  - `./bin/dev workflow` on net10.0 and net11.0
  - the audit outcome

  Every checklist item is still unticked.
- Suggestion: Re-run the singleton comparison with a temporary row or a stashed branch, or recover it from the implementer's run. Put both rows (mean, error, allocated) and the recommendation in `## Results`, and cite them from `architecture.md`. Run `./bin/dev workflow` and record the outcome. Record the gate output, the CI decision, and the audit result. Tick the checklist.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-mediator-benchmarks-comparison/aot-benchmark-runner.cs:186-194
- Description: Two parts of the runner's measurement distort the numbers.
  - Each call goes through an `Action` lambda.
  - `Keep<T>` stores the result in `object? Consumed`, so a value-type result is boxed on every iteration.

  The published Allocated column then reports the runner, not the dispatcher. Measured in AOT:
  | Row | AOT runner | BDN JIT |
  | --- | --- | --- |
  | `TimeWarpGeneratedMonomorphicSendUnit` (boxed `Unit`) | 24 B | 0 B |
  | `TimeWarpGeneratedArchitectureSend` (boxed `OneOf`) | 56 B | 24 B |

  Times are 3–6× the BDN JIT means (for example, InterfaceSend 26 ns vs 10 ns), and the file does not say that delegate dispatch accounts for part of that.
- Suggestion: Use a non-boxing sink, such as a generic `static class Sink<T> { public static T? Value; }`, or compare `Result.Message` to a local. Consider a non-capturing static-lambda or a direct-call loop per row. Add a line to the file header that the runner's per-call overhead is included.
- Status: open

### Issue 5 — Severity: nit
- File: tools/dev-cli/endpoints/benchmark-command.cs:210, :229
- Description: `Environment.ExitCode = publish.ExitCode == 0 ? 1 : publish.ExitCode;` sits inside `if (publish.ExitCode != 0)`, so the ternary is dead. The same applies to `run.ExitCode`.
- Suggestion: Assign `publish.ExitCode` and `run.ExitCode` directly.
- Status: open
