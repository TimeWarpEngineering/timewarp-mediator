# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commits `5131840` and `b6031c5` vs `f6f70ca` — `agent.md`, `documentation/overview.md`, `documentation/generated-vs-legacy.md`, `documentation/release-notes/release-notes-v14.0.0.md`, `readme.md`. Checked against `.github/workflows/workflow.yml`, `tools/dev-cli`, `source/Directory.Build.props`, the four csproj files, `AnalyzerReleases.*.md`, and `TimeWarp.Nuru.DevCli` 3.0.0-beta.79 `release` / `check-version` help.

## Summary

overview.md matches the real pipeline: one `<Version>` in `source/Directory.Build.props`, `workflow.yml` `ci` / `publish` jobs, the four release guards, OIDC via `NUGET_API_KEY`, rebuild-from-tag (not CI-artifact promotion), break-glass `confirm=release`, and probe. The dev CLI table matches `./bin/dev --help`, including `--strict` and `release --dry-run`. Beta.2 through beta.4 notes match the changelog they replaced. Two statements in the refreshed agent.md and the new release notes are wrong, and the unreleased section skips two consumer-visible changes that landed after `v14.0.0-beta.4`.

## Issues

### Issue 1 — Severity: bug
- File: agent.md:57
- Description: The refreshed "Two dispatchers" section lists streams via `IStreamRequest<>` as a pattern shared by both dispatchers. The generated mediator's `CreateStream` throws `NotSupportedException` (`source/timewarp-mediator-generators/mediator-emitter.cs`), and `IStreamRequestHandler` is TWM009 (`documentation/diagnostics.md`). Only `AddMediator()` dispatches streams.
- Suggestion: Say streams are reflection-only, and point at `documentation/diagnostics.md`.
- Status: open

### Issue 2 — Severity: suggestion
- File: documentation/release-notes/release-notes-v14.0.0.md:70
- Description: The beta.4 note says every profile emits internal `GeneratedMediatorServiceCollectionExtensions`. At `v14.0.0-beta.4` and on this branch, `Emit` adds that file only when `!graph.IsAotProfile` (`mediator-emitter.cs`). Aot and Link never emit `AddGeneratedMediator()`.
- Suggestion: Keep the internal-visibility point. Say the DI extension is Host-only.
- Status: open

### Issue 3 — Severity: suggestion
- File: documentation/release-notes/release-notes-v14.0.0.md:16
- Description: The unreleased section is the consumer list of what merged after beta.4. It covers the net10.0 floor, the public-API reduction, the struct-request fix, and the Roslyn 4.8 test leg. It does not mention two other consumer-visible changes on that range: `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` now use `Unlicense AND Apache-2.0` (Generators and Analyzers stay `Unlicense`), and the reflection entry points (`AddMediator` both overloads, `Mediator` constructors, and the two exception-processor behavior constructors) carry `RequiresUnreferencedCode` and `RequiresDynamicCode`.
- Suggestion: Add those two items under the unreleased section. Leave the generator behavior fixes (handler binding, type-switch order, TWM005–TWM011) to `documentation/diagnostics.md` and `m1-generated-mediator.md`; overview.md already says TWM005–TWM011 move at the next release.
- Status: open

### Issue 4 — Severity: nit
- File: agent.md:7
- Description: The opening sentence still says the library provides messaging "with no dependencies". `TimeWarp.Mediator` references `Microsoft.Extensions.DependencyInjection.Abstractions`. The sentence was not rewritten, but it sits in the file this task refreshed and contradicts the new package table.
- Suggestion: Drop "with no dependencies".
- Status: open
