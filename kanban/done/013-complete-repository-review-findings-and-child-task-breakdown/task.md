# Complete repository review: findings and child task breakdown

## Description

Full read-only review of timewarp-mediator at origin-home `0c4d846` (14.0.0-beta.4), covering the shipped runtime and contracts, the source generator and analyzers, build/CI/release/packaging, and docs/samples/tests. Findings are filed here; each actionable group is broken out as a child task (013-NNN) so implementers can claim them independently.

Full reviewer reports with file:line evidence live in `review/`:

- `review/core-runtime-and-contracts.md` (C-1..C-30)
- `review/generators-and-analyzers.md` (G-1..G-25)
- `review/build-ci-release-hygiene.md` (B-1..B-20)
- `review/docs-samples-tests.md` (D-1..D-24)
- `review/baseline.txt` (audit, outdated, build, test results at the reviewed commit)

## Requirements

- Every finding is either mapped to a child task or recorded below as accepted/no-action.
- Child tasks are Definition-of-Ready: description with finding ids, requirements, checklist, verification.
- This parent stays open until every child is merged; closing it is a board-only `done --publish`.

## Checklist

- [x] Baseline: `ganda repo audit`, `ganda nuget outdated`, Release build, tests (see `review/baseline.txt`)
- [x] Review core runtime and contracts
- [x] Review generators, analyzers, and their tests
- [x] Review build, CI, release, packaging, hygiene
- [x] Review docs, samples, runtime tests, benchmarks
- [x] Spot-verify every bug-severity finding against source (C-1, C-3, C-9, C-22, G-3, G-5, G-6, G-7, B-4/D-6 confirmed; B-7/B-9 workflow lines confirmed)
- [x] File findings in this kitchen
- [x] Create child tasks 013-001 .. 013-022 and publish them to origin-home to-do
- [x] All children merged; parent `done --publish`

## Results

### Outcome (2026-10-12)

