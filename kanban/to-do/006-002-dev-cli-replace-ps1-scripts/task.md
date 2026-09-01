# Dev CLI replace PS1 scripts

## Description

Parent: **006**. Add TimeWarp `tools/dev-cli` + `bin/dev`. Replace PowerShell as the build/test/pack/push/hooks path.

## Depends on

- 006-001

## Requirements

- `tools/dev-cli/dev.cs` (Nuru) with `#region Purpose`; `bin/dev` executable
- Commands at least: build, test, pack (map today’s `Build.ps1`)
- `.github/scripts/Push.ps1` and CI pack steps call `dev` (or `dotnet run tools/dev-cli/dev.cs --`) not ad-hoc pwsh
- GitHooks: TimeWarp `.githooks` / `ganda` memsearch path, not `Tools/GitHooks/*.ps1` as the live hook
- `dev --capabilities` works (`dev-cli-capabilities`, `nuru` package ref)
- Delete or quarantine leftover `.ps1` once `dev` covers them (attribution/file-mapping one-shots can go to `documentation/` or `tools/` as runfiles if still needed)

## Scripts to retire

- `Build.ps1`
- `.github/scripts/Push.ps1`
- `Tools/GitHooks/SetupGitHooks.ps1`, `PreCommit.ps1`
- `Tools/FileSync/SyncConfigurableFiles.ps1`, `.github/scripts/SyncConfigurableFiles.ps1`
- One-shots: `Add-AttributionComments.ps1`, `Create-FileMapping.ps1`, `Filter-Files.ps1`

## Out of scope

- Kebab rename (**006-003**)
- Full audit green (**006-005**)

## Session

- Created: 162284 (2026-09-01)
