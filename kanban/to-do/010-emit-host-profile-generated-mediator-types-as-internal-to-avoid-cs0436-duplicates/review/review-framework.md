# Review framework — task 010

**Date:** 2026-09-26
**Host task:** kanban/to-do/010-emit-host-profile-generated-mediator-types-as-internal-to-avoid-cs0436-duplicates/
**Diff scope:** branch `task/010-emit-host-profile-generated-mediator-types-as-inte` vs `4f134c5` (master merge base), commits `7084678` (spec) and `e57df9c` (implementation)
**Plan / brief:** Emit every generated top-level type as `internal` in the Host, Aot, and Link profiles
(`Mediator`, `Sender_*` / `Publisher_*`, `MediatorManifest`, `GeneratedMediatorServiceCollectionExtensions`)
so a compilation that references two hosts sees no CS0436 duplicates and no ambiguous
`AddGeneratedMediator()` (CS0121). Add `tests/timewarp-mediator-generators-compilation-tests`, which drives
`MediatorGenerator` through `CSharpGeneratorDriver` for accessibility, a third host referencing two hosts, and
a plain consumer that cannot see `AddGeneratedMediator()`. Bump to 14.0.0-beta.4 with changelog and docs.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** cursor implementer-cursor review oracle (ganda task work, headless; no vendor session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
