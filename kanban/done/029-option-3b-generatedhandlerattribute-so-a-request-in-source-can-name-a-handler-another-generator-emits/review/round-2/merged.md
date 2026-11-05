# Round 2 — merged findings
**Date:** 2026-11-06
**Sources:** general
**Scope:** fix delta `b0c49a7..ae64cbd`

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-analyzers/handler-type-name.cs
- Description: A cross-assembly attribute was read from getter IL, which a reference assembly does not carry.
- Disposition notes: Fixed in `ae64cbd`. The contract is now a constructor parameter named `handlerTypeName`, and a default value counts. The IL and private-reflection path is deleted. TWM012 names the fix. Metadata-only image tests cover both the passing case and the getter-only case.

### M2 — Severity: bug — Status: fixed
- File: tests/timewarp-mediator-generators-compilation-tests/generated-handler-tests.cs
- Description: No test covered a handler emitted from `RegisterSourceOutput`.
- Disposition notes: Fixed in `ae64cbd`. `DeclaredHandler_FromRegisterSourceOutput_IsDispatched` (nested `CounterState.Handler`, Send returns `pong`). The post-init test is renamed as the dedupe case.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/message-graph-builder.handlers.cs
- Description: A declared type that is not an `IRequestHandler` produced only a bare CS error.
- Disposition notes: Fixed in `ae64cbd`. `DeclaredHandlerNotAHandler` reports TWM012 and names the interface. There are analyzer and compilation tests. A probe confirmed that a void `ActionHandler<T>` handler gets no false positive.

### M4 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/request-handler-analyzer.cs
- Description: A generator host built the full graph at compilation end on every build.
- Disposition notes: Fixed in `ae64cbd`. The `HasGeneratedHandlerAttribute` gate covers nested types too.

### M5 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-analyzers/handler-type-name.cs
- Description: Any first string constructor argument was taken as the name, and the named-argument branch was dead code.
- Disposition notes: Fixed in `ae64cbd`. Only `handlerTypeName` is read, and the dead branch is removed. Test: `DeclaredHandler_UnrelatedConstructorString_UsesTheGetter`.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-analyzers/diagnostic-descriptors.cs
- Description: Every TWM012 case used the missing-type message.
- Disposition notes: Fixed in `ae64cbd`. Each of the three new cases has its own descriptor under TWM012, and the generator maps `Problem` to the right one.

### M7 — Severity: nit — Status: fixed
- File: skills/tw-mediator/SKILL.md; agent.md
- Description: The skill said `[GeneratedHandler]`, and `agent.md` omitted the attribute and TWM012.
- Disposition notes: Fixed in `ae64cbd`.

## New findings this round

None. Two optional doc observations, the csproj `<Description>` TWM012 cases and the CS1061 note, are recorded in `general.md` under Observations. They are not findings.

## Duplicates / conflicts

- None. One reviewer.
