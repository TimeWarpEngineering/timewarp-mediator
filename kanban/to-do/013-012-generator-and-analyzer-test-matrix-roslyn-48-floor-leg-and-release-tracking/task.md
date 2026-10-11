# Generator and analyzer test matrix, Roslyn 4.8 floor leg, and release tracking

## Description

Generator and analyzer tests are happy-path only in one assembly under the Host profile, and release tracking says no rule has shipped although four betas are tagged. Parent review task 013, findings G-16, G-20, G-23, G-24, B-16.

- G-23 (debt): no tests for Aot output beyond visibility, `MediatorModule`, `MediatorAssemblies`, profile/namespace overrides, TWM001-003 via the generator, duplicate handlers, multi-interface or derived handlers, nested/generic/internal/keyword-named types, notification ordering or base-type delivery, referenced-assembly members, unknown-scope `ArgumentException`, `CreateStream` `NotSupportedException`, null guards, incrementality. Analyzer tests have one positive case per rule and no location assertions.
- G-24 (debt): manifest assertions use exact substrings; static `PipelineLog` couples test order; analyzer tests use `Any(...)`; compilation tests reference Roslyn 5.9.0 so the 4.8.0 floor the packages declare is never exercised.
- G-16, G-20, B-16: `AnalyzerReleases.Shipped.md` is empty while v14.0.0-beta.1 to beta.4 exist; docs state no minimum SDK although 4.8.0 means SDK 8.0.100 / VS 17.8.

## Requirements

- Table-driven `HostCompiler` tests asserting zero diagnostics for a set of source shapes, plus analyzer negatives with location checks.
- Parse manifest JSON in tests instead of substring matching.
- One test leg (or CI job) that runs the generator against Microsoft.CodeAnalysis.CSharp 4.8.0.
- Move TWM001-004 to a `## Release 14.0.0-beta.1` section in Shipped.md; add the step to the release checklist.
- Document the minimum SDK in readme and `documentation/generated-vs-legacy.md`.

## Checklist

- [ ] Shape matrix tests
- [ ] Analyzer negative and location tests
- [ ] Manifest JSON parsing
- [ ] 4.8.0 compiler leg
- [ ] Release tracking files and minimum-SDK docs
- [ ] `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. Prefer landing before the incremental rewrite so it has a safety net.

## Session

- Created: 686431 (2026-10-11)
