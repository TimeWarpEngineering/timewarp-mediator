# Round 1 — general
**Date:** 2026-10-28
**Scope reviewed:** branch vs merge-base with `master` (same as framework)

## Summary

The blocker fix is correct and small. `Unit.ValueTask` is now `=> new(Instance)`, so `Unit`
has no static field of type `ValueTask<Unit>`. `UnitValueTaskTests` guards that by
reflection and also compiles the ergonomics snippets and the Nuru `using static Unit`
handler. `./bin/dev test --quiet` passes in this worktree. The version bump, the readme
and skill install lines, and the beta.2 release notes are consistent. The goal restatement
matches the task, so there is no misread.

The risk is in `documentation/migration.md`. The new `ActionHandler<TAction>` example
(the `IActionHandler<>` replacement-signature checklist item) does not compile. Two smaller
wording problems mislead readers about the manifest.

## Issues

### Issue 1 — Severity: bug
- File: documentation/migration.md:159
- Description: The `IncrementHandler` example overrides `ValueTask Handle(...)` and returns
  `Unit.ValueTask`, which is `ValueTask<Unit>`. `ValueTask<T>` has no implicit conversion to
  the non-generic `ValueTask`, so the example fails with CS0029. A throwaway runfile
  (`static ValueTask F() => G(); static ValueTask<int> G() => new(1);`) reproduces
  `error CS0029: Cannot implicitly convert type 'System.Threading.Tasks.ValueTask<int>' to 'System.Threading.Tasks.ValueTask'`.
  The checklist item asked for the replacement signature "so a reader does not have to
  guess". A reader who copies this example gets a compile error. The task also asked that
  the migration examples compile.
- Suggestion: Return `ValueTask.CompletedTask` (or `default`) at line 162. Add a test-project
  handler shaped like the doc (an `IAction` plus an `ActionHandler<T>` subclass) so the
  example stays compiled, as `NuruStyleUnitHandler` does for the Nuru example. If the file
  has `using static TimeWarp.Mediator.Unit`, write
  `System.Threading.Tasks.ValueTask.CompletedTask`.
- Status: open

### Issue 2 — Severity: nit
- File: documentation/migration.md:268
- Description: The Nuru bullet says detection "reads the manifest JSON the app passes
  (`KindQuery`, `KindIdempotentCommand`, …)". Those are the C# constant names on the
  internal `MediatorManifest`, and the next sentence says the library cannot name that
  type. The JSON holds the values `"Query"`, `"IdempotentCommand"`, `"Command"`,
  `"Action"`, and `"Request"` (`source/timewarp-mediator-generators/manifest-emitter.cs:33-37`).
- Suggestion: List the JSON string values the library compares, not the constant names.
- Status: open

### Issue 3 — Severity: nit
- File: documentation/migration.md:193
- Description: The text says a public manifest "would give a compilation that references
  two hosts two types with the same name (CS0436)". When a library references two
  assemblies that both declare a public `MediatorManifest`, the error is CS0433 (type
  exists in both). CS0436 is the warning when a source type conflicts with an imported
  type, for example a host that references another host. The beta.2 release notes repeat
  CS0436.
- Suggestion: Name CS0433 for the library case, or cite both codes.
  Fix `documentation/release-notes/15.0.0-beta.2.md` the same way.
- Status: open
