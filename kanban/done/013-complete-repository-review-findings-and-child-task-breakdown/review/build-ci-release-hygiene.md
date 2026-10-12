# Review findings: build, CI, packaging, release, hygiene
Worktree: task-013-complete-repository-review-findings-and-child-task (read-only; no dotnet build/test run)

### B-1: Apache 2.0 attribution is not shipped in the NuGet packages
- Severity: risk
- Area: Directory.Build.props:5-17, source/Directory.Build.props:12-24, notice
- Evidence: Packages declare only `PackageLicenseExpression=Unlicense` (source/Directory.Build.props:13); the only packed files are assets/logo.png and readme.md (source/Directory.Build.props:21-22). `notice` (Apache text plus a header saying MediatR code is Apache-2.0) is never packed.
- Impact: The nupkgs derive from Apache-2.0 code yet carry neither the Apache license text nor the NOTICE; Apache section 4 asks redistributors to include a copy of the license. Flagged factually, not legal advice.
- Proposed fix: Pack `notice` (and `unlicense`) into each package, and consider a `PackageLicenseFile`/expression that reflects both licenses (e.g. `Unlicense AND Apache-2.0`) after the owner decides.
- Child task: yes

### B-2: notice file is only a header plus the stock Apache text; no MediatR copyright line in the Appendix and filename mismatch with readme
- Severity: debt
- Area: notice, readme.md (License section, last lines), Directory.Build.props:8
- Evidence: notice opens with "Copyright Jimmy Bogard" and the fork statement, followed by the unmodified Apache template (Appendix still reads "Copyright {yyyy} {name of copyright owner}"). readme.md License section refers to `UNLICENSE` and `NOTICE`, but files are lowercase `unlicense` and `notice` (renamed in c074fe3, 2026-09-02). Directory.Build.props:8 copyright string has no year.
- Impact: On case-sensitive hosts the readme references do not resolve, and the notice has no year-qualified upstream copyright line. Original MediatR LICENSE/NOTICE text was not verified against upstream here.
- Proposed fix: Fix the readme file names (or links), add the upstream copyright line with year as in MediatR's license, and add a year to the Copyright property.
- Child task: no (fold into B-1)

### B-3: Version is defined in two files and the docs describe a third mechanism (MinVer)
- Severity: debt
- Area: Directory.Build.props:24, source/Directory.Build.props:6, tools/dev-cli/endpoints/workflow-command.cs:196-222 (AssertVersionSsot), documentation/overview.md
- Evidence: Both files set `<Version>14.0.0-beta.4</Version>`. Workflow release path asserts they are equal (workflow-command.cs Design region: "Until 006-003..."). documentation/overview.md says "Source of Truth: `<Version>` in `Directory.Build.props`" and describes MinVer tags and -alpha.N CI builds; no MinVer reference exists in any props/csproj.
- Impact: Every bump needs two edits and the drift check only runs on the release path, not on PRs; the documented release flow is not the real one.
- Proposed fix: Keep a single Version (root props), delete the duplicate in source/Directory.Build.props, and run the check in the PR pipeline until then.
- Child task: yes

### B-4: documentation/overview.md (release process) is wholly stale
- Severity: bug
- Area: documentation/overview.md, analysis/release-process-analysis.md, analysis/nuget-publishing-analysis.md
- Evidence: overview.md references `.github/workflows/ci-cd.yml` (real file is workflow.yml, renamed in kanban 002), MyGet feeds and `MYGET_TIMEWARP_MEDIATOR_CI_API_KEY`, MinVer, "both packages" (there are four), version examples 12.x, and `build-and-publish` job (actual job is `ci`). agent.md:113-115 points readers to this file as the release process. analysis/release-process-analysis.md is dated 2025-07-05 and analyses the removed PowerShell scripts.
- Impact: Anyone following the documented release process will do the wrong thing; the real process lives in the tw-release skill, workflow.yml and kanban 001-003.
- Proposed fix: Rewrite overview.md to the actual flow (version bump PR, GitHub Release, OIDC publish, probe and break-glass modes) and move the 2025 analysis files to kanban archive or delete them.
- Child task: yes

