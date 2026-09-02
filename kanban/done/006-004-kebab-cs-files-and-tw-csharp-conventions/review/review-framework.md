# Review framework — task 006-004

**Date:** 2026-09-02
**Host task:** kanban/in-progress/006-004-kebab-cs-files-and-tw-csharp-conventions/
**Diff scope:** branch `task/006-004-kebab-cs-files-and-tw-csharp-conventions` vs `origin/master` (product commit `168bc5c` refactor: kebab-case cs files and apply tw-csharp conventions; kitchen `c0b3c96` docs(kanban): record 006-004 implementer results)
**Plan / brief:** Rewrite the MediatR-fork `.cs` tree to TimeWarp C# conventions: kebab `.cs` basenames (TW0001 regex), `global-usings.cs` per project, file-scoped namespaces, explicit types / target-typed `new()`. Public **type** names stay PascalCase. Package ids stay `TimeWarp.Mediator*`. Split `Handlers.cs` (two namespaces) into `handlers.cs` + `included-handlers.cs`. Do not change mediator **behavior** except as required by file moves. Out of scope: enabling TW0001 analyzer package (006-005), deleting reflection Mediator, 2-space indent (repo `.editorconfig` is 4 spaces).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
