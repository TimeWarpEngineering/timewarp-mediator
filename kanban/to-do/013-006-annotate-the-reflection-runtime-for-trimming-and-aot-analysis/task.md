# Annotate the reflection runtime for trimming and AOT analysis

## Description

The product pitches a source-generated AOT path, but the shipped TimeWarp.Mediator runtime gives trimming and AOT analyzers no signal. Parent review task 013, finding C-14.

- `request-exception-processor-behavior.cs` and `request-exception-action-processor-behavior.cs` use `MakeGenericType`, `GetMethod`, and `MethodInfo.Invoke` per exception.
- `mediator.cs` uses `MakeGenericType` plus `Activator.CreateInstance`.
- No `RequiresUnreferencedCode`, `RequiresDynamicCode`, `IsAotCompatible`, or `DynamicallyAccessedMembers` anywhere in source/.

## Requirements

- Annotate `AddMediator` and the legacy entry points with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` (guarded for netstandard2.0 with polyfill attributes or `#if`).
- Cache `MethodInfo` per (request, response, exception) tuple in the exception behaviors.
- Document in `documentation/generated-vs-legacy.md` that the reflection runtime is not trim or AOT safe and the generated path is.
- Confirm the AOT sample still publishes with no new warnings.

## Checklist

- [x] Add attributes to legacy entry points
- [x] Cache reflection in exception behaviors
- [x] Doc update
- [x] `./bin/dev workflow` green; AOT sample publish check
- [ ] PR with merge commit (host open-pr node)

## Notes

Parent: 013.

Review: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`. Effort 2, roster general. Outcome accepted-exceptions (M1 wontfix, M2 fixed).

## Results

- `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]` on both `AddMediator` overloads, both `Mediator`
  constructors, and the `RequestExceptionProcessorBehavior<,>` / `RequestExceptionActionProcessorBehavior<,>`
  constructors. Messages shared in `source/timewarp-mediator/internal/trimming-messages.cs` and point at
  `AddGeneratedMediator()`.
- Polyfills in `source/timewarp-mediator/internal/trimming-attributes.cs`: internal `RequiresUnreferencedCodeAttribute`
  (`#if !NET5_0_OR_GREATER`, i.e. netstandard2.0) and `RequiresDynamicCodeAttribute` (`#if !NET7_0_OR_GREATER`,
  netstandard2.0 and net6.0). ILLink/ILC match by full name.
- Exception behaviors: static `ConcurrentDictionary<Type, (enumerable service type, MethodInfo)>` on the closed
  generic, so `MakeGenericType`/`GetMethod` run once per (request, response, exception) tuple instead of per exception.
- `documentation/generated-vs-legacy.md`: new "Trimming and NativeAOT" section (reflection runtime not trim/AOT safe;
  generated path is).
- New test `tests/timewarp-mediator-tests/trimming-annotation-tests.cs` (6 cases) asserts both attributes on each entry point.
- Not done (out of scope): `IsAotCompatible` / trim analyzer on the `TimeWarp.Mediator` project itself. Its TFMs are
  netstandard2.0/net6.0, so the AOT analyzer cannot run, and the trim analyzer would flag the whole reflection core.
- `./bin/dev workflow` green (248 passed, 2 pre-existing skips). AOT sample publishes with zero warnings and prints `aot-pong`.
  The AOT sample does not reference `TimeWarp.Mediator`, so the annotations cannot add warnings there.
  Host note: native link needs `libz.so` (zlib1g-dev). Without it, set `LIBRARY_PATH` to a dir holding a `libz.so -> libz.so.1` symlink.
- Review fix: `TrimmingMessages` no longer says `AddGeneratedMediator()` ships from the Generators assembly. It tells the caller to reference `TimeWarp.Mediator.Contracts` and the `TimeWarp.Mediator.Generators` source generator, which emits `AddGeneratedMediator()` into the host.

### Review

- Rounds: 2. Roster: general. Effort: 2 (220-line diff).
- Round 1 counts: bug 0, suggestion 1 (M1 wontfix), nit 1 (M2 fixed). Round 2: no new issues. Final open count: 0.
- Disposition: `accepted-exceptions`. M1 stays wontfix. Annotating `Send`, `Publish`, `CreateStream`, or behavior `Handle` without the same attributes on `ISender`, `IPublisher`, and `IPipelineBehavior` is IL2046. Annotating those contracts would mark the generated mediator unsafe. `AddMediator` and the `Mediator` constructors remain the call-site signal.
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

**Smoke**

```bash
./bin/dev workflow
dotnet test tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj -c Release --no-build --filter "FullyQualifiedName~TrimmingAnnotationTests"
dotnet publish samples/timewarp-mediator-examples-aot/timewarp-mediator-examples-aot.csproj -c Release -o /tmp/tw-aot && /tmp/tw-aot/timewarp-mediator-examples-aot
```

**Expect**

- Workflow exits 0 (build, all tests, pack).
- `TrimmingAnnotationTests`: 6 passed.
- AOT publish: no IL2026/IL3050 (or any IL2xxx/IL3xxx) warnings; binary prints `aot-pong`.
- In a net8+ app with `<PublishAot>true</PublishAot>` that calls `services.AddMediator(...)`, the build reports IL2026 and IL3050 at that call.

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12 implement oracle (Claude Opus 5.5): annotations, reflection cache, doc, test; workflow + AOT publish green.
- Review: Grok session 01a12ef0-c0e7-7dc0-aedc-0f1c19826ae7 (2026-10-12)
