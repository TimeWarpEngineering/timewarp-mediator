# Review framework — task 013-018

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-018-readme-and-docs-restructure-per-package-readmes-single-two-stacks-ssot-13x-to-14x-migration/
**Diff scope:** commits `fcde87e` and `b7288d0` vs parent `f9a9975` (merge of PR #89) on `task/013-018-readme-and-docs-restructure-per-package-readmes-si`. 851 lines changed (596 insertions, 255 deletions). Product files: `agent.md`, `documentation/generated-vs-legacy.md`, `documentation/m1-benchmark-gap.md`, `documentation/m1-generated-mediator.md`, `documentation/m2-named-pipelines.md`, `documentation/overview.md`, `migration.md`, `readme.md`, `source/Directory.Build.props`, the four `source/*/readme.md` files, `tools/dev-cli/endpoints/pack-command.cs`, `tools/dev-cli/services/nupkg-readme-check.cs`. Kitchen `task.md` is out of product-review scope except for Results accuracy.
**Plan / brief:** Per-package NuGet readmes (absolute links, package-specific top), `documentation/generated-vs-legacy.md` as the two-stacks SSOT with short pointers elsewhere, a 13.x reflection → 14.x generated migration section and feature matrix, and the readme nits (stray paren, CI badge, duplicate intro, API-compat wording, `IIdempotent` example, internal task ids). Parent findings B-11, D-2, D-3, D-4, D-5, D-10, D-24.
**Effort:** 3 (general only; by-diff budget, not a specialist roster)
**Reviewer roster:** general
**Session IDs:** Grok `01a1301c-afcd-76a3-8ca5-e705fcdc5fdd` (2026-10-12)

## Budget (by-diff)

- Lines changed: 851
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Check the new matrix and package readmes against `mediator-emitter.cs`, the four csproj license expressions, and `documentation/diagnostics.md`. Do not treat `analysis/` as user-facing.
