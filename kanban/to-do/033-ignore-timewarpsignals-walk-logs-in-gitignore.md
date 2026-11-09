# Ignore .timewarp/signals/ walk logs in .gitignore

## Description

`ganda repo audit` fails `timewarp-signals-gitignore` on master: `.timewarp/signals/ is not ignored; walk signal logs dirty origin-home`. `ganda task work` writes signal logs under `.timewarp/signals/`. When that path is not ignored, the logs dirty the origin-home checkout, which blocks `ganda kanban` commands and `dev release`, and the post-merge attest refuses to sign master. Seen 2026-11-09 after merging PR #126.

## Requirements

- Run `ganda repo audit --fix` and commit what it changes. That should add the `.timewarp/signals/` ignore entry to `.gitignore`.
- If `--fix` does not add the entry, add `.timewarp/signals/` to `.gitignore` by hand.
- No file under `.timewarp/signals/` is tracked. If one is, remove it with `git rm --cached`.
- `ganda repo audit` is clean, with `timewarp-signals-gitignore` passing.
- No product code changes. `<Version>` is unchanged, and this does not need a release.

## Checklist

- [ ] `ganda repo audit --fix` applied and committed
- [ ] `.timewarp/signals/` is ignored and nothing under it is tracked
- [ ] `ganda repo audit` clean
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-09)

## Notes

- Audit failure seen on master `69419b4` (the PR #126 merge).
