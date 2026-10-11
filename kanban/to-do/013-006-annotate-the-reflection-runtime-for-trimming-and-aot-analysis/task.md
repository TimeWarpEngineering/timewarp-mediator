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

- [ ] Add attributes to legacy entry points
- [ ] Cache reflection in exception behaviors
- [ ] Doc update
- [ ] `./bin/dev workflow` green; AOT sample publish check; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
