# Docs generated vs legacy AddMediator

## Description

Parent: **005**. Document the two stacks so State/Nuru do not call `AddMediator()` and think they got source-gen.

## Requirements

- README / `Documentation/`: `AddMediator()` = 13.x reflection fork (still in this repo); `AddGeneratedMediator()` and `AddGeneratedMediator<TScope>()` = generated
- Membership: `[assembly: MediatorAssembly]`, `[MediatorScope]`, `[MediatorBehavior]`
- Named pipelines: marker types, not strings; example `ClientPipeline` / `ServerPipeline`
- Package list for a host: Contracts + Generators (and Analyzers if not pulled transitively)
- Explicit: **14.0.0-beta** is untested outside M1/M2 golden files; not a drop-in for 13.0.0

## Checklist

- [x] README
- [x] Keep/extend `Documentation/m1-generated-mediator.md` and `m2-named-pipelines.md`
- [x] Issue #52 stays **open** until a stable 14.0.0 (not this epic)
- [x] Implementation review (effort 1, general) — disposition clean

## Out of scope

- Version bump (005-001)
- NuGet publish (005-003)

## Session

- Created: 150754 (2026-09-01)
- Implementer: grok session (2026-09-02)
- Review: grok, effort 1, roster general (2026-09-02)

## Results

Documented the two independent dispatchers so a State/Nuru host cannot treat `AddMediator()` as source-gen.

**What landed**

- `readme.md` — two-stack table first; generated install is Contracts + Generators; membership + marker-type pipelines; reflection `AddMediator` labeled as the 13.x fork
- `documentation/generated-vs-legacy.md` — comparison SSOT (packages, membership, named pipelines, 14.0.0-beta warning)
- `documentation/m1-generated-mediator.md` and `documentation/m2-named-pipelines.md` — kept and extended (host packages, attributes, `ClientPipeline` / `ServerPipeline` example)
- `migration.md` — callout that the MediatR rename is 13.x `AddMediator()`, not generated

**Issue #52:** confirmed **OPEN** (`gh issue view 52`). Not closed. Stable 14.0.0 is not this epic.

**Decisions**

- Comparison lives at `documentation/generated-vs-legacy.md` (repo path is kebab `documentation/`, not `Documentation/`)
- Generators nupkg packs `TimeWarp.Mediator.Analyzers.dll`; Analyzers is a separate package only when the generator is not referenced
- Tree `<Version>` is `14.0.0-beta.1`; nuget.org still serves 13.0.0 until 005-003

**Tests:** docs-only. No product code change. `gh issue view 52` → `state: OPEN`.

### How to validate

**Smoke**

```bash
# from repo root
rg -n "AddGeneratedMediator|14.0.0-beta is not a drop-in|ClientPipeline|TimeWarp.Mediator.Contracts" \
  readme.md documentation/generated-vs-legacy.md documentation/m1-generated-mediator.md documentation/m2-named-pipelines.md migration.md
gh issue view 52 --json state,title,url
```

**Expect**

- `readme.md` and `documentation/generated-vs-legacy.md` both state `AddMediator()` is the 13.x reflection fork and `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` are generated
- Host package list names `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`, with Analyzers only when Generators is not referenced
- Membership shows `[assembly: MediatorAssembly]`, `[MediatorScope]`, `[MediatorBehavior]`
- Named pipelines use marker types `ClientPipeline` / `ServerPipeline`, not strings
- Text **14.0.0-beta is not a drop-in for 13.0.0** appears in README, generated-vs-legacy, m1, m2, and migration.md
- `gh issue view 52` prints `"state": "OPEN"`

**Automated gate**

None beyond the greps above (docs-only; no runtime surface).

**Not in scope:** version bump (005-001, already merged); NuGet push (005-003); closing #52; `AddMediator()` auto-preferring generated types.

### Review disposition

- **Outcome:** clean
- **Rounds:** 1
- **Effort / roster:** 1, general
- **Counts (final, round 1):** bug 0; suggestion 0; nit 0 — final open count 0
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
