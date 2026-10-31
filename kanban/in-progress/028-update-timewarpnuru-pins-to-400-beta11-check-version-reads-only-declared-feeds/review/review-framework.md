# Review framework

## Budget (by-diff)

- Lines changed: 154
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers do not edit product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Every new finding starts open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Check every falsifiable claim against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
