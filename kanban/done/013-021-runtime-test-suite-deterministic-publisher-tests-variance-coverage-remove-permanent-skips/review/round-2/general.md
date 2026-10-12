# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** M1 fix in `tests/timewarp-mediator-tests/variance-tests.cs` and the D-20 sentence in `task.md`. Prior publisher, timeout, skip, counter, and rename checks were not re-opened.

## Summary

M1 is fixed. The file summary now states the registrar rules that the tests actually lock: notification base handlers run when a derived handler is scanned with them, and a derived request reaches a base handler only through an explicit closed registration. The request test is renamed to match that registration. `VarianceTests` passed 5/5 after the edit. No new defects on the fix delta.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-mediator-tests/variance-tests.cs:3
- Description: Resolved. See round 2 merged M1.
- Suggestion: None remaining.
- Status: fixed
