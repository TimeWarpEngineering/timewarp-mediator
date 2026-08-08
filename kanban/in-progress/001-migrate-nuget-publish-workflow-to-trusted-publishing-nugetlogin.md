# Migrate NuGet publish workflow to trusted publishing (nuget/login)

## Description

The trusted publishing policy for this repo already exists on NuGet.org
(owner TimeWarp.Enterprises, created 2026-08-08) but is INERT until the
publish workflow exchanges an OIDC token for a temp key instead of using a
stored secret. Org program context: timewarp-nuru kanban 458-009.

Current state (2026-08-07 org audit): secret `NUGET_API_KEY` in ci-cd.yml (repo carries 7 workflows total — legacy MediatR-fork tooling); publishes TimeWarp.Mediator + TimeWarp.Mediator.Contracts.

Reference implementation: timewarp-nuru `.github/workflows/workflow.yml` —
`nuget/login@v1` step (user: TimeWarp.Enterprises) gated on the release
condition, `id-token: write` job permission, push via
`--api-key ${{ steps.nuget-login.outputs.NUGET_API_KEY }}`.

NOTE: if this repo's full convention conversion (reusable-workflow caller,
timewarp-nuru 458 rollout) is imminent, do the conversion instead — it
includes this migration for free.

## Checklist

- [x] Add `id-token: write` (with `contents: read`) permissions to the publish job
- [x] Add `nuget/login@v1` gated on the publish condition
- [x] Replace the stored-secret `--api-key` with the login step output
- [ ] Verify the publish path end-to-end on the next release
- [ ] AFTER verified: operator revokes the long-lived NuGet key and deletes the GitHub secret (org-wide revocation tracked in nuru 458-009)

## Notes

Created from the timewarp-nuru 458-009 rollout session (2026-08-08).

### Implementation plan (2026-08-08)

**Decision:** minimal migration of existing publish job — full 458 reusable-workflow conversion is NOT imminent for this repo (mediator is a nonconformer in wave 4 of 458; no reusable workflow host ready).

#### Target end state
On `release: published`:
1. Job has `contents: read` + `id-token: write`
2. `nuget/login@v1` with `user: TimeWarp.Enterprises` (OIDC → short-lived key)
3. `dotnet nuget push` uses `${{ steps.nuget-login.outputs.NUGET_API_KEY }}`
4. No `secrets.NUGET_API_KEY` in the active workflow

#### Files
- **Required:** `.github/workflows/ci-cd.yml` job `build-and-publish`:
  - A. Add job `permissions: contents: read` + `id-token: write`
  - B. Insert gated `nuget/login@v1` (id: nuget-login, user: TimeWarp.Enterprises) before publish step, `if: github.event_name == 'release'`
  - C. Replace `--api-key ${{ secrets.NUGET_API_KEY }}` with `steps.nuget-login.outputs.NUGET_API_KEY`
- **Optional:** `Documentation/Overview.md` if it still documents configuring NUGET_API_KEY secret
- **Out of scope:** *.disabled workflows, Push.ps1, MyGet, full 458 conversion, secret/key revocation (operator after verify / nuru 458-009)

#### Order
1. Preflight: confirm NuGet.org trusted-publishing policy for this repo binds to workflow `ci-cd.yml` / owner TimeWarp.Enterprises
2. Implement YAML edits (+ optional docs)
3. Static verify: no secrets.NUGET_API_KEY in active workflow; login+permissions present; both steps gated on release
4. Merge to master
5. Live verify on next real release (new version past 13.0.0)
6. AFTER success: operator revokes long-lived key + deletes GitHub secret (org-wide 458-009)

#### Verify now vs later
- Now: YAML static checks; policy filename match
- Only on release: OIDC login success + actual package push
- After OIDC success: revoke secret

Reference: timewarp-nuru `.github/workflows/workflow.yml`

## Session

Orchestration: grok session (2026-08-08)

### Progress (2026-08-08) — implementer

Implemented minimal OIDC migration in `.github/workflows/ci-cd.yml` job `build-and-publish`:
- Job `permissions: contents: read` + `id-token: write`
- Gated `nuget/login@v1` (`user: TimeWarp.Enterprises`, `id: nuget-login`) before publish
- Publish uses `steps.nuget-login.outputs.NUGET_API_KEY` (no `secrets.NUGET_API_KEY`)
- Docs: `Documentation/Overview.md` Setup updated for trusted publishing

Static verify: no `secrets.NUGET_API_KEY` in active workflow; login + id-token + steps.nuget-login present; both login and publish gated on `github.event_name == 'release'`.

Left unchecked: live release verification; secret/key revocation after success.
