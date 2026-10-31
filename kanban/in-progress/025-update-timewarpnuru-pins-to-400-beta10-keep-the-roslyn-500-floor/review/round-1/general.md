# Round 1 — general
**Date:** 2026-10-31
**Scope reviewed:** same as framework (branch vs merge-base with `master`)

## Summary

The change moves `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from `4.0.0-beta.7` to
`4.0.0-beta.10` in `Directory.Packages.props`, updates the beta number in the `agent.md` dev
CLI line, and makes `WorkflowCommand.Handler` internal. The handler change is required:
DevCli beta.10 ships its services as source content (`content/any/services/*.cs`) and
changed `NuGetVersionService`, `IRepoConfigService` and `IPackableProjectService` from
`public` (beta.7) to `internal`. A public type with a public constructor cannot take
internal parameters (CS0051). The content files compile into the dev CLI assembly, so an
internal handler can see them. This is a small change with low risk. It does not touch
`source/` or the benchmark projects.

## Verified claims

- `Directory.Packages.props:26-27`: both Nuru pins are at `4.0.0-beta.10`. The local cache
  has beta.8 through beta.10, and beta.10 is the newest.
- `Microsoft.CodeAnalysis.CSharp` and `.Workspaces` are still `5.0.0` (lines 38 and 45).
  `VersionOverride="5.0.0"` is still on the analyzer and generator csproj files, and the
  Dependabot ignore for `Microsoft.CodeAnalysis.CSharp*` is still in place.
- The `Compiler_IsTheDeclaredFloor` test, rerun by this reviewer with `--no-build`, passed
  1/1 on net10.0 and on net11.0.
- `~/.nuget/packages/timewarp.nuru/4.0.0-beta.10/lib/` holds only `net10.0`, so keeping
  `#:property TargetFramework=net10.0` is correct, and Results says so.
- In beta.7 the three DevCli services are `public`; in beta.10 they are `internal`. That
  confirms the reason given in the new comment in the workflow-command.cs region.
- `./bin/dev --capabilities` lists benchmark, build, pack, test, verify-samples, workflow,
  check-version, clean, release and self-install. benchmark has the `framework`, `filter`,
  `record` and `gate` options. That matches every command `agent.md` documents.
- `<Version>` is unchanged.

## Goal restatement vs task

The restatement matches the Requirements. No misread.

## Issues

None.
