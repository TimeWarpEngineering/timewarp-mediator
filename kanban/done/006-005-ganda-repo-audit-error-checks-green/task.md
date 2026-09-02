# ganda repo audit error checks green

## Description

Parent: **006**. Gate: `ganda repo audit` exits **0** for error-severity checks. This is what unblocks **005-003** (NuGet 14.0.0-beta.1).

## Depends on

- 006-002
- 006-004

## Requirements

- `ganda repo audit` — no **Error** FAILs
- Warning-only (memsearch, vscode-window-icon) may remain unless cheap `--fix`
- `dev test` (or equivalent) green
- Record remaining warning exceptions in `.editorconfig` `[ganda.audit]` with a reason if we keep them

## Checklist

- [x] Audit table: Error FAILs = 0
- [x] Tests green
- [x] Note on **005-003**: unblocked
- [x] Implementation review (effort 1, general) — disposition clean

## Out of scope

- Publishing NuGet (005-003)
- State/Nuru consume

## Session

- Created: 162284 (2026-09-01)
- Implementer: grok (2026-09-02)
- Review: grok, effort 1, roster general (2026-09-02)

## Results

`ganda repo audit` exits **0** with **Error FAILs = 0**. That is the 006 audit gate that was blocking **005-003**.

Starting state on this clone: Passed 22 / Failed 3 / Skipped 1. The two **Error** FAILs were `bin-dev` and `dev-cli-capabilities` (`bin/dev` missing; gitignored `[Bb]in/`). The one **Warning** FAIL was `vscode-window-icon` (avatar + `.vscode` task/settings). memsearch already passed from 006-002.

Applied `ganda repo audit --fix --checks bin-dev,vscode-window-icon`:

- `bin-dev` — local self-install to `./bin/dev` (not committed)
- `vscode-window-icon` — cheap `--fix` (committed): `assets/timewarp-mediator-avatar.svg`, `.vscode/tasks.json` (`ganda repo avatar window` on folderOpen), `.vscode/settings.json` (`window.title` + `timewarp.blurImagePath`), `.timewarp/ganda.jsonc` (avatar seed)

Re-audit: **Passed 25 / Failed 0 / Skipped 1**. Skip is `runfile-project-directives` (no `#:project` directives). No remaining warning exceptions, so no `.editorconfig` `[ganda.audit]` severity overrides.

**005-003:** unblocked on the **006 audit gate**. Publishing is still out of scope here; 005-003 still depends on **005-001** (version pack 14.0.0-beta.1) and master CI.

**Tests** (`DOTNET_ROLL_FORWARD=LatestMajor ./bin/dev test`): Mediator.Tests **163 passed / 2 skipped**; Analyzers **6 passed**; Generators **19 passed**. Exit 0.

### Files changed

- Added: `assets/timewarp-mediator-avatar.svg`, `.vscode/settings.json`, `.vscode/tasks.json`, `.timewarp/ganda.jsonc`
- Local only (gitignored): `bin/dev` via self-install
- Unchanged: `.editorconfig` (no `[ganda.audit]` exceptions)

### Key decisions

- Cheap `--fix` for vscode-window-icon instead of keeping a warning exception.
- Did not enable TimeWarp.SourceGenerators TW0001 (006-004 leftover; not an audit check).
- `bin/dev` stays gitignored; fresh clones bootstrap with `dotnet run --file tools/dev-cli/dev.cs -- self-install` or `ganda repo audit --fix --checks bin-dev`.

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-006-005-ganda-repo-audit-error-checks-green
# Fresh clone without bin/dev:
test -x ./bin/dev || dotnet run --file tools/dev-cli/dev.cs -- self-install
ganda repo audit; echo "exit=$?"
test -f assets/timewarp-mediator-avatar.svg
rg -n 'ganda repo avatar window' .vscode/tasks.json
rg -n 'window.title|timewarp.blurImagePath' .vscode/settings.json
```

**Expect**

- `ganda repo audit` exit **0**
- Table: Passed **25**, Failed **0**, Skipped **1** (`runfile-project-directives`)
- No **Error** FAIL rows; `bin-dev`, `dev-cli-capabilities`, `vscode-window-icon` are PASS
- Banner: `Repository passes all audit checks.`
- Avatar + `.vscode` files exist as above
- No `[ganda.audit]` section in `.editorconfig` (no kept warning exceptions)

**Automated gate**

```bash
export DOTNET_ROLL_FORWARD=LatestMajor
./bin/dev test
# expect: exit 0
# expect: timewarp-mediator-tests 163 passed / 2 skipped
# expect: timewarp-mediator-analyzers-tests 6 passed
# expect: timewarp-mediator-generators-tests 19 passed
```

**Depends on:** `DOTNET_ROLL_FORWARD=LatestMajor` on machines without a net8.0 testhost (this image has net10/net11; CI `setup-dotnet` installs `8.0.x`). Fresh clones need `bin/dev` via self-install before `dev-cli-capabilities` can run.

**Not in scope:** publishing 14.0.0-beta.1 (005-003); version bump (005-001); State/Nuru consume; enabling TW0001 analyzer package.

### Review disposition

- **Outcome:** clean
- **Rounds:** 1
- **Effort / roster:** 1, general
- **Counts (final, round 1):** bug 0; suggestion 0; nit 0 — final open count 0
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
