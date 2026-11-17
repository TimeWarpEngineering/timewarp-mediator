# Bump TimeWarp.SourceGenerators to 1.0.0-beta.17

## Description

`ganda nuget outdated` (2026-11-17) reports `TimeWarp.SourceGenerators` 1.0.0-beta.16 -> 1.0.0-beta.17.
Update the pin in `Directory.Packages.props`.

The package is not a consumer dependency: `Directory.Build.props` references it with
`PrivateAssets="all"`, so no nupkg lists it as a dependency. It still runs as a source
generator when every project that does not remove it compiles, and that includes
`source/timewarp-mediator` (the shipped `TimeWarp.Mediator` assembly). A generator change can
change the shipped DLL. The generators project, the analyzers project, two test projects, and
one sample remove the reference.

## Checklist

- [ ] Bump `TimeWarp.SourceGenerators` to `1.0.0-beta.17` in `Directory.Packages.props`
- [ ] Read the beta.16 -> beta.17 changes (release notes or package diff) and note in Results what changed
- [ ] Check whether the generated source in `TimeWarp.Mediator` changes (for example build both versions with `EmitCompilerGeneratedFiles=true` and diff), or whether its public API changes
- [ ] `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples`, `./bin/dev pack` pass
- [ ] If the shipped output changes, say so in Results; no version bump unless the change needs a release note

## Session

- Created: 3d261cef-4c88-4500-a89f-105b82c12ac0 (2026-11-17)

## Notes

Not a breaking change to a stable package (15.0 is beta), so a normal walk may merge.

## Results
