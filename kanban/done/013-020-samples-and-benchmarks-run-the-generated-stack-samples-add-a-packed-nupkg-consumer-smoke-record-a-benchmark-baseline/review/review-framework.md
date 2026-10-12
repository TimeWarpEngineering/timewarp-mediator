# Review framework — task 013-020

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-020-samples-and-benchmarks-run-the-generated-stack-samples-add-a-packed-nupkg-consumer-smoke-record-a-benchmark-baseline/
**Diff scope:** commits `eb11b24`..`ff3d017` vs parent `d681d99` (merge of PR #91) on `task/013-020-samples-and-benchmarks-run-the-generated-stack-sam`. 717 lines changed (666 insertions, 51 deletions) across 34 files. Product scope: `dev verify-samples` execution, `dev workflow` wiring, packed-nupkg consumer smoke, sample container fixes and `samples/readme.md`, RS0030 `NoWarn` and test Nullable props, `dev benchmark`, Windows diagnoser conditioning, and the comparison baseline plus doc links. Kitchen `task.md` is out of product-review scope except for Results accuracy. Historical parent-review notes under task 013 are out of scope.
**Plan / brief:** Parent task 013 findings B-19, B-20, D-11, D-12, D-13, D-14, D-22, D-23. Run the aot and named-pipelines samples from `verify-samples` and wire that into `dev workflow`. Add a post-pack smoke that consumes packed Contracts and Generators from the local artifacts feed, and extend layout asserts to `lib/` TFM folders. Label container samples as legacy, decide which to keep, and align TFMs with the net10.0 floor. Silence RS0030 in samples and benchmarks and fix the Nullable conflict in test props. Add `dev benchmark`, condition the Windows diagnoser, store a baseline with commit SHA and date, and link `m1-benchmark-gap.md` from `generated-vs-legacy.md`.
**Effort:** 2 (general only; by-diff budget, not a specialist roster)
**Reviewer roster:** general
**Session IDs:** Grok `01a1303d-a90a-7872-a205-de2b354d3226` (2026-10-12)

## Budget (by-diff)

- Lines changed: 717
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
