# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit f85c41c vs its parent (shape matrix, analyzer negatives, manifest JSON, Roslyn 4.8 floor leg, release tracking, struct null guard)

## Summary

The shape matrix compiles ten source shapes under Host and Aot and compares parsed manifest bindings, so a clean compile that drops a handler still fails. Analyzer negatives cover abstract, open-generic, and interface requests, handler-in-base, multi-request classes, notification fan-out, matching and inherited scopes, and `Send(object)`, in both library and host membership. TWM001 is pinned per orphan and TWM004 per wrong scoped send. `ManifestDocument` reads version, profile, namespace, requests, notifications, and membership; missing keys throw. Runtime generator tests now assert those fields. `PipelineLog` stores its list in `AsyncLocal`, and `Clear()` runs on the test's flow before the send, so dropping the xunit collection does not mix logs across parallel classes. The floor project links the compilation tests, overrides `Microsoft.CodeAnalysis.CSharp` to 4.8.0, and the loaded assembly version is 4.8. `EmitRequestNullGuard` skips value-type requests only; object and generic interface sends still null-check. TWM001–TWM004 moved to `AnalyzerReleases.Shipped.md` under `## Release 14.0.0` because RS2007 requires a `System.Version` header; each row notes `shipped in 14.0.0-beta.1`, and the analyzer project builds with that file. Minimum SDK text is in the readme and `generated-vs-legacy.md`. Checked against the emitter, the release files, the floor deps (`Microsoft.CodeAnalysis.CSharp/4.8.0`), and the test runs below. No defect found.

## Issues

No issues.
