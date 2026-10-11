# Generator and analyzer: replace generated-code compile errors with TWM diagnostics

## Description

Invalid or unsupported shapes surface as compiler errors inside `.g.cs` files instead of actionable diagnostics. Parent review task 013, findings G-9, G-11, G-13, G-14, G-17, G-18, G-21.

- G-9 (risk): internal handlers or requests in a referenced member assembly without `InternalsVisibleTo` produce CS0122. `IsEffectivelyPrivate` rejects only private/protected.
- G-21 (risk): a project referencing only the Generators package gets a wall of CS0234/CS0246 because `IMediator` and DI abstractions are emitted unconditionally.
- G-11 (debt): `IStreamRequestHandler`, pre/post processors, and exception handlers in member assemblies are ignored with no warning; stream requests compile and throw `NotSupportedException` at runtime (message embeds internal ids "004-001"/"004-002").
- G-13 (debt): `[MediatorBehavior]` entries that are not `IPipelineBehavior`, or close over zero requests, are dropped silently.
- G-14 (nit): TWM003 fires only when both sides are scoped; the descriptor says both must match or both be unscoped; notifications are not checked.
- G-17 (nit): descriptors lack `helpLinkUri` and `WellKnownDiagnosticTags.CompilationEnd` for TWM001-003 reported at compilation end.
- G-18 (debt): TWM004 misses `sender?.Send(...)`, variables typed as `IRequest<T>`, unscoped `ISender`/`IMediator` with a scoped request, and matches on method name only.

## Requirements

- Use `compilation.IsSymbolAccessibleWithin` in the builder; report a diagnostic for inaccessible members.
- Emit nothing and report one diagnostic when `TimeWarp.Mediator.IMediator` (or DI abstractions under Host) cannot be resolved.
- New diagnostics for unsupported handler kinds and invalid behavior registrations; update the deferred table in `documentation/m1-generated-mediator.md`.
- Align TWM003 description or behavior; add help links and the CompilationEnd tag.
- Harden TWM004 receiver detection.
- Reword the `NotSupportedException` messages without internal ids.
- Add new ids to `AnalyzerReleases.Unshipped.md`.

## Checklist

- [ ] G-9 accessibility diagnostic with test
- [ ] G-21 missing-reference guard with test
- [ ] G-11 unsupported-shape diagnostics with tests; doc table updated
- [ ] G-13 behavior-registration diagnostics with tests
- [ ] G-14, G-17 descriptor fixes
- [ ] G-18 TWM004 hardening with negative tests
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
