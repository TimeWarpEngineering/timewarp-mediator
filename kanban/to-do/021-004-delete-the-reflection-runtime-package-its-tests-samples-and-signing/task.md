# Delete the reflection runtime package, its tests, samples and signing

## Description

No consumer references the `TimeWarp.Mediator` reflection runtime; State, Architecture and Nuru
are generator-only. The MediatR fork, its nine DI-container samples, the legacy benchmark
project, the type forwards and the strong-name signing exist only for backward compatibility.
Parent review task 021, findings F1 and F16; decision D1 signed off 2026-10-23 (no shim).

## Requirements

- Delete `source/timewarp-mediator` (reflection `Mediator`, publishers, pre/post/exception
  processor behaviors, `AddMediator()`, `MediatorServiceConfiguration`, `ServiceRegistrar`).
- Delete `tests/timewarp-mediator-tests` and `tests/timewarp-mediator-benchmarks` (legacy).
- Delete the legacy samples (Autofac, DryIoc, Lamar, LightInject, SimpleInjector, Stashbox,
  Windsor, Microsoft DI, publish strategies) and update `samples/readme.md`.
- Remove `timewarp-mediator.snk`, `SignAssembly`, `InternalsVisibleTo` and the `tests/`
  signing overrides; remove type forwards.
- `dev pack` asserts the new package set; `dev benchmark --project legacy` is removed; dev CLI
  capabilities updated.
- Drop `Microsoft.Extensions.DependencyInjection` (full container) from the remaining projects
  where only Abstractions is needed.
- Delete or rewrite docs that describe the reflection dispatcher: `generated-vs-legacy.md`,
  the "two dispatchers" section of `agent.md`, `m1-benchmark-gap.md`, 13.x migration sections
  (the replacement architecture page is 021-007; this task removes the stale text).

## Checklist

- [ ] Runtime project, tests, legacy benchmark deleted
- [ ] Legacy samples deleted, `samples/readme.md` updated
- [ ] Signing and type forwards removed
- [ ] `dev pack` / `dev benchmark` / capabilities updated
- [ ] Stale docs removed
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-003 (the generated dispatcher must be the complete replacement
before the fallback is gone). Review sections 3 (F1, F16), 5. The runtime inventory is in the
parent kitchen `research/reflection-runtime-inventory.md`. Apache notice files stay in the repo
because the surviving contracts still carry MediatR-derived code.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
