# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** `master...HEAD` product paths under `tools/dev-cli/` (same as framework)

## Summary

The change adds `.UseTelemetry(options => options.ServiceName = "timewarp-mediator-dev")` to the `NuruApp.CreateBuilder()` chain, a string literal as required. It adds `IncomingTraceContext` and wraps `RunAsync` with `using IDisposable? incomingTrace = IncomingTraceContext.Restore();`. It also adds `tools/dev-cli/telemetry.md`. Risk is low. The change affects only the dev CLI, and nothing is exported unless `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

## Checks performed

- **Requirements vs diff.** Every requirement is met: the literal service name, the `UseTelemetry` call before `DiscoverEndpoints`/`Build` so DevCli routes get the behavior span, the `IncomingTraceContext` copy, and the `RunAsync` wrap. No `ActivitySource` was registered. `grep -rn ActivitySource --include=*.cs` finds only the Design comment in `dev.cs`, so the repo has no source to register.
- **Fidelity to the reference.** `tools/dev-cli/endpoints/incoming-trace-context.cs` is byte-identical to timewarp-nuru master `tools/dev-cli/endpoints/incoming-trace-context.cs` (`diff` is empty). `telemetry.md` differs from the nuru copy only in the service name and in replacing nuru's workflow stage-span section (nuru-specific) with a one-paragraph statement for this repo. The `dev.cs` wiring matches nuru lines 62 and 73–74.
- **Compile surface.** The `Directory.Build.props` glob `endpoints/**/*.cs` compiles the new file. `global-usings.cs` already supplies `System.Diagnostics`, `System.Runtime.CompilerServices`, and `DevCli`.
- **Smoke.** `./bin/dev --capabilities` returns JSON with the endpoints. `TRACEPARENT=garbage ./bin/dev --help` exits 0, so an invalid header is ignored. A valid `TRACEPARENT` with no endpoint also exits 0, and nothing is exported. The implementer's Results record an OTLP export to a local collector with the right trace id and parent span id. I did not re-run that collector test.
- **Goal restatement vs task.** The restatement matches the task and adds nothing beyond it. This is not a misread.

## Issues

None.

Observation, not a finding: `IncomingTraceContext` writes `Activity._spanId` through `UnsafeAccessor`. A future runtime that renames that field would make the accessor throw `MissingFieldException` at startup whenever `TRACEPARENT` is set. The task requires copying the nuru reference verbatim, and the dev CLI pins `net10.0`, so any change belongs upstream in timewarp-nuru, not on this id.
