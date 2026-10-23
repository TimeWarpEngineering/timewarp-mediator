# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-tests/benchmark-gate-tests.cs:134
- Description: `Parse_should_read_the_recorded_comparison_baseline` pins measured baseline
  values (`Rows.Count == 13`, State pipeline `AllocatedBytes == 272`). The documented
  `--record` after an intentional change, which 021-003 will run, changes those values and
  fails `./bin/dev test` / `workflow`. The re-record instructions do not mention this.
- Suggestion: Assert structural facts instead: parses with no errors, the gated rows this
  task added are present, and the tolerance sentence matches `MeanTolerancePercent`. Drop the
  count and the 272 B pin. Or, if the pin is a deliberate tripwire, say in the generated gate
  section that a re-record also updates this test.
- Source: general
- Disposition notes: Fixed on the implement-fix-2 pass. The test asserts a clean parse, that
  the five rows this task added (scoped send, unscoped and scoped publish, State pipeline,
  Architecture send) are present and gated, and that the tolerance sentence matches
  `BenchmarkGate.MeanTolerancePercent`. Row count and allocated-byte pins are gone, including
  the 0 B pins on the void and publish rows. A later `--record` can change the measurements
  without failing the suite. The generated gate section is unchanged.

## Duplicates / conflicts

- None (single reviewer).
