# Round 1 — general
**Date:** 2026-09-25
**Scope reviewed:** same as framework (branch vs `675b58c`), plus call sites in `mediator-emitter.cs`,
`manifest-emitter.cs`, and `request-handler-analyzer.cs`

## Summary

The change fixes both #68 bugs at their source. `TryGetRequestHandler` now reports `isVoid` only when
`IRequestHandler<T>` matched. Bindings read `IsUnitResponse` from a `voidRequests` set, so an explicit
`IRequestHandler<T, Unit>` is registered as the two-arity interface and returns the handler's `Unit`. Response
types are `ITypeSymbol` from discovery through `CloseBehaviors`, `TryCloseBehavior`, `ImplementsPipeline`,
`RequestBinding`, and `Fq`, so arrays bind like named types. Risk is low.

Every emitter use of `IsUnitResponse` (dispatch body, innermost next delegate, handler registration) now
agrees with the matched interface. The emitter never writes `typeof(<response>)`, so widening the type adds
no new emission hazard. `CanConstruct` handles arrays correctly: `IsReferenceType` is true for `T[]`.
`request-handler-analyzer.cs` only inspects `TypeArguments[0]`, so neither bug applies there. The tests
cover every requirement: explicit Unit through the monomorphic, generic, and object `Send`; void staying void;
array and `List<T>` discovery and dispatch; a closed `IPipelineBehavior<SearchWidgets, string[]>` running in
order; `UnitOnlyBehavior<>` still closing over the explicit-Unit command; and manifest response shapes.

Verified: `./bin/dev workflow` shows `Pipeline SUCCEEDED` (generators 34/34, analyzers 6/6, runtime 165
passed with 2 pre-existing skips), and the 14.0.0-beta.3 packages are packed with the layout verified.
`ganda repo audit` passes all checks. Version is 14.0.0-beta.3 in both `Directory.Build.props` files. The
changelog references #68, and the commit body carries `Fixes #68`.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:269
- Description: `iface.TypeArguments[1] is ITypeSymbol response2` is always true because `TypeArguments` is
  `ImmutableArray<ITypeSymbol>` with non-null elements. The pattern now only binds a variable.
- Suggestion: Assign `responseType = iface.TypeArguments[1]` directly, or keep the pattern so it mirrors the
  `request2` binding.
- Status: open