### B-5: No release notes for the 14.0.0 line; v13 notes sit at the repo root
- Severity: debt
- Area: release-notes-v13.0.0.md, documentation/
- Evidence: Only release-notes-v13.0.0.md exists (added a303fc2, 2025-08-04). No file for 14.0.0-beta.1..4, although four betas have shipped (kanban 005-003, 008, 009). It is not referenced from documentation/ and is not in the package.
- Impact: Consumers cannot find what changed across betas or the not-a-drop-in warning in a release-notes form.
- Proposed fix: Move release notes under documentation/ (e.g. documentation/release-notes/) and add a 14.0.0-beta rolling note; rely on GitHub Release notes for the rest.
- Child task: no (group with B-4)

### B-6: Migration scratch files at repo root are dead
- Severity: debt
- Area: file-mapping.csv, files-to-modify.txt, files-to-modify-filtered.txt, original-fork-files.txt
- Evidence: All four added and last touched in 934bc1d (2025-07-08, "Add scripts and data files for fork attribution task"). Only reference anywhere is a mention in kanban/done/006-002.../task.md; no code, workflow, doc or script reads them. They list pre-rename PascalCase paths.
- Impact: Roughly 40 KB of misleading root clutter that duplicates info obsolete since the kebab-case rename.
- Proposed fix: git rm the four files (history keeps them).
- Child task: no (group as cleanup; see B-14)

### B-7: CI workflow actions are pinned to mutable tags, and the release job holds id-token for every event
- Severity: risk
- Area: .github/workflows/workflow.yml:52-54, 59, 64, 79, 102
- Evidence: `actions/checkout@v4`, `actions/setup-dotnet@v4`, `nuget/login@v1`, `actions/upload-artifact@v4` (tags, no SHA). Job-level permissions grant `id-token: write` and `actions: read` to push, PR and dispatch runs alike, although only the release/probe path uses OIDC. The comment on line 55 ("Release mode lists CI runs and downloads their package artifacts") describes behavior that does not exist: nothing in tools/dev-cli downloads artifacts.
- Impact: A compromised or moved tag of the third-party `nuget/login` action runs in a job that can mint a publish credential; broader-than-needed token scopes on PR builds. Fork PRs do not get id-token, so exposure is same-repo branches.
- Proposed fix: Pin actions to commit SHAs (dependabot for github-actions can maintain them), split a `publish` job with `id-token: write` that runs only for release/dispatch release/probe, and leave the test job with `contents: read`. Drop `actions: read` and the stale comment.
- Child task: yes

### B-8: No concurrency control, caching, timeouts, or dependabot
- Severity: debt
- Area: .github/workflows/workflow.yml, .github/ (only workflows/workflow.yml exists)
- Evidence: No `concurrency:`, no `timeout-minutes`, no NuGet cache (setup-dotnet `cache: true` unused; no packages.lock.json), no dependabot.yml, no CODEOWNERS, SECURITY.md or CONTRIBUTING (git ls-files).
- Impact: Superseded PR runs keep burning minutes; two release events could race on `nuget push`; dependency pins (Directory.Packages.props) and action versions are updated only by hand (kanban 012).
- Proposed fix: Add `concurrency` (cancel-in-progress for PRs only, never for release), a job timeout, and a dependabot config for nuget and github-actions.
- Child task: yes (group with B-7 if preferred)

### B-9: Workflow secret handling and release guard gaps
- Severity: risk
- Area: .github/workflows/workflow.yml:92-95
- Evidence: The OIDC-minted key is expanded directly into the shell script via `${{ steps.nuget-login.outputs.NUGET_API_KEY }}` and passed as the `--api-key` argument (visible in the process list for the step). The release path has no check that the release tag equals `v` + Version: guards are in `check-version` (NuGet duplicate check, shared from TimeWarp.Nuru.DevCli, not reviewed) and AssertVersionSsot (root vs source props only). Release runs skip tests by design (workflow header line 5) and take no needs on the master CI run.
- Impact: A release created from a tag or commit whose tests never passed, or whose tag disagrees with the packed version, can publish; the key briefly appears on a command line.
- Proposed fix: Pass the key via `env:` (NUGET_API_KEY is already read by PushAsync) instead of interpolating it; add a tag-vs-Version assertion and verify the tagged commit is on master with a green CI run before pushing. Confirm what the tw-release `dev release` guard covers; no `release` command is defined in tools/dev-cli/endpoints (only build, pack, test, verify-samples, workflow plus shared clean/self-install/check-version).
- Child task: yes

