# Repo hygiene: remove fork-migration scratch files, archive design history, fix gitattributes and nuget.config

## Description

Root-level leftovers and small config defects. Parent review task 013, findings B-6, B-14, B-17, B-18, D-7, D-8, D-9.

- B-6 / D-8: `file-mapping.csv`, `files-to-modify.txt`, `files-to-modify-filtered.txt`, `original-fork-files.txt` were added in one commit on 2025-07-08 for the fork attribution task; nothing references them.
- D-7: `analysis/` holds the live design spec plus three superseded analyses and two 2025 release-process analyses; `.agents/collaboration/readme.md` links to `Analysis/` (capitalized; broken on Linux). m1 and m2 docs cite the spec as "Design SSOT".
- B-14: `.gitattributes` has `core.eol crlf` (not a valid attribute) and `*.sln` / `*.csproj merge=union` (no .sln exists; union merges on csproj can duplicate lines); `skills/` holds only `.gitkeep`; `build-command.cs` header claims both .sln and .slnx exist.
- B-17: `nuget.config` has no `<clear/>`, no package source mapping, and a UTF-8 BOM.
- B-18: the committed `timewarp-mediator.snk` is intentional (public identity key, MediatR convention) and only Mediator and Contracts are signed, but no doc says so.
- D-9: a single orphan blip post with no index.

## Requirements

- `git rm` the four scratch files.
- Move the live spec to `documentation/design/` and the superseded analyses plus agent threads to an archive location; fix links in m1, m2, and `.agents`.
- Fix `.gitattributes`, `nuget.config`, the build-command comment.
- Add a short note in agent.md about the snk and which assemblies are signed.
- Use `git mv` for moves.

## Checklist

- [ ] Scratch files removed
- [ ] Design docs moved and links fixed
- [ ] gitattributes and nuget.config fixed
- [ ] snk note
- [ ] `ganda repo audit` clean; PR with merge commit

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
