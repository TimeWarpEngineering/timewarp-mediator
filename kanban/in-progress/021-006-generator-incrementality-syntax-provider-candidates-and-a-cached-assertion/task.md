# Generator incrementality: syntax-provider candidates and a Cached assertion

## Goal (implementer restatement)

The generator rebuilds its whole message graph from `CompilationProvider` on every host edit, so an unrelated keystroke still walks every member assembly. The incrementality test treats that as success because the model step comes back `Unchanged` (it ran, and the value compared equal). `Cached` is the result that means the step did not run.

This change makes the model a pure function of equatable inputs. Host requests, handlers, notifications, scopes, and `[assembly: MediatorBehavior]` come from syntax providers. Each provider returns records of strings, flags, and spans, with no `ISymbol` and no `SyntaxNode`. Referenced member assemblies are read from `MetadataReferencesProvider` plus the host assembly name (a compiler-visible property, the stable slice of the compilation identity), so that walk runs only when references or that property change. The model step combines those inputs, closes behaviors, and emits the same sources and diagnostics as the symbol walk the analyzer still uses.

Acceptance criteria:

- An unrelated edit leaves the model step `Cached`, and a new handler leaves it `Modified`.
- Options, host candidates, referenced assemblies, model, sources, and diagnostics each have a tracking name.
- Emitted mediator text stays the same, including manifest membership order and behavior order.
- `./bin/dev workflow` is green.

## Description

The generator's model step hangs off `CompilationProvider`, so every keystroke in the host
re-walks the whole message graph, including referenced assemblies. The incrementality test
asserts `Unchanged`, which passes even when the step re-executed. Parent review task 021,
finding F9; gate in section 9.

## Requirements

- Host candidates (requests, handlers, behaviors, scopes, membership attributes) come from
  `ForAttributeWithMetadataName` / syntax providers with equatable models.
- Referenced-assembly membership is keyed on `MetadataReferencesProvider` (or the equivalent
  compilation-options slice), not on the full `Compilation`.
- Models are value-equatable records with no `ISymbol` or `SyntaxNode` fields.
- `tests/timewarp-mediator-generators-compilation-tests` asserts
  `IncrementalStepRunReason.Cached` for the model step on an unrelated edit, and `Modified`
  on a relevant edit; add a trackable step name for each stage.
- No change to emitted output: golden files stay identical apart from the ordering guarantees
  already documented.

## Checklist

- [x] Syntax-provider candidates with equatable models
- [x] Metadata-reference keying for referenced assemblies
- [x] `Cached` and `Modified` assertions in the incrementality test
- [x] Golden files unchanged
- [x] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-003 (emitter settled first so the pipeline refactor does not
chase a moving target). Review section 3 (F9), 4 (generator pipeline). Can run in parallel
with 021-004 and 021-005.

## Results

Host candidates are `CreateSyntaxProvider` facts: type declarations that have a base list, and attributes whose name contains `MediatorBehavior`. `ForAttributeWithMetadataName` stays unused because that API combines `CompilationProvider`, which re-runs on an unrelated edit (`Unchanged`, not `Cached`). Referenced member assemblies come from `MetadataReferencesProvider` plus the compiler-visible `TimeWarpMediatorAssemblyName` property. Models are equatable records of strings, flags, and spans. The analyzer still walks symbols.

Tracking names: `TimeWarpMediator.Options`, `HostCandidates`, `ReferencedAssemblies`, `Model`, `Sources`, `Diagnostics`.

`UnrelatedEdit_LeavesModelCached_AndSourcesCached` requires Cached for options, host candidates, referenced assemblies, the model, and sources. `NewHandler_ModifiesModel_AndReEmits` requires host candidates, the model, and sources Modified, and referenced assemblies Cached.

The nine mediator files under `tests/timewarp-mediator-generators-tests` (`Mediator.g.cs`, manifest, service-collection extensions, and the three sender and publisher pipelines) match the pre-change snapshot byte for byte. `./bin/dev workflow --mode pr` succeeded: clean, build with 0 warnings, tests 57 + 42 + 56, both samples, and pack. `ganda repo audit` passed (32 passed, 0 failed). The pull request is the host open-pr node.

### How to validate

Smoke: `dotnet test tests/timewarp-mediator-generators-compilation-tests --filter FullyQualifiedName~IncrementalGeneratorTests`

Expect: `UnrelatedEdit_LeavesModelCached_AndSourcesCached` passes, and the model step's reason is `IncrementalStepRunReason.Cached`. `NewHandler_ModifiesModel_AndReEmits` passes, and the model step's reason is `Modified`. Rebuilding `tests/timewarp-mediator-generators-tests` emits the same mediator sources as before this change.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: 01a16d96-39b7-74c0-80e7-46cb56ac2191 (2026-10-24)
