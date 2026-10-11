# Decide the target-framework floor and test the netstandard2.0 asset

## Description

TimeWarp.Mediator targets `netstandard2.0;net6.0` against 10.0.12 dependencies whose support floor is net8.0; the build emits TFM-support warnings and the non-net8.0 assets are never executed. Parent review task 013, findings B-12, C-24, part of D-11.

- Library: `netstandard2.0;net6.0`. Contracts, Analyzers, Generators: `netstandard2.0`. Tests and benchmarks: `net8.0`. Windsor sample: `net6.0`. Shared sample library: `netstandard2.1`. CI installs SDK 10.0.x and 8.0.x only.
- No `#if NET*` code exists in the library; the net6.0 asset only changes dependency resolution.
- net6.0 and net8.0 are out of support as of 2026-10.

## Requirements

- Decide the supported floor with the maintainer (recommended: `netstandard2.0` plus one current LTS such as net8.0 or net10.0; drop net6.0).
- Align samples, tests, and benchmarks to the chosen TFMs.
- Add at least one test leg that exercises the netstandard2.0 asset (net48 on Windows or a net8.0 project referencing the packed nupkg's netstandard2.0 lib).
- Update agent.md "Target Frameworks" and the readme.
- Release-notes entry if a TFM is removed.

## Checklist

- [ ] Decision recorded in Notes
- [ ] csproj changes across source, tests, samples
- [ ] netstandard2.0 execution leg
- [ ] Docs and release notes
- [ ] `./bin/dev workflow` green with zero TFM warnings; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
