# Round 1 — general
**Date:** 2026-09-02
**Scope reviewed:** branch `task/005-002-docs-generated-vs-legacy-addmediator` vs `origin/master` (product commit `846f731`: `readme.md`, `documentation/generated-vs-legacy.md`, `documentation/m1-generated-mediator.md`, `documentation/m2-named-pipelines.md`, `migration.md`)

## Summary

Docs-only change that makes the two-stack contract explicit for hosts: `AddMediator()` remains the 13.x reflection fork; `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` are the generated path. README, the new `generated-vs-legacy.md` SSOT, m1/m2, and `migration.md` all carry the required membership, package, marker-pipeline, and **14.0.0-beta is not a drop-in for 13.0.0** guidance, and issue #52 is correctly left open. Overall risk is low; re-checked claims against generators, contracts, samples, and package props match the text.

## Issues
