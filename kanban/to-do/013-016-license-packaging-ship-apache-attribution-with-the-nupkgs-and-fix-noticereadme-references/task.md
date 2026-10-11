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
- [ ] Files packed; nuspec and nupkg contents inspected
- [ ] readme, notice, Copyright fixes
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

## Session

- Created: 686431 (2026-10-11)
