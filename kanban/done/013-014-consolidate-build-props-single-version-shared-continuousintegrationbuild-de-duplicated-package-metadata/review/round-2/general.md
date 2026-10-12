# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** Fix delta for M1–M3 (runtime description, shared `PackageTags`, generator description and tags) plus `./bin/dev pack` nuspecs. Prior open IDs re-checked. No other product edits.

## Summary

M1, M2, and M3 are fixed. The packed `TimeWarp.Mediator` nuspec describes `AddMediator()` as the reflection dispatcher and states that `AddGeneratedMediator()` lives on Contracts plus Generators and does not reference this package. Its tags no longer include `source-generator` or `aot`. Contracts and Analyzers keep the shared tag list only. Generators append `source-generator` and `aot`, and the description names analyzer diagnostics TWM001–TWM004 and generator diagnostics TWM005–TWM011. `./bin/dev pack` succeeded and the analyzer layout asserts passed. No new defects on the fix delta.

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/timewarp-mediator.csproj:4
- Description: Re-verified against the packed nuspec. The generated-mediator-builds-on-this-package sentence is gone.
- Status: fixed

### M2 — Severity: bug — Status: fixed
- File: source/Directory.Build.props:12
- Description: Shared tags are `mediator;request;response;queries;commands;notifications;opensource;free;unlicense`. Generators csproj line 11 appends `source-generator;aot`. Packed tags match: only `TimeWarp.Mediator.Generators` carries those two tokens.
- Status: fixed

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:10
- Description: Packed description says the bundle is TimeWarp.Mediator.Analyzers (TWM001-TWM004) and that this package reports TWM005-TWM011. That matches `documentation/diagnostics.md`.
- Status: fixed
