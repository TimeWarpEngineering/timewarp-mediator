# Docs and release notes for 15.0: one architecture page, migration guide, skill update

## Description

With one dispatcher and two packages the "two stacks" documentation set, the 13.x migration
guide and the agent instructions are wrong. Parent review task 021, finding F1 (docs rows in
section 5) and section 6 (consumer impact).

## Goal (implementer restatement)

15.0 ships one generated dispatcher and two packages (`TimeWarp.Mediator` and
`TimeWarp.Mediator.Generators`). The pages a consumer or an agent would follow still describe
Host/Aot/Link profiles, a separate analyzers package, `Task` handlers, and `TimeWarpMediatorProfile`.
This task rewrites that set so it matches the shapes 021-002 through 021-006 already shipped.

Done when:

- `documentation/architecture.md` is the single architecture page (contracts, dispatcher,
  registration, `MediatorManifest`, membership, diagnostics) and the profile page
  `documentation/m1-generated-mediator.md` is gone. `generated-vs-legacy.md` is already gone.
- `documentation/migration.md` is the 15.0 migration page, with sections for TimeWarp.State,
  TimeWarp.Architecture, and TimeWarp.Nuru, plus the `Unit.ValueTask` static-using note and the
  synchronous-`Send` note from the task.
- `documentation/release-notes/15.0.0.md` lists each breaking change with its parent finding
  number and the replacement, including manifest version 2.
- `agent.md` and `skills/tw-mediator/SKILL.md` describe the two packages, the one dispatcher,
  the test tree, the build properties, and unsigned assemblies.
- Each package `readme.md` matches those shapes and uses absolute links only.
- The benchmarks page points at the 021-003 re-record.
- `./bin/dev workflow` exits 0 (pack checks the readme links).
- The changes are committed and pushed. Opening the pull request stays on the host open-pr node.

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

- [x] Architecture page written, old pages deleted
- [x] 15.0 migration page with three consumer sections
- [x] Release notes `15.0.0.md`
- [x] `agent.md` and tw-mediator skill updated
- [x] Package readmes updated
- [x] `./bin/dev workflow` green (pack asserts readme links)
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-002 through 021-006 (documents the final shapes). Last child
before `15.0.0-beta.1`; the consumer dogfood tasks (State, Architecture, Nuru) live in their
own repos and gate the beta push.

Migration page: under `using static TimeWarp.Mediator.Unit`, `Unit.ValueTask` does not hide
`System.Threading.Tasks.Task` (the removed `Unit.Task` property did). It does shadow
`System.Threading.Tasks.ValueTask`. Tell callers to use the qualified names `Unit.Value` and
`Unit.ValueTask`. From 021-002 review M2.

Migration page: typed `Send` and `Dispatch_*` return the handler `ValueTask` directly. A null
argument, `NoHandlerException`, and a handler that throws before returning a `ValueTask` leave
`Send` as a thrown exception. A handler that returns a faulted or canceled `ValueTask` stays
faulted; `Send` does not throw that fault synchronously. 14.x `async` `Send` captured synchronous
throws in the returned task. Callers that project `Send` inside `Enumerable.Select` (for example
`Task.WhenAll`) must not assume a synchronous failure is captured. From 021-003 review M4.

## Results

`documentation/architecture.md` is the 15.0 contract: two packages, `ValueTask` handlers, one generated dispatcher, concrete registration, `MediatorManifest` version 2, membership attributes, and the live TWM ids. `documentation/m1-generated-mediator.md` (the profile page) is deleted. `generated-vs-legacy.md` was already gone. `documentation/migration.md` replaces the root `migration.md` and has TimeWarp.State, TimeWarp.Architecture, and TimeWarp.Nuru sections, the `Unit.ValueTask` static-using note, and the synchronous-`Send` note. `documentation/release-notes/15.0.0.md` lists each break with its parent finding and the replacement, including manifest version 2. `agent.md`, `skills/tw-mediator/SKILL.md`, the root readme, `documentation/overview.md`, `samples/readme.md`, and the three `source/*/readme.md` files match that surface. Packed readmes use absolute links. The comparison baseline page names the 021-003 re-record at `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`. 14.0 and 13.0 notes point at the tag blobs of the old migration guide.

`.gitignore` ignores `stacked-task-set*.resume.json`. A clean-tree `ganda repo audit` failed `kanban-porcelain-gitignore` and `routine-journals-gitignore` without that line. `ganda repo audit --fix` reported both checks fixed and wrote nothing, because its predicate still treats the older journal globs as complete. The appended block is the one that predicate's writer would have added. After the line is present those two checks pass. `kanban-landed` stays an error on a full audit: this branch's kitchen differs from origin-home, which the host land node writes, and parent task 021 has no usable worktree. Attest of this task worktree records that check as a warning and still signs.

The pull request is the host open-pr node.

### How to validate

Smoke: `./bin/dev workflow`

Expect: the log ends with `Pipeline SUCCEEDED`. Build 0 warnings. Tests passed: generators 57, analyzers 42, compilation 69, failed 0. Both samples verify, and the AOT sample prints `aot-pong`. Pack reports layout verified for `TimeWarp.Mediator.14.0.0.nupkg` and `TimeWarp.Mediator.Generators.14.0.0.nupkg` only (readme headings and absolute links included). Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.

Ran 2026-10-26: `./bin/dev workflow` (mode Pr) exited 0 with that output.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: 2026-10-26 (implementer-grok). Wrote the 15.0 architecture page, migration guide, and release notes, and pointed the agent instructions and package readmes at that surface.
- Audit: 2026-10-26. Appended `stacked-task-set*.resume.json` so the porcelain and routine-journal gitignore checks pass. `audit --fix` did not write the line.
