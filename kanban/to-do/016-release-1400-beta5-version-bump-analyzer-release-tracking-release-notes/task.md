# Release 14.0.0-beta.5: version bump, analyzer release tracking, release notes

## Description

Cut `14.0.0-beta.5` so the 22 repository-review children (kanban 013, PR #95) reach consumers as a
prerelease before `14.0.0` stable. State, Architecture and Nuru have only run against `14.0.0-beta.4`;
master since then changes all four packages (net10.0 floor, smaller runtime API, generator binding and
diagnostics rewrite, TWM005-TWM011). Maintainer decision 2026-10-13: beta.5 first, dogfood, then 14.0.0.

Follows `documentation/overview.md` "Cutting a release".

## Requirements

- `<Version>` in `source/Directory.Build.props` is `14.0.0-beta.5`.
- `AnalyzerReleases.Unshipped.md` holds only its header; TWM005-TWM011 are in `AnalyzerReleases.Shipped.md`
  under `## Release 14.0.0` with "shipped in 14.0.0-beta.5".
- Release notes: the "Unreleased" section becomes `## 14.0.0-beta.5`.
- PR merged with a merge commit, push CI green, then `./bin/dev release --dry-run` and `./bin/dev release`
  from synced origin-home master.

## Checklist

- [x] Version bump to 14.0.0-beta.5
- [x] Analyzer release tracking rows moved (TWM005-TWM011)
- [x] Release notes section retitled; overview analyzer-tracking note updated
- [x] Build/pack green locally (RS2000-RS2008 release tracking validates)
- [ ] PR merged (merge commit), push CI green
- [ ] `dev release --dry-run` clean; `dev release` tags v14.0.0-beta.5 and the publish job pushes all four packages
- [ ] nuget.org shows 14.0.0-beta.5 for Contracts, Mediator, Generators, Analyzers

## Session

- Created: 981705 (2026-10-13)
- Implement: Claude (timewarp-mediator cockpit session), 2026-10-13

## Notes

- Next: bump State, Architecture and Nuru to beta.5 and run their suites; then 14.0.0 (task to be created). Nuru 489 waits on the stable.
- Stale issues #63, #64, #65 were fixed by beta.4 (generated types internal); close them when convenient. #52 closes at 14.0.0.
