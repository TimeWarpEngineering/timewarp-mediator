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

- Orchestration: grok session (2026-08-08)
- Plan: plan agent 019fe06f-c7ac-7291-a4b9-5769481dfa26 (2026-08-08)
- Implementation: general-purpose 019fe072-6361-7043-b96c-79a160aab7cc (2026-08-08)
- Review round 1 (general): 019fe073-ae75-7d53-b1b8-6d88c82c58fe (2026-08-08)

### Progress (2026-08-08) — implementer

Implemented minimal OIDC migration in `.github/workflows/ci-cd.yml` job `build-and-publish`:
- Job `permissions: contents: read` + `id-token: write`
- Gated `nuget/login@v1` (`user: TimeWarp.Enterprises`, `id: nuget-login`) before publish
- Publish uses `steps.nuget-login.outputs.NUGET_API_KEY` (no `secrets.NUGET_API_KEY`)
- Docs: `Documentation/Overview.md` Setup updated for trusted publishing

Static verify: no `secrets.NUGET_API_KEY` in active workflow; login + id-token + steps.nuget-login present; both login and publish gated on `github.event_name == 'release'`.

Left unchecked: live release verification; secret/key revocation after success.

### Progress (2026-08-08) — review

Folderized task; Phase 4b effort 1 under `review/`. Round 1 general: 0 findings. Disposition: **clean**.

## Results

### What was implemented

Minimal migration of the active NuGet publish path to **trusted publishing (OIDC)** so the existing NuGet.org policy for this repo becomes active:

1. Job `build-and-publish` permissions: `contents: read` + `id-token: write`
2. Gated `nuget/login@v1` (`id: nuget-login`, `user: TimeWarp.Enterprises`) before push
3. `dotnet nuget push` uses `${{ steps.nuget-login.outputs.NUGET_API_KEY }}` (no long-lived secret in the active workflow)
4. Docs Setup in `Documentation/Overview.md` updated for OIDC trusted publishing

**Decision:** minimal in-place migration — full 458 reusable-workflow conversion is not imminent for this nonconformer repo.

### Files changed

| File | Change |
|------|--------|
| `.github/workflows/ci-cd.yml` | OIDC permissions, login step, API key source |
| `Documentation/Overview.md` | Setup: trusted publishing instead of `NUGET_API_KEY` secret |
| `kanban/.../001-.../` | Plan, checklist, session, review kitchen, Results |

Implementation commit: `208d78c`

### Key decisions / deviations

- No deviations from the plan
- Did not touch `*.disabled` workflows, MyGet, version/release, or invent `workflow_dispatch`
- Secret revocation deliberately deferred until after a successful OIDC publish (operator / nuru 458-009)

### Test outcomes

| Check | Result |
|-------|--------|
| Static: no `secrets.NUGET_API_KEY` in `ci-cd.yml` | pass |
| Static: `nuget/login`, `id-token`, `steps.nuget-login` present | pass |
| Login + publish both `if: github.event_name == 'release'` | pass |
| Live OIDC publish on a real release | **not run** (requires next release with new version) |

### Review (Phase 4b)

- **Effort:** 1 (general only)
- **Rounds:** 1
- **Roster:** general
- **Final counts:** 0 open / 0 fixed / 0 wontfix (all severities)
- **Disposition:** `clean`
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

### Residual operator work (not blocked on implementer)

Checklist items still open by design:

1. Verify publish path on the **next** real GitHub Release (new version past published `13.0.0`)
2. **After** that success: revoke long-lived NuGet API key(s) and delete GitHub secret `NUGET_API_KEY` (org-wide: nuru 458-009). Do not delete the secret before a successful OIDC publish.

### How to validate

**Smoke (static, any machine with this checkout)**

```bash
cd /path/to/timewarp-mediator
rg 'secrets\.NUGET_API_KEY' .github/workflows/ci-cd.yml   # expect: no matches
rg -n 'nuget/login|id-token|steps\.nuget-login' .github/workflows/ci-cd.yml
# expect: id-token: write; nuget/login@v1; steps.nuget-login.outputs.NUGET_API_KEY
rg -n 'if: github.event_name == .release.' .github/workflows/ci-cd.yml
# expect: both NuGet login and Publish steps gated
```

**Expect (static):** empty secret reference; login step before publish; job has `contents: read` + `id-token: write`; docs Setup describes OIDC / `ci-cd.yml` policy.

**Preflight (NuGet.org UI):** trusted-publishing policy for owner `TimeWarp.Enterprises` / repo `TimeWarpEngineering/timewarp-mediator` targets workflow **`ci-cd.yml`** (and no Environment restriction the job does not use).

**Live smoke (next release — operator)**

1. Bump `<Version>` in `Directory.Build.props` past currently published version; tag/create GitHub Release (`release: published`).
2. Open the Actions run for “NuGet Publish”.
3. Expect: step **NuGet login (OIDC Trusted Publishing)** succeeds; **Publish to NuGet.org** pushes both `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` without using a long-lived secret.
4. Confirm packages on nuget.org for the new version.

**Automated gate:** none beyond static `rg` above (no unit tests for GitHub Actions YAML).

**Depends on:** merge of the workflow change to the default branch before the release runs it; NuGet.org policy already present for this repo.

**Not in scope for this session:** cutting a release solely to test; deleting `NUGET_API_KEY` or revoking NuGet keys before OIDC success; full 458 reusable-workflow conversion.
