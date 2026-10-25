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

Fix pass for review round 1: the nine open findings are corrected against the code that
shipped, and the pages stay consistent with each other.

- M1: under `using static TimeWarp.Mediator.Unit`, signatures still bind to
  `System.Threading.Tasks.ValueTask`. A bare `ValueTask` in an expression is ambiguous.
  The same rule applied to the removed `Unit.Task` property. The migration page (both
  places), the 15.0.0 F8 row, and the skill gotcha say that.
- M2: `dotnet add package` lines pin `--version 15.0.0-beta.1`, matching the skill's
  `Directory.Packages.props` example, so they do not resolve stable 14.0.0.
- M3: through 14.0.0 the id `TimeWarp.Mediator` was the reflection runtime. 15.0 reuses
  that id for the contracts and the analyzer. The migration table, the package readme, and
  the skill's previous-surface list say so.
- M4: the comparison baseline header matches what `dev benchmark --record` writes. The
  021-003 provenance stays on the architecture page, the release notes, and `agent.md`.
- M5: `TimeWarpMediatorAssembly` is analyzer membership. The generator does not read it and
  always emits for a Generators reference. A library avoids emission by not referencing
  Generators. The Nuru bullet and the property table match that.
- M6: a lower `Order` is outermost, and declaration order breaks ties. A handler class is
  bound once per interface and registered once per concrete type.
- M7: the typed-`Send` row cites F2 and F3 (F7 kept). The `[GeneratedMediator]` row cites
  F10 and F11.
- M8: the root quick start puts statements first and resolves `ISender` from a scope. The
  m2 sample puts `[assembly: …]` above the type declarations.
- M9: the analyzer readme drops the impossible "does not reference TimeWarp.Mediator"
  clause, the skill description lists live diagnostic ids, and the release notes call the
  old runtime the 13.x/14.x reflection dispatcher.

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
- [x] Review round 1 findings M1–M9 corrected in the docs
- [ ] PR with merge commit

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7, M8, M9
- Fix pass: M1–M9 implemented on this id. Round 2 is the host review node.
- Round 2: Done
## Notes

Parent: 021. Depends on: 021-002 through 021-006 (documents the final shapes). Last child
before `15.0.0-beta.1`; the consumer dogfood tasks (State, Architecture, Nuru) live in their
own repos and gate the beta push.

Migration page: under `using static TimeWarp.Mediator.Unit`, signatures still bind to
`System.Threading.Tasks.ValueTask`. A bare `ValueTask` in an expression is ambiguous
(CS0229). Write `Unit.ValueTask` for the value and `System.Threading.Tasks.ValueTask` for
the type's statics. The removed `Unit.Task` property did not hide `Task`. Corrected in the
round-1 fix pass; the 021-002 review M2 note had described this as hiding.

Migration page: typed `Send` and `Dispatch_*` return the handler `ValueTask` directly. A null
argument, `NoHandlerException`, and a handler that throws before returning a `ValueTask` leave
`Send` as a thrown exception. A handler that returns a faulted or canceled `ValueTask` stays
faulted; `Send` does not throw that fault synchronously. 14.x `async` `Send` captured synchronous
throws in the returned task. Callers that project `Send` inside `Enumerable.Select` (for example
`Task.WhenAll`) must not assume a synchronous failure is captured. From 021-003 review M4.

## Results

`documentation/architecture.md` is the 15.0 contract: two packages, `ValueTask` handlers, one generated dispatcher, concrete registration, `MediatorManifest` version 2, membership attributes, and the live TWM ids. `documentation/m1-generated-mediator.md` (the profile page) is deleted. `generated-vs-legacy.md` was already gone. `documentation/migration.md` replaces the root `migration.md` and has TimeWarp.State, TimeWarp.Architecture, and TimeWarp.Nuru sections, the `Unit.ValueTask` static-using note, and the synchronous-`Send` note. `documentation/release-notes/15.0.0.md` lists each break with its parent finding and the replacement, including manifest version 2. `agent.md`, `skills/tw-mediator/SKILL.md`, the root readme, `documentation/overview.md`, `samples/readme.md`, and the three `source/*/readme.md` files match that surface. Packed readmes use absolute links. The comparison baseline header is the recorder's header. Its commit is `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`. The architecture page and the 15.0.0 notes name that re-record as task 021-003. 14.0 and 13.0 notes point at the tag blobs of the old migration guide.

`.gitignore` ignores `stacked-task-set*.resume.json`. A clean-tree `ganda repo audit` failed `kanban-porcelain-gitignore` and `routine-journals-gitignore` without that line. `ganda repo audit --fix` reported both checks fixed and wrote nothing, because its predicate still treats the older journal globs as complete. The appended block is the one that predicate's writer would have added. After the line is present those two checks pass. `kanban-landed` stays an error on a full audit: this branch's kitchen differs from origin-home, which the host land node writes, and parent task 021 has no usable worktree. Attest of this task worktree records that check as a warning and still signs.

