# Generator Aot profile: behaviors silently dropped while the manifest lists them

## Description

Under the Aot/Link profile the emitter skips every closed behavior, but the manifest still lists them and no diagnostic fires. Parent review task 013, findings G-7, G-8.

- G-7 (bug): `mediator-emitter.cs` skips `ClosedBehaviors` when `aot`; `manifest-emitter.cs` writes them regardless. Validation or transaction behaviors configured via `[assembly: MediatorBehavior]` vanish silently.
- G-8 (risk): Aot `ServiceGen` emits `new Handler()` for every handler. A handler with constructor parameters yields CS7036 in generated code; handlers become process-wide singletons in `static readonly` fields with no documentation.

## Requirements

- Either weave behaviors in Aot (static instances with the same parameterless rule) or emit a warning diagnostic and omit them from the manifest in that profile.
- Add a TWM diagnostic at the handler location when the Aot profile is active and the handler has no accessible parameterless constructor.
- Document singleton lifetime for Aot handlers.
- Tests: Aot profile with a behavior; Aot handler with constructor parameters.

## Checklist

- [ ] Decide weave vs diagnose (record in Notes)
- [ ] Implement with tests
- [ ] Manifest matches emitted pipeline
- [ ] Parameterless-constructor diagnostic with test
- [ ] Docs; `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
