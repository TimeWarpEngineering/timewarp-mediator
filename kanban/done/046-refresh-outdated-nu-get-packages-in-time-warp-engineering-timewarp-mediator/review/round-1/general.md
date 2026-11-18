# Round 1 — general
**Date:** 2026-11-19
**Scope reviewed:** branch `task/046-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `958c9bb`. Product files: `Directory.Packages.props`, `agent.md`, `tools/dev-cli/endpoints/benchmark-command.cs`. Checked against `task.md` description, `## Goal (implementer restatement)`, and `## Results`.

## Summary

The four dev-only pins move to the versions the task names, and `<Version>` in `source/Directory.Build.props` stays `15.0.0-beta.10`. The Amuru 3.0.0-beta.10 analyzer break is fixed by calling `DotNet.Publish()` with the same project, configuration, framework, runtime, `BenchmarkAot`, and output the old `dotnet publish` line used. Recorded gates cover that path: self-install, `./bin/dev build`, `./bin/dev benchmark --aot`, and `ganda repo audit`. The restatement matches the task. The design comment that still names Nuru.DevCli 4.0.0-beta.15 is the only miss against the restatement's accuracy bar.

The cached package at `~/.nuget/packages/timewarp.nuru/4.0.0-beta.16/lib` contains only `net11.0`, so the `agent.md` sentence is accurate. That listing was a check of the edited fact, not a build or an audit. No finding follows from it.

## Issues

### Issue 1 — Severity: nit
- File: tools/dev-cli/endpoints/workflow-command.cs:19
- Description: The Design comment still says TimeWarp.Nuru.DevCli 4.0.0-beta.15 ships `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` as internal. The pin is now 4.0.0-beta.16. Those three types are still `internal` in the beta.16 package content, so the handler stays internal for the same reason. The version token is the stale fact. The restatement says doc and comment references that name the old Nuru version as a fact stay accurate. `agent.md` was updated. This comment was not. The original task does not name the comment; the miss is against that restatement bar.
- Suggestion: Change `4.0.0-beta.15` to `4.0.0-beta.16` in that comment.
- Status: open