### B-10: Path filters skip builds for files that change packaged output
- Severity: risk
- Area: .github/workflows/workflow.yml:9-31
- Evidence: Triggers filter to source/tests/samples/tools/msbuild/.github/workflows/Directory.*.props/slnx. Not covered: readme.md and assets/logo.png (both packed into every nupkg, source/Directory.Build.props:21-22), BannedSymbols.txt, nuget.config, .editorconfig, timewarp-mediator.snk, .githooks/**, notice/unlicense.
- Impact: A PR that only edits those files gets no CI. If the CI job is a required status check, docs-only PRs would also hang as "pending" (branch protection not visible from the repo).
- Proposed fix: Add the missing paths, or drop path filters and rely on concurrency/caching to keep runs cheap.
- Child task: no (group with B-7)

### B-11: README CI badge points at a workflow name that does not exist
- Severity: nit
- Area: readme.md:3
- Evidence: Badge URL is `.../workflows/CI/badge.svg`; workflow.yml:1 is `name: CI/CD`. The readme also keeps the upstream MediatR badges including a MyGet feed for MediatR (readme.md "Original MediatR" section).
- Impact: The CI badge renders "no status" / 404.
- Proposed fix: Use `actions/workflows/workflow.yml/badge.svg` and trim or label the upstream badges.
- Child task: no

### B-12: Target-framework strategy is inconsistent with the dependency floor
- Severity: debt
- Area: source/*/*.csproj, tests/*/*.csproj, samples/*/*.csproj
- Evidence: TimeWarp.Mediator targets netstandard2.0;net6.0 (timewarp-mediator.csproj:6). Contracts, Analyzers and Generators are netstandard2.0. Dependencies are MS.Extensions.DI(.Abstractions) 10.0.12 and Microsoft.Bcl.AsyncInterfaces 10.0.12 (Directory.Packages.props:28-34). Tests, benchmarks and 10 samples are net8.0; the Windsor sample is net6.0 and the base sample netstandard2.1 (csproj files). CI installs only SDK 10.0.x and 8.0.x (workflow.yml:66-68). tests/timewarp-mediator-tests targets only net8.0, so the net6.0 and netstandard2.0 assets are never executed.
- Impact: net6.0 reached end of support and is paired with 10.x dependencies; the netstandard2.0/net6.0 binaries ship untested at runtime; the Windsor sample targets a runtime the CI does not install.
- Proposed fix: Decide the supported floor (net8.0 plus netstandard2.0 only if needed), drop net6.0, align samples, and add net48 or net8 run coverage for the netstandard2.0 asset if it must stay.
- Child task: yes

### B-13: ContinuousIntegrationBuild is set in only one of four packable projects
- Severity: risk
- Area: source/timewarp-mediator/timewarp-mediator.csproj:7
- Evidence: grep across csproj/props/targets finds `ContinuousIntegrationBuild` only there. Contracts, Analyzers and Generators do not set it (and root props do not). Directory.Build.props sets Deterministic and SourceLink but not the CI flag.
- Impact: Published Contracts/Analyzers/Generators nupkgs can embed CI runner paths and are not fully deterministic or reproducible, unlike the main package.
- Proposed fix: Move the condition to the root Directory.Build.props.
- Child task: no (group with B-3 build-props cleanup)

