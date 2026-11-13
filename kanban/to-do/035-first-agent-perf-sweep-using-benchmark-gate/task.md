# First agent perf sweep using benchmark --gate

## Description

Implement, size M. Run the first agent perf-sweep once on Mediator, using the existing `./bin/dev benchmark --gate` command and the recorded baselines under `documentation/benchmarks/`. Use 1–2 workers. Open a PR only when the gate shows a win against master. Then report PRs opened, merged and closed, dollars spent, and Steven's review minutes, and decide on a weekly cadence and whether a Taratibu routine or Ramani PR-lifecycle machine (options B and C, killed for now) is worth building.

This run depends on the playbook in timewarp-ganda task [599](https://github.com/TimeWarpEngineering/timewarp-ganda/blob/master/kanban/to-do/599-write-agent-perf-sweep-playbook-skill/task.md) ("Write agent perf-sweep playbook skill"). Do not start this sweep until that playbook exists. This is a cross-repo dependency note, not a same-repo `## Depends on` merge-wait edge.

Source: [Agent perf-sweep eval](https://github.com/TimeWarpEngineering/timewarp-intel/blob/master/evaluations/2026-11-11-agent-perf-sweep.md) ([timewarp-intel PR #123](https://github.com/TimeWarpEngineering/timewarp-intel/pull/123)). Option F, GREEN by Busara. Overall verdict REWORK (high): written playbook run by hand on Mediator first.

Filed 2026-11-11 at Steven's request ("Yes, create the timewarp-mediator task for the first agent perf sweep using benchmark --gate."). Not launched.

## Requirements

1. Follow the playbook from timewarp-ganda task 599. Do not start until that skill is written.
2. Run once, with 1–2 workers on separate areas. The open unreviewed PR cap is the one Steven sets in that playbook; do not exceed it.
3. Open a PR only when `./bin/dev benchmark --gate` shows a win against the master baseline. Keep bench jobs off the PR path (Mediator's weekly hosted `benchmark` job already is).
4. No public API, breaking, or behavior changes; no unsafe code; one small PR per win; PRs must be green.
5. Merges only after write-access review. Re-measure on the final head after review changes. No empty or retrigger commits.
6. After the run, report: PRs opened, merged, and closed; dollars spent; Steven's review minutes.
7. From that report, decide whether to adopt a weekly cadence, and whether a Taratibu weekly perf-sweep routine (B) or a Ramani PR-lifecycle state machine (C) is worth building. Do not build B or C in this task.

## Out of scope

- Writing the playbook skill (that is timewarp-ganda task 599)
- Building a Taratibu routine (B) or Ramani machine (C); both are KILL for now and only get revisited after this run (and 1–2 more) has worked
- Extending the sweep to Nuru, Amuru, or Terminal (start Nuru to-do 567 first)
- Extracting Mediator's benchmark-gate comparer for other repos (K)
- A Copilot cloud-agent path (H)
- Starting more than this one run, or launching workers before Steven says to start

## Depends on

- 038

## Related

- mediator 038 (conform `dev benchmark` to the shared contract; this run uses its `dev benchmark run <name> --base <ref>` verdict)
- nuru 578 (shared `dev benchmark` contract and plumbing)
- flow 204 (`tw-perf-sweep` skill; written alongside this run, drafted first)
- ganda 599 (perf-sweep umbrella; supersedes the playbook-skill framing above, the judgment rules now live in flow 204)

This is step 2 in the umbrella order: runs after mediator 038, with the draft skill from flow 204.

## Checklist

- [ ] Confirm the ganda task 599 playbook exists and follow it
- [ ] Run once with 1–2 workers on separate areas, inside Steven's open-PR cap
- [ ] Open a PR only when `./bin/dev benchmark --gate` shows a win against master
- [ ] Keep bench jobs off the PR path; no empty or retrigger commits; merge only after write-access review and re-measure after review changes
- [ ] Report PRs opened, merged, and closed; dollars spent; Steven's review minutes
- [ ] Decide on a weekly cadence and whether B (Taratibu routine) or C (Ramani machine) is worth building — do not build them here

## Session

- Created: 3156381 (2026-11-11)

## Notes

- Cross-repo dependency: timewarp-ganda task 599, https://github.com/TimeWarpEngineering/timewarp-ganda/blob/master/kanban/to-do/599-write-agent-perf-sweep-playbook-skill/task.md
- Gate: `./bin/dev benchmark --gate` (see `tools/dev-cli/services/benchmark-gate.cs` and `documentation/benchmarks/`)
- Eval: https://github.com/TimeWarpEngineering/timewarp-intel/blob/master/evaluations/2026-11-11-agent-perf-sweep.md
- Intel PR: https://github.com/TimeWarpEngineering/timewarp-intel/pull/123
- Fowler issue: https://github.com/dotnet/aspnetcore/issues/70219
- Fowler post: https://x.com/davidfowl/status/2117020540628340983
- Filed only; not launched. Do not start until Steven says so, and not before the ganda 599 playbook exists.
- Order: run after mediator 038 conforms `dev benchmark` to the shared contract. The verdict from `dev benchmark run`, not `--gate` baselines, decides whether a PR opens.
- why: Steven 2026-11-13, perf-sweep split across repos
