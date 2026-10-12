# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** `339e65a` vs `0631929` (product files). Runtime and Contracts csproj, Directory.Build.props, Directory.Packages.props, workflow.yml, samples and test csproj files, host-compiler reference filter, trimming-attribute polyfill deletion, agent.md, readme, generated-vs-legacy.md, release notes.

## Summary

The change sets a net10.0 floor for everything that ships or runs runtime code, and leaves the Roslyn components on netstandard2.0. `TimeWarp.Mediator` drops the `netstandard2.0;net6.0` multi-target, `IsExternalInit`, and the conditional `Microsoft.Bcl.AsyncInterfaces` reference. Contracts drops the same Bcl reference and targets net10.0. Tests, benchmarks, the shared sample library, and all container samples (including Windsor) target net10.0. Lamar moves to 16.0.0, which is what makes that sample legal on net10.0. CI installs only the 10.0.x SDK. The repo-wide `LangVersion` 10.0 pin is gone; netstandard2.0 Roslyn projects still set `latest`.

No leftover `TargetFrameworks`, net6.0, or net8.0 remains in project or workflow files. Generators do not project-reference Contracts, so the netstandard2.0 analyzer host does not take a net10.0 dependency. The deleted `trimming-attributes.cs` polyfills were already empty on net10.0 (`#if !NET5_0_OR_GREATER` / `#if !NET7_0_OR_GREATER`); trimming tests still match the BCL attribute full names. Packed nuspecs have a single `net10.0` group: Mediator depends only on Contracts and DI.Abstractions 10.0.12; Contracts has an empty dependency group. Release build of `timewarp-mediator.slnx` exits 0 (0 errors; remaining warnings are pre-existing RS0030). No findings.
