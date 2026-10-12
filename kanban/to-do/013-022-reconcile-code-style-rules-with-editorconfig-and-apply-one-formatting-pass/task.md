# Reconcile code style rules with .editorconfig and apply one formatting pass

## Description

agent.md and the tw-csharp conventions call for 2-space indentation and no underscore prefixes; every `.cs` file in source/ and tests/ uses 4 spaces, `.editorconfig` pins `indent_size=4` for `*.cs`, and underscore-prefixed fields remain. Parent review task 013, findings C-25 and the style part of D-21.

- Underscore fields in `mediator.cs`, the four processor behaviors, and `unit.cs`.
- About 40 files carry a `// Modified by Steven T. Cramer` header.
- Trailing whitespace and doubled blank lines in `mediator.cs`, `service-registrar.cs`, `mediator-service-configuration.cs`.
- `tools/dev-cli` already uses 2 spaces, so the repo is internally inconsistent.

## Requirements

- Decide which rule wins and make agent.md and `.editorconfig` agree.
- Apply one mechanical formatting pass (`dotnet format` with the agreed editorconfig) in a dedicated PR with no behavior changes.
- Rename underscore fields per convention; drop or standardize the "Modified by" headers.
- Land after the open behavior-fix tasks merge, or rebase them, to avoid conflicts.

## Checklist

- [x] Decision recorded; agent.md and .editorconfig updated
- [x] Formatting pass
- [x] Field renames and header cleanup
- [ ] `./bin/dev workflow` green; PR with merge commit (workflow green; PR opened by host)

## Notes

Parent: 013. Sequence this last among the runtime tasks.

## Results

### Decision

The TimeWarp convention wins: 2-space C# indentation, LF, no trailing whitespace, PascalCase
fields with no underscore prefix. `.editorconfig` (`indent_size=2`, `end_of_line=lf`,
`trim_trailing_whitespace=true`, `dotnet_naming_rule` entries for PascalCase fields and camelCase
locals/parameters) and a new `## Code Style` section in `agent.md` now agree. `tools/dev-cli`
already used 2 spaces, so the repo is consistent.

"Modified by" headers are **standardized, not dropped**: they are the Apache-2.0 §4(b) notice on
MediatR-derived files (repo policy keeps Apache attribution). They already used one form and sat on
line 1. `agent.md` records the rule: keep on MediatR-derived files, never add to new files.

### Changes

- `dotnet format whitespace --folder --include source/ tests/ samples/`: 215 `.cs` files re-indented
  from 4 to 2 spaces with trailing whitespace removed. Doubled blank lines collapsed by hand in the
  11 files that still had them.
- Underscore fields renamed using the type-stem names (`_serviceProvider` -> `ServiceProvider`,
  `_publisher` -> `NotificationPublisher`, `_preProcessors` -> `PreProcessors`, `_requestHandlers`
  -> `RequestHandlers`, etc.) in `mediator.cs`, the four processor behaviors, `unit.cs`
  (`_value` -> `Instance`, because a field cannot share its enclosing type's name), and every test
  and sample. Names that collided with existing members were chosen by hand:
  `HandlerGate.RecordedEvents`/`StartedCount`, `ConstructorCallCounter.CallCount`,
  `SendTests.SharedDependency`, and `CustomMediator.PublishStrategy`.
- The changes are formatting and renames only. Golden files, the generator tests and the
  package smoke all pass unchanged.

### How to validate

Smoke:

```bash
dotnet build-server shutdown
./bin/dev workflow --mode pr
git grep -nE '^\s*(private|protected|internal|readonly|static)[^=(;]*\s_[a-z]\w*\s*[;=]' -- source tests samples
ganda repo audit
```

Expect: `Pipeline SUCCEEDED` (build, test, verify-samples, pack + package smoke); the `git grep`
prints nothing (no underscore-prefixed fields remain); audit reports "Repository passes all audit
checks." Note: if the build reports CS8784 `TypeLoadException ... MembershipOptions`, a stale
compiler server is serving an analyzer DLL from another worktree. Run `dotnet build-server shutdown`
and rebuild.

Whitespace and the new naming rules, after the review fix:

```bash
dotnet format whitespace --folder --include source/ tests/ samples/ --verify-no-changes
dotnet format style timewarp-mediator.slnx --diagnostics IDE1006 --verify-no-changes
```

Expect: both commands exit 0.

### Review

- Rounds: 2. Roster: general. Effort: 3 (by-diff; 26726 lines).
- Round 1 counts: bug 2 open. Round 2 counts: bug 2 fixed. Final open count: 0.
- Disposition: **clean**. No wontfix, no escalation.
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

Round 1 found the whitespace pass still dirty on 13 files, and IDE1006 violations against the new
naming rules (six `logger` fields, one PascalCase deconstruction in `service-registrar.cs`, and
method-local consts). Those are fixed on this task. `dotnet format whitespace --verify-no-changes`
and `dotnet format style --diagnostics IDE1006 --verify-no-changes` both exit 0. Release tests:
analyzers 43, generators 40 (after `dotnet build-server shutdown`), compilation 85, Roslyn-floor 86,
mediator 211. All passed.

## Session

- Created: 686431 (2026-10-11)
- Implemented: 2026-10-12 (format pass, field renames, decision recorded; workflow + audit green)
- Review: Grok `01a13066-9a08-75d3-b180-e15c0ff7481b` (2026-10-12)
