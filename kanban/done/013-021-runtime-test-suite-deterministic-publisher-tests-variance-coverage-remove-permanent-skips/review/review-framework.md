# Review framework — task 013-021

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-021-runtime-test-suite-deterministic-publisher-tests-variance-coverage-remove-permanent-skips/
**Diff scope:** commit `8063f41` vs parent `f2a5a3f` on `task/013-021-runtime-test-suite-deterministic-publisher-tests-v`. 552 lines changed (440 insertions, 112 deletions) across 10 files. Product scope: `tests/timewarp-mediator-tests/notification-publisher-tests.cs`, `variance-tests.cs`, `send-tests.cs`, `pipeline-tests.cs`, `stream-pipeline-tests.cs`, and the `microsoft-extensions-di/` renames (`pipeline-multi-call-to-constructor-tests.cs`, `service-collection-pipeline-tests.cs`, `service-collection-stream-pipeline-tests.cs`, `service-collection-notification-publisher-tests.cs`). Kitchen `task.md` is out of product-review scope except for Results accuracy.
**Plan / brief:** Parent task 013 findings D-16 through D-21. Replace Stopwatch publisher tests with gated handlers that assert sequential order and concurrency. Add variance dispatch tests. Cancel timeout tests explicitly instead of waiting on real delays. Port the two skipped Lamar open-generic tests to MS.DI and record the Lamar decision. Instance-scope the constructor counter and rename duplicate test files.
**Effort:** 2 (general only; by-diff budget, not a specialist roster)
**Reviewer roster:** general
**Session IDs:** Grok `01a13050-c2e2-7a30-8756-0c7717742fe5` (2026-10-12)

## Budget (by-diff)

- Lines changed: 552
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
