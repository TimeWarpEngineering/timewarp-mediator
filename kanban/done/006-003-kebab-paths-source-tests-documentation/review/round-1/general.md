# Round 1 — general
**Date:** 2026-09-02
**Scope reviewed:** branch task/006-003-kebab-paths-source-tests-documentation vs origin/master (product commit c074fe3)

## Summary

Mechanical kebab-layout move lands cleanly: `src/`/`test/`/`Documentation/`/`Assets/`/`Kanban/`/`TimeWarp.Mediator.sln` are gone; `timewarp-mediator.slnx` project paths resolve under `/source/`, `/tests/`, and `/samples/`; live `*.csproj`/`*.props`/`workflow.yml`/`dev-cli` references no longer point at old trees. PackageId/AssemblyName stay PascalCase, generators still pack `buildTransitive/TimeWarp.Mediator.Generators.props`, snk sha256 matches `origin/master:TimeWarp.Mediator.snk`, and `kebab-path-names` / nuget icon+url audits PASS. Dual root vs `source/Directory.Build.props` Version is no longer a pack/check-version split (product evaluates `source/` after import; `AssertVersionSsot` still guards drift). Overall risk is low; residual gap is incomplete path refresh in a few live docs this slice renamed.

## Issues

### Issue 1 — Severity: suggestion
- File: documentation/m1-generated-mediator.md:12
- Description: This commit kebab-renamed samples and `Analysis/`→`analysis/`, and updated the adjacent benchmarks path to `tests/timewarp-mediator-benchmarks-comparison`, but left the AOT sample as `samples/TimeWarp.Mediator.Examples.Aot` (missing on disk; real path `samples/timewarp-mediator-examples-aot`) and the Design SSOT as `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` (missing; real path `analysis/...`). Same pattern in `documentation/m2-named-pipelines.md:3` (`Analysis/...`) and `:12` (`samples/TimeWarp.Mediator.Examples.NamedPipelines` vs `samples/timewarp-mediator-examples-named-pipelines`). Pre-existing wrong `ci-cd.yml`/MinVer prose in `documentation/overview.md` was not re-litigated.
- Suggestion: Point those live doc paths at the kebab folders (`samples/timewarp-mediator-examples-aot`, `samples/timewarp-mediator-examples-named-pipelines`, `analysis/2026-06-17-source-gen-aot-rewrite-spec.md`).
- Status: open