The pull request is the host open-pr node.

Round 1 fix pass corrected M1–M9 in the docs. Install lines pin `--version 15.0.0-beta.1`. The `using static Unit` note now says signatures still bind and a bare `ValueTask` in an expression is CS0229. The package id `TimeWarp.Mediator` is described as the reflection runtime through 14.0.0 and as the contracts package in 15.0. The comparison baseline header matches `dev benchmark --record`; the 021-003 commit stays on the architecture page and in the release notes, and the Ratio-column note moved there with it. `TimeWarpMediatorAssembly` is documented as analyzer membership. The generator does not read it, so a library avoids emission by not referencing Generators. That is the shipped shape from 021-006, so this docs task did not change the generator. Behavior `Order` is the primary key, and a handler is bound per interface and registered once. The typed-`Send` row cites F2, F3, and F7. The `[GeneratedMediator]` row cites F10 and F11. Quick-start statements come first and resolve `ISender` from a scope. m2 assembly attributes precede the types. The analyzer readme, the skill's diagnostic list, and the "13.x/14.x reflection dispatcher" phrase are updated.

`ganda repo audit` on 2026-10-26 still fails only `kanban-landed`: this branch's kitchen differs from origin-home, which the host land node writes, and parent task 021 has no usable worktree. The other 32 checks passed (2 skipped).

### Review

Round 1 ran at effort 3 with a general reviewer, backed by two read-only fact-check subagents. The counts were 5 bug open, 3 suggestion open, and 1 nit open. The fix pass above addresses M1–M9. Disposition is the host's review round 2.
- M1: `using static Unit` is CS0229 in an expression. Signatures are unchanged. Confirmed with a net10.0 file-based probe (`ValueTask<int>` and bare `ValueTask` return types compile; `ValueTask` in an expression is CS0229).
- M2: install lines pin `15.0.0-beta.1`.
- M3: the id-reuse sentence is on the migration page, the contracts readme, and the skill.
- M4: the benchmark header matches the recorder.
- M5: docs match the generator. Emission is not gated on `TimeWarpMediatorAssembly`.
- M6: lower `Order` is outermost. Registration is once per concrete type.
- M7: finding citations updated.
- M8: quick start and m2 snippets put statements and assembly attributes where the compiler accepts them. A file-based probe of that shape compiled.
- M9: stale phrases removed.

Round 2 ran at effort 2 with a general reviewer over the fix delta `835b203..7b8074f`. It re-checked M1–M9 against the delta and the source (`CompareOrdered`, `membership.cs`, and the TWM008 emission gate in `candidate-graph.cs`). All nine are fixed, and there are no new findings.

Final counts: bug 0 open / 5 fixed, suggestion 0 open / 3 fixed, nit 0 open / 1 fixed, wontfix 0.

**Review disposition: clean** (2 rounds, final open 0). Paths:

- `review/review-framework.md`
- `review/round-2/merged.md`
- `review/disposition.md`

### How to validate

Smoke: `./bin/dev workflow`

Expect: the log ends with `Pipeline SUCCEEDED`. Build 0 warnings. Tests passed: generators 57, analyzers 42, compilation 69, failed 0. Both samples verify (the AOT verifier expects `aot-pong`). Pack reports layout verified for `TimeWarp.Mediator.14.0.0.nupkg` and `TimeWarp.Mediator.Generators.14.0.0.nupkg` only (readme headings and absolute links included). Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.

Ran 2026-10-26 after the round-1 fix pass: `./bin/dev workflow` (mode Pr) exited 0. Build 0 warnings. Tests passed: generators 57, analyzers 42, compilation 69, failed 0. Samples verified. Pack layout verified for both 14.0.0 packages. Package smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`. The workflow transcript did not echo the `aot-pong` token; the sample step succeeded, and that step fails unless the AOT sample prints it.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: 2026-10-26 (implementer-grok). Wrote the 15.0 architecture page, migration guide, and release notes, and pointed the agent instructions and package readmes at that surface.
- Audit: 2026-10-26. Appended `stacked-task-set*.resume.json` so the porcelain and routine-journal gitignore checks pass. `audit --fix` did not write the line.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-25T19:36:32Z
- Review oracle round 1: 2026-10-26, Claude Opus 5.5 (headless print mode, session id not exposed). Effort 3, roster general plus two read-only fact-check subagents. Nine open findings (M1–M9). A fix pass is requested on this id.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-25T19:45:58Z
- Fix pass: 2026-10-26 (implementer-grok). Corrected review round 1 findings M1–M9 in the docs. `./bin/dev workflow` exited 0. `ganda repo audit` still fails only `kanban-landed`, which the host land node clears.
- Review oracle round 2: 2026-10-26, Claude Opus 5.5 (headless print mode, session id not exposed). Effort 2, roster general, scope `835b203..7b8074f`. M1–M9 are fixed, with 0 open and no new findings. Disposition is `clean`.
