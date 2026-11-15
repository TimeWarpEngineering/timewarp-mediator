# Conform dev benchmark to the shared contract

## Description

Implement, size M. Make Mediator's `dev benchmark` conform to the shared dev-cli benchmark contract from nuru 578: `dev benchmark list`, `dev benchmark run <name> --base <git-ref>`, and a machine-readable verdict (ratio, noise band, win / noise / regression).

Mediator has the closest thing today: `tools/dev-cli/endpoints/benchmark-command.cs`, `dev benchmark --gate`, `tools/dev-cli/services/benchmark-gate.cs`, and baselines under `documentation/benchmarks/`. It compares against competitor libraries in the same run. Keep that comparison available as a named benchmark if it still earns its place. The base-vs-head A/B on one machine in one run is what the perf sweep needs, and the stored baselines are the part that goes.

Part of the cross-repo perf sweep, umbrella timewarp-ganda 599.

## Related

- nuru 578 (defines the shared contract and plumbing)
- mediator 035 (first manual perf sweep; runs after this task)
- ganda 599 (umbrella)

Cross-repo order: do not start until nuru 578 has shipped the shared piece.

## Checklist

- [ ] Re-read the contract and shared plumbing from nuru 578
- [ ] Name Mediator's benchmarks and expose them through `dev benchmark list`
- [ ] Implement `dev benchmark run <name> --base <git-ref>` on the shared plumbing (same-run A/B, ratio, noise band, verdict)
- [ ] Retire the stored-baseline comparison under `documentation/benchmarks/` and `--gate` once `run --base` replaces it (delete outright, no compat shim)
- [ ] Update docs and keep the weekly hosted benchmark job off the PR path

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-13)

## Notes

- Eval: https://github.com/TimeWarpEngineering/timewarp-intel/blob/master/evaluations/2026-11-11-agent-perf-sweep.md
- Filed only; do not start until Steven says so.
- why: Steven 2026-11-13, perf-sweep split across repos
