# Docs and release notes for 15.0: one architecture page, migration guide, skill update

## Description

With one dispatcher and two packages the "two stacks" documentation set, the 13.x migration
guide and the agent instructions are wrong. Parent review task 021, finding F1 (docs rows in
section 5) and section 6 (consumer impact).

## Requirements

- One architecture page replaces `generated-vs-legacy.md` and the profile docs: contracts,
  generated dispatcher, registration, `MediatorManifest`, membership attributes, diagnostics.
- `documentation/migration.md` rewritten as a 15.0 migration page with per-consumer sections for
  TimeWarp.State, TimeWarp.Architecture and TimeWarp.Nuru taken from review section 6: handler
  signature changes (`Task` → `ValueTask`), behavior signature (`MessageHandlerDelegate`),
  package reference changes (`Contracts` + `Analyzers` → `TimeWarp.Mediator`), removed types,
  `Unit.Task` → `Unit.ValueTask`, manifest replacing DI inspection and string matching,
  `[GeneratedMediator]` replacing `build_property.TimeWarpMediatorProfile`.
- `documentation/release-notes/15.0.0.md` listing every breaking change with its finding
  number and the replacement.
- `agent.md` (CLAUDE.md) updated: packages table, dispatcher section, test tree, build
  properties, signing paragraph.
- `skills/tw-mediator/SKILL.md` updated to the 15.0 shapes.
- Each package `readme.md` updated with absolute links only (`dev pack` asserts this).
- Benchmarks page links the re-recorded baseline from 021-003.

## Checklist

- [ ] Architecture page written, old pages deleted
- [ ] 15.0 migration page with three consumer sections
- [ ] Release notes `15.0.0.md`
- [ ] `agent.md` and tw-mediator skill updated
- [ ] Package readmes updated
- [ ] `./bin/dev workflow` green (pack asserts readme links)
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-002 through 021-006 (documents the final shapes). Last child
before `15.0.0-beta.1`; the consumer dogfood tasks (State, Architecture, Nuru) live in their
own repos and gate the beta push.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
