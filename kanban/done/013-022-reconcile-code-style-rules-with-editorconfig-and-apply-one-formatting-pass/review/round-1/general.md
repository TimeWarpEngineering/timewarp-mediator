# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `89e09c5` vs `5016054`. Whitespace-only reindent was sampled; the non-whitespace delta (style config, field renames, header decision) was read in full. `dotnet format whitespace --verify-no-changes` and `dotnet format style --diagnostics IDE1006` were run against the tree.

## Summary

The decision matches C-25 and the style half of D-21: `.editorconfig` and `agent.md` now both say 2-space C# indentation, LF, no trailing whitespace, and PascalCase fields. Underscore-prefixed fields are gone, including the runtime fields named in C-25, and the MediatR "Modified by" headers stay as the Apache-2.0 §4(b) notice. Generator and analyzer sources are indent-only outside string literals, so emitted code is unchanged. Two gaps remain: the documented whitespace pass is not clean, and the new naming rules are already violated by fields and locals the rename did not touch.

## Issues

### Issue 1 — Severity: bug
- File: tests/timewarp-mediator-tests/publish-tests.cs:52
- Description: `dotnet format whitespace --folder --include source/ tests/ samples/ --verify-no-changes` still reports whitespace fixes in 13 files. Lambda bodies under `cfg.Scan(scanner => { ... })` (and the same shape in pipeline, stream, send, and exception tests, plus `samples/timewarp-mediator-examples-lamar/program.cs`) sit at the brace column. The formatter inserts two spaces on those lines. The pass described in Results did not leave the tree clean.
- Suggestion: Re-run that whitespace command and keep the result. It is indentation only.
- Status: open

### Issue 2 — Severity: bug
- File: .editorconfig:90
- Description: The new naming rules do not match the code. `dotnet format style --diagnostics IDE1006` reports 21 violations. Six test fields named `logger` (`IBlogger<T>`) stay camelCase in `service-collection-pipeline-tests.cs` (they never had an underscore, so the rename skipped them). The shipped deconstruction `foreach ((Type? Service, Type? Implementation) in registrationTypes)` in `service-registrar.cs:321` is PascalCase locals, which the new local rule rejects and which `TreatWarningsAsErrors` would surface as errors once style analysis runs. Twelve method-local `const string Source` values and one `const string Bindings` are the same local-rule miss.
- Suggestion: Rename the `IBlogger<T>` fields to `Blogger`. Rename the deconstruction locals and the method-local consts to camelCase. Leave tuple element names on return types PascalCase; IDE1006 does not flag those.
- Status: open