- All 22 children implemented and merged onto `feature/013-review` via `ganda task work-set --parent 013` (implementer-claude; review by implementer-opencode for 013-001/002, implementer-grok for 013-003 onward after OpenCode hit its usage limit). Child PRs #73-#94.
- `feature/013-review` merged to master as PR #95 (merge commit `6fcdb67`, 462 files, +22,619 / -12,654, CI green).
- Consumer acceptance (recorded in 013-015): State, Architecture, and Nuru build with 0 errors against the packed nupkgs. 013-004 touched only the runtime package, which none of them references.
- Walk incidents filed on ganda: 377 (`--into` branch not verified before the walk), 381 (oracle launcher carries the first child's task id across the set).

### Baseline

Audit clean (30 pass, 1 skip). Build exits 0 with 41 warnings (RS0030 in samples/benchmarks; net6.0 TFM-support warnings). Tests: 212 passed, 2 skipped, 0 failed. Two outdated pins: Lamar (deliberate hold) and xunit.runner.visualstudio patch.

### Severity totals

| Area | bug | risk | debt | nit | total |
|------|-----|------|------|-----|-------|
| Core runtime and contracts (C) | 3 | 9 | 8 | 10 | 30 |
| Generators and analyzers (G) | 4 | 8 | 7 | 6 | 25 |
| Build, CI, release, hygiene (B) | 1 | 5 | 8 | 6 | 20 |
| Docs, samples, tests (D) | 1 | 3 | 10 | 10 | 24 |
| **Total** | **9** | **25** | **33** | **32** | **99** |

### Bug-severity findings (all verified by reading source)

- C-1 `RegistrationTimeout = 0` is documented as "disable" but cancels immediately, so `RegisterGenericHandlers` throws `TimeoutException`. (`service-registrar.cs:22`, `mediator-service-configuration.cs:79`)
- C-3 Registration-count guard tests `MaxGenericTypeParameters > 0` instead of `MaxGenericTypeRegistrations > 0`; unchecked `long` multiply. (`service-registrar.cs:264`)
- C-9 `Publish(object)` error message has a stray `$`, no `paramName`. (`mediator.cs:122`)
- G-3 Generator binds only the first handler interface per class; multi-notification handlers lose bindings, other requests get false TWM001. (`message-graph-builder.cs:244-295`)
- G-5 Alphabetical `case` order emits CS8120 for base-before-derived types, or silently drops base-type notification delivery. (`mediator-emitter.cs:267-332`)
- G-6 Pattern locals derived from type names collide with keywords and parameters (`Request`, `Event`, `Object`, `Lock`, `Base`). (`mediator-emitter.cs:631-646`)
- G-7 Aot profile skips all behaviors while the manifest still lists them; no diagnostic. (`mediator-emitter.cs:410-446`, `manifest-emitter.cs:53-62`)
- B-4 / D-6 `documentation/overview.md` describes `ci-cd.yml`, MinVer, MyGet, and two packages; none exist. agent.md points to it as the release process.

### Headline risks

- Supply chain: actions tag-pinned, `id-token: write` on every run, OIDC key interpolated on the command line, no tag-vs-Version or green-CI guard on release (B-7, B-9).
- Generator robustness: non-incremental pipeline capturing `Compilation` (G-1); duplicate TWM diagnostics when the analyzer ships inside the generator package (G-2); invalid shapes surface as compile errors in `.g.cs` rather than diagnostics (G-9, G-11, G-13, G-21).
- Shipped surface: wrappers and `ServiceRegistrar` public with mutable static config (C-4, C-20); `IStreamRequest<>` missing from type forwards (C-22); no AOT/trimming annotations on the reflection runtime (C-14).
- Packaging: Version declared twice (B-3); `ContinuousIntegrationBuild` on one of four packages (B-13); Apache notice not packed (B-1); readme reused for every package with relative links (D-2); net6.0 target against 10.x dependencies with the netstandard2.0 asset never executed (B-12).
- Tests: timing-based publisher test (D-16); no variance coverage (D-20); generator tests happy-path only, Roslyn 4.8 floor untested, release tracking empty (G-16, G-23, G-24).

### Child tasks

| Child | Title | Findings |
|-------|-------|----------|
| 013-001 | Fix legacy registrar generic-registration limits and timeout | C-1, C-2, C-3 |
| 013-002 | Harden ServiceRegistrar: remove static configuration and fix generic handler edge cases | C-4, C-5, C-6, C-7 |
| 013-003 | Runtime correctness sweep: Mediator, wrappers, pipeline behaviors, publishers | C-8, C-9, C-10, C-11, C-12, C-13, C-15, C-16, C-17, C-22, C-26, C-27, C-30 |
| 013-004 | Decide and reduce the public API surface before 14.0.0 stable | C-20, C-21, C-4 (internal) |
| 013-005 | XML documentation: unsuppress CS1591 for shipped packages and fix stale doc text | C-18, C-19, C-28, C-29 |
| 013-006 | Annotate the reflection runtime for trimming and AOT analysis | C-14 |
| 013-007 | Generator: bind every handler interface per class, skip inherited bindings, order handlers deterministically | G-3, G-4, G-10 |
| 013-008 | Generator: safe type-switch emission (case order, pattern names, sanitized-name collisions) | G-5, G-6, G-15 |
| 013-009 | Generator Aot profile: behaviors silently dropped while the manifest lists them | G-7, G-8 |
| 013-010 | Generator and analyzer: replace generated-code compile errors with TWM diagnostics | G-9, G-11, G-13, G-14, G-17, G-18, G-21 |
| 013-011 | Generator: make the pipeline incremental and stop double-reporting diagnostics | G-1, G-2, G-19 |
| 013-012 | Generator and analyzer test matrix, Roslyn 4.8 floor leg, and release tracking | G-16, G-20, G-23, G-24, B-16 |
| 013-013 | CI hardening: pin actions, scope OIDC to the publish job, protect the release path | B-7, B-8, B-9, B-10 |
| 013-014 | Consolidate build props: single Version, shared ContinuousIntegrationBuild, de-duplicated package metadata | B-3, B-13, C-23 |
| 013-015 | Decide the target-framework floor and test the netstandard2.0 asset | B-12, C-24, D-11 (TFM) |
| 013-016 | License packaging: ship Apache attribution with the nupkgs and fix notice/readme references | B-1, B-2 |
| 013-017 | Rewrite the release process doc, add 14.x release notes, refresh agent.md | B-4, B-5, B-15, D-1, D-6 |
| 013-018 | Readme and docs restructure: per-package readmes, single two-stacks SSOT, 13.x to 14.x migration | B-11, D-2, D-3, D-4, D-5, D-10, D-24 |
| 013-019 | Repo hygiene: remove fork-migration scratch files, archive design history, fix gitattributes and nuget.config | B-6, B-14, B-17, B-18, D-7, D-8, D-9 |
| 013-020 | Samples and benchmarks: run the generated-stack samples, add a packed-nupkg consumer smoke, record a benchmark baseline | B-19, B-20, D-11, D-12, D-13, D-14, D-22, D-23 |
| 013-021 | Runtime test suite: deterministic publisher tests, variance coverage, remove permanent skips | D-16, D-17, D-18, D-19, D-20, D-21 |
| 013-022 | Reconcile code style rules with .editorconfig and apply one formatting pass | C-25, D-21 (style) |

No-action findings: G-12 (behavior token fallback, matches the reflection runtime; document only if it comes up), G-22 (dead-code nits, fold into whichever generator task touches the file), G-25 and D-15 (no-finding checks). D-21 is split between 013-021 (file names) and 013-022 (indentation).

Suggested order: 013-012 (test safety net) before 013-011; 013-001, 013-003 early (user-facing bugs); 013-015 before 013-020 TFM bumps; 013-022 last among runtime tasks to avoid formatting conflicts. All maintainer decisions are recorded. Decided 2026-10-11: 013-015 targets net10.0 or newer for runtime, contracts, tests, and samples (Generators and Analyzers stay netstandard2.0 because Roslyn requires it); 013-004 minimizes the public surface as far as the three consumers (State, Architecture, Nuru) allow; 013-016 keeps the Apache attribution, no renames to shed it; the MediatR-named base contracts stay as they are for now.

## Notes

Reviewers were four read-only sub-agents, one per area, instructed to verify every claim with file:line evidence. Bug-severity claims were re-read by the orchestrating session. Behavioral claims the reviewers could not execute are marked in their reports (C-5, C-6, C-8, G-2).

## Session

- Created: 686431 (2026-10-11)
- Review: 686431 (2026-10-11), four sonnet reviewer sub-agents (core, generators, build, docs)
- Decisions folded into 013-004, 013-015, and 013-016: 686431 (2026-10-11)
