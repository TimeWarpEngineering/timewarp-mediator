# Round 1 — general
**Date:** 2026-10-28
**Scope reviewed:** same as framework (`master...task/022-…`, product paths `Directory.Packages.props`, `.githooks/pre-push.cs`, `glossary.md`)

## Summary

The change moves the five requested central pins to the exact versions in the requirements: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.6, `Microsoft.Extensions.DependencyInjection` 10.0.14, `Microsoft.NET.Test.Sdk` 18.11.0, and `Microsoft.SourceLink.GitHub` 10.0.403. `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.CSharp.Workspaces` stay at 5.0.0. All three floor guards are still in place: `VersionOverride="5.0.0"` in both shipped csproj files, the Dependabot ignore for `Microsoft.CodeAnalysis.CSharp*`, and `Compiler_IsTheDeclaredFloor`. The reviewer re-ran that test in Release and it passed 1/1. `<Version>` stays `15.0.0-beta.1`. Nothing under `source/`, `tests/`, `tools/`, or `.github/` changed.

The other two product edits came from `ganda repo audit --fix`, and audit fixes are policy. One is the root `glossary.md` template. The other is the `.githooks/pre-push.cs` change, which allows an empty dest list (no ref updates) even when HEAD is master. That is safe: any push that would update a home branch still lists that dest and is still refused. Overall risk is low. The workflow, audit, and capabilities results come from the implementer's Results; this review re-ran only the floor test.

The goal restatement matches the original task. No misread.

## Issues

<!-- None. -->
