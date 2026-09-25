# Review framework — task 009

**Date:** 2026-09-25
**Host task:** kanban/to-do/009-fix-generator-unit-response-and-array-response-handler-discovery-68-release-1400-beta3/
**Diff scope:** branch `task/009-fix-generator-unit-response-and-array-response-han` vs `675b58c` (master merge base), commits `780101d` (spec) and `f4f5a53` (implementation)
**Plan / brief:** Apply the #68 fixes to `MessageGraphBuilder` and the emitter: take void dispatch from the
matched `IRequestHandler<T>` interface instead of `TResponse == Unit`, so an explicit `IRequestHandler<T, Unit>`
registers and dispatches as the two-arity handler (was CS0311). Widen response types to `ITypeSymbol` through
discovery, behavior closing, `RequestBinding`, and `MediatorEmitter.Fq`, so `IQuery<T[]>` handlers are
discovered (was TWM001). Add generator tests for explicit-Unit, void, array, and `List<T>` responses and a
closed behavior over `string[]`. Bump to 14.0.0-beta.3 with a changelog entry that references #68.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** cursor implementer-cursor review oracle (ganda task work, headless; no vendor session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
