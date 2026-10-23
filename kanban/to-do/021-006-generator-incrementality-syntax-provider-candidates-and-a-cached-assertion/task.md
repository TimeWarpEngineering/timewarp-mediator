# Generator incrementality: syntax-provider candidates and a Cached assertion

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

- [ ] Syntax-provider candidates with equatable models
- [ ] Metadata-reference keying for referenced assemblies
- [ ] `Cached` and `Modified` assertions in the incrementality test
- [ ] Golden files unchanged
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-003 (emitter settled first so the pipeline refactor does not
chase a moving target). Review section 3 (F9), 4 (generator pipeline). Can run in parallel
with 021-004 and 021-005.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
