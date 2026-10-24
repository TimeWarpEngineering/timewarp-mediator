# Generator incrementality: syntax-provider candidates and a Cached assertion

## Goal (implementer restatement)

The model step is a pure function of equatable inputs. An unrelated edit leaves it `Cached`; a new handler leaves it `Modified`. Host requests, handlers, notifications, scopes, and assembly behaviors come from syntax providers as records of strings, flags, and spans. Referenced member assemblies come from metadata references. The analyzer still walks symbols. Emitted mediator text matches that walk, including membership order and behavior order. Options, host candidates, referenced assemblies, the model, sources, and diagnostics each have a tracking name.

Review round 1 found places the fact path still diverges from the symbol walk outside the in-repo goldens. This pass closes them:

- Host name and public key come from the compilation, so a package consumer is a member and a strong-named `InternalsVisibleTo` grant still binds.
- The handler match carries the message scope, so a notification scoped only on its own type keeps that pipeline and a scope mismatch is still reported.
- Every type, including arrays, keeps interface keys, so a behavior with an interface constraint on `TResponse` still closes.
- Inherited handler interfaces are removed before the per-request dedupe, so a derived handler with a different response still reports TWM002.
- Every assembly attribute is tested by symbol, so a using-alias of `MediatorBehavior` is not dropped.
- A type-parameter constraint does not close. A nested pipeline argument such as `Result<TRequest>` does.
- Duplicate metadata references are skipped. An inaccessible notification handler does not suppress TWM001. Facts are ordered by assembly name. TWM007 uses the inaccessible type's location. A runtime type declared in the host is not a false TWM008.

Acceptance criteria:

- An unrelated edit leaves the model step `Cached`, and a new handler leaves it `Modified`.
- The six tracking names stay in place.
- Compilation tests cover the divergences above. Package smoke asserts the host assembly is in manifest membership and a host behavior is woven.
- Existing goldens stay the same except where a finding above changes output the goldens never reached.

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
- [x] Review round 1 findings M1–M12 closed on this branch
- [ ] PR with merge commit

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12
- Fix pass: M1–M12 implemented on this branch (a1979f8).
- Round 2: Done
## Notes

Parent: 021. Depends on: 021-003 (emitter settled first so the pipeline refactor does not
chase a moving target). Review section 3 (F9), 4 (generator pipeline). Can run in parallel
with 021-004 and 021-005.

## Results

The fact path now follows the symbol walk on the round 1 gaps, and the model step still does not take `CompilationProvider`.

Host name and public key come from `CompilationProvider.Select` into `HostIdentity`. The referenced-assemblies step consumes that value, including `WithCryptoPublicKey` on the scratch compilation, so a strong-named `InternalsVisibleTo` grant matches the host. `TimeWarpMediatorAssemblyName` is no longer set in the buildTransitive props. The model step still combines only host facts, referenced facts, and options, so an unrelated edit stays `Cached`.

Each handler match carries `ResolveScope` of the message. Facts are ordered by assembly name, then discovery index. `BoundByBase` runs before the per-request dedupe. An inaccessible notification match does not suppress TWM001. Duplicate metadata references are skipped. Array types keep `AllInterfaces` and nested type arguments, so an interface constraint on `TResponse` closes. A type-parameter constraint does not close. A nested pipeline argument such as `Result<TRequest>` does, and the woven behavior is the closed behavior type (`App.Wrap<App.Ping>`). Every assembly-targeted attribute is tested by symbol, so a using-alias of `MediatorBehavior` is kept. TWM007 uses the inaccessible symbol's location. A required runtime type declared in the host is removed from the missing set before TWM008.

Tracking names are unchanged: `TimeWarpMediator.Options`, `HostCandidates`, `ReferencedAssemblies`, `Model`, `Sources`, `Diagnostics`.

`./bin/dev workflow --mode pr` succeeded: clean, build with 0 warnings, tests 57 + 42 + 69, both samples, and pack. Package smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` and exited 0, which includes the host membership and host behavior checks. `ganda repo audit` passed (32 passed, 0 failed). The pull request is the host open-pr node.

### Review disposition

- **Outcome:** accepted-exceptions. 2 rounds. Final open count: 0.
- **Roster and effort:**
  - Round 1: effort 3, roster general, two parallel general passes split by file.
  - Round 2: effort 3, roster general, one pass over the fix delta `be72432..a1979f8`.
- **Final counts:**

  | Severity | fixed | wontfix | open |
  |----------|-------|---------|------|
  | bug | 6 | 0 | 0 |
  | suggestion | 1 | 0 | 0 |
  | nit | 5 | 2 | 0 |

- **Wontfix:**
  - M13: a dead `MembershipOptions.HostAssemblyName` option. It has no behavior effect, and removing it changes the Analyzers public surface, so it is deferred to cleanup.
  - M14: an unused `RequestModel` parameter in a private helper.
  - The review oracle decided both.
- **Round 2 verification:** `./bin/dev build` clean; `./bin/dev test` 57 + 42 + 69 passed; `./bin/dev pack` succeeded, and package smoke exited 0.
- **Follow-up candidate (not blocking):** the `ExtractBehavior` `GetAttributes` index can go stale across files during IDE incremental runs. It affects only the same-`Order` tie-break.
- **Paths:** `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke: `dotnet test tests/timewarp-mediator-generators-compilation-tests --filter FullyQualifiedName~IncrementalGeneratorTests|FullyQualifiedName~ReviewFindingsTests`

Expect: `UnrelatedEdit_LeavesModelCached_AndSourcesCached` passes with the model step `Cached`. `NewHandler_ModifiesModel_AndReEmits` passes with the model step `Modified`. `ReviewFindingsTests` passes 13/13, including an empty `TimeWarpMediatorAssemblyName` that still lists the compilation as a member, a strong-named host that binds an internal handler, and a notification scope that survives when the message has no fact. `./bin/dev workflow --mode pr` stays green, and package smoke exits 0 after asserting the host assembly name is inside manifest membership and `SmokeBehavior<SmokePing, string>` is woven.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: 01a16d96-39b7-74c0-80e7-46cb56ac2191 (2026-10-24)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-24T11:41:19Z
- Review oracle round 1: claude-opus-5-5 (Claude Code headless), effort 3, roster general. Subagents aee4966440ea66a39 (general-a, extraction) and a0e0c0b5a6062138b (general-b, graph/emitters). 12 open findings; fix pass needed — 2026-10-24
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-24T11:52:54Z
- Fix pass (implement-fix-2): 01a16dc1-b941-7e80-b3a6-619a113fcc54 (2026-10-24). M1–M12 implemented. Workflow and audit green.
- Review oracle round 2: reviewer-claude (claude-opus-5-5, Claude Code headless), effort 3, roster general, max-turns 200, session not reported. M1–M12 fixed, M13 and M14 wontfix, disposition accepted-exceptions — 2026-10-24
