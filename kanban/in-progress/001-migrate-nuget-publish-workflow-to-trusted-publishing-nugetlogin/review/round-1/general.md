# Round 1 — general
**Date:** 2026-08-08
**Scope reviewed:** commit 208d78c (ci-cd.yml, Overview.md, kanban checklist)

## Summary

Minimal, correct migration of NuGet.org publish from a long-lived `secrets.NUGET_API_KEY` to OIDC trusted publishing via `nuget/login@v1`. Job permissions are least-privilege (`contents: read`, `id-token: write`); login and push are both gated on `github.event_name == 'release'`; push uses `steps.nuget-login.outputs.NUGET_API_KEY` only. Matches the timewarp-nuru reference pattern and the task plan. Risk is low; remaining risk is operational (live release verify + secret/key revocation, correctly left unchecked).

## Issues

<!-- None. Implementation matches plan, reference workflow, and focus criteria. -->
