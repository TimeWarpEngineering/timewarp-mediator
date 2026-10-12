# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit d86e920 vs parent (013-009 Aot TWM005/TWM006)

## Summary

Aot and Link still do not weave `ClosedBehaviors`. `Dispatch_*` and `Publish_*` call `ServiceGen` handler fields only, and `ManifestEmitter` writes `"behaviors":[]` for every request under `MessageGraph.IsAotProfile`, so the manifest matches the pipeline. TWM005 is one warning per closed behavior's original definition, on `[assembly: MediatorBehavior]` when that attribute is in this compilation. TWM006 is an error for a request or notification handler with no constructor the host assembly can call with zero arguments; optional and `params` parameters count, and those handlers are emitted as `default!` instead of `new Handler()`. Host is unchanged, and the singleton lifetime is documented.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-mediator-generators/aot-profile.cs:109
- Description: `HasAccessibleParameterlessConstructor` treats any non-abstract reference type with an accessible all-optional constructor as `new Handler()` (`mediator-emitter.cs:228`). A `required` field or property still makes that call CS9035 in the generated file, and TWM006 is not reported, so the error lands in `Mediator.g.cs` instead of on the handler. The implicit public constructor is not marked `[SetsRequiredMembers]`.
- Suggestion: If any instance field or property is `IsRequired`, treat the handler as unconstructible unless an accessible zero-argument constructor has `SetsRequiredMembersAttribute`. Cover it with a compilation test that expects TWM006 and no CS9035.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-mediator-generators-compilation-tests/aot-profile-tests.cs:56
- Description: `Behavior_DoesNotRunUnderAot` only asserts that `Send` returns `"pong"`. `LoggingBehavior.Handle` (`aot-profile-tests.cs:134`) forwards `next()` unchanged, so the assertion passes whether or not the behavior runs. TWM005 placement and `"behaviors":[]` are tested separately; this runtime test does not lock the weave.
- Suggestion: Record a side effect in the behavior (static counter, mutated response, or throw) and assert that it did not happen.
- Status: open
