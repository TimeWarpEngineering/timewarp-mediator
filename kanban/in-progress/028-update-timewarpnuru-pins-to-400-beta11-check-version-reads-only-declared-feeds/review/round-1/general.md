# Round 1 — general
**Date:** 2026-10-31
**Scope reviewed:** branch `task/028-update-timewarpnuru-pins-to-400-beta11-check-versi` vs merge-base with `master` (commit 9ad9aac plus kanban landings), product paths only

## Summary

The change moves both Nuru pins to `4.0.0-beta.11`, sets `<Version>` to `15.0.0-beta.4`, updates
every install line and pin, renames the beta.3 release note to beta.4 with `git mv`, and updates
both links to it. Risk is low: no code changes, and dev CLI call sites compile unchanged.

Verified against the repo and the restored packages:

- DevCli 4.0.0-beta.11 `content/any/services/*.cs` still declares `NuGetVersionService`,
  `IRepoConfigService`, and `IPackableProjectService` `internal`, so keeping
  `WorkflowCommand.Handler` internal is correct.
- `TimeWarp.Nuru` 4.0.0-beta.11 has `lib/net10.0` only, so the `agent.md` bullet is accurate.
- `.timewarp/dev.jsonc` has no `feeds` key. `.github/workflows/workflow.yml` is not in the diff.
  Its only `GH_TOKEN` is the existing one on the release-guards step.
- `git grep '15\.0\.0-beta\.3' -- ':!kanban'` hits only the historical paragraph in
  `documentation/release-notes/15.0.0-beta.4.md`. No `beta.10` remains outside release notes.
- I reran `env -u GITHUB_TOKEN -u GH_TOKEN GH_CONFIG_DIR=/nonexistent ./bin/dev check-version`. It
  exited 0 and printed `Version in source: 15.0.0-beta.4`, `Latest NuGet version: 15.0.0-beta.2`,
  and `safe to release`. The output does not mention GitHub Packages, a token, or `Unknown`.

There is one problem: the new opening paragraph of the beta.4 release note gets the compatibility
facts wrong.

## Issues

### Issue 1 — Severity: bug
- File: documentation/release-notes/15.0.0-beta.4.md:3
- Description: The new paragraph ends with "and the shipped packages are unchanged since
  15.0.0-beta.2". That is false. `git diff v15.0.0-beta.2 HEAD` shows that the shipped
  generator and analyzer dropped `VersionOverride="5.0.0"` on `Microsoft.CodeAnalysis.CSharp` and
  now reference the central pin, `5.9.0` (`source/timewarp-mediator-generators/timewarp-mediator-generators.csproj`,
  `source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj`, `Directory.Packages.props`).
  A consumer on the oldest .NET 10 SDK band could load beta.2 and now gets CS9057. The generator
  emits nothing. The same note says this two paragraphs later: the SDK compiler must be at least
  the referenced Roslyn version. The F14 row in `15.0.0.md` links to this note to explain that
  change. A consumer moving from beta.2 to beta.4 who reads the first paragraph is told there is
  nothing to check. The wording comes from the task addendum, so the task text is wrong, not the
  implementer's work. The published note is still wrong.
- Suggestion: Keep the rest of the paragraph and replace the false clause. Example: "15.0.0-beta.4
  ships what beta.3 would have shipped. Only the dev tooling changed: it moved to DevCli
  4.0.0-beta.11, so `check-version` reads only nuget.org." The next paragraph already says the
  dispatcher has no runtime change since beta.2. Leave that paragraph as it is.
- Status: open
