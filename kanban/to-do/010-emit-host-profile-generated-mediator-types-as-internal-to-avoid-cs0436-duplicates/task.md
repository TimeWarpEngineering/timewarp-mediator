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

- [ ] Host types internal
- [ ] Two-host test, no CS0436
- [ ] Docs, version bump

## Notes

- Implementer: **commit and push your changes before reporting done.**
- Run the build and test gate in the foreground.
