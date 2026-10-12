# Rewrite the release process doc, add 14.x release notes, refresh agent.md

## Description

`documentation/overview.md` describes a release process that no longer exists, and there are no release notes for the 14.0.0 line. Parent review task 013, findings B-4, B-5, B-15, D-1, D-6.

- B-4 / D-6 (bug): overview.md references `.github/workflows/ci-cd.yml`, MinVer tags, MyGet feeds and `MYGET_TIMEWARP_MEDIATOR_CI_API_KEY`, a `build-and-publish` job, two packages, and 12.x version examples. agent.md points readers here for the release process.
- B-5 / D-1 (debt): only `release-notes-v13.0.0.md` exists, at the repo root; beta.2 to beta.4 changes are buried in `generated-vs-legacy.md` and `m1-generated-mediator.md`.
- B-15 (debt): agent.md says "both packages" (there are four), lists only Contracts and Mediator, misdescribes `microsoft-extensions-di/`, uses PascalCase test project names, omits `verify-samples` and `check-version`, and claims warnings-as-errors repo-wide.

## Requirements

- Rewrite overview.md to the real flow: version bump PR, GitHub Release, OIDC publish via `workflow.yml`, probe and break-glass modes, `dev workflow` / `dev release`, all four packages, `Directory.Build.props` versioning. Link to the tw-release skill where appropriate.
- Create `documentation/release-notes/`, move the v13 file, add a 14.0.0-beta rolling note covering beta.1 to beta.4 and the not-a-drop-in warning; link from readme.
- Refresh agent.md architecture, build, and command sections from the csproj files and `./bin/dev --help`.

## Checklist

- [x] overview.md rewritten and verified against workflow.yml and dev-cli
- [x] release-notes folder with v13 and v14.0.0-beta notes
- [x] agent.md refreshed
- [x] Implementation review: round 2 clean (M1–M4 fixed)
- [ ] PR with merge commit

## Notes

Parent: 013.

Review trail: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md` (clean, two rounds, general, effort 2).

## Session

- Created: 686431 (2026-10-11)
- Implementer: claude (ganda task work, 2026-10-12)
- Review: Grok `01a13008-1f8a-78f2-a86b-e35ce86f7320` (2026-10-12)
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 120 — 2026-10-12T12:26:54Z

## Results

- `documentation/overview.md` rewritten from `.github/workflows/workflow.yml` and `tools/dev-cli`:
  four packages, single `<Version>` in `source/Directory.Build.props` (no MinVer, no MyGet), the
  `ci` / `publish` jobs and their triggers, the four release guards, OIDC trusted publishing,
  version bump PR then `dev release [--dry-run]`, break-glass (`--ref v<Version>`,
  `confirm=release`) and probe modes, analyzer release tracking, and local dev commands. Links
  the `tw-release` skill and notes that this repo rebuilds from the tag instead of promoting the
  CI artifact.
- `git mv release-notes-v13.0.0.md documentation/release-notes/`.
- `documentation/release-notes/release-notes-v14.0.0.md`: not-a-drop-in warning up front, the
  existing post-beta.4 content under "Unreleased (next 14.0.0 beta)", and new beta.4, beta.3,
  beta.2 and beta.1 sections (from git tags `v14.0.0-beta.1`..`.4`, `generated-vs-legacy.md` and
  `m1-generated-mediator.md`). The per-beta changelog in `generated-vs-legacy.md` is replaced by a
  link to the notes.
- `readme.md` links both release notes.
- `agent.md`: dev CLI command table (incl. `verify-samples`, `check-version`, `release`, `clean`),
  four-package table from the csproj files, accurate `microsoft-extensions-di/` description,
  kebab-case test project list, warnings-as-errors scoped (tests/samples opt out), release pointer.
- Checked: `./bin/dev check-version` refuses 14.0.0-beta.4 as already published (matches doc);
  `./bin/dev release --dry-run` runs its guards (refused on dirty tree, as documented).

### Review

- Rounds: 2. Roster: general. Effort: 2 (by-diff, 511 lines).
- Round 1 counts: bug 1 open, suggestion 2 open, nit 1 open.
- Round 2 counts: bug 0 open / 1 fixed, suggestion 0 open / 2 fixed, nit 0 open / 1 fixed. No wontfix.
- Disposition: clean. M1 (`agent.md` streams are reflection-only), M2 (beta.4 DI extension is Host-only), M3 (unreleased license expression and trim annotations), M4 (dropped "no dependencies").
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
grep -nE "ci-cd.yml|MinVer|MyGet|MYGET|build-and-publish|12\.[45]\.0" documentation/overview.md agent.md
ls documentation/release-notes/ && test ! -e release-notes-v13.0.0.md
grep -n "release-notes" readme.md agent.md
./bin/dev --help
grep -nE "mode|confirm|id-token|Release guards" .github/workflows/workflow.yml
```

Expect: the first grep prints nothing; `documentation/release-notes/` holds
`release-notes-v13.0.0.md` and `release-notes-v14.0.0.md` and no root copy exists; readme and
agent.md link the release notes; every command named in overview.md and agent.md appears in
`./bin/dev --help`; the workflow modes, `confirm=release` gate, `id-token: write` on `publish` and
the guard step match what overview.md describes.
