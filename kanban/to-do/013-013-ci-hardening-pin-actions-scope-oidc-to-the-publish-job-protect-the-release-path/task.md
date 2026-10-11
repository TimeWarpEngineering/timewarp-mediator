# CI hardening: pin actions, scope OIDC to the publish job, protect the release path

## Description

`.github/workflows/workflow.yml` grants publish-capable permissions to every run and has no release guard. Parent review task 013, findings B-7, B-8, B-9, B-10.

- B-7 (risk): `actions/checkout@v4`, `actions/setup-dotnet@v4`, `nuget/login@v1`, `actions/upload-artifact@v4` are tag-pinned. Job-level `id-token: write` and `actions: read` apply to push, PR, and dispatch runs. The comment about downloading CI artifacts describes behavior that does not exist.
- B-9 (risk): the OIDC key is interpolated into the shell command as `--api-key "${{ ... }}"`. No check that the release tag equals `v` + Version, and no requirement that the tagged commit has a green CI run. Release runs skip tests.
- B-8 (debt): no `concurrency`, no `timeout-minutes`, no NuGet cache, no dependabot for nuget or github-actions, no CODEOWNERS/SECURITY.md.
- B-10 (risk): path filters omit `readme.md`, `assets/logo.png` (both packed), `BannedSymbols.txt`, `nuget.config`, `.editorconfig`, `.githooks/**`, `notice`, `unlicense`.

## Requirements

- Pin all actions to commit SHAs with a version comment; add `.github/dependabot.yml` for github-actions and nuget.
- Split a `publish` job with `id-token: write` that runs only on release and dispatch; the CI job keeps `contents: read`.
- Pass the key via `env: NUGET_API_KEY` (already read by the push code) instead of a command-line argument.
- Add a tag-vs-Version assertion and verify the tagged commit is on master with a successful CI run before pushing.
- Add `concurrency` (cancel-in-progress for PRs only), `timeout-minutes`, and setup-dotnet caching.
- Fix path filters or drop them.
- Confirm the NuGet trusted-publishing policy still matches the workflow file and job names after the split.

## Checklist

- [ ] SHA pins and dependabot
- [ ] Publish job split with least-privilege permissions
- [ ] Key via env
- [ ] Tag and green-CI guards
- [ ] Concurrency, timeouts, caching, path filters
- [ ] Probe mode run against the new layout; PR with merge commit

## Notes

Parent: 013. Coordinate with the tw-release skill so its documented flow still holds.

## Session

- Created: 686431 (2026-10-11)
