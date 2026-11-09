# Ignore .timewarp/signals/ walk logs in .gitignore

## Goal (implementer restatement)

Keep `ganda task work` signal logs from dirtying the origin-home checkout. Those logs land under `.timewarp/signals/`. When that directory is not ignored, the logs show up as untracked files, `ganda kanban` and `dev release` refuse to run, and the post-merge attest will not sign master. The audit check `timewarp-signals-gitignore` already fails on master `69419b4` for this reason.

Acceptance:

- `ganda repo audit --fix` has been run, and whatever it changes is committed. That run adds a `.timewarp/signals/` ignore entry to `.gitignore`. If it does not, the entry is added by hand.
- Git tracks nothing under `.timewarp/signals/`. A tracked path there is removed from the index with `git rm --cached` and left on disk.
- `ganda repo audit` is clean and `timewarp-signals-gitignore` passes.
- Product code and `<Version>` stay as they are. This walk does not cut a release.
- Opening the pull request is a later host node.

## Description

`ganda repo audit` fails `timewarp-signals-gitignore` on master: `.timewarp/signals/ is not ignored; walk signal logs dirty origin-home`. `ganda task work` writes signal logs under `.timewarp/signals/`. When that path is not ignored, the logs dirty the origin-home checkout, which blocks `ganda kanban` commands and `dev release`, and the post-merge attest refuses to sign master. Seen 2026-11-09 after merging PR #126.

## Requirements

- Run `ganda repo audit --fix` and commit what it changes. That should add the `.timewarp/signals/` ignore entry to `.gitignore`.
- If `--fix` does not add the entry, add `.timewarp/signals/` to `.gitignore` by hand.
- No file under `.timewarp/signals/` is tracked. If one is, remove it with `git rm --cached`.
- `ganda repo audit` is clean, with `timewarp-signals-gitignore` passing.
- No product code changes. `<Version>` is unchanged, and this does not need a release.

## Checklist

- [x] `ganda repo audit --fix` applied and committed
- [x] `.timewarp/signals/` is ignored and nothing under it is tracked
- [x] `ganda repo audit` clean
- [ ] PR opened

## Results

`ganda repo audit --fix` exited 0. It appended `/.timewarp/signals/` to `.gitignore`, with the comment the check owns. Passed 39, failed 0, skipped 3. `timewarp-signals-gitignore` reports `.timewarp/signals/ is ignored`. The dev-cli compile inside that audit passed. No other tracked file changed. `.vscode/settings.json` stayed ignored.

`git ls-files .timewarp/signals` prints nothing. The tracked `.timewarp` paths are `dev.jsonc` and `ganda.jsonc` only. `source/Directory.Build.props` `<Version>` is still `15.0.0-beta.6`. No product code changed, and this walk does not cut a release.

The kitchen commit left the folderized task off origin-home, so the next `ganda repo audit` failed `kanban-landed`. `ganda kanban reconcile` landed that kitchen. The audit after the land passed 39, failed 0, including `timewarp-signals-gitignore` and `kanban-landed`.

The pull request stays for the host open-pr node.

### How to validate

Smoke: from this worktree, run `git check-ignore -v .timewarp/signals/example.log`, then `git ls-files .timewarp/signals`, then `ganda repo audit`.

Expect: check-ignore names `.gitignore` and `/.timewarp/signals/`. `git ls-files` prints nothing. Audit exits 0, passes `timewarp-signals-gitignore`, and reports failed 0. `<Version>` in `source/Directory.Build.props` is still `15.0.0-beta.6`.

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-09)
- Implementation: 01a1c0f9-bf92-7aa3-b0f8-3e2aef182808 (2026-11-09)
- Land: `ganda kanban reconcile` (2026-11-09) so `kanban-landed` matches origin-home
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c0f9-bf92-7aa3-b0f8-3e2aef182808, max-turns 500, Done — 2026-11-09T15:52:48Z

## Notes

- Audit failure seen on master `69419b4` (the PR #126 merge).
