# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** branch vs `master` (d71f299, 6e35f96), as recorded in review-framework.md

## Summary

The branch deletes the reflection runtime, its tests, the legacy benchmark, the nine container
samples, the `.snk`, and every product `SignAssembly`. Test, benchmark, and dev CLI references move
to Contracts. The deletion is complete in the slnx, the csprojs, `RepoLayout`, and the workflow.
Nothing in `source/`, `tests/`, `samples/`, `tools/`, or `.github/` still references
`source/timewarp-mediator`, `timewarp-mediator-tests`, or `timewarp-mediator-benchmarks`.

Re-verified on the review checkout:

- `./bin/dev build`: green
- `./bin/dev test --no-build`: green
- `./bin/dev pack --no-build`: Contracts, Analyzers, and Generators layouts verified; the smoke printed `package-pong`
- `./bin/dev benchmark --help`: lists `comparison (default)` only

The six projects that keep the full `Microsoft.Extensions.DependencyInjection` all construct a
`ServiceCollection`. The Roslyn-floor project compiles the compilation tests' sources, so it
does too. The DI requirement is therefore satisfied: no project can drop to Abstractions.

The TWM009 compilation test now declares its own `TimeWarp.Mediator.Pipeline.IRequestPreProcessor<T>`
in the test source. That is a sound replacement for the deleted runtime metadata reference.

The findings below are doc leftovers. One says something false that this diff made false (M1).
One leaves a signing property that the acceptance list says is gone (M3). One is a relative link
that this diff broke (M5). No code defects.

## Issues

### Issue 1 — Severity: suggestion
- File: agent.md:79
- Description: `### Key Dependencies` still says "Microsoft.Extensions.DependencyInjection.Abstractions (version pinned in `Directory.Packages.props`; central package management)". This branch removed that `PackageVersion` (audit --fix, Directory.Packages.props:37), so agent.md (loaded as CLAUDE.md into every agent session) now names a pin that does not exist. The remaining pin is the full `Microsoft.Extensions.DependencyInjection`, used by test, benchmark, sample, and smoke hosts.
- Suggestion: Replace that bullet with something like: "Microsoft.Extensions.DependencyInjection (test, benchmark, sample and smoke hosts; version pinned in `Directory.Packages.props`). A Host-profile consumer needs `Microsoft.Extensions.DependencyInjection.Abstractions` (TWM008)."
- Status: open

### Issue 2 — Severity: nit
- File: readme.md:16
- Description: The Contracts row in the Packages table still ends "Needed by both stacks." The same diff deleted the second stack and the "Two stacks" section, so this phrase is the last "two dispatchers" claim on the GitHub landing page. Acceptance: docs no longer describe a reflection dispatcher that ships from this tree.
- Suggestion: Change the phrase to "Needed by every host and message library." or drop it.
- Status: open

### Issue 3 — Severity: nit
- File: .githooks/Directory.Build.props:10
- Description: `<SignAssembly>false</SignAssembly>` survives. The root props no longer sign, so the override is dead. Requirements: "Remove `timewarp-mediator.snk`, `SignAssembly` … and the `tests/` signing overrides". Acceptance: "`SignAssembly` are gone". The diff removed the identical override from `tests/`, `samples/`, `tools/dev-cli/`, Analyzers and Generators, but missed this one. `git grep SignAssembly` outside `documentation/archive` returns only this line.
- Suggestion: Delete the line. The `*.snk binary` line in `.gitattributes` is harmless and may stay.
- Status: open

### Issue 4 — Severity: nit
- File: agent.md:57
- Description: The rewritten Dispatcher section says "`CreateStream` / `IStreamRequest<>` throw `NotSupportedException` and report TWM009". Neither type exists in `source/`; 021-002 removed them. After this branch, the four processor interfaces in `MessageGraphBuilder.UnsupportedHandlerKinds` (`source/timewarp-mediator-analyzers/message-graph-builder.cs:45-51`) ship in no package either. So the TWM009 advice in these places points at a warning a consumer cannot get: the Contracts and Generators readmes, `migration.md:18-20`, `documentation/diagnostics.md:85-91`, and `skills/tw-mediator/SKILL.md:30-31,252-253,272`. A 13.x processor fails with CS0246 instead.
- Suggestion: Defer. 021-005 deletes TWM009, and 021-007 rewrites agent.md, the skill, the package readmes, and migration. This matches 021-002 M3, which deferred the same stale TWM009 text to 021-007.
- Status: open

### Issue 5 — Severity: nit
- File: documentation/release-notes/release-notes-v14.0.0.md:16
- Description: The diff deletes `documentation/generated-vs-legacy.md`. Three links into it break:
  - `release-notes-v14.0.0.md:16` and `:62` (relative `../generated-vs-legacy.md`)
  - `github-release-v14.0.0-draft.md:97` (an absolute `blob/master/...` URL, which 404s once this merges)

  14.0.0 is published on nuget.org and tag `v14.0.0` exists (43ced87), so these are historical notes. They should still resolve.
- Suggestion: Repoint the three links to `https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/documentation/generated-vs-legacy.md`. Do not rewrite the notes' content.
- Status: open

### Issue 6 — Severity: nit
- File: documentation/benchmarks/comparison-baseline.md:5
- Description: The comparison benchmark moved `Baseline = true` to `TimeWarpGeneratedInterfaceSend` (`dispatch-benchmarks.cs:68`). The recorded table was hand-edited to drop the legacy row, but its Ratio and Alloc Ratio columns are still relative to that row. The header says "Do not edit by hand; re-record instead". `BenchmarkGate` compares only Mean and Allocated, so `--gate` is unaffected. The added note discloses this, and so do the task Results.
- Suggestion: Re-record with `./bin/dev benchmark --project comparison --record` on a quiet machine. 021-007 already links "the re-recorded baseline"; deferring to that re-record is acceptable.
- Status: open
