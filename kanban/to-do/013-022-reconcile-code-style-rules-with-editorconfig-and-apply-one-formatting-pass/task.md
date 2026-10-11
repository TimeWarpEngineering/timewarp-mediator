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

- [ ] Decision recorded; agent.md and .editorconfig updated
- [ ] Formatting pass
- [ ] Field renames and header cleanup
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Sequence this last among the runtime tasks.

## Session

- Created: 686431 (2026-10-11)