### B-14: Root and tracked-artifact hygiene
- Severity: nit
- Area: skills/.gitkeep, kanban/backlog/.gitkeep, kanban/in-progress/.gitkeep, .gitattributes, .agents/, analysis/, .envrc, build-command.cs
- Evidence: skills/ contains only an empty .gitkeep (convention scaffold). .gitattributes has `core.eol crlf` (line 13), which is not a valid gitattributes entry (it belongs in git config) and is a no-op; `*.sln merge=union` and `*.csproj merge=union` risk silent bad merges, and no .sln exists (git ls-files finds none). .agents/collaboration and analysis/2026-06-17-* are design history (last touched 8a918a0/c074fe3). build-command.cs header says "the root still has both .sln and .slnx" which is false. bin/dev and bin/dev.dbg (12 MB and 25 MB) exist on disk but are gitignored via `[Bb]in/` (git check-ignore), so they are not committed: this is correct.
- Impact: Minor noise and one misleading dev-cli comment; `merge=union` on csproj can duplicate lines on conflicts.
- Proposed fix: Remove the invalid `core.eol` line and the `.sln`/`.csproj` union merges, fix the build-command comment, and archive analysis/ and .agents/ into kanban or documentation/archive if no longer consulted.
- Child task: no

### B-15: agent.md contains stale or contradictory statements
- Severity: debt
- Area: agent.md:21-27, 40-41, 58-65, 85-92, 107
- Evidence: Verified TRUE: C# 10.0 (Directory.Build.props:42, core csproj files), .NET Standard 2.0 and .NET 6.0 targets, Microsoft.CodeAnalysis.CSharp 4.8.0 via VersionOverride (analyzers and generators csproj:26), DI.Abstractions and Bcl.AsyncInterfaces at 10.0.12, strong-name key, merge-commit-only policy. Verified STALE: "Both packages are now built together" (there are four); Core Projects lists only two (Contracts, Mediator) and omits Analyzers and Generators; "microsoft-extensions-di/" is described as "multiple DI container integrations" but is a folder inside source/timewarp-mediator and tests/timewarp-mediator-tests; tests described as "TimeWarp.Mediator.Tests" and "TimeWarp.Mediator.Benchmarks" (directories are kebab-case); "Target Frameworks" omits the net8.0 test projects; "Treats warnings as errors" is true only for source (tests and samples override it off, tests/Directory.Build.props, samples/Directory.Build.props); command list never mentions `verify-samples` or `check-version`. CLAUDE.md is exactly `@agent.md`, which is fine.
- Impact: Agents reading agent.md get a partial picture of the shipped packages and the dev CLI surface.
- Proposed fix: Refresh the architecture and build sections from the csproj files and `./bin/dev --help`.
- Child task: no (docs reviewer may overlap; fold into B-4)

### B-16: Dependency pins: no unused pins; holdout and prerelease notes
- Severity: nit
- Area: Directory.Packages.props
- Evidence: Every PackageVersion has at least one referencing csproj/props (scripted check; no zero-reference pins). Lamar 15.0.1 is a deliberate holdout (kanban 012 task.md line 22: 16.0.0 ships only net9.0/net10.0, tests and the Lamar sample are net8.0). Prerelease pins are confined to build-time or dev-only items: TimeWarp.SourceGenerators 1.0.0-beta.11 (PrivateAssets=all), TimeWarp.Nuru* 3.0.0-beta.79 and TimeWarp.Amuru* 2.0.0-beta.2 (dev-cli only). Shipped runtime dependencies are stable. Microsoft.CodeAnalysis.CSharp is 5.9.0 centrally but the two shipped Roslyn components override to 4.8.0, so the compilation-tests and analyzers-tests (Workspaces 5.9.0) exercise newer Roslyn than the floor they ship against. Microsoft.CodeAnalysis.Analyzers is 5.9.0 for analyzer authoring.
- Impact: The Roslyn floor (4.8.0) is never tested by a consumer-side build.
- Proposed fix: Add one CI matrix leg or compile test that loads the packed Generators/Analyzers against an older SDK or Roslyn 4.8, and document the Lamar hold in the pins file.
- Child task: no

