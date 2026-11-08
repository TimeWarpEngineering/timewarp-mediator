#!/usr/bin/env -S dotnet --
#:property TargetFramework=net10.0
// ═══════════════════════════════════════════════════════════════════════════════
// DEV CLI - timewarp-mediator DEVELOPMENT TOOL
// ═══════════════════════════════════════════════════════════════════════════════
//
// Usage:
//   As runfile:  dotnet run --file tools/dev-cli/dev.cs -- <command>
//   As AOT:      ./bin/dev <command>
//
// Run `./bin/dev --help` for available commands.
//
// To bootstrap:
//   dotnet run --file tools/dev-cli/dev.cs -- self-install
//   direnv allow
//   dev --help
// ═══════════════════════════════════════════════════════════════════════════════

#region Purpose
// Entry point for the TimeWarp.Mediator development CLI
#endregion
#region Design
// Thin Nuru wrapper. Shared endpoints (clean, self-install, check-version) come
// from TimeWarp.Nuru.DevCli. Local endpoints map the former Build.ps1 / Push.ps1
// path: build and test the root slnx, pack the product projects, push via
// workflow (key from NUGET_API_KEY env; --api-key for local use). Product lives
// under source/.
//
// UseTelemetry is on so a CI job or a ganda walk that shells out to dev
// exports one command span per route, including the DevCli routes. The OTLP
// endpoint stays unset here: a non-literal OtlpEndpoint is ignored, and an
// unset OTEL_EXPORTER_OTLP_ENDPOINT exports nothing. ServiceName is the
// literal timewarp-mediator-dev. OTEL_SERVICE_NAME overrides it. This repo has
// no ActivitySource of its own, so none is registered. IncomingTraceContext
// restores TRACEPARENT before RunAsync because TelemetryBehavior parents its
// span to Activity.Current. See tools/dev-cli/telemetry.md.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-mediator")
  .UseTelemetry(options => options.ServiceName = "timewarp-mediator-dev")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

using IDisposable? incomingTrace = IncomingTraceContext.Restore();
return await app.RunAsync(args);
