# License packaging: ship Apache attribution with the nupkgs and fix notice/readme references

## Description

The packages derive from Apache-2.0 MediatR code but ship only `PackageLicenseExpression=Unlicense`, the logo, and the readme. Parent review task 013, findings B-1, B-2. Flagged factually; the maintainer decides.

- B-1 (risk): `notice` (Apache text plus fork header) and `unlicense` are never packed. Apache 2.0 section 4 asks redistributors to include a copy of the license.
- B-2 (debt): the readme License section refers to `UNLICENSE` and `NOTICE` but the files are lowercase; the notice Appendix still reads "Copyright {yyyy} {name of copyright owner}"; `Copyright` in the root props has no year.

## Requirements

- Maintainer decision on the license expression (for example `Unlicense AND Apache-2.0`) or keeping Unlicense with a packed NOTICE.
- Pack `notice` and `unlicense` into every nupkg.
- Fix readme file references, fill the notice Appendix with the upstream copyright line and year, add a year to the Copyright property.
- Compare the notice text with upstream MediatR LICENSE/NOTICE and record the result in Notes.

## Checklist

- [ ] Decision recorded
- [ ] Files packed; nuspec and nupkg contents inspected
- [ ] readme, notice, Copyright fixes
- [ ] PR with merge commit

## Notes

Parent: 013. Not legal advice; the finding records what the packages contain.

## Session

- Created: 686431 (2026-10-11)
