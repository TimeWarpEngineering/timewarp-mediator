# License packaging: ship Apache attribution with the nupkgs and fix notice/readme references

## Description

The packages derive from Apache-2.0 MediatR code but ship only `PackageLicenseExpression=Unlicense`, the logo, and the readme. Parent review task 013, findings B-1, B-2. Flagged factually; the maintainer decides.

- B-1 (risk): `notice` (Apache text plus fork header) and `unlicense` are never packed. Apache 2.0 section 4 asks redistributors to include a copy of the license.
- B-2 (debt): the readme License section refers to `UNLICENSE` and `NOTICE` but the files are lowercase; the notice Appendix still reads "Copyright {yyyy} {name of copyright owner}"; `Copyright` in the root props has no year.

## Requirements

- Decision (Steven, 2026-10-11): keep the Apache attribution. No renames or rewrites to shed it. TimeWarp-authored code stays under the Unlicense; the MediatR-derived portions keep their Apache 2.0 notice. Pick the license expression that states that honestly (for example `Unlicense AND Apache-2.0`) or keep `Unlicense` with the packed NOTICE making the Apache portion explicit; record which in Results.
- Pack `notice` and `unlicense` into every nupkg.
- Fix readme file references, fill the notice Appendix with the upstream copyright line and year, add a year to the Copyright property.
- Compare the notice text with upstream MediatR LICENSE/NOTICE and record the result in Notes.

## Checklist

- [x] Decision recorded (keep attribution; see Requirements and Notes)
- [x] Files packed; nuspec and nupkg contents inspected
- [x] readme, notice, Copyright fixes
- [ ] PR with merge commit

## Notes

Parent: 013. Not legal advice; the finding records what the packages contain.

### Lineage measurement (2026-10-11)

Compared against the upstream commit the fork left from (`8a64581`, Jimmy Bogard, 2025-03-24) with the namespace rename and `Mediatr`→`Mediator` normalized away:

| Package | Normalized lines in files that came from upstream | Lines in new TimeWarp files |
|---|---|---|
| TimeWarp.Mediator (runtime) | 1,760 | 27 |
| TimeWarp.Mediator.Contracts | 329 | 280 |
| TimeWarp.Mediator.Generators | 0 | 773 |
| TimeWarp.Mediator.Analyzers | 0 | 1,110 |

1,807 of 2,115 upstream lines are still present verbatim (85%). The runtime package and the MediatR-named contracts (`IRequest`, `INotification`, `IRequestHandler`, `IPipelineBehavior`, `ISender`, `IPublisher`, `IMediator`, `Unit`, processors, exception handlers) are derived work. Generators, Analyzers, and the new contracts (`ICommand`, `IQuery`, `IAction`, `IIdempotent*`, the `Mediator*` attributes, `NoHandlerException`) are TimeWarp-original. The runtime test suite is a port of MediatR's tests.

Implication for packaging: Generators and Analyzers nupkgs can carry Unlicense alone. Mediator and Contracts nupkgs must carry the Apache notice. Decide whether to pack the notice into all four for simplicity or only the two that need it; record the choice.

### Upstream notice comparison (2026-10-12)

Compared the Apache text in `notice` (below the fork header) with `LICENSE` at upstream `8a64581` (`git show 8a64581:LICENSE`). Upstream ships `LICENSE` only; it has no `NOTICE` file, so there is no upstream NOTICE text to carry forward. Upstream package metadata: `Copyright Jimmy Bogard`, `PackageLicenseExpression=Apache-2.0`; the `LICENSE` file dates from the 2014 initial commit.

Differences found before this task:

- Section 1 "Object" definition read "translation of Source form"; upstream reads "translation of a Source form". Restored to upstream wording.
- Appendix copyright line was the `{yyyy} {name of copyright owner}` placeholder (same as upstream). Filled with `Copyright 2014-2025 Jimmy Bogard`.
- Whitespace only: separator line above the Apache text and no trailing blank line.

After the fix, `diff -w` against upstream shows only the filled Appendix line and the trailing blank line.

## Results

### License expression decision

- `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts`: `Unlicense AND Apache-2.0` (set per csproj). Both contain MediatR-derived code (see lineage measurement), so the expression states that consumers take Apache-2.0 terms for that portion.
- `TimeWarp.Mediator.Generators` and `TimeWarp.Mediator.Analyzers`: stay `Unlicense` (root `source/Directory.Build.props`). They are TimeWarp-original.
- `notice` and `unlicense` are packed into all four nupkgs for simplicity and so every package explains the split.

### Changes

- `source/Directory.Build.props`: pack `notice` and `unlicense` at the package root; `Copyright` now `Copyright 2014-2025 Jimmy Bogard (original MediatR, Apache-2.0); 2025-2026 TimeWarp Engineering (Unlicense)`.
- `source/timewarp-mediator/timewarp-mediator.csproj`, `source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj`: `PackageLicenseExpression` `Unlicense AND Apache-2.0`.
- `notice`: fork header year, `see unlicense` (lowercase), Appendix filled, upstream wording restored.
- `readme.md`: License section refers to `unlicense` and `notice`.
- `tools/dev-cli/services/repo-layout.cs`: `dev pack` layout gate now asserts `notice` and `unlicense` in all four nupkgs, plus the `lib/net10.0` dll for Mediator and Contracts.

### Validation

`./bin/dev workflow` passed: build, all test projects (455 passed, 2 skipped), pack, and "Package layout verified" for all four nupkgs. Inspected each nuspec `<license>` and `<copyright>`; `unzip -p` of the packed `notice` is byte-identical to the repo file.

One run first failed with `CS8784 TypeLoadException ... MembershipOptions` from a stale compiler server holding an old Analyzers assembly; `dotnet build-server shutdown` cleared it. Not related to this change.

### How to validate

**Smoke:**

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
dotnet build-server shutdown
./bin/dev workflow
for n in artifacts/packages/*.nupkg; do echo "$n"; unzip -l "$n" | grep -E ' (notice|unlicense)$'; unzip -p "$n" '*.nuspec' | grep '<license'; done
```

**Expect:**

- Workflow ends with `Package layout verified:` for TimeWarp.Mediator, .Contracts, .Analyzers and .Generators.
- Every nupkg lists `notice` and `unlicense` at the root.
- Mediator and Contracts nuspecs show `<license type="expression">Unlicense AND Apache-2.0</license>`; Analyzers and Generators show `Unlicense`.

## Session

- Created: 686431 (2026-10-11)
