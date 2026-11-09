# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** `master...HEAD` product diff: `Directory.Packages.props`, `source/Directory.Build.props`, the root readme, three package readmes, `skills/tw-mediator/SKILL.md`, and `documentation/release-notes/15.0.0-beta.6.md`

## Summary

The change is small. It moves `TimeWarp.SourceGenerators` from `1.0.0-beta.11` to `1.0.0-beta.12`, sets `<Version>` to `15.0.0-beta.6`, updates every install snippet that names the packed version, and adds a release note that is linked from the root readme. No line touches the task 023-001 pins: `global.json`, the net11.0 DependencyInjection RC, CI `dotnet-quality`, and BenchmarkDotNet. `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`. Master has advanced by only two kanban landings, so nothing conflicts.

Checks:
- The release note says beta.12 drops the interface-delegation generator and the markdown sidecar. In the cached DLLs, beta.11 contains `GenerateMethodDelegation`, `GeneratePropertyDelegation`, `GenerateEventDelegation`, and `ConvertMarkdownToXmlDocs`. Beta.12 contains none of them.
- Both nuspecs still declare `Microsoft.CodeAnalysis.CSharp` 4.11.0.
- Every project that removes `TimeWarp.SourceGenerators` is a Roslyn component or a test host, as the note says.

The dependency claim no longer holds. A fresh `ganda nuget outdated --no-cache` now reports `TimeWarp.Nuru.DevCli 4.0.0-beta.11 -> 4.0.0-beta.12`. nuget.org lists `4.0.0-beta.12` as published at 2026-11-09T08:54:04Z, a few minutes before the implement oracle finished at 08:59Z. The implementer's outdated run probably came before that version appeared in the index. `TimeWarp.Nuru` itself has no beta.12 yet.

## Issues

### Issue 1 — Severity: bug
- File: Directory.Packages.props:27
- Description: The requirement is "Update every pin … to the latest version on nuget.org … a prerelease pin moves to the latest prerelease." `TimeWarp.Nuru.DevCli` is pinned at `4.0.0-beta.11`, but `4.0.0-beta.12` is now the latest prerelease on nuget.org. The release note (`documentation/release-notes/15.0.0-beta.6.md:7`) and task Results also say every other pin is current and that `TimeWarp.Nuru.DevCli` stays at `4.0.0-beta.11`. An outdated pin also blocks the publish or PR gate (`ganda repo audit` / `nuget outdated`).
- Suggestion: Move `TimeWarp.Nuru.DevCli` to `4.0.0-beta.12`, run `self-install` again, then run `./bin/dev workflow` and `./bin/dev check-version --strict`. Then update the release note sentence and Results. The beta.12 nuspec declares no dependency on `TimeWarp.Nuru`, while `TimeWarp.Nuru` stays at `4.0.0-beta.11` because nothing newer is published. Confirm that DevCli beta.12 builds and runs against Nuru beta.11. If it does not, keep beta.11, say why in the release note and Results, and mark this finding `wontfix` with that evidence.
- Status: open

### Issue 2 — Severity: nit
- File: documentation/release-notes/15.0.0-beta.6.md:5
- Description: "It still depends on `Microsoft.CodeAnalysis.CSharp` 4.11.0. The generator project, the analyzer project, and their test hosts remove that reference so the analyzer does not run inside those Roslyn components." Here "that reference" most naturally means the `Microsoft.CodeAnalysis.CSharp` 4.11.0 dependency. The projects actually remove the `TimeWarp.SourceGenerators` package reference. "The analyzer" is also unclear, because this repo has its own analyzer project.
- Suggestion: Name the reference. For example: "The generator project, the analyzer project, and their test hosts remove the `TimeWarp.SourceGenerators` reference, so its generators do not run inside those Roslyn components."
- Status: open
