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
- [x] CI pack: `TimeWarp.Mediator.Analyzers` nupkg must have content (NU5017)
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

### Reopened 2026-09-01 — CI red on PR #53 (inbound)

`ganda pr merge 53 --task-id 004-001` refused: **build-and-publish** failed.

```
error NU5017: Cannot create a package that has no dependencies nor content.
[/home/runner/work/timewarp-mediator/timewarp-mediator/src/TimeWarp.Mediator.Analyzers/TimeWarp.Mediator.Analyzers.csproj]
```

Log also printed `Successfully created package '.../TimeWarp.Mediator.Analyzers.13.0.0.nupkg'` then NU5017 on the same csproj (likely a second pack / snupkg / empty pack). Tests passed (Analyzers 4, Mediator.Tests 163/2 skipped). Mediator + Contracts nupkgs built.

**This slice:** make `Build.ps1` / CI pack produce a non-empty Analyzers nupkg (analyzer DLL in `analyzers/dotnet/cs` or equivalent). Push to the existing PR branch. Do not start 004-002. Do not merge.

## Session

- Created: 2438044 (2026-08-31)
- Implementer: grok session 2438044 (2026-08-31)
- Review oracle: grok (2026-08-31); round-1 general 01a057c6-4895-7580-b307-f322776c2ca7; round-2 general 01a057cf-1c3e-7973-a604-b04431292981
- Reopened: cockpit 2026-09-01 — PR #53 NU5017; dispatched back onto this id
- Implementer: grok (2026-09-01) — NU5017 analyzer pack fix; session 01a058f1-1b82-76d3-aef4-e895f0b2e2e2
- Review oracle: grok (2026-09-01) round-3 general 01a058fc-c569-7ee3-a5b5-7c7929ecd0ce; disposition clean

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

### Reopen 2026-09-01 — NU5017 pack

PR #53 `build-and-publish` failed because repo-wide `IncludeSymbols`/`snupkg` plus `IncludeBuildOutput=false` packed a good Analyzers nupkg (README + Logo + `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`) then NU5017 on the empty snupkg. Generators would have failed the same way next.

Fix: `IncludeSymbols=false` on Analyzers and Generators. `Build.ps1` asserts the analyzer DLLs are in the nupkgs. Mediator and Contracts still emit snupkg.

Local `pwsh -File ./Build.ps1` (2026-09-01): tests 4 + 10 + 163/2 skipped; Analyzers and Generators nupkgs created with no snupkg and no NU5017.

Files this slice: `src/TimeWarp.Mediator.Analyzers/TimeWarp.Mediator.Analyzers.csproj`, `src/TimeWarp.Mediator.Generators/TimeWarp.Mediator.Generators.csproj`, `Build.ps1`.

### How to validate

**Smoke**

```bash
DOTNET_ROLL_FORWARD=LatestMajor pwsh -File ./Build.ps1
python3 - <<'PY'
import glob, zipfile
nupkg = glob.glob("Artifacts/TimeWarp.Mediator.Analyzers.*.nupkg")[0]
assert glob.glob("Artifacts/TimeWarp.Mediator.Analyzers.*.snupkg") == []
with zipfile.ZipFile(nupkg) as z:
    names = z.namelist()
assert "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll" in names
print("ok", nupkg)
PY
```

**Expect**

- `Build.ps1` exits 0. No NU5017.
- `Artifacts/TimeWarp.Mediator.Analyzers.13.0.0.nupkg` exists and contains `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`.
- No `TimeWarp.Mediator.Analyzers.*.snupkg` or `TimeWarp.Mediator.Generators.*.snupkg`.
- Generators nupkg contains both analyzer DLLs plus `buildTransitive/TimeWarp.Mediator.Generators.props`.
- Mediator and Contracts still produce `.snupkg`.
- TWM001 is an error on a member assembly request with no handler; a compilation without `[assembly: MediatorAssembly]` (and without generator props) reports nothing.
- Generated `TimeWarp.Mediator.Generated.Mediator` is sealed, implements `IMediator`, and `MediatorManifest.Version == 1`.

**Automated gate**

```bash
DOTNET_ROLL_FORWARD=LatestMajor pwsh -File ./Build.ps1
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Tests -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Analyzers.Tests -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Generators.Tests -c Release
dotnet build samples/TimeWarp.Mediator.Examples.Aot -c Release
```

Expect: `Build.ps1` packs all four nupkgs; existing suite 163 passed / 2 skipped; analyzer 4 passed; generators 10 passed; AOT sample 0 warnings.

**Depends on**

- `DOTNET_ROLL_FORWARD=LatestMajor` when the host SDK is newer than net8.0 and the net8 runtime is not installed.
- `pwsh` for `Build.ps1` (CI uses `shell: pwsh`).
- Native `dotnet publish -p:PublishAot=true` needs zlib (`-lz`) on the linker path; analyzer-clean is the M1 gate.

**Not in scope**

- Live TimeWarp.State NuGet switch, `ISender<TScope>`, interceptors as default dispatch, native AOT link on machines without libz, merge of PR #53.

### Review

- **Effort:** 1 (general only). **Rounds:** 3.
- **Roster:** general (round 1: 01a057c6-4895-7580-b307-f322776c2ca7; round 2: 01a057cf-1c3e-7973-a604-b04431292981; round 3: 01a058fc-c569-7ee3-a5b5-7c7929ecd0ce).
- **Final counts:** bug 0 open / 1 fixed; suggestion 0 open / 3 fixed; nit 0. Open = 0.
- **Disposition:** **clean** (`review/disposition.md`).
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/round-3/general.md`, `review/round-3/merged.md`, `review/disposition.md`.
- Round-1 findings fixed on this id: M1 `TryCloseBehavior` null on failed `ImplementsPipeline` + `UnitOnlyBehavior` regression; M2 embedded `MediatorManifest` v1 (no loose file / unused path props); M3 deterministic assembly-name `SourceIndex`; M4 document Aot skipping behaviors.
- Round 3 (pack-fix reopen): no new findings. `IncludeSymbols=false` on Analyzers/Generators closes NU5017; `Build.ps1` asserts analyzer DLLs; Mediator/Contracts still snupkg. M1–M4 still fixed. No wontfix, no escalation.
