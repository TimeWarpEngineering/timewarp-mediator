# Round 1 — general
**Date:** 2026-11-22
**Scope reviewed:** branch `task/044-test-generated-request-pattern-with-contracts-and` vs merge-base `8dd270fd`. Product files: `source/timewarp-mediator-analyzers/request-pattern.type-names.cs` (shared into the generators project), the referenced-assembly tests and the `Load` companion change, version pins and `documentation/release-notes/15.0.0-beta.11.md`, and the readme / `documentation/architecture.md` paragraph. Checked against the original checklist and `## Goal (implementer restatement)`.

## Summary

The restatement matches the original task. No misread. The new tests put contracts in `Web.Contracts`, the derived `[ContractState]` const templates in a metadata-only `State.Attributes` image, and a two-arity `OneOf` stand-in in its own assembly. The host only applies the attribute. `Send` round-trips both OneOf arms, a referenced `TimeWarp.State.IAction` behavior runs, and a host behavior constrained on `TimeWarp.Mediator.IAction` does not. TWM013 fires when `{ContractType}.Response` does not resolve, and the request-missing TWM013 names the referenced contract.

The duplicate-name probe found a real miss: `Compilation.GetTypeByMetadataName` returns null when an inaccessible type shares the metadata name. `ByMetadataName` falls back to `GetTypesByMetadataName` and keeps the single type `IsSymbolAccessibleWithin` allows from the host assembly. Two accessible types stay unresolved. That note is in the Design region. The version bump to `15.0.0-beta.11` matches a generator and analyzer change. Recorded gates cover `./bin/dev build`, the `GeneratedRequestPatternTests` filter (18/18, including the five new tests and the thirteen existing ones that share `Load`), `./bin/dev check-version`, and `ganda repo audit`. Those gates were not re-run.

## Issues

<!-- None. -->
