# Round 1 — general
**Date:** 2026-10-09
**Scope reviewed:** master...HEAD on task/012 branch

## Summary
The dev-cli migration to Nuru 3.0.0-beta.79 is mechanically correct: every `return Value` became `return Unit.Value`, `ValueTask<Unit>` became `Task<Unit>` (including `RunStepAsync`), awaits are kept, and `Environment.ExitCode` failure signaling did not change. `dotnet build timewarp-mediator.slnx -c Release` exits 0 (41 warnings, 0 errors), and `dev --help` lists all 9 commands. The `.githooks` memsearch removal and the `.gitignore` additions are consistent with the task notes. The main defect is that the shipped Roslyn components (analyzers and generators) now compile against Microsoft.CodeAnalysis.CSharp 5.9.0. That raises the minimum compiler consumers need and silently disables the generator on older SDKs and IDEs. The product library's dependency floor also moved to M.E.DI.Abstractions 10.x.

## Issues
### Issue 1 — Severity: bug
- File: Directory.Packages.props:31 (consumed by source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj:24-27 and source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:24-27)
- Description: `Microsoft.CodeAnalysis.CSharp` (and `.Analyzers`) moved from 4.8.0 to 5.9.0. The generators and analyzers nupkgs ship these DLLs under `analyzers/dotnet/cs`. A Roslyn component that references compiler 5.9 only loads in a host compiler of 5.9 or newer. On this machine, SDK 10.0.301/302 ship Roslyn 5.6, 11.0.100-preview.5 ships 5.8, and 10.0.400 ships 5.9. Consumers on .NET 8/9 SDKs, .NET 10.0.1xx–3xx, or older Visual Studio get CS9057 ("analyzer references a newer version of the compiler"), and the generator/analyzers do not run. Because the generator emits the sealed Mediator and the DI registration, those consumers then fail to compile or lose diagnostics. The removed csproj comment said "Roslyn components pin 4.8 for older hosts". That intent was dropped, not addressed. Local build and CI (`setup-dotnet 10.0.x`, which resolves to the newest band) still pass because they use a recent compiler, so the tests do not catch this.
- Suggestion: Keep the shipped Roslyn components on a low compiler floor. Add `VersionOverride="4.8.0"` (or a deliberately chosen floor) on the analyzers/generators `PackageReference`s, or split the central pins: `Microsoft.CodeAnalysis.CSharp` stays at the floor for components, while test-only `Microsoft.CodeAnalysis.CSharp.Workspaces` and the compilation tests can float to 5.9.0. If raising the floor is intentional, record the minimum SDK/VS requirement in task.md and the package docs, and treat it as a breaking change for the release version.
- Status: open

### Issue 2 — Severity: suggestion
- File: Directory.Packages.props:29-32 (consumed by source/timewarp-mediator/timewarp-mediator.csproj:20-21)
- Description: The packable `TimeWarp.Mediator` library now declares NuGet dependencies on `Microsoft.Extensions.DependencyInjection.Abstractions` >= 10.0.12 and (netstandard2.0) `Microsoft.Bcl.AsyncInterfaces` >= 10.0.12, up from 8.0.0. This forces every downstream consumer onto the 10.x DI abstractions, and the `net6.0` target is outside 10.x's support floor (task.md notes the resulting warning). The justification in task.md (Autofac.Extensions 11.0.2 and LightInject.MS.DI 4.1.2 need abstractions 10) only applies to samples, not to the product package's declared floor. Also, `agent.md` "Key Dependencies" still lists 8.0.0.
- Suggestion: Either keep the product library's lower bound low via `VersionOverride` on the library's PackageReferences (samples and tests can still resolve 10.x), or accept the floor raise explicitly. If accepting it: drop or reconsider `net6.0`, update `agent.md` Key Dependencies, and note it in release notes.
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj:26 (same text in generators csproj:26, analyzers-tests csproj:18, generators-compilation-tests csproj:19)
- Description: The new comments hard-code "pinned to 5.9.0". The version lives in Directory.Packages.props, so these comments will go stale on the next bump. They also no longer explain why the version matters for a Roslyn component (see Issue 1).
- Suggestion: Drop the literal version from the comments, or say that the Roslyn component floor is controlled centrally and why.
- Status: open
