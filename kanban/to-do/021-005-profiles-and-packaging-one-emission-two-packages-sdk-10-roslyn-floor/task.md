# Profiles and packaging: one emission, two packages, SDK-10 Roslyn floor

## Description

The generator emits three profiles (Host, Aot, Link) with `ServiceGen` and diagnostics TWM005
and TWM006 to describe them; the Aot and Link profiles drop pipeline behaviors. Four packages
ship where two are needed, and the Roslyn 4.8 floor plus its test project exist only to support
hosts no TimeWarp consumer uses. Parent review task 021, findings F11, F14, F15; decisions D4
and D5 signed off 2026-10-23.

## Requirements

- Remove `TimeWarpMediatorProfile`, the Aot and Link emission paths, `ServiceGen`, TWM005 and
  TWM006. One emission; the host supplies `IServiceProvider`.
- The AOT sample publishes with `PublishAot`, `IsAotCompatible`, trim and AOT analyzers on, no
  `NoWarn` on IL2026/IL3050, with behaviors woven (gate from review section 9).
- Rename the contracts package to `TimeWarp.Mediator` (`PackageId`, `AssemblyName`, project
  folder via `git mv`). The analyzer DLL (TWM001-TWM004) ships as an analyzer asset inside it;
  the separate `TimeWarp.Mediator.Analyzers` package is deleted. `TimeWarp.Mediator.Generators`
  stays, bundling TWM007-TWM011. TWM009 is deleted with the stream feature.
- Raise the Roslyn floor to the version shipped with the .NET 10 SDK; delete
  `tests/timewarp-mediator-generators-roslyn-floor-tests`; update `Directory.Packages.props`.
- `dev pack` asserts exactly two nupkgs with the expected layout; package smoke consumer
  references the two packages only.
- `documentation/diagnostics.md` updated for the removed and moved diagnostics.
- Keep `TreatWarningsAsErrors`, banned-API analyzer and Source Link intact.

## Checklist

- [ ] Profiles, `ServiceGen`, TWM005, TWM006 removed
- [ ] AOT sample publishes with behaviors woven and analyzers on
- [ ] Contracts package renamed to `TimeWarp.Mediator`, analyzer bundled, Analyzers package deleted
- [ ] Roslyn floor raised, floor test project deleted
- [ ] `dev pack` asserts two packages; smoke consumer updated
- [ ] `documentation/diagnostics.md` updated
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-004 (the old `TimeWarp.Mediator` package id must be free before
the rename). Review sections 3 (F11, F14, F15), 4 (packages, diagnostics), 7 (D4, D5).
Nuru reads `build_property.TimeWarpMediatorProfile` today; its replacement is the
`[GeneratedMediator]` attribute from 021-002/021-003, tracked in the Nuru consumer task.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
