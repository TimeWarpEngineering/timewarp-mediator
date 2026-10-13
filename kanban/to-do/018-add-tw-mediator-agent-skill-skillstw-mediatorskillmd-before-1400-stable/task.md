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

- [x] `skills/tw-mediator/SKILL.md` written per the six sections above
- [x] Code samples compiled against the pinned Mediator version
- [x] Readme "For AI agents" paragraph added
- [x] `ganda agents sync` distributes `tw-mediator` (or reason recorded)
- [x] `./bin/dev workflow` green; `ganda repo audit` clean
- [ ] PR merged (merge commit)
- [x] Results: line count, where samples were compiled, anything deliberately left out

## Session

- Created: 1774055 (2026-10-13)
- 2026-10-13: implementer (ganda task work) wrote skill + readme paragraph; samples compiled; workflow + audit green.
- Implement oracle: implemented by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-10-13T12:36:11Z
- Review oracle: Grok session `01a13543-65d3-7a02-bd6a-f600ef005ce9` (2026-10-13). Effort 2, roster: general. Artifacts under `review/`.

## Results

- `skills/tw-mediator/SKILL.md`: 289 lines, frontmatter `name: tw-mediator` + `description:` (same shape as tw-nuru).
  Sections in the required order: which dispatcher, minimal host (CPM packages, membership, request/handler/send,
  notifications, behaviors, named pipelines), profiles, TWM001-TWM011 table, gotchas, 13.x to 14.0 migration
  pointers. Detail stays in generated-vs-legacy.md, diagnostics.md, m2-named-pipelines.md, migration.md and the
  14.0.0 release notes, which the skill links by absolute GitHub URL (the skill is copied out of the repo by sync).
- `readme.md`: "For AI agents" paragraph under Documentation, mirroring Nuru's. `skills/.gitkeep` removed.
- Samples compiled and run against **14.0.0-beta.5 from nuget.org** (the version master pins) in a scratch
  CPM project outside the repo (`/tmp/tw-mediator-skill-check`, TreatWarningsAsErrors on): one Host-profile
  program with request + handler + `ISender.Send`, `INotification` + `IPublisher.Publish`, an unscoped
  `[assembly: MediatorBehavior]`, and a `ClientPipeline` named pipeline with a scoped behavior (output
  `-> Ping / hi pong / email for order 42 / client stamp / client pong`); a second Aot-profile program with
  `new Mediator()` (`aot pong`), rerun with `TimeWarpMediatorNamespace=My.App.Mediation` (`aot pong`).
  Not compiled: the `extern alias` snippet (two-host friend setup is a csproj/alias fragment, not a program).
- Distribution: `ganda agents sync` does **not** carry skills; it only syncs the global preferences file
  (`ganda agents targets` lists the AGENTS.md/CLAUDE.md targets). Skills ship through `ganda skills`; tw-nuru is
  registered there from the timewarp-nuru master worktree. After this PR merges, the operator runs
  `ganda skills add tw-mediator worktree://github.com/TimeWarpEngineering/timewarp-mediator/master/skills/tw-mediator`
  then `ganda skills sync`. Not registered from the task worktree because that path disappears at `worktree gc`.
- `./bin/dev workflow --mode pr`: green (build, test, verify-samples, pack + package smoke). `ganda repo audit`: clean.
- Gotcha hit while verifying (repo-dev only, left out of the skill): building against the nuget beta.5 analyzers and
  then the in-repo generator of the same assembly version in one compiler-server session fails with
  CS8784 `TypeLoadException ... MembershipOptions`. `dotnet build-server shutdown` clears it.
- Deliberately left out: reflection-stack registration detail (the readme covers `AddMediator` and the
  container samples), idempotency contracts beyond a one-line mention, benchmarks.
- Review: 2 rounds, roster general, effort 2 (by-diff, 352 lines). Round 1 raised 2 bugs and 1 suggestion, all open. Round 2 fixed all three; 0 open, 0 wontfix, no new findings. Disposition: **clean**. No exceptions and no escalation.
  - M1 (bug): membership gotcha. An unmarked library is omitted. When its requests are outside the graph the host build stays clean and `Send` throws `NoHandlerException`. TWM001 only when the request is already in a member assembly.
  - M2 (bug): TWM008. Contracts is required in every profile. Aot is only the escape for a missing Host-profile MS.DI Abstractions reference.
  - M3 (suggestion): Link bullet no longer says call-site inlining is reserved.
  - Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

```bash
wc -l skills/tw-mediator/SKILL.md
head -4 skills/tw-mediator/SKILL.md
grep -n "For AI agents" readme.md
./bin/dev workflow --mode pr
ganda repo audit
```

Expect: 289 lines (inside the 250-350 target); frontmatter shows `name: tw-mediator` and a `description:`;
readme links `skills/tw-mediator/SKILL.md`; workflow ends `Pipeline SUCCEEDED`; audit reports
`Repository passes all audit checks.` To recheck the samples, paste the section 2 snippets into a net10.0
console project that references Contracts + Generators 14.0.0-beta.5 and run it (run
`dotnet build-server shutdown` first if you just built the repo).

## Notes

- Blocks: 017 (14.0.0 stable release) depends on this task.
- Template: https://github.com/TimeWarpEngineering/timewarp-nuru/blob/master/skills/tw-nuru/SKILL.md
- Sources: readme.md, source/*/readme.md, documentation/generated-vs-legacy.md, documentation/diagnostics.md,
  documentation/m2-named-pipelines.md, documentation/release-notes/release-notes-v14.0.0.md.
- Launch pack (task 015) and the GitHub Release draft should link the skill once it exists.
- Review trail: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/merged.md`, `review/disposition.md`. Outcome clean (2026-10-13).
