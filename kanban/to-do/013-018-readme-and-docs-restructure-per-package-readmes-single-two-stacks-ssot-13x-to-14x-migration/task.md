# Readme and docs restructure: per-package readmes, single two-stacks SSOT, 13.x to 14.x migration

## Description

`readme.md` is the NuGet readme for every package and the "two stacks / not a drop-in" block is copied across five documents. Parent review task 013, findings B-11, D-2, D-3, D-4, D-5, D-10, D-24.

- D-2 (risk): relative links (`./migration.md`, `./documentation/...`) 404 on nuget.org; the Contracts, Generators, and Analyzers package pages show reflection `AddMediator` instructions first and the upstream MediatR badges.
- D-4 (debt): the same paragraph, packages table, membership table, and ClientPipeline snippet appear in readme, generated-vs-legacy, m1, m2, and migration, each with `14.0.0-beta.4` hard-coded.
- D-5 (debt): migration.md covers only MediatR to 13.x; no 13.x to 14.x generated-stack guidance or supported/unsupported feature matrix.
- D-3, B-11, D-10, D-24 (nit): stray `)` in readme; "full API compatibility" conflicts with "not a drop-in"; duplicated intro paragraph; CI badge points at a workflow named `CI` (actual name `CI/CD`); IIdempotent has no usage example; internal task ids ("004-001", "OQ-B", "Nuru 444") appear in user-facing headings and tables.

## Requirements

- Per-package readmes (or one readme with absolute github.com links and a package-neutral top section).
- `documentation/generated-vs-legacy.md` becomes the SSOT for the two-stacks block; other docs keep a short pointer; remove version literals from m1/m2.
- Add a "13.x reflection to 14.x generated" migration section with a feature matrix (streams, exception cascade, pre/post processors, container support).
- Fix the nits and remove internal ids from docs.

## Checklist

- [x] Package readme strategy decided and implemented; `./bin/dev pack` nuspec check
- [x] Two-stacks SSOT and pointers
- [x] 13.x to 14.x migration section
- [x] Nits and badge
- [ ] PR with merge commit

## Notes

Parent: 013. Coordinate with the release-process doc task to avoid overlapping edits to readme.

## Session

- Created: 686431 (2026-10-11)
- 2026-10-12: implemented under `ganda task work` (implement oracle).

## Results

**Readme strategy: per-package readmes.** Each packable project has its own
`source/<project>/readme.md` (package-specific top section, absolute github.com links only, no
version literals). `source/Directory.Build.props` packs `$(MSBuildProjectDirectory)/readme.md`
instead of the repository readme. The root `readme.md` is now the GitHub page only.

**Pack gate.** New `tools/dev-cli/services/nupkg-readme-check.cs`; `dev pack` fails when a
nupkg has no `readme.md`, its first heading is not `# <PackageId>`, or it has a relative link.

**Two-stacks SSOT.** `documentation/generated-vs-legacy.md` owns the two-stacks block,
packages table, membership table and named-pipeline snippet; readme, m1, m2 and migration keep
a short pointer. Version literals (`14.0.0-beta.4`) removed from readme, SSOT, m1 and m2 header
text; install snippets use `dotnet add package ... --prerelease`. Historical "shipped in
beta.N" bullets in m1 stay (they are facts, not current-version claims).

**Migration.** `migration.md` now has two sections: MediatR → 13.x (unchanged content,
headings demoted, "full API compatibility" scoped to 13.x) and a new
"13.x reflection to 14.x generated" section with a feature matrix (streams, exception cascade,
pre/post processors, behaviors, named pipelines, container support, lifetime, AOT, target)
and migration steps.

**Nits.** Stray `)` removed; "full API compatibility" no longer contradicts "not a drop-in";
duplicate intro paragraph and upstream MediatR badges removed; CI badge uses
`actions/workflows/workflow.yml/badge.svg` (workflow is named `CI/CD`); `IIdempotent` usage
example added to the readme; internal ids (004-001, 004-002, 013-010, OQ-B, Nuru 444, epic
005) removed from m1, m2 and m1-benchmark-gap. Also corrected the SSOT Analyzers row
(generator adds TWM005–TWM011, not only TWM005/TWM006). `agent.md` and `overview.md` describe
the new readme gate.

Verification: `dev build --clean`, `dev test` green; `dev pack` green with the readme gate;
negative check (relative link injected into the Contracts readme) fails pack; relative links
and anchors in readme/migration/documentation resolve; `ganda repo audit` passes.

Note: a first `dev build` failed with CS8784 (generator TypeLoadException on
`MembershipOptions`) from a stale compiler server; `dotnet build-server shutdown` and a clean
build fixed it. Not related to this change.

### How to validate

**Smoke:**

```bash
dotnet run --file tools/dev-cli/dev.cs -- pack
unzip -p artifacts/packages/TimeWarp.Mediator.Generators.*[0-9].nupkg readme.md | head -3
grep -n "13.x reflection to 14.x generated" migration.md
grep -rn "14.0.0-beta.4\|004-00\|OQ-B\|Nuru 444" readme.md documentation/m1-*.md documentation/m2-*.md documentation/generated-vs-legacy.md
```

**Expect:** pack ends with `Pack completed successfully!` and prints
`Package layout verified` for all four nupkgs; the Generators nupkg readme starts with
`# TimeWarp.Mediator.Generators`; migration.md has the 13.x → 14.x heading; the final grep
prints nothing.
