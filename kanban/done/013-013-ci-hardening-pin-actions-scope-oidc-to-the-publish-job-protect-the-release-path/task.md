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

- [x] SHA pins and dependabot
- [x] Publish job split with least-privilege permissions
- [x] Key via env
- [x] Tag and green-CI guards
- [x] Concurrency, timeouts, caching, path filters
- [x] Probe mode run against the new layout (PR with merge commit is the host open-pr / merge step)
- [x] Implementation review disposition clean

## Notes

Parent: 013. Coordinate with the tw-release skill so its documented flow still holds.

Review: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 2, roster general. Disposition clean.

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12 implement (Claude Opus 5.5): workflow split, guards, dependabot, docs; probe + CI dispatch runs green.
- Review: Grok session 01a12fcb-8780-7f83-b446-2936ad131c6f (2026-10-12)
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 120 — 2026-10-12T11:24:29Z

## Results

`.github/workflows/workflow.yml` rewritten; `.github/dependabot.yml` added; `documentation/overview.md`
(workflow/trusted-publishing section) and dev-cli Design comments updated.

- **B-7 pins.** Every action is pinned to a commit SHA with a version comment: `actions/checkout`
  v4.4.0, `actions/setup-dotnet` v4.3.1, `actions/cache` v4.3.0, `actions/upload-artifact` v4.6.2,
  `NuGet/login` v1.2.0. Dependabot (weekly, grouped) covers `github-actions` and `nuget`. Pins stay on
  the current majors; dependabot will propose the major bumps.
- **B-7 / B-9 least privilege.** Workflow default is `contents: read`. The `ci` job (push, PR, dispatch
  `merge`) has only `contents: read`. A separate `publish` job (release, dispatch `release`/`probe`)
  alone has `id-token: write` and `actions: read`. The `actions: read` is now used: the guard lists CI runs.
  The old comment about downloading CI artifacts is gone. Checkouts use `persist-credentials: false`.
- **B-9 key handling.** The OIDC key goes to `dev workflow --mode release` through
  `env: NUGET_API_KEY` (already read by `PushAsync`). It is no longer a command-line argument.
- **B-9 guards** (publish job, before OIDC login and build). The step refuses unless:
  - the release tag equals `v` + `<Version>` from `source/Directory.Build.props`;
  - tag `v<Version>` exists and points at the checked-out commit;
  - that commit is an ancestor of `origin/master`;
  - `workflow.yml` has at least one successful `push` run for that SHA.

  The publish job still runs no tests. That green push run is the test gate. Break-glass dispatch must
  run with `--ref v<Version>`.
- **B-8.** Concurrency group `workflow-event-ref` cancels in-progress runs for `pull_request` only, so
  release and publish runs are never cancelled. Both jobs have `timeout-minutes: 30`. The NuGet cache is
  `actions/cache` on `~/.nuget/packages`, keyed on `Directory.Packages.props`, `**/*.csproj`,
  `tools/dev-cli/**` and `nuget.config`. setup-dotnet's built-in `cache:` needs `packages.lock.json`,
  and this repo has none.
- **B-10.** Path filters are removed. Every master commit now gets a CI run, and the green-CI guard
  depends on that. 129 master commits since 0c4d8463 never had a CI run.
- **Trusted publishing.** The NuGet.org policy binds the repo and the workflow *file* `workflow.yml`, not
  job names. Probe run 38239709602 (dispatch `mode=probe` on this branch, new `publish` job) succeeded:
  the OIDC exchange still matches after the split. The tw-release skill commands still hold for this
  repo: probe via `gh workflow run workflow.yml -f mode=probe`, and break-glass via `mode=release` +
  `confirm=release`.
- **Pre-existing master break, fixed at CI level.** A clean `dev workflow` failed with CS8784
  `TypeLoadException: TimeWarp.Mediator.Analyzers.MembershipOptions` and then CS0234 / CS1061 for
  `TimeWarp.Mediator.Generated`. The cause:
  1. The dev-cli runfile references TimeWarp.Nuru.
  2. TimeWarp.Nuru pulls the published TimeWarp.Mediator.Generators 14.0.0-beta.4.
  3. That package bundles an older `TimeWarp.Mediator.Analyzers` with the same `14.0.0.0` identity.
  4. The shared compiler server then serves that stale analyzer to the in-repo generator.

  Workflow-level `env: UseSharedCompilation: 'false'` fixes it. It is verified locally (full pipeline
  green) and in CI run 38240258310.
- **Out of scope / follow-ups.**
  - CODEOWNERS and SECURITY.md (B-8) are not added. They need an owner/contact decision.
  - The analyzer assembly-identity collision should get a product fix, for example a distinct
    AssemblyVersion per prerelease or a dev-cli that does not load the Mediator generator. Local
    `./bin/dev workflow` after a runfile compile can still hit it.
  - The tw-release skill's promotion pipeline (`dev release`, locate-run / download-artifact) is not
    implemented in this repo. Releases here rebuild from the tag behind the new guards.

### How to validate

**Smoke**

```bash
# lint
actionlint .github/workflows/workflow.yml
# trusted publishing probe against the new publish job
gh workflow run workflow.yml --ref <branch-or-master> -f mode=probe
# CI job only (publish skipped)
gh workflow run workflow.yml --ref <branch-or-master> -f mode=merge
# guard logic, locally, on a non-tag commit
GITHUB_REPOSITORY=TimeWarpEngineering/timewarp-mediator RELEASE_TAG=v9.9.9 bash -c "$(python3 -c "import yaml;print(next(s['run'] for s in yaml.safe_load(open('.github/workflows/workflow.yml'))['jobs']['publish']['steps'] if s['name'].startswith('Release guards')))")"
```

**Expect**

- actionlint is clean.
- The probe run succeeds, with job `publish` green (NuGet login OK) and `ci` skipped. Verified: run 38239709602.
- The merge run succeeds, with `ci` green, artifact `Packages-<n>` uploaded and `publish` skipped. Verified: run 38240258310.
- The local guard exits 1 with "Release tag 'v9.9.9' does not match v+Version". With an empty
  `RELEASE_TAG` on a branch commit it exits 1 with "Tag v… must … point at the commit". In a detached
  checkout of `v14.0.0-beta.4` it prints "Release guards passed (1 successful CI run(s) …)" and exits 0.

### Review

- Rounds: 1. Roster: general. Effort: 2 (299-line diff).
- Round 1: no findings.
- Final counts: bug 0 open / 0 fixed / 0 wontfix. Suggestion 0. Nit 0.
- Disposition: **clean**. No wontfix. No escalation.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Re-checked: action SHA pins match the commented tags; `actionlint` 1.7.7 clean; tag `v14.0.0-beta.4` is on `origin/master` with one successful push run of `workflow.yml`; a dispatch-only SHA does not count. Product code was not changed in this round.
