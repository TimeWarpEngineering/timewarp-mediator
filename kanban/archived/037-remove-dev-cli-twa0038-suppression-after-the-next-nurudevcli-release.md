# Remove dev CLI TWA0038 suppression after the next Nuru.DevCli release

## Description

Task 036 turned off TWA0038 (500-line file cap, TimeWarp.SourceGenerators 1.0.0-beta.14) for the dev CLI build only. `TimeWarp.Nuru.DevCli` 4.0.0-beta.12 ships `release-command.cs` (609 lines) and `nuget-version-service.cs` (704 lines) as content files, and this repo cannot edit them. timewarp-nuru task 576 splits them. Once a Nuru.DevCli release with that split is on NuGet, remove the suppression here.

**Do not start until timewarp-nuru task 576 has shipped a `TimeWarp.Nuru.DevCli` release.**

## Requirements

- Pin `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to the release that contains the 576 split.
- Remove the TWA0038 suppression from `tools/dev-cli/dev.cs` and its comment.
- Rebuild the dev CLI: `dotnet run --file tools/dev-cli/dev.cs -- self-install` succeeds with TWA0038 active.
- `ganda repo audit` clean; `./bin/dev workflow` green.
- No `<Version>` bump needed on its own.

## Checklist

- [ ] Nuru.DevCli release with the 576 split is on NuGet
- [ ] Nuru pins updated
- [ ] TWA0038 suppression removed from `tools/dev-cli/dev.cs`
- [ ] `self-install` builds with TWA0038 active
- [ ] `ganda repo audit` clean, `./bin/dev workflow` green

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-12), from task 036 (Steven chose the dev-CLI-only suppression)

## Notes

- Upstream: timewarp-nuru task 576.
