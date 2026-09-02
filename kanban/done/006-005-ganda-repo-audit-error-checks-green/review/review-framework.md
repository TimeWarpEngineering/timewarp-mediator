# Review framework — task 006-005

**Date:** 2026-09-02
**Host task:** kanban/in-progress/006-005-ganda-repo-audit-error-checks-green/
**Diff scope:** branch `task/006-005-ganda-repo-audit-error-checks-green` vs `origin/master` (product commit `048a23d` chore: scaffold vscode window-icon so ganda repo audit is green; kitchen `3409cff` docs(kanban): record 006-005 implementer results)
**Plan / brief:** Gate: `ganda repo audit` exits 0 for **error** severity (unblocks 005-003). Starting clone: Passed 22 / Failed 3 / Skipped 1. Error FAILs were `bin-dev` and `dev-cli-capabilities` (`bin/dev` missing; gitignored `[Bb]in/`). Warning FAIL was `vscode-window-icon`. Applied `ganda repo audit --fix --checks bin-dev,vscode-window-icon`: local self-install of `./bin/dev` (not committed); cheap `--fix` committed avatar + `.vscode` folderOpen task + window.title/blur path + `.timewarp/ganda.jsonc`. Claimed re-audit: Passed 25 / Failed 0 / Skipped 1 (`runfile-project-directives`). No `[ganda.audit]` exceptions. Tests claimed green (`./bin/dev test`). Out of scope: publishing NuGet (005-003), enabling TW0001 analyzer package, State/Nuru consume.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
