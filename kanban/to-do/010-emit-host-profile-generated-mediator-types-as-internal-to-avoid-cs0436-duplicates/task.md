# Emit host-profile generated mediator types as internal to avoid CS0436 duplicates

## Description

Found by TimeWarp.Nuru 443-001 (2026-09-25). In the Host profile the generator emits `Mediator`,
`MediatorManifest`, and `GeneratedMediatorServiceCollectionExtensions` as **public**. Every Nuru app is a
mediator host, so a compilation that references two apps sees duplicate types: Nuru's
`tests/ci-tests/run-ci-tests.cs` references the mcp and search apps and gets 4 `CS0436` warnings in
`MediatorServiceCollectionExtensions.g.cs`. Nuru also has to strip the generator from its own library compile
because the public `AddGeneratedMediator()` would be ambiguous in apps.

## Requirements

- Emit Host-profile types as `internal` (keep library/graph-member behavior as designed; check the Aot and
  Link profiles too).
- `AddGeneratedMediator()` stays callable from the host app's own code (internal extension in the same
  assembly).
- Test: two host assemblies referenced by a third compile with no CS0436.
- Document in docs; bump to the next 14.0.0-beta and release (cockpit, public repo, NuGet.org).
- Note for consumers: Nuru can later drop its `RemoveTimeWarpMediatorGenerator` target if this makes it
  unnecessary (separate Nuru follow-up).

## Checklist

- [x] Host types internal
- [x] Two-host test, no CS0436
- [x] Docs, version bump
- [ ] Released (cockpit, after merge)

## Notes

- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground.

## Results

Every generated top-level type is now `internal` in every profile.

- `source/timewarp-mediator-generators/mediator-emitter.cs`: `Mediator`, `Sender_*`, and `Publisher_*`
  (shared `EmitDispatcher`) and `GeneratedMediatorServiceCollectionExtensions` are emitted `internal`.
  Members stay `public` (interface implementations, `Dispatch_*` for CallSiteInlining). `ServiceGen` was
  already internal. The Design region records why.
- `source/timewarp-mediator-generators/manifest-emitter.cs`: `MediatorManifest` is `internal`.
- Aot and Link profiles: they emit the same `Mediator` / `Sender_*` / `Publisher_*` / `MediatorManifest`
  in the host compilation and collided the same way, so they are internal too. They still emit no
  registration extension. Library / graph-member compilations are unchanged (the generator only runs in
  hosts; analyzers-only libraries emit nothing).
- `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` stay callable from the host's own code. No
  in-repo code used generated types across assemblies; samples, benchmarks, and generator tests build
  unchanged.
- New test project `tests/timewarp-mediator-generators-compilation-tests` (added to the slnx, so
  `dotnet test` / `./bin/dev test` run it). It drives `MediatorGenerator` through `CSharpGeneratorDriver`
  with Host / Aot / Link build properties:
  - `GeneratedTypes_AreInternal` (Host, Aot, Link): `Mediator`, `MediatorManifest`,
    `Sender_AppPipeline`, `Publisher_AppPipeline`, and (Host only) the registration class are internal.
  - `ThirdHost_ReferencingTwoHosts_CompilesWithoutCS0436` (Host, Aot, Link): HostA and HostB (same
    generated namespace, same scope name) are emitted and referenced by HostC, which runs the generator
    and uses its own `AddGeneratedMediator()`, `AddGeneratedMediator<AppPipeline>()`, `Mediator`, and
    `MediatorManifest`. No CS0436 / CS0433 / CS0121 and no warnings or errors; HostC emits.
  - `AddGeneratedMediator_IsNotVisibleOutsideTheHost`: a plain consumer of HostA gets only CS1061.
- Negative check: with `source/` stashed (public types), all 7 tests fail with CS0436 (14), CS0433 (3),
  and CS0121 (3), which reproduces the Nuru `run-ci-tests.cs` warnings and the ambiguous
  `AddGeneratedMediator()`.
- `<Version>` 14.0.0-beta.4 in `Directory.Build.props` and `source/Directory.Build.props`; readme and
  documentation version references bumped. Changelog "Changes in 14.0.0-beta.4" in
  `documentation/generated-vs-legacy.md`, a bullet and `internal sealed` wording in
  `documentation/m1-generated-mediator.md`, and a visibility paragraph in the readme's generated-membership
  section. The changelog notes consumers can drop a generator-stripping step (Nuru's
  `RemoveTimeWarpMediatorGenerator` is a separate Nuru follow-up).
- Gates: `./bin/dev workflow` shows `Pipeline SUCCEEDED` (generators 34/34, analyzers 6/6, compilation
  7/7, runtime 165 passed / 2 skipped pre-existing; beta.4 nupkgs packed and analyzer/generator layout
  verified). `ganda repo audit`: 28 passed, 0 failed, 1 skipped.
- Not done here (by design): release to NuGet.org after merge + green master CI (cockpit, `dev release`).

### How to validate

Smoke:

```bash
./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --no-build
ganda repo audit
```

Expect:

- Workflow ends with `Pipeline SUCCEEDED`; packages are `*.14.0.0-beta.4.nupkg`.
- Compilation tests: 7 passed, 0 failed (`GeneratedTypes_AreInternal` x3,
  `ThirdHost_ReferencingTwoHosts_CompilesWithoutCS0436` x3, `AddGeneratedMediator_IsNotVisibleOutsideTheHost`).
- `ganda repo audit`: `Failed: 0`.
- Stashing the emitter change (`git stash push -- source/`) makes all 7 compilation tests fail with
  CS0436 / CS0433 / CS0121 diagnostics; `git stash pop` restores green.
