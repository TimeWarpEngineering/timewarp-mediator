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

- [ ] Package readme strategy decided and implemented; `./bin/dev pack` nuspec check
- [ ] Two-stacks SSOT and pointers
- [ ] 13.x to 14.x migration section
- [ ] Nits and badge
- [ ] PR with merge commit

## Notes

Parent: 013. Coordinate with the release-process doc task to avoid overlapping edits to readme.

## Session

- Created: 686431 (2026-10-11)
