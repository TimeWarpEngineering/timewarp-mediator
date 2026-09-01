# Kebab cs files and tw-csharp conventions

## Description

Parent: **006**. ~188 `.cs` files in MediatR fork shape (`Mediator.cs`, per-file usings, block namespaces). Make them TimeWarp: kebab file names, file-scoped namespaces, global usings, explicit types (no `var`), Allman already likely.

Public **type** names stay PascalCase. This is the large rewrite-of-the-tree slice. Keep tests green.

## Depends on

- 006-003

## Requirements

- TW0001 / kebab `.cs` basenames
- `global-usings.cs` per project; strip redundant per-file usings
- File-scoped `namespace TimeWarp.Mediator;`
- Explicit types; target-typed `new()`
- Analyzers/generators/tests still compile and pass
- Do not change mediator **behavior** except as required by file moves

## Out of scope

- Deleting reflection Mediator
- New features (streams, interceptors)

## Session

- Created: 162284 (2026-09-01)
