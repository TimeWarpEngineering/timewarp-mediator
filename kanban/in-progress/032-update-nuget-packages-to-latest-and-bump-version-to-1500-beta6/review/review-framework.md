# Review framework

## Budget (by-diff)

- Lines changed: 169
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers do not edit product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the code that calls it. Check each claim that can be tested against the repo and nuget.org.
- Prior rounds do not change. New work goes in `round-(N+1)/`.
