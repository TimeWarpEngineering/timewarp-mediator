# Round 2 — general
**Date:** 2026-10-24
**Scope reviewed:** fix delta `be72432..a1979f8` (`fix: match generator facts to the symbol walk`): generators `candidate-extractor.cs`, `candidate-facts.cs`, `candidate-graph.cs`, `mediator-generator.cs`, `referenced-assembly-candidates.cs`, `type-model.cs`, build-transitive props, analyzers `membership-options.cs`, compilation tests (`host-compiler.cs`, `review-findings-tests.cs`, `generator-diagnostics-tests.cs`), and package smoke `program.cs`. The comparison reference is the symbol walk in `source/timewarp-mediator-analyzers/message-graph-builder.cs`.

## Summary

The fix pass closes all twelve round 1 findings, and each fix now matches the symbol walk in `MessageGraphBuilder`. The host identity is a `CompilationProvider.Select` into an equatable `HostIdentity`. The model step still does not take a `Compilation`, and `IncrementalGeneratorTests` still asserts `Cached` on an unrelated edit and `Modified` on a new handler. Verification on this branch:

- `./bin/dev build`: clean.
- `./bin/dev test`: 57 + 42 + 69 passed, 0 failed.
- `./bin/dev pack`: layout verified, and package smoke exited 0, which now asserts host membership and a woven host behavior.

The only new issues are two cosmetic nits.

## Prior findings re-verified

| ID | Status | Evidence |
|----|--------|----------|
| M1 | fixed | The props no longer set `TimeWarpMediatorAssemblyName`. `HostIdentity.From(compilation)` feeds `ReferencedInput.HostAssemblyName`, which is used for membership and TWM007. Package smoke passes the manifest-membership check and the `SmokeBehavior<SmokePing, string>` weave check. Test `EmptyAssemblyNameProperty_StillTreatsTheCompilationAsTheHost`. |
| M2 | fixed | `HandlerMatch.MessageScope = ResolveScope(message)`. `ScopeOf` falls back to `scopeByMessage`, which matches `ResolveScope(pair.Key)` at message-graph-builder.cs:237. Tests `NotificationScopeOnTheMessage_AssignsThePublisher` and `NotificationScopeMismatch_IsTwm003`. |
| M3 | fixed | The scratch compilation uses `WithCryptoPublicKey(identity.PublicKey)`. Test `StrongNamedHost_BindsInternalsVisibleToWithPublicKey`. |
| M4 | fixed | `type.AllInterfaces` is read for every `ITypeSymbol`, and arrays carry `ArrayRank` and the element `TypeArguments`. Test `ArrayResponse_ClosesInterfaceConstraint`. |
| M5 | fixed | The extractor dedupe was removed. `CandidateGraph` runs `BoundByBase`, then the per-fact `boundRequests` dedupe. That is the same order as `IsBoundByBaseHandler && bound.Add` at message-graph-builder.cs:364-365. Test `DerivedHandlerWithADifferentResponse_IsTwm002`. |
| M6 | fixed | The predicate matches every `assembly:`-targeted attribute, and `ExtractBehavior` filters by `AttributeClass`. Test `AliasedMediatorBehavior_IsWoven`. |
| M7 | fixed | `HasTypeParameterConstraint` makes `CanConstruct` false. That matches `ImplementsOrInherits(concrete, TParam)`, which is false (message-graph-builder.cs:699-725). Nested and array `TypeArgSlot` patterns plus `SlotMatches` reproduce `Construct` + `ImplementsPipeline` for `Wrap<T> : IPipelineBehavior<T, Result<T>>`. Tests `TypeParameterConstraint_DoesNotClose` and `NestedPipelineArgument_Closes`. |
| M8 | fixed | `HashSet<IAssemblySymbol>` skips repeated references. Test `DuplicateReference_RegistersTheBehaviorOnce`. |
| M9 | fixed | `dropped.Add` is guarded by `!match.IsNotification`, which matches message-graph-builder.cs:84 and :106-109. Test `InaccessibleNotificationHandler_DoesNotSuppressTwm001`. |
| M10 | fixed | `OrderFacts` sorts by ordinal assembly name, then by discovery index. Test `MissingRequestDiagnostics_FollowAssemblyName`. |
| M11 | fixed | TWM007 uses `inaccessible.Location`, and the test asserts `Location.None` for a metadata type. The `RequestModel` fallback now throws, and it cannot be reached because `requests[key]` is set wherever `handlersByRequest` gains a key (candidate-graph.cs:126-133). The Path/SpanStart Design comment is corrected. |
| M12 | fixed | A `DeclaredRuntime` syntax provider supplies the required names declared in the host. `FirstMissingRuntime` skips them. Test `HostDeclaredRuntimeType_IsNotAFalseTwm008`. |

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-mediator-analyzers/membership-options.cs:40-43; source/timewarp-mediator-analyzers/membership.cs:26
- Description: `MembershipOptions.HostAssemblyName` and `AssemblyNamePropertyName` are now dead in product. The generator no longer reads them, and the props no longer declare `TimeWarpMediatorAssemblyName` as a `CompilerVisibleProperty`, so in a real build the value is always empty. The XML doc says "when the project sets that compiler-visible property", which implies a consumer override that has no effect. Only the test harness still supplies the property.
- Suggestion: Remove the option and its equality term, or reword the doc to say it is ignored and kept only for tests.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-mediator-generators/candidate-graph.cs:340-351
- Description: The `RequestModel` parameter `responseByRequest` is unused now that the fallback throws.
- Suggestion: Drop the parameter.
- Status: open

## Out-of-delta observation (not a finding)

- `ExtractBehavior` (candidate-extractor.cs:95-128, unchanged in this delta) records `SpanStart` as the index into `compilation.Assembly.GetAttributes()`. That index depends on assembly attributes in other files. A syntax provider re-runs the transform only for changed trees, so in an IDE session an edit that adds an assembly attribute to file B can leave file A's behavior index stale. When two behaviors share an `Order`, the tie-break could differ from a fresh build until file A is next edited. A command-line build always runs fresh, so emitted packages are unaffected. This was not raised in round 1 and is outside the fix delta, so it is recorded here for a possible follow-up task and is not a blocking finding.
