# Review framework — task 013-002

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-002-harden-serviceregistrar-remove-static-configuration-and-fix-generic-handler-edge-cases/
**Diff scope:** commit `b9017cc` on `task/013-002-harden-serviceregistrar-remove-static-configuratio` (parent `2a4dd6c`, which already contains 013-001). Product files: `source/timewarp-mediator/registration/service-registrar.cs`, `source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs`, `tests/timewarp-mediator-tests/generic-request-handler-tests.cs`, `tests/timewarp-mediator-tests/assembly-info.cs`.
**Plan / brief:** Harden `ServiceRegistrar` for parent findings C-4 through C-7. Pass `MediatorServiceConfiguration` instead of mutable statics. Guard closed non-generic requests before `GetGenericTypeDefinition`. Match handler interfaces by generic definition and arguments. Catch only `ArgumentException` around `MakeGenericType`, delete the dead non-interface branch, and scan candidate closing types once. Tests for the C-5 and C-6 shapes.
**Effort:** 2 (general only; host roster axes: general)
**Reviewer roster:** general
**Session IDs:** review oracle opencode session `ses_ed3cf5905ffeg80qfPyIfj4epL` (2026-10-12). General review written in this session; no separate reviewer subagent.

## Budget (by-diff)

- Lines changed: 299
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
