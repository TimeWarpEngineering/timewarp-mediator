# 14.0.0-beta source-gen Mediator not 13.0.0

## Description

M1/M2 (epic **004**) are on git `master` still labeled **13.0.0**. NuGet **13.0.0** is the **reflection MediatR fork**. Do **not** publish the source-gen rewrite as 13.0.0.

This epic cuts **14.0.0-beta.1** (prerelease) so State and Nuru can soak it. **14.0.0 stable is not this epic** — file that only after State (and Nuru 443) have actually run on the beta.

Coexistence stays: `AddMediator()` = legacy reflection; `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` = generated. Do not delete the fork runtime here.

## Children

- **005-001** Version + pack `14.0.0-beta.1` (Contracts, Mediator, Analyzers, Generators)
- **005-002** Consumer docs (generated vs legacy)
- **005-003** Publish NuGet **prerelease** (depends on 005-001 **and 006-005**)

## Requirements

- Version is `14.0.0-beta.1` (or the next unused 14.0.0-beta.n). Never 13.0.0.
- Four packages pack; Analyzers/Generators have no empty snupkg (004-001 NU5017).
- 13.0.0 on NuGet remains the last reflection line.
- No `14.0.0` (non-beta) tag or push on this epic.

## Out of scope

- TimeWarp.State switch (State epic, after 005-003)
- Nuru 443 consume
- Deleting `AddMediator()` / wrappers / scanning (Phase D)
- Interceptors, pruning, streams

## Notes

- Spec said major version / clean break. Beta is the test vehicle; stable waits for consumers.
- Cross-repo: State **080**; Nuru **443**.
- **006** (TimeWarp repo conformance) is the missing piece: this tree is still a MediatR fork. Do **not** run **005-003** until **006-005** (`ganda repo audit` error-green). Add `## Depends on 006-005` on 005-003 when 005-001 is merged (claim currently blocked on 005-001). 005-001 pack may land on the current layout; NuGet beta does not.

## Session

- Created: 150754 (2026-09-01)
- Updated: 162284 (2026-09-01) — 005-003 gated on 006-005
