# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator.
Filter: (all)
Outdated:
  TimeWarp.SourceGenerators 1.0.0-beta.14 -> 1.0.0-beta.16 (patch)
  TimeWarp.Nuru 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
  TimeWarp.Nuru.DevCli 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
  Microsoft.CodeAnalysis.Analyzers 5.9.0 -> 5.12.0 (minor)
  Microsoft.CodeAnalysis.CSharp 5.9.0 -> 5.12.0 (minor)
  Microsoft.Extensions.DependencyInjection 10.0.15 -> 11.0.0 (major)
  Microsoft.CodeAnalysis.CSharp.Workspaces 5.9.0 -> 5.12.0 (minor)
  MediatR 14.3.0 -> 15.0.0 (major)
  Microsoft.Extensions.Logging 10.0.15 -> 11.0.0 (major)
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 15.0.0-beta.7 to 15.0.0-beta.8 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Session

- Minted by ganda task wave run 20261114T054552Z-9a0f39
