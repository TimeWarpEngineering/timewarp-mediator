# M1 generated Mediator analyzer and State golden-file

## Description

Prove the rewrite core: a source generator plus analyzer that treats handlers + behaviors as a compile-time graph, emits a real `sealed Mediator : IMediator`, and matches TimeWarp.State's current pipeline semantics on one ActionSet.

Parent: **004**. Do not implement `ISender<TScope>` here — that is **004-002**.

Design SSOT: `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14 (M1 table).

## Requirements

- Handler-first discovery with explicit assembly membership (`MediatorOptions.Assemblies`, `[assembly: MediatorAssembly]`, or equivalent)
- `TimeWarp.Mediator.Analyzers`: TWM001 (request with no handler), TWM002 (duplicate handler)
- Generated `sealed Mediator : IMediator` with monomorphic `Send`; `Send(object)` is a generated switch (JsonRequestHandler / JS interop path)
- `ValueTask` contracts, including void/`Unit` actions (`IAction` / nested `Handler`)
- MS.DI scope-resolved handlers and behaviors (Host/State default). ServiceGen static fields may appear as an AOT sample, not the State path
- One real State-shaped golden file: `IncrementActionSet` + `StateTransactionBehavior` (scoped) matching today's `Reverse().Aggregate` order (short-circuit, clone/restore, exception notification)
- `mediator.manifest.json` v1
- AOT sample publishes trim/AOT-analyzer-clean with **no** `NoWarn` on IL2026/IL3050

## Checklist

### Design
- [x] Fold spec header: this epic is 004, this task is 004-001
- [x] Confirm membership rule so multi-project solutions do not cross-link by accident

### Implementation
- [x] Analyzer package (TWM001, TWM002) usable on a library that does not run the generator
- [x] Generator emits `Mediator` + `Send(object)` switch
- [x] State golden-file: IncrementActionSet + StateTransactionBehavior
- [x] AOT sample: EnableTrimAnalyzer + EnableAotAnalyzer + IsAotCompatible, warning-clean
- [x] Benchmarks vs current MakeGenericType fork **and** martinothamar (document the gap; include a CallSiteInlining prototype number even if interceptors stay out of product)

### Documentation
- [x] Point GitHub issue #52 at 004 / 004-001
- [x] Notes on what is deferred to 004-002 (scoped senders) and later (interceptors, pruning, streams)

## Out of scope

- `ISender<TScope>` / `IPublisher<TScope>` emit (004-002)
- Call-site interceptors as the default dispatch
- Call-graph pruning
- TimeWarp.State NuGet switch
- TimeWarp.ServiceGen (Nuru 444)

## Notes

- Re-entrancy is mandatory: handlers and behaviors inject `ISender` and `Send` while a dispatch is active. Keep a real injectable `ISender` object; static-only dispatch is not the default.
- Pipeline default: compile-time-fixed order, scope-resolved instances at send (OQ-B in the spec).
- Consumers of this package: TimeWarp.State (primary), then Nuru 443. Do not wait on 444.

## Session

- Created: 2438044 (2026-08-31)
- Implementer: grok session 2438044 (2026-08-31)
- Review oracle: grok (2026-08-31); round-1 general 01a057c6-4895-7580-b307-f322776c2ca7; round-2 general 01a057cf-1c3e-7973-a604-b04431292981

## Results

M1 rewrite core: analyzer + generator + State golden-file + AOT sample. Reflection `TimeWarp.Mediator.Mediator` stays for Phase A coexistence (`AddMediator()`). Generated path is `AddGeneratedMediator()` → `TimeWarp.Mediator.Generated.Mediator`.

### What shipped

- Spec header folded to epic **004** / task **004-001**. Membership rule confirmed in spec §9: no `[assembly: MediatorAssembly]` / `MediatorAssemblies` / generator MSBuild opt-in → not linked. Behaviors only via `[assembly: MediatorBehavior]`; first listed is outermost (`Reverse().Aggregate`).
- `TimeWarp.Mediator.Analyzers`: TWM001, TWM002. Analyzer-only tests prove TWM001 fires without the generator.
- `TimeWarp.Mediator.Generators`: sealed generated `Mediator`, monomorphic `ValueTask Send`, `Send(object)` switch, `MediatorManifest` v1 JSON, Host `AddGeneratedMediator()`, Aot `ServiceGen` static fields. `Dispatch_*` is the CallSiteInlining prototype target.
- Contracts: `IAction` / `ActionHandler` (`ValueTask`), `ICommand` / `IQuery`, membership attributes, `NoHandlerException`. `ISender` / `IMediator` / handlers / pipeline interfaces moved to Contracts (type-forwarded) so the AOT sample does not reference the reflection assembly.
- Golden file `test/TimeWarp.Mediator.Generators.Tests`: `IncrementActionSet` + scoped `StateTransactionBehavior` (clone/restore + `ExceptionNotification`), short-circuit, re-entrant `ISender`, pipeline order matches legacy.
- AOT sample `samples/TimeWarp.Mediator.Examples.Aot`: `EnableTrimAnalyzer` + `EnableAotAnalyzer` + `IsAotCompatible`, **no** `NoWarn` on IL2026/IL3050, warning-clean build; `dotnet run` prints `aot-pong`. Native `dotnet publish` reached IL compile then failed link (`-lz` missing on this machine) — not an IL warning.
- Benchmarks: `Documentation/m1-benchmark-gap.md`. Legacy 50.0 ns / 224 B; generated 29.2 ns / 96 B; CallSiteInlining prototype 28.5 ns / 96 B; martinothamar 8.3 ns / 24 B. Gap documented; interceptors stay out of product.

### Files (high level)

- `src/TimeWarp.Mediator.Analyzers/**`
- `src/TimeWarp.Mediator.Generators/**`
- `src/TimeWarp.Mediator.Contracts/**` (moved interfaces + new M1 contracts)
- `test/TimeWarp.Mediator.Analyzers.Tests/**`
- `test/TimeWarp.Mediator.Generators.Tests/**`
- `test/TimeWarp.Mediator.Benchmarks.Comparison/**`
- `samples/TimeWarp.Mediator.Examples.Aot/**`
- `Documentation/m1-generated-mediator.md`, `Documentation/m1-benchmark-gap.md`
- `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` (header + §9)

### Deferred (not this task)

- **004-002:** `ISender<TScope>` / `IPublisher<TScope>`
- Later: interceptors as default dispatch, pruning, streams, exception cascade, TimeWarp.State NuGet switch, ServiceGen (Nuru 444), `AddMediator()` auto-preferring generated type

### Test outcomes

- Analyzers.Tests: 4 passed
- Generators.Tests (golden file): 10 passed
- TimeWarp.Mediator.Tests: 163 passed, 2 skipped
- AOT sample build: 0 warnings, 0 errors; run prints `aot-pong`

### How to validate

**Smoke**

```bash
# Analyzer-only TWM001/TWM002 (no generator)
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Analyzers.Tests -c Release
# expect: 4 passed; Twm001_RequestWithNoHandler_IsError is the library-without-generator proof

# State golden-file + generated Mediator
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Generators.Tests -c Release
# expect: 10 passed including PipelineOrder_MatchesReverseAggregate, Increment_OnHandlerException_RestoresStateAndPublishes, Increment_ShortCircuit_SkipsHandler, UnitOnlyBehavior_ClosesOntoUnitRequestsOnly

# AOT/trim analyzers, no IL2026/IL3050 NoWarn
dotnet build samples/TimeWarp.Mediator.Examples.Aot -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet run --project samples/TimeWarp.Mediator.Examples.Aot -c Release --no-build
# expect: Build succeeded, 0 Warning(s); stdout aot-pong
```

**Expect**

- TWM001 is an error on a member assembly request with no handler; a compilation without `[assembly: MediatorAssembly]` (and without generator props) reports nothing.
- Generated `TimeWarp.Mediator.Generated.Mediator` is sealed, implements `IMediator`, and `MediatorManifest.Version == 1`.
- Increment by 2 on Count=8 yields 10 with a new state Guid; negative amount restores Count=8 and publishes `ExceptionNotification`.
- Ping pipeline log equals `outer-before, inner-before, handler, inner-after, outer-after` on both generated and legacy `Mediator`.

**Automated gate**

```bash
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Tests -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Analyzers.Tests -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Generators.Tests -c Release
dotnet build samples/TimeWarp.Mediator.Examples.Aot -c Release
```

Expect: existing suite 163 passed / 2 skipped; new suites all passed; AOT sample 0 warnings.

**Depends on**

- `DOTNET_ROLL_FORWARD=LatestMajor` when the host SDK is newer than net8.0 and the net8 runtime is not installed.
- Native `dotnet publish -p:PublishAot=true` needs zlib (`-lz`) on the linker path; analyzer-clean is the M1 gate.

**Not in scope**

- Live TimeWarp.State NuGet switch, `ISender<TScope>`, interceptors as default dispatch, native AOT link on machines without libz.

### Review

- **Effort:** 1 (general only). **Rounds:** 2.
- **Roster:** general (round 1: 01a057c6-4895-7580-b307-f322776c2ca7; round 2: 01a057cf-1c3e-7973-a604-b04431292981).
- **Final counts:** bug 0 open / 1 fixed; suggestion 0 open / 3 fixed; nit 0. Open = 0.
- **Disposition:** **clean** (`review/disposition.md`).
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.
- Round-1 findings fixed on this id: M1 `TryCloseBehavior` null on failed `ImplementsPipeline` + `UnitOnlyBehavior` regression; M2 embedded `MediatorManifest` v1 (no loose file / unused path props); M3 deterministic assembly-name `SourceIndex`; M4 document Aot skipping behaviors. No wontfix, no escalation.
