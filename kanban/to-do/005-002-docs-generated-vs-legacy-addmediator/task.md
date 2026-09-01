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

- [ ] README
- [ ] Keep/extend `Documentation/m1-generated-mediator.md` and `m2-named-pipelines.md`
- [ ] Issue #52 stays **open** until a stable 14.0.0 (not this epic)

## Out of scope

- Version bump (005-001)
- NuGet publish (005-003)

## Session

- Created: 150754 (2026-09-01)
