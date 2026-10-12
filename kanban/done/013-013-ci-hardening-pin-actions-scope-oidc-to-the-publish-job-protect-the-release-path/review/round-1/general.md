# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** `5fdd7fd..HEAD` — workflow split, SHA pins, dependabot, release guards, `NUGET_API_KEY` env, `UseSharedCompilation`, overview trusted-publishing section, dev-cli comments

## Summary

The publish job is the only job with `id-token: write` and `actions: read`. It runs on `release: published` and on `workflow_dispatch` `release`/`probe`. The ci job stays `contents: read` and runs tests (`CiModeDetector` maps `push` and `workflow_dispatch` to Merge unless `--mode release` is passed). The OIDC key is step `env`, and `PushAsync` already reads `NUGET_API_KEY`. The guard script has no `${{ }}` in the `run` block. Tag, version, and SHA comparisons are quoted parameter expansions, so a hostile tag or `<Version>` is not executed. `actions/checkout` at the pinned SHA with `fetch-depth: 0` fetches every branch and tag, so `origin/master` and `refs/tags/v<Version>` exist for the ancestor and tag checks. Current GitHub docs set `GITHUB_SHA` on `release` to the tagged commit, which is what checkout checks out. Each pin matches the tag named in the comment (`actions/checkout` v4.4.0, `actions/setup-dotnet` v4.3.1, `actions/cache` v4.3.0, `actions/upload-artifact` v4.6.2, `NuGet/login` v1.2.0). `actionlint` 1.7.7 is clean. Against tag `v14.0.0-beta.4` (`0c20d9f1`), `<Version>` is `14.0.0-beta.4`, the commit is an ancestor of `origin/master`, and the runs API returns `total_count` 1 for `event=push&status=success`. The same filter returns 0 for the dispatch-only SHA `ec1ffad` and for a bogus SHA, so a probe or merge dispatch cannot satisfy the guard. Probe run 38239709602 logged in and skipped the release pipeline. CI run 38240258310 ran Mode Merge (clean, build, test, pack) and succeeded with `UseSharedCompilation: false`. `source/Directory.Build.props` has no CR, so the sed capture does not grow a trailing `\r`. CODEOWNERS, SECURITY.md, and the overview MinVer/MyGet rewrite stay on the task's stated follow-ups (013-017 for the release doc). No defect found.

## Issues

No issues.
