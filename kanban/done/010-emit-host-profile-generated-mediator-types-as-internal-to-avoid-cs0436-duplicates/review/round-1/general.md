# Round 1 — general
**Date:** 2026-09-26
**Scope reviewed:** same as framework

## Summary

The change flips the four top-level emits (`EmitDispatcher` class line, `EmitRegistration`, and
`ManifestEmitter`) from `public` to `internal`; `ServiceGen` was already internal. A search of the generator
for emitted type declarations finds no remaining `public` top-level type. Members stay `public`, which is
required for interface implementation and for `Dispatch_*` interceptor targets in the same assembly. The new
compilation tests cover every requirement across all three profiles, and the negative check recorded in
Results (CS0436 / CS0433 / CS0121 with public types) shows they detect the regression. Risk is low. One
documentation gap.

## Issues

### Issue 1 — Severity: nit
- File: readme.md:155, documentation/generated-vs-legacy.md:38
- Description: The docs say a project that references two hosts sees no duplicate generated types. That is
  false when both hosts grant the project `InternalsVisibleTo`, which is common for test projects: the
  internal types become visible again and CS0433 / CS0436 / CS0121 return.
- Suggestion: Add a one-line caveat about `InternalsVisibleTo` in the readme paragraph and the beta.4
  changelog.
- Status: open
