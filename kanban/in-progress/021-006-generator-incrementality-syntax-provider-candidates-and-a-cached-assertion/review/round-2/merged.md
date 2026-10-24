# Round 2 — merged findings
**Date:** 2026-10-24
**Sources:** general (single pass by the review oracle; see `general.md`)
**Scope:** fix delta `be72432..a1979f8`

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 6 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 5 | 2 |

## Resolved prior (round 1)

| ID | Severity | Status | Note |
|----|----------|--------|------|
| M1 | bug | fixed | Host identity comes from the compilation. Package smoke asserts host membership and the host behavior weave. |
| M2 | bug | fixed | `HandlerMatch.MessageScope` supplies the scope when the message has no fact. |
| M3 | bug | fixed | The scratch compilation uses `WithCryptoPublicKey`. |
| M4 | bug | fixed | Every type keeps `AllInterfaces`, including arrays. |
| M5 | bug | fixed | `BoundByBase` runs before the per-request dedupe. |
| M6 | bug | fixed | Every assembly attribute is tested by symbol. |
| M7 | suggestion | fixed | A type-parameter constraint does not close. A nested or array slot does close. |
| M8 | nit | fixed | Duplicate references are skipped. |
| M9 | nit | fixed | `dropped` holds request matches only. |
| M10 | nit | fixed | Facts are ordered by assembly name. |
| M11 | nit | fixed | TWM007 uses the inaccessible type's location. The unreachable fallback throws. The Design comment is corrected. |
| M12 | nit | fixed | A runtime type declared in the host is not reported missing. |

Evidence for each is in `general.md`. Verification: `./bin/dev build` clean; `./bin/dev test` 57 + 42 + 69 passed; `./bin/dev pack` succeeded, and package smoke exited 0.

## Issues

### M13 — Severity: nit — Status: wontfix
- File: source/timewarp-mediator-analyzers/membership-options.cs:40-43; membership.cs:26
- Description: `MembershipOptions.HostAssemblyName` and `AssemblyNamePropertyName` are dead in product, and the XML doc suggests a consumer override that has no effect.
- Suggestion: Remove the option, or document that it is ignored.
- Source: general Issue 1
- Disposition notes: wontfix, decided by the review oracle (round 2). Cosmetic only: it causes no behavior change, no wrong output, and no diagnostic. The Design region at membership-options.cs:10-12 already says the generator does not read the property. The compilation tests use it to prove that an empty value is ignored. Removing it changes the public surface of `MembershipOptions` in the Analyzers assembly. That change can fold into later 15.0 cleanup and does not justify spending the last fix pass on this task.

### M14 — Severity: nit — Status: wontfix
- File: source/timewarp-mediator-generators/candidate-graph.cs:340-351
- Description: The `RequestModel` parameter `responseByRequest` is unused.
- Suggestion: Drop the parameter.
- Source: general Issue 2
- Disposition notes: wontfix, decided by the review oracle (round 2). It is a private helper and has no runtime effect. The build is clean under `TreatWarningsAsErrors`. Not worth a fix pass on its own.

## Duplicates / conflicts

- None. This round had a single source.

## Out-of-delta observation

- The `ExtractBehavior` attribute index can go stale across files during IDE incremental runs. It affects only the tie-break when two behaviors share an `Order`, and a command-line build always runs fresh. The code is unchanged in this delta and was not raised in round 1. It is recorded in `general.md` for a possible follow-up and is not a finding on this round.

## Plan alignment

- Restatement vs task: no misread. The restatement now adds the round 1 fix list, and each item maps to M1–M12. The core requirements are still met:
  - equatable syntax and metadata facts, with no `ISymbol` or `SyntaxNode` in the models;
  - `MetadataReferencesProvider` keying, plus an equatable `Select` for host identity;
  - `Cached` and `Modified` assertions;
  - the six tracking names;
  - goldens unchanged.