### B-17: nuget.config has a single source and no package source mapping
- Severity: nit
- Area: nuget.config
- Evidence: Only nuget.org (https://api.nuget.org/v3/index.json) is configured; file has a UTF-8 BOM and no `<packageSourceMapping>`; no `<clear/>` so user/machine feeds still merge in.
- Impact: Low, since only nuget.org is intended; user-level feeds could still be consulted on developer machines and in CI images.
- Proposed fix: Add `<clear/>` before the source and a sourceMapping for `*`; remove the BOM.
- Child task: no

### B-18: Strong-name key is committed intentionally but undocumented
- Severity: nit
- Area: timewarp-mediator.snk, Directory.Build.props:44-45, agent.md:91
- Evidence: timewarp-mediator.snk (596 bytes, a private+public pair) is tracked since c074fe3 (rename). Archived task kanban/archived/legacy-mediatr-rename/done/006 generated it fresh rather than reusing MediatR's key. agent.md:91 mentions signing, but neither readme nor documentation explains that the committed snk is a public identity-only key. Signing applies only to Mediator and Contracts (Analyzers/Generators override SignAssembly=false).
- Impact: Readers may mistake it for a leaked secret, and the partial signing is not stated.
- Proposed fix: Add a short note in agent.md or documentation that the key is intentionally public (MediatR convention) and which assemblies are signed.
- Child task: no

### B-19: Dev CLI surface vs documentation, and overlap with ganda
- Severity: nit
- Area: tools/dev-cli/dev.cs, tools/dev-cli/endpoints/*, agent.md:11-36, .timewarp/dev.jsonc
- Evidence: Local commands: build, test, pack, verify-samples, workflow (endpoints/). Shared from TimeWarp.Nuru.DevCli: clean, self-install, check-version (dev.cs Design region). agent.md documents build/test/pack/workflow and self-install, not verify-samples, check-version or clean. verify-samples rebuilds samples that `dev build` already compiles through the slnx (verify-samples-command.cs Design region) and is not called by workflow. The workflow header calls `ganda`-independent steps; ganda covers repo audit, kanban and worktrees, with no overlapping build command. `check-version` packages list is in .timewarp/dev.jsonc, correctly all four ids. bin/dev is not committed (gitignored); self-install bootstraps it (agent.md:19-21).
- Impact: Small. verify-samples is redundant, and the undocumented commands are discoverable only through --help.
- Proposed fix: Delete verify-samples or wire it into the PR pipeline; list all commands in agent.md.
- Child task: no

### B-20: PR pipeline never exercises the published-consumer path or the net6.0 test target
- Severity: debt
- Area: tools/dev-cli/endpoints/workflow-command.cs, .github/workflows/workflow.yml:87-98
- Evidence: PR pipeline = clean, build, test, pack (layout asserts for Analyzers and Generators only, repo-layout.cs:25-32; Mediator and Contracts nupkg contents are not asserted). Samples build because they are in the slnx, but nothing restores the produced nupkgs from artifacts/packages into a clean consumer project (the AddGeneratedMediator install path documented in readme.md). Upload step uploads nupkg only, not snupkg for Mediator/Contracts.
- Impact: A broken buildTransitive props, missing dependency metadata or wrong TFM folder in the packed nupkgs would only be seen after publishing a beta.
- Proposed fix: Add a post-pack smoke project that references the local artifacts feed for Contracts + Generators and builds; extend PackageLayout to lib/ TFM entries and buildTransitive props.
- Child task: yes

## Summary
Counts by severity: bug 1, risk 5, debt 8, nit 6 (20 findings; B-1, B-3, B-4, B-7/B-8, B-9, B-12, B-20 suggested as child tasks).

Overall health: The build and release pipeline is solid in structure. It is a thin workflow that delegates to one AOT dev CLI, uses OIDC trusted publishing with a probe mode and break-glass guard, applies central package management, and gates analyzer nupkg layout. The weaker areas are supply-chain hardening (unpinned actions, broad job permissions, key passed on the command line, no tag-to-version or green-CI guard on release) and release-time drift between the two Version declarations. The documentation lags behind the tooling: documentation/overview.md describes a MinVer/MyGet/ci-cd.yml process that no longer exists, and agent.md and the readme license section reference outdated structure and file names. The licensing artifacts (notice and unlicense) exist but the Apache text is not packed into the nupkgs, which merits a deliberate owner decision. Four one-off migration scratch files and 2025 analysis files can be deleted outright.
