# Round 2 — general
**Date:** 2026-11-09
**Scope reviewed:** delta since round 1 (`8944f20..8d6fdef`), product paths: `Directory.Packages.props`, `agent.md`, `documentation/release-notes/15.0.0-beta.6.md`, `tools/dev-cli/dev.cs`, and the six `tools/dev-cli/endpoints/*-command.cs` handlers.

## Summary

The fix pass moves `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to `4.0.0-beta.12`. That resolves M1. Nuru beta.12 ships `lib/net11.0` only and uses the Mediator 15 handler contract, so the dev CLI moves to `net11.0`. The six local handlers change from `ICommandHandler`/`Task<Unit>` to `IRequestHandler`/`ValueTask<Unit>`. The release-note sentence now names the `TimeWarp.SourceGenerators` reference and its generators, which resolves M2. The delta is small and consistent, and the risk is low.

## Verification

- nuget.org flat container: the latest `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are `4.0.0-beta.12`. The latest `TimeWarp.SourceGenerators` is `1.0.0-beta.12`.
- `ganda nuget outdated --no-cache` printed `All packages are up to date!` (27 entries).
- `dotnet build tools/dev-cli/dev.cs` exited 0 on SDK `11.0.100-rc.3`. `./bin/dev --capabilities` exited 0.
- `./bin/dev check-version --strict` exited 0. The source version is one prerelease increment ahead of `v15.0.0-beta.5`.
- `RunStepAsync` now takes `ValueTask<Unit>`. Each call site awaits the handler's `ValueTask` exactly once. That includes the DevCli `CleanCommand.Handler` and `CheckVersionCommand` handler, which compile against the same signature.
- No `ICommandHandler` or `Task<Unit>` remains under `tools/dev-cli`. The remaining `net10.0` mentions in the dev CLI refer to product/benchmark TFMs, not to the CLI's own TFM.
- CI (`.github/workflows/workflow.yml`) already installs `11.0.x` before `dotnet run --file tools/dev-cli/dev.cs`, so the `net11.0` CLI runs there.
- `agent.md` Target Frameworks line matches the new CLI TFM and Nuru version.
- `git diff --check` is clean.
- The 023-001 pins (`global.json`, the net11.0 DI RC pin, the BenchmarkDotNet pins, and CI `dotnet-quality: preview`) are untouched in this delta.

I did not rerun the full `./bin/dev workflow` at effort 1. The fix pass recorded it as green in Results. This delta touches only the dev CLI, and that CLI builds.

## Issues

None.
