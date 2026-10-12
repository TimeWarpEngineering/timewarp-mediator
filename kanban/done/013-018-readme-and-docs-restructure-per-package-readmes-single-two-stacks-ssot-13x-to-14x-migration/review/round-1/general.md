# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commits `fcde87e` and `b7288d0` vs `f9a9975` — package readmes, `migration.md` feature matrix, two-stacks pointers, `nupkg-readme-check.cs`, and the docs those claims cite (`mediator-emitter.cs`, `service-registrar.cs`, `diagnostic-descriptors.cs`, package csproj license expressions, `documentation/diagnostics.md`, `documentation/generated-vs-legacy.md`).

## Summary

The restructure matches the brief. Each packable project has its own `readme.md`, `source/Directory.Build.props` packs that file, and `dev pack` rejects a missing readme, a first heading other than `# <PackageId>`, or an inline relative link. `generated-vs-legacy.md` holds the two-stacks tables; m1, m2, the root readme, and migration point at it. The new 13.x → 14.x section covers streams, the exception cascade, pre/post processors, behaviors, named pipelines, containers, trimming, and the target framework, and those rows match the emitter and the diagnostic severities. Two cells and the new package readmes state facts the code does not.

## Issues

### Issue 1 — Severity: bug
- File: migration.md:195
- Description: The feature-matrix "Handler lifetime" row says the Host profile (`AddGeneratedMediator`) is transient. `EmitHandlerRegistrations` registers every handler and closed pipeline behavior with `AddScoped`. The generated mediator, `ISender` / `IPublisher`, and the named-pipeline sender/publisher are `AddTransient`. Reflection scanned handlers stay `AddTransient` (`service-registrar.cs`). A host that moves to the generated stack and keeps a transient-lifetime assumption is wrong: handlers are scoped.
- Suggestion: Set the Host cell to scoped. Say in the paragraph under the table that the mediator services are transient and the handlers and pipeline behaviors are scoped.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-mediator/readme.md:15
- Description: This file is the readme packed into the 14.x `TimeWarp.Mediator` nupkg (`source/Directory.Build.props` includes `$(MSBuildProjectDirectory)/readme.md`). The Install section says 14.x targets net10.0 and then gives only `dotnet add package TimeWarp.Mediator --version 13.0.0`. The 14.x package page never shows how to install the package the reader is on. The root GitHub readme can keep 13.0.0 as the last stable multi-target line; the nupkg readme cannot point only at that version.
- Suggestion: Lead with `dotnet add package TimeWarp.Mediator --prerelease` for this net10.0 package, and keep the 13.0.0 command as the last stable line that also targets netstandard2.0 and net6.0.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-mediator/readme.md:49
- Description: The new Mediator and Contracts package readmes open the license section with "Released under The Unlicense". Those two projects set `PackageLicenseExpression` to `Unlicense AND Apache-2.0` (`timewarp-mediator.csproj`, `timewarp-mediator-contracts.csproj`). Generators and Analyzers inherit `Unlicense` from `source/Directory.Build.props` and use the same paragraph, including "Contains code derived from MediatR … under Apache 2.0", which is the Contracts/Mediator fact rather than their package license. `documentation/release-notes/release-notes-v14.0.0.md` already states the split.
- Suggestion: Mediator and Contracts readmes name `Unlicense AND Apache-2.0`. Generators and Analyzers stay "Released under The Unlicense" and still say that `notice` and `unlicense` ship in the package.
- Status: open

### Issue 4 — Severity: suggestion
- File: documentation/m1-generated-mediator.md:17
- Description: The 14.0.0-beta.4 bullet says `GeneratedMediatorServiceCollectionExtensions` is one of the types that is internal in all profiles. `MediatorEmitter.Emit` adds `MediatorServiceCollectionExtensions.g.cs` only when `!graph.IsAotProfile`. Aot and Link emit no `AddGeneratedMediator()`. The same sentence was corrected in the 14.0.0 release notes; this task kept it in m1.
- Suggestion: Keep the internal-visibility point. Say the DI extension is Host-only.
- Status: open

### Issue 5 — Severity: suggestion
- File: migration.md:120
- Description: The 13.x "License Change" paragraph still says the library is "freely available for any use without attribution requirements", then points at Apache 2.0 MediatR code and the NOTICE file. Apache-2.0 section 4 requires that notice. The paragraph was only re-headed in this diff, and it now sits in a guide that also covers 14.x, where Mediator and Contracts are `Unlicense AND Apache-2.0`.
- Suggestion: Say 13.x is Unlicense and that MediatR-derived code stays Apache 2.0, so redistributions still carry NOTICE. Do not say there is no attribution requirement.
- Status: open

### Issue 6 — Severity: suggestion
- File: source/timewarp-mediator-analyzers/readme.md:34
- Description: The requirements line says Roslyn 4.8 / .NET SDK 8.0.100 is enough. The install snippet on the same page also adds `TimeWarp.Mediator.Contracts`, which targets net10.0. The Generators readme already says the practical floor is .NET SDK 10.0.100 for that reason. SDK 8 cannot compile the library the snippet creates.
- Suggestion: Keep the Roslyn 4.8 floor and add the SDK 10.0.100 note, matching the Generators readme.
- Status: open

### Issue 7 — Severity: nit
- File: migration.md:214
- Description: Step 3 says the optional `Scope` argument "is in the same section" without naming a section. `Scope` is documented under Membership in `generated-vs-legacy.md`, which step 2 already links for a different reason.
- Suggestion: Link `[Membership](./documentation/generated-vs-legacy.md#membership)` from step 3.
- Status: open
