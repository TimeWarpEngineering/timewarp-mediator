# Delete the reflection runtime package, its tests, samples and signing

## Goal (implementer restatement)

The repository keeps one dispatcher: the source-generated mediator. This task deletes the reflection
MediatR fork and everything that exists only to keep it callable. There is no compatibility shim.

Acceptance:

- `source/timewarp-mediator`, `tests/timewarp-mediator-tests`, and `tests/timewarp-mediator-benchmarks` are gone, and no project still references them.
- The container samples (Autofac, DryIoc, Lamar, LightInject, SimpleInjector, Stashbox, Windsor, Microsoft DI, publish strategies) are gone, and `samples/readme.md` lists the generated samples.
- `timewarp-mediator.snk` and every `SignAssembly` property are gone, including the git-hook override. Type forwards leave with the runtime project. The strong-name `InternalsVisibleTo` for the runtime tests leaves with that project. Generator and analyzer friend assemblies that tests and the generator still compile against stay. That narrowing is deliberate: those friend assemblies are how the generator and the incrementality tests compile.
- `dev pack` asserts Contracts, Analyzers, and Generators. `dev benchmark` has no `legacy` project. Dev CLI capabilities match that set.
- Projects that construct a `ServiceCollection` keep the full `Microsoft.Extensions.DependencyInjection` package. No remaining project referenced Abstractions alone, so that central pin is gone. `agent.md` names the full package pin and notes that a Host-profile consumer still needs Abstractions (TWM008).
- `generated-vs-legacy.md`, `m1-benchmark-gap.md`, the two-dispatchers section of `agent.md`, and the 13.x migration sections no longer describe a reflection dispatcher that ships from this tree. Historical 14.0.0 notes that still cite the deleted page point at the `v14.0.0` tag. The 15.0 architecture page stays on 021-007.
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
- [x] Review round 1 fix pass: M1, M2, M3, M5
- [ ] PR with merge commit

## Review

- Round 1: fix pass applied M1, M2, M3, M5. M4 and M6 stay wontfix (021-005 and 021-007).

## Notes

Parent: 021. Depends on: 021-003 (the generated dispatcher must be the complete replacement
before the fallback is gone). Review sections 3 (F1, F16), 5. The runtime inventory is in the
parent kitchen `research/reflection-runtime-inventory.md`. Apache notice files stay in the repo
because the surviving contracts still carry MediatR-derived code.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- 2026-10-24: implementer oracle removed the reflection runtime, its tests, the legacy benchmark, the container samples, and strong-name signing. Living docs no longer tell a host to call `AddMediator()`. `./bin/dev workflow` succeeded. `ganda repo audit --fix` dropped the orphaned `Microsoft.Extensions.DependencyInjection.Abstractions` package version.
- 2026-10-24: restored the executable bit on `tools/dev-cli/dev.cs`. The removal edit rewrote that shebang runfile as a normal file, and the attest hook will not sign until it is executable again.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-24T08:38:33Z
- Review round 1 (2026-10-24): review oracle (claude, model claude-opus-5-5), effort 3, roster general, session not reported. Re-ran `./bin/dev build`, `test --no-build`, and `pack --no-build`; all green. Found 0 bugs, 1 suggestion, 5 nits. M4 and M6 are `wontfix` (deferred to 021-005 and 021-007). M1, M2, M3, and M5 are open and need a fix pass. Files: `review/review-framework.md` and `review/round-1/{general,merged}.md`.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-24T08:43:56Z
- 2026-10-24: fix pass for review round 1. M1 names the full `Microsoft.Extensions.DependencyInjection` pin in `agent.md` and notes the Host-profile Abstractions requirement (TWM008). M2 changes the Contracts row from "both stacks" to "every host and message library". M3 deletes the dead `SignAssembly` override in `.githooks/Directory.Build.props`. M5 repoints the three 14.0.0 release-note links at the `v14.0.0` blob of `generated-vs-legacy.md`. M4 and M6 stay wontfix.

## Learnings

- Misread (review round 1): the first `## Goal (implementer restatement)` left out the requirement "Drop `Microsoft.Extensions.DependencyInjection` (full container) from the remaining projects where only Abstractions is needed". Results cover it: every remaining full-DI project builds a `ServiceCollection`, so none can drop. The restatement also narrowed "remove `InternalsVisibleTo`" to the strong-name pairing for the runtime tests. That reading is reasonable, because generator and analyzer friend assemblies are how the tests compile. The fix-pass restatement now lists that DI requirement and states the friend-assembly narrowing as deliberate.

## Results

The generated dispatcher is the only one in this tree. `source/timewarp-mediator`, `tests/timewarp-mediator-tests`, `tests/timewarp-mediator-benchmarks`, the nine container samples, `timewarp-mediator.snk`, and `SignAssembly` are gone. Type forwards left with the runtime project.

Analyzer `InternalsVisibleTo` for `TimeWarp.Mediator.Generators`, and generator `InternalsVisibleTo` for the compilation and Roslyn-floor test assemblies, stay. Those friend assemblies are how the generator and the incrementality tests compile. They are not the strong-name pairing for the deleted runtime tests.

`dev pack` asserts `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, and `TimeWarp.Mediator.Generators`. `dev benchmark --project` is comparison only. Hosts that construct a `ServiceCollection` still reference the full `Microsoft.Extensions.DependencyInjection` package. Nothing in the tree referenced Abstractions alone after the runtime project left, so that central version is gone. The comparison baseline no longer lists `TimeWarpLegacyMakeGenericType`. Its Ratio column still treats that removed row as 1.00 until the next `./bin/dev benchmark --project comparison --record`. `<Version>` stays `14.0.0`.

`./bin/dev workflow` (clean, build, test, verify-samples, pack) succeeded on the deletion commit. Tests: generators 57, analyzers 42, compilation 88, Roslyn floor 89, all passed. Samples `aot` and `named-pipelines` ran. Pack verified the three nupkgs and the package smoke printed `package-pong`. `ganda repo audit` passes.

Review round 1 fix pass (M1, M2, M3, M5): `agent.md` names the full `Microsoft.Extensions.DependencyInjection` pin used by the test, benchmark, sample, and smoke hosts, and notes that a Host-profile consumer needs Abstractions (TWM008). The Contracts row says the package is needed by every host and message library. `.githooks/Directory.Build.props` no longer sets `SignAssembly`. The three links in the 14.0.0 release notes and the GitHub release draft point at `https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/documentation/generated-vs-legacy.md`. Tag `v14.0.0` (`96e3e427`) still contains that file. M4 and M6 stay wontfix. The fix pass did not re-run `./bin/dev workflow`; the delta is prose plus one dead MSBuild property outside the solution. This pass re-ran `ganda repo audit` (32 passed, 0 failed). `git grep` for `SignAssembly` outside `kanban/` and `documentation/archive`, and for `both stacks` in `readme.md` and `agent.md`, printed no lines. The three release-note links use the `v14.0.0` blob.

### How to validate

Smoke: `git grep -n SignAssembly -- ':!kanban' ':!documentation/archive'`; `git grep -n 'both stacks' -- readme.md agent.md`; `git grep -n 'blob/v14.0.0/documentation/generated-vs-legacy.md' -- documentation/release-notes`; `./bin/dev workflow`

Expect: `SignAssembly` and `both stacks` print no lines. The release-note grep prints the two notes lines and the draft line, each with the `v14.0.0` blob URL. The pipeline prints `Pipeline SUCCEEDED`. Pack verifies `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, and `TimeWarp.Mediator.Generators` only.
