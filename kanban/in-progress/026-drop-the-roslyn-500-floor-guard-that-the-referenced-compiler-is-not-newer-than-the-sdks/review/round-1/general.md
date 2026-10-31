# Round 1 — general
**Date:** 2026-10-31
**Scope reviewed:** `master...1bc8de7`, product paths: `Directory.Packages.props`, both shipped csproj files, `.github/dependabot.yml`, the compilation-tests csproj, `roslyn-compiler-tests.cs` (which replaces `roslyn-floor-tests.cs`), `agent.md`, `readme.md`, the three `source/*/readme.md`, `documentation/architecture.md`, `documentation/migration.md`, `documentation/release-notes/15.0.0.md`, the new `documentation/release-notes/15.0.0-beta.3.md`, and `skills/tw-mediator/SKILL.md`.

## Summary

The change drops the Roslyn 5.0.0 floor. `Microsoft.CodeAnalysis.CSharp` and `.Workspaces` are pinned at 5.9.0. Both shipped projects take the central pin, with no `VersionOverride`. The Dependabot ignore is gone. The floor test is replaced by `RoslynCompilerTests.ReferencedCompiler_IsNotNewerThanTheSdkCompiler`, which compares the generator's and the analyzer's `Microsoft.CodeAnalysis` reference with the AssemblyVersion of `$(RoslynTargetsPath)/bincore/Microsoft.CodeAnalysis.dll`. That path is baked in at build time as `AssemblyMetadata`. This is the right source. `typeof(CSharpCompilation)` in the test process is the NuGet pin, so comparing against it would compare the pin to itself. The Design region says so, as the task requires. Every living mention of the floor now carries the required sentence, word for word. `<Version>` stays `15.0.0-beta.2`. Risk is low. No issues raised.

## Verification

- `git grep` over non-kanban paths for `5.0.0`, `Roslyn floor`, `RoslynFloor`, `IsTheDeclaredFloor`, `VersionOverride`, `roslyn-floor`, and `10.0.100`. The only hits left are the historical design review (`documentation/design/2026-10-13-…`), the `agent.md` line that says the old floor-tests project is removed, and version strings for the 15.0.0-beta.x packages. None of them is a live floor claim.
- `dotnet test …compilation-tests.csproj --filter FullyQualifiedName~RoslynCompilerTests` on SDK 11.0.100-rc.3: passed on net10.0 and net11.0, 1 test each. The tracked tree stayed clean afterwards.
- The packed readmes gained no relative links. The `dev pack` absolute-link assertion still holds.
- Nothing in `tools/` or `.github/` reads `documentation/release-notes/`, so the new `15.0.0-beta.3.md` file does not change release tooling.
- The goal restatement matches the task. It records no misread.

## Issues

None.
