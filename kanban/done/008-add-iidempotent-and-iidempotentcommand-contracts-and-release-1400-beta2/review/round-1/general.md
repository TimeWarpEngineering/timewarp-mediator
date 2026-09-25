# Round 1 — general
**Date:** 2026-09-25
**Scope reviewed:** same as framework (branch vs `7f18058`)

## Summary

The change adds the idempotency contracts as pure marker interfaces layered on `ICommand` / `ICommand<T>`,
with handler interfaces constrained to the idempotent command types, and makes `IQuery<T>` implement
`IIdempotent`. All five new types are type-forwarded from `TimeWarp.Mediator` like the existing contracts.
Neither the generator nor the analyzer classifies messages by interface name (both key on
`IRequestHandler<>` / `IRequestHandler<,>`), so dispatch is unchanged; generator tests (registration,
`Send`, `Send(object)`, manifest) and reflection `Mediator` tests confirm it. Risk is low.

Verified in the worktree: `./bin/dev workflow` → Pipeline SUCCEEDED (generators 26 passed, analyzers 6
passed, mediator 165 passed / 2 skipped; four `14.0.0-beta.2` nupkgs, analyzer/generator layout asserts
pass). `ganda repo audit` → passes all checks. Published nuspec dependencies for `TimeWarp.Mediator` and
`TimeWarp.Mediator.Contracts` are unchanged (no SourceLink / `TimeWarp.SourceGenerators` / `System.IO.Hashing`
leak; both are `PrivateAssets="all"`).

## Issues

### Issue 1 — Severity: nit
- File: Directory.Packages.props:13
- Description: `Microsoft.SourceLink.GitHub` 10.0.401 brings `System.IO.Hashing` 10.0.12 as a build-time
  dependency, whose buildTransitive targets emit "doesn't support net6.0" warnings when packing the
  `net6.0` target of `source/timewarp-mediator`. Not fatal and not in the package's dependency groups;
  log noise only.
- Suggestion: Leave as is, or set `SuppressTfmSupportBuildWarnings` on `source/timewarp-mediator` if the
  noise matters; dropping the EOL `net6.0` target would remove it for good.
- Status: open
