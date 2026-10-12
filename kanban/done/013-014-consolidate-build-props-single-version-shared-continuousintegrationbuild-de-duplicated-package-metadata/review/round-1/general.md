# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** `31d0956` vs `2e54bcb` (props, four package projects, overview version path, dev-cli version assert removal) plus packed nuspecs and evaluated MSBuild properties.

## Summary

Version, authors, project URL, and the CI deterministic flag land on the owners the release path actually reads. `source/Directory.Build.props` is the sole `<Version>` (TimeWarp.Nuru.DevCli and `workflow.yml` both read that file), root props set `ContinuousIntegrationBuild` when `GITHUB_ACTIONS` is true, and the duplicate `AssertVersionSsot` is gone because there is nothing left to drift. Evaluated properties on all four packages are `Version=14.0.0-beta.4`, `Authors=Steven T. Cramer`, `ContinuousIntegrationBuild=true`, `PackageLicenseExpression=Unlicense`. LangVersion stays 10.0 on the runtime and Contracts and `latest` on the Roslyn components. Analyzers and Generators still set `IncludeSymbols=false`, so the NU5017 override survives the move of symbol settings into `source/Directory.Build.props`.

The package blurbs and tags rewritten in this commit mis-state the reflection-vs-generated boundary. `documentation/generated-vs-legacy.md` says the generated dispatcher does not reference the `TimeWarp.Mediator` assembly and that the reflection runtime is not trim or Native AOT safe. The new runtime description and the shared `aot` / `source-generator` tags say the opposite. The generator description also stops the diagnostic list at TWM006 and attaches that range to the analyzer package; `documentation/diagnostics.md` gives the analyzer TWM001–TWM004 and the generator TWM005–TWM011.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator/timewarp-mediator.csproj:4
- Description: The new description says this package contains "the registration and pipeline infrastructure the source-generated mediator (AddGeneratedMediator, from TimeWarp.Mediator.Generators) builds on." Generated emission uses Contracts types (`IMediator`, `IPipelineBehavior`, `NoHandlerException`, `Unit`) and Microsoft.Extensions.DependencyInjection. `documentation/generated-vs-legacy.md` states the generated path does not reference the `TimeWarp.Mediator` assembly. The packed nuspec repeats the false sentence.
- Suggestion: Describe `AddMediator()` as the reflection dispatcher, and say the generated stack is TimeWarp.Mediator.Contracts plus TimeWarp.Mediator.Generators and does not reference this package.
- Status: open

### Issue 2 — Severity: bug
- File: source/Directory.Build.props:12
- Description: `PackageTags` gained `source-generator;aot` on the shared metadata group, so every package inherits them. The packed `TimeWarp.Mediator` nuspec tags include `source-generator` and `aot`. That package is the reflection runtime, which the same doc marks not trim/AOT safe. Contracts and Analyzers are not source generators either.
- Suggestion: Keep the shared tags free of `source-generator` and `aot`. Append those two tokens only on `timewarp-mediator-generators.csproj`.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:10
- Description: The description says the package "Bundles TimeWarp.Mediator.Analyzers (TWM001-TWM006)." The analyzer package reports TWM001–TWM004. TWM005 and TWM006 are generator diagnostics, and the generator also reports TWM007–TWM011.
- Suggestion: Say the bundle is the analyzer package (TWM001–TWM004) and that this package reports TWM005–TWM011.
- Status: open
