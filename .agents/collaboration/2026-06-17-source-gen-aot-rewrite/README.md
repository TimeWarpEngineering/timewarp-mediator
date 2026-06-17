# Collaboration Thread: Source-Gen / AOT Mediator Rewrite

**Date:** 2026-06-17  
**Topic:** TimeWarp.Mediator rewrite — source generators, Native AOT, linker architecture

## Documents in play

| File | Author | Role |
|------|--------|------|
| [`Analysis/2026-06-17-source-gen-aot-rewrite.md`](../../../Analysis/2026-06-17-source-gen-aot-rewrite.md) | Composer (initial) | Broad survey of ideas |
| [`Analysis/2026-06-17-source-gen-aot-rewrite-opus.md`](../../../Analysis/2026-06-17-source-gen-aot-rewrite-opus.md) | Opus | Vanishing mediator / linker thesis |

## Feedback in this thread

- [`composer-feedback-for-opus.md`](./composer-feedback-for-opus.md) — Composer's dissent and counter-proposal for Opus
- [`opus-response-for-composer.md`](./opus-response-for-composer.md) — Opus's reply: concedes 4½ of 5, sharpens the pipeline-scaling and DI points, defends "vanishing" as a profile name

## Decisions (resolved 2026-06-17)

| # | Decision | Resolution |
|---|----------|-----------|
| 1 | Default dispatch model | **Generated `Mediator` monomorph** default; interceptors = opt-in `CallSiteInlining` (Link profile) |
| 2 | Default DI | **MS.DI scope-resolved** default; ServiceGen static fields = AOT/Link profile; linker errors on scoped-into-static |
| 3 | Pipeline emission | **Composed-at-init delegate** default; inline weave via `[Inline]`/threshold |
| 4 | Call-graph pruning | **Off by default**; opt-in, manifest-logged, strict-mode-checked |
| 5 | M1 scope | Composer's revised M1 table — adopted (see opus response) |

Architectural center agreed: **the MessageGraph IR**, with `Mediator` + manifest as the always-on emitters and interceptors as an optional second emitter from the same graph. The opus analysis doc has been revised to match.

## Open / next

- Concrete M1 spec (generator stages, discovery + membership model, exact generated output, diagnostics catalog, test/benchmark harness).
- Scoped-sender (`ISender<TScope>`) emission design.
- Whether `TimeWarp.ServiceGen` leads or follows Mediator (Nuru tasks 443/444 sequencing).