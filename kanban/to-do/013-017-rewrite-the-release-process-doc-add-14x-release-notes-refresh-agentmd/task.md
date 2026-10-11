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

- [ ] overview.md rewritten and verified against workflow.yml and dev-cli
- [ ] release-notes folder with v13 and v14.0.0-beta notes
- [ ] agent.md refreshed
- [ ] PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
