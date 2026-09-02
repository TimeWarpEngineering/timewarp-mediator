#!/usr/bin/env -S dotnet --
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
// path: build and test the root slnx, pack the four product projects, push via
// workflow --api-key. Product lives under source/.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-mediator")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
