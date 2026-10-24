# Delete the reflection runtime package, its tests, samples and signing

## Goal (implementer restatement)

The repository keeps one dispatcher: the source-generated mediator. This task deletes the reflection
MediatR fork and everything that exists only to keep it callable. There is no compatibility shim.

Acceptance:

- `source/timewarp-mediator`, `tests/timewarp-mediator-tests`, and `tests/timewarp-mediator-benchmarks` are gone, and no project still references them.
- The container samples (Autofac, DryIoc, Lamar, LightInject, SimpleInjector, Stashbox, Windsor, Microsoft DI, publish strategies) are gone, and `samples/readme.md` lists the generated samples.
- `timewarp-mediator.snk` and `SignAssembly` are gone. Type forwards leave with the runtime project. The strong-name `InternalsVisibleTo` for the runtime tests leaves with that project. Generator and analyzer friend assemblies that tests and the generator still compile against stay.
- `dev pack` asserts Contracts, Analyzers, and Generators. `dev benchmark` has no `legacy` project.
- `generated-vs-legacy.md`, `m1-benchmark-gap.md`, the two-dispatchers section of `agent.md`, and the 13.x migration sections no longer describe a reflection dispatcher that ships from this tree. The 15.0 architecture page stays on 021-007.
- `./bin/dev workflow` passes.
- The pull request is the host open-pr node.

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

- [x] Runtime project, tests, legacy benchmark deleted
- [x] Legacy samples deleted, `samples/readme.md` updated
- [x] Signing and type forwards removed
- [x] `dev pack` / `dev benchmark` / capabilities updated
- [x] Stale docs removed
- [x] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-003 (the generated dispatcher must be the complete replacement
before the fallback is gone). Review sections 3 (F1, F16), 5. The runtime inventory is in the
parent kitchen `research/reflection-runtime-inventory.md`. Apache notice files stay in the repo
because the surviving contracts still carry MediatR-derived code.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- 2026-10-24: implementer oracle removed the reflection runtime, its tests, the legacy benchmark, the container samples, and strong-name signing. Living docs no longer tell a host to call `AddMediator()`. `./bin/dev workflow` succeeded. `ganda repo audit --fix` dropped the orphaned `Microsoft.Extensions.DependencyInjection.Abstractions` package version.

## Results

The generated dispatcher is the only one in this tree. `source/timewarp-mediator`, `tests/timewarp-mediator-tests`, `tests/timewarp-mediator-benchmarks`, the nine container samples, `timewarp-mediator.snk`, and `SignAssembly` are gone. Type forwards left with the runtime project.

Analyzer `InternalsVisibleTo` for `TimeWarp.Mediator.Generators`, and generator `InternalsVisibleTo` for the compilation and Roslyn-floor test assemblies, stay. Those friend assemblies are how the generator and the incrementality tests compile. They are not the strong-name pairing for the deleted runtime tests.

`dev pack` asserts `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, and `TimeWarp.Mediator.Generators`. `dev benchmark --project` is comparison only. Hosts that construct a `ServiceCollection` still reference the full `Microsoft.Extensions.DependencyInjection` package. Nothing in the tree referenced Abstractions alone after the runtime project left, so that central version is gone. The comparison baseline no longer lists `TimeWarpLegacyMakeGenericType`. Its Ratio column still treats that removed row as 1.00 until the next `./bin/dev benchmark --project comparison --record`. `<Version>` stays `14.0.0`.

`./bin/dev workflow` (clean, build, test, verify-samples, pack) succeeded. Tests: generators 57, analyzers 42, compilation 88, Roslyn floor 89, all passed. Samples `aot` and `named-pipelines` ran. Pack verified the three nupkgs and the package smoke printed `package-pong`. `ganda repo audit` passes.

### How to validate

Smoke: `./bin/dev workflow` and `./bin/dev benchmark --help`

Expect: the pipeline prints `Pipeline SUCCEEDED`. Pack verifies `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, and `TimeWarp.Mediator.Generators` only. Benchmark help describes the project as `comparison (default)` and does not list `legacy`.
