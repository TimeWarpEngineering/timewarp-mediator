# Add tw-mediator agent skill (skills/tw-mediator/SKILL.md) before 14.0.0 stable

## Description

The repo has `skills/.gitkeep` and nothing else. The readme (197 lines) plus `documentation/diagnostics.md`
and `documentation/generated-vs-legacy.md` (another ~300 lines) explain Mediator to humans; an agent asked to add
Mediator to a project has to read all of it and still guess at the decision points. timewarp-nuru ships
`skills/tw-nuru/SKILL.md` (543 lines), links it from the readme ("For AI agents: load the built-in Nuru skill"),
and `ganda agents sync` distributes it into every cockpit. Mediator should ship the same before `14.0.0` stable
so the launch pack (task 015) and the readme can point agents at it. Maintainer decision 2026-10-13.

The skill is repo-only; it is not packed into any nupkg and does not change the version-bump PR.

## Requirements

- `skills/tw-mediator/SKILL.md` with frontmatter `name: tw-mediator` and a `description:` that says when to load it
  (adding TimeWarp.Mediator to a project, writing requests/handlers/notifications/behaviors, choosing generated vs
  reflection, fixing TWM diagnostics). Same frontmatter shape as `skills/tw-nuru/SKILL.md` in timewarp-nuru.
- Content, in this order, sourced from the existing docs so the two cannot disagree (link rather than duplicate
  where a doc already has the detail):
  1. **Which dispatcher.** Generated (`TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`,
     `AddGeneratedMediator()`) is the default; reflection (`TimeWarp.Mediator`, `AddMediator()`) only for streams
     (`IStreamRequest<>`) or runtime assembly scanning. One table, same split as generated-vs-legacy.md.
  2. **Minimal host.** Package references (CPM form), `[assembly: MediatorAssembly]`, one request + handler,
     `AddGeneratedMediator()`, `ISender.Send`. Then `INotification` + `IPublisher.Publish`, `IPipelineBehavior<,>`,
     and named pipelines (`ISender<TScope>`, `[MediatorScope]`).
  3. **Profiles.** Host vs Aot vs Link in a few lines each; Aot and Link do not weave behaviors; Aot/Link emit no
     `AddGeneratedMediator()`.
  4. **TWM001-TWM011.** One line each: meaning and the fix. Link `documentation/diagnostics.md`.
  5. **Gotchas from the beta dogfoods** (state 119, architecture 342, nuru 498): generated types are `internal`, so
     a friend of two hosts needs an extern alias; `TimeWarpMediatorAssembly=false` for libraries that host no
     handlers; `TimeWarpMediatorNamespace`; net10.0 floor for Contracts and runtime; streams are reflection-only.
  6. **13.x to 14.0 migration pointers.** A short list that links `documentation/release-notes/release-notes-v14.0.0.md`.
- Length target 250-350 lines. No prose that restates a doc the skill already links.
- Readme: an "For AI agents" paragraph pointing at `skills/tw-mediator/SKILL.md`, mirroring Nuru's.
- Every code sample in the skill compiles against `14.0.0-beta.5` (or whatever master pins) — verify by dropping
  the minimal host into a scratch runfile or into `tests/timewarp-mediator-package-smoke`, not by eye.
- `ganda agents sync` picks the skill up with no changes to ganda (check `ganda agents targets` lists it afterwards,
  or record why not).
- `./bin/dev workflow` green (docs-only change; this is the kebab-path and audit gate).

## Checklist

- [ ] `skills/tw-mediator/SKILL.md` written per the six sections above
- [ ] Code samples compiled against the pinned Mediator version
- [ ] Readme "For AI agents" paragraph added
- [ ] `ganda agents sync` distributes `tw-mediator` (or reason recorded)
- [ ] `./bin/dev workflow` green; `ganda repo audit` clean
- [ ] PR merged (merge commit)
- [ ] Results: line count, where samples were compiled, anything deliberately left out

## Session

- Created: 1774055 (2026-10-13)

## Notes

- Blocks: 017 (14.0.0 stable release) depends on this task.
- Template: https://github.com/TimeWarpEngineering/timewarp-nuru/blob/master/skills/tw-nuru/SKILL.md
- Sources: readme.md, source/*/readme.md, documentation/generated-vs-legacy.md, documentation/diagnostics.md,
  documentation/m2-named-pipelines.md, documentation/release-notes/release-notes-v14.0.0.md.
- Launch pack (task 015) and the GitHub Release draft should link the skill once it exists.
