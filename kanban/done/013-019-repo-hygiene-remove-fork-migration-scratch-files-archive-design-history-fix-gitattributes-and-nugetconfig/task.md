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

- [x] Scratch files removed
- [x] Design docs moved and links fixed
- [x] gitattributes and nuget.config fixed
- [x] snk note
- [ ] `ganda repo audit` clean; PR with merge commit

## Notes

Parent: 013.

Review trail: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md` (clean, two rounds, general, effort 2).

## Results

- B-6 / D-8: `git rm` of `file-mapping.csv`, `files-to-modify.txt`, `files-to-modify-filtered.txt`, `original-fork-files.txt`.
- D-7: `git mv` of the live spec to `documentation/design/2026-06-17-source-gen-aot-rewrite-spec.md`. The three superseded analyses and the Composer/Opus thread (from `.agents/collaboration/2026-06-17-source-gen-aot-rewrite/`) moved to `documentation/archive/2026-06-17-source-gen-aot-rewrite/` (thread under `collaboration/`). The two 2025 release analyses moved to `documentation/archive/2025-release-process/`. `analysis/` is gone. m1 and m2 link the new spec path. Cross-links inside the spec, the archived analyses and the thread were rewritten (no more capitalized `Analysis/`). `.agents/collaboration/readme.md` now points at `documentation/design/` and documents the archive location.
- D-9: the orphan blip moved to `documentation/archive/posts/blips/`.
- B-14: `.gitattributes` drops `core.eol crlf` and every `merge=union` line (csproj/vbproj/fsproj/dbproj/sln), adds `*.snk binary`. `build-command.cs` Design comment no longer claims a .sln exists. `skills/.gitkeep` kept: it is the TimeWarp convention scaffold and `ganda repo audit` directory-structure passes with it.
- B-17: `nuget.config` has no BOM, `<clear />`, and a `packageSourceMapping` of `*` to nuget.org.
- B-18: agent.md Build Properties notes the snk is an intentional public identity key, that only Mediator and Contracts (of the four packages) are strong-named, and that only `timewarp-mediator-tests` signs with that key (`tests/Directory.Build.props` turns signing off) so `TimeWarp.Mediator` can grant `InternalsVisibleTo`.
- Gates: `ganda repo audit` clean; `./bin/dev build --clean` and `./bin/dev test` pass (a stale VBCSCompiler server first served an old Analyzers DLL; `dotnet build-server shutdown` cleared it).

### Review

- Rounds: 2. Roster: general. Effort: 2 (by-diff, 678 lines).
- Round 1 counts: suggestion 1 open. No bugs, no nits.
- Round 2 counts: suggestion 0 open / 1 fixed. No wontfix.
- Disposition: clean. M1 (`agent.md` snk note named the whole test tree as signed; only `timewarp-mediator-tests` is).
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:
```bash
ls analysis file-mapping.csv files-to-modify.txt files-to-modify-filtered.txt original-fork-files.txt 2>&1
ls documentation/design documentation/archive
grep -rn "Analysis/\|analysis/2026" --exclude-dir=kanban --exclude-dir=.git . | grep -v documentation/archive
grep -n "core.eol\|merge=union" .gitattributes
head -c3 nuget.config | xxd; grep -n "clear\|packageSourceMapping" nuget.config
grep -n "snk" agent.md
ganda repo audit && ./bin/dev build --clean --quiet && ./bin/dev test --no-build --quiet
```

Expect: every `ls` of a removed path reports "No such file"; `documentation/design` holds the spec and `documentation/archive` holds `2026-06-17-source-gen-aot-rewrite/`, `2025-release-process/`, `posts/`; both greps print nothing; nuget.config starts with `3c3f 78` (no BOM) and shows `<clear />` and `<packageSourceMapping>`; agent.md shows the snk note; audit, build and test all succeed.

## Session

- Created: 686431 (2026-10-11)
- Review: Grok `01a1302a-bd57-7e33-ae63-6cb21c134225` (2026-10-12)
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 120 — 2026-10-12T12:57:45Z
