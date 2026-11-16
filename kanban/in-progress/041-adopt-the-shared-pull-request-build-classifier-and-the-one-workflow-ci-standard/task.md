# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-mediator to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: no `pull_request` paths filter; `dev workflow` does not classify. Work: add the classifier call.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Goal (implementer restatement)

Make every pull request report the `CI` check, and let a pull request that only touches
non-build paths (kanban, documentation, root markdown, ...) pass it quickly without building.

Acceptance:

- `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are pinned at `4.0.0-beta.15`.
- `dev workflow` in pr/merge mode calls `PullRequestBuildClassifier.DecideAsync(RepoRoot, ct)` before
  any step; when it says no build is needed it prints the message and returns exit 0. Release mode is
  unchanged.
- `workflow.yml` has no `paths` / `paths-ignore` on `pull_request` (already true; there is no `push`
  filter either, so there is nothing to keep).
- Files outside the default build roots that feed the build are declared in `.timewarp/dev.jsonc`
  `pullRequestBuild`: `assets/logo.png` is packed into both nupkgs. `samples/` already builds under
  the default rule (anything not declared non-build builds).
- No hand-copied classification rule in `tools/dev-cli/`.
- `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` pass (audit `--fix` changes committed).

## Checklist

- [x] `ganda repo audit --fix` run and committed (no tracked change; it wrote the gitignored `.vscode/settings.json`)
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` filter kept with a why comment (there is no `push` filter; the comment says why both stay unfiltered)
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Notes


After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Results

Changes:

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.14 -> 4.0.0-beta.15.
  `TimeWarp.Amuru` 3.0.0-beta.4 -> 3.0.0-beta.5, because Nuru beta.15 needs Amuru >= beta.5 and the
  dev CLI restore failed with NU1605 (package downgrade) at beta.4. `TimeWarp.Amuru.Tools`
  3.0.0-beta.4 -> 3.0.0-beta.5: the commit-hook attest then failed `package-pin-currency` on it.
- `tools/dev-cli/endpoints/workflow-command.cs`: `RunPrAsync` calls
  `PullRequestBuildClassifier.DecideAsync(RepoRoot, Ct)` first, prints `decision.Message`, and returns
  with exit 0 when `RequiresBuild` is false. Release mode is unchanged. No local copy of the rule.
- `.timewarp/dev.jsonc`: `pullRequestBuild.buildPaths = ["assets/logo.png"]`. Both nupkgs pack the
  logo, and the default rule skips `assets/**`. The root `readme.md` is not packed (each package packs
  its own `source/*/readme.md`), so it stays non-build. `samples/` already builds under the default
  rule.
- `.github/workflows/workflow.yml`: comments only. Neither `pull_request` nor `push` had a `paths`
  filter. The comment now says why both stay unfiltered (every pull request reports CI; every master
  commit gets the CI run the release green-CI guard needs).
- `agent.md`: Nuru version note and the `dev workflow` row mention the classifier.

### Gates

| Command | Result |
|---|---|
| `./bin/dev build` | PASS (0 warnings, 0 errors) |
| `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev CLI changed) | PASS after the Amuru bump |
| `./bin/dev test` | PASS: generators 61/61, analyzers 50/50, compilation 80/80 |
| `ganda repo audit` (ganda 1.0.0-beta.38) | PASS with 1 advisory (`vscode-window-icon`); `--fix` wrote the gitignored `.vscode/settings.json`; rerun: "Repository passes all audit checks", `dev-workflow-classifier` PASS |
| commit-hook attest on the first commit | FAIL `package-pin-currency` (`TimeWarp.Amuru.Tools` 3.0.0-beta.4 behind beta.5) |
| fix: bump `TimeWarp.Amuru.Tools`, `self-install`, `./bin/dev build`, `ganda repo audit` | PASS: "Repository passes all audit checks"; classifier skip smoke still exit 0 |

The installed ganda has no separate `pull_request` paths-filter check. `workflow.yml` was checked
by hand: no `paths` / `paths-ignore` anywhere.

CI run URL: the pull request is opened by the host `open-pr` node after this one, so its `CI` run URL
is not available on this node.

### How to validate

Smoke:

```bash
./bin/dev build && dotnet run --file tools/dev-cli/dev.cs -- self-install
B=$(git rev-parse origin/master); H=$(git rev-parse HEAD)
echo "{\"pull_request\":{\"base\":{\"sha\":\"$B\"},\"head\":{\"sha\":\"$H\"}}}" > /tmp/ev.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=/tmp/ev.json ./bin/dev workflow; echo "exit=$?"
```

Expect: this branch changes `tools/dev-cli/` and `.github/workflows/`, so the run prints
`Build path changed: ... Full build.` and starts `clean -> build -> ...`. With a kanban-only range
(for example base = `HEAD~1`, head = `HEAD` on a kanban-only commit) it prints
`Pull request changes only declared non-build paths. Skipping build and test. ...` and `exit=0`
without building. Without `GITHUB_EVENT_NAME` (a local run) it always builds.
Smoke-tested on this node: kanban-only range -> skip, exit 0; range `eafa561~1...eafa561` (touches
`.github/workflows/workflow.yml` and source) -> `Build path changed: .github/workflows/workflow.yml. Full build.`

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle 2026-11-16: classifier call, pins, dev.jsonc buildPaths; gates recorded under Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session c956032c-0330-4502-8d82-8603e32af74f, max-turns 500, Done — 2026-11-16T16:07:35Z
