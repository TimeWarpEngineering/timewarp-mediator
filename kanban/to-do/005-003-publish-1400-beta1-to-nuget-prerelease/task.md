# Publish 14.0.0-beta.1 to NuGet prerelease

## Description

Parent: **005**. After **005-001** is merged and master CI is green, cut the **prerelease** with the repo’s trusted-publishing path (`dev release` / workflow). This is how State and Nuru consume the rewrite. It is **not** a stable 14.0.0.

## Depends on

- 005-001

## Requirements

- Tag/version is `14.0.0-beta.1` (prerelease)
- All four packages on nuget.org as **prerelease**
- Do **not** unlist or overwrite 13.0.0
- Do **not** ship 14.0.0 without `-beta`

## Checklist

- [ ] 005-001 merged; master CI green; Packages artifact exists
- [ ] `dev release --dry-run` then `dev release` (or this repo’s equivalent)
- [ ] Confirm nuget.org versions include `14.0.0-beta.1` for all four ids

## Out of scope

- Stable 14.0.0
- State/Nuru code changes

## Notes

- tw-release: bump is 005-001; this task is the cut. Never hand-type tags.

## Session

- Created: 150754 (2026-09-01)
