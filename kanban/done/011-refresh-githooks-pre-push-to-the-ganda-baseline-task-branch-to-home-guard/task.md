# Refresh githooks pre-push to the ganda baseline (task branch to home guard)

## Description

Refresh this repo's `.githooks/pre-push.cs` (and the `.githooks/pre-push` shim if the baseline
changed it) to the current ganda repo baseline.

Ganda task 323 (timewarp-ganda PR #197, merged 2026-10-01) added a guard to the baseline
pre-push hook. It refuses a push whose local ref is `refs/heads/task/*` and whose destination
is the home branch (`master` / `main`), and the message names the task branch. Raw-sha pushes,
such as the `kanban publish` merge commit, stay allowed. Until each repo picks up the new hook,
`ganda repo audit` warns `memsearch-scaffold: .githooks/pre-push.cs (outdated)`, and the repo
lacks the guard.

## Requirements

1. Apply the baseline with ganda itself, not by hand:
   `ganda repo audit --fix --checks memsearch-scaffold`. Confirm that `.githooks/pre-push.cs`
   now matches the baseline and the warning is gone.
2. Keep any repo-specific hook content the baseline intends to preserve. If `--fix` would drop a
   local customization, stop and record it in Notes rather than overwrite it.
3. Re-run `ganda repo audit` and fix anything else it reports (boyscout welcome), then commit.
4. Smoke-test the hook without touching home:
   - Pipe a fake pre-push line into the hook,
     `refs/heads/task/x <sha> refs/heads/master <sha>`, and confirm it is refused.
   - Pipe `<sha> <sha> refs/heads/master <sha>` and confirm it is allowed.

   Do not push to master to test.

## Checklist

- [x] `.githooks/pre-push.cs` refreshed via `ganda repo audit --fix --checks memsearch-scaffold`
- [x] Audit clean (no `memsearch-scaffold` warning)
- [x] Hook smoke test: task→home refused, raw sha→home allowed (stdin simulation only)
- [x] Gates per this repo's `tw-pr` (a hook-only change needs no full build unless the skill's
      scope table says otherwise)
- [x] Implementation review (clean); host `open-pr`

## Notes

- One of a set of identical tasks filed in each repo that carries `.githooks/pre-push.cs`:
  amuru, architecture, bayline, ganda, kiini, mediator, nuru, state, taratibu.
- Do not start any app host. Run builds serially and call `dotnet build-server shutdown` before
  finishing.

## Results

- `ganda repo audit --fix --checks memsearch-scaffold` refreshed `.githooks/pre-push.cs` to the
  baseline. The diff is additive only: the task→home guard and its comment were added, and no
  local customization was dropped. `.githooks/pre-push` is a symlink to `pre-push.cs`, so it
  needs no separate change.
- Boyscout: the audit also failed `required-gitignore-entries`, because `.local/` was missing.
  `--fix` added it to the root `.gitignore`. The audit also failed `bin-dev` and
  `dev-cli-capabilities`, because `bin/dev` was not installed in this fresh worktree. `bin/dev`
  is gitignored and installed per clone, and `--fix` installed it, so there is no tracked change.
- `ganda repo audit` now reports "Repository passes all audit checks." with no
  `memsearch-scaffold` warning.
- This is a hook-only and `.gitignore`-only change, so no product build or test is needed.

Smoke test (stdin simulation, run from the worktree, with `S=$(git rev-parse HEAD)`):

```
$ echo "refs/heads/task/x $S refs/heads/master $S" | dotnet .githooks/pre-push.cs; echo exit=$?
Refusing push of task branch to home: task/x -> master.
Task branches publish to origin/<task branch> and land on home via PR.
Fix tracking: ganda repo audit --fix --checks task-branch-upstream, then git push.
Escape hatch (intentional only): git push --no-verify
exit=1
$ echo "$S $S refs/heads/master $S" | dotnet .githooks/pre-push.cs; echo exit=$?
exit=0
```

### Review

- Implementation review: 1 round, effort 1, roster `general`.
- Final counts: 0 bug, 0 suggestion, 0 nit (0 open, 0 fixed, 0 wontfix).
- Disposition: **clean**.
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

**Smoke:**

```bash
ganda repo audit
S=$(git rev-parse HEAD)
echo "refs/heads/task/x $S refs/heads/master $S" | dotnet .githooks/pre-push.cs; echo exit=$?
echo "$S $S refs/heads/master $S" | dotnet .githooks/pre-push.cs; echo exit=$?
```

**Expect:** The audit prints "Repository passes all audit checks." with no `memsearch-scaffold`
warning. The first hook run prints `Refusing push of task branch to home: task/x -> master.`
and exits 1. The second hook run prints nothing and exits 0. Do not push to master to test.

## Session

- Created: 2026-10-01
- 2026-10-01: implement oracle refreshed the hook via audit --fix, added .local/ to .gitignore, and ran the stdin smoke test (pass).
- 2026-10-01: review oracle (claude, effort 1, general) — round 1 found no issues; disposition clean.
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 80 — 2026-10-01T07:42:06Z
