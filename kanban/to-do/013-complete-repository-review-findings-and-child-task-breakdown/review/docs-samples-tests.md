# Findings: docs, samples, runtime tests, benchmarks

### D-1: No 14.0.0 release notes; release-notes convention is a single stale root file
- Severity: debt
- Area: release-notes-v13.0.0.md, documentation/generated-vs-legacy.md:57-91
- Evidence: `git ls-files | grep release-notes` returns only release-notes-v13.0.0.md. The beta.2/.3/.4 changelog is embedded as "Changes in 14.0.0-beta.N" sections in generated-vs-legacy.md:57-91 and m1-generated-mediator.md:30-38.
- Impact: Consumers and the release process have no single place for 14.x notes, and per-beta history is buried in a comparison doc.
- Proposed fix: Create `documentation/release-notes/` (move the v13 file, add v14.0.0-beta.N), and link it from readme.md. Trim generated-vs-legacy.md to a pointer.
- Child task: yes

### D-2: readme.md is the NuGet package readme for every package but is a repo landing page with relative links
- Severity: risk
- Area: readme.md, Directory.Build.props:16
- Evidence: Directory.Build.props:16 sets PackageReadmeFile=readme.md for all packages. readme.md:25,49-51,161-163 use relative links (`./migration.md`, `./documentation/...`) that 404 on nuget.org. The readme also embeds the upstream "Original MediatR" badges and text (readme.md:55-68) and gives install steps for all three stacks to every package page.
- Impact: Broken links and a confusing page on every NuGet package (the Contracts/Generators/Analyzers pages show reflection `AddMediator` instructions first).
- Proposed fix: Use absolute github.com blob URLs, drop or shrink the "Original MediatR" block, and ideally add per-package readmes (Contracts, Generators, Analyzers) via a per-project PackageReadmeFile.
- Child task: yes

### D-3: readme.md content defects
- Severity: nit
- Area: readme.md
- Evidence: readme.md:190 has a stray `)` in "`IRequestExceptionAction<,>)`". readme.md:20 claims "full API compatibility with MediatR" while also saying 14.0.0-beta is not a drop-in (readme.md:39) and the namespace/method are renamed. readme.md:55-68 duplicates the intro of lines 8-12 (repeated "Simple mediator implementation..." paragraph). Section order puts "Installing the reflection stack" under the "Original MediatR" heading (readme.md:70).
- Impact: Sloppy first impression; headings misattribute TimeWarp install steps to upstream MediatR.
- Proposed fix: Fix the typo, reword "full API compatibility" to "MediatR-compatible 13.x API", move the upstream block to the end as an acknowledgement.
- Child task: no

### D-4: Heavy duplication of the "two stacks / not a drop-in / issue #52" block
- Severity: debt
- Area: readme.md:28-51, documentation/generated-vs-legacy.md:1-55, m1-generated-mediator.md:3-15, m2-named-pipelines.md:3-20, migration.md:3-8
- Evidence: The same "AddMediator does not register the generated mediator / not a drop-in / issue #52 stays open" paragraph, the packages table, the membership table, and the ClientPipeline code block appear in readme.md, generated-vs-legacy.md, m1 and m2 (e.g. ClientPipeline snippet at readme.md:134-153, generated-vs-legacy.md:123-152, m2:34-62).
- Impact: Five places to update per beta (version `14.0.0-beta.4` is hard-coded in readme.md:43,90,91,96-99, generated-vs-legacy.md:41,48,75-76,112 and m1:8, m2:15). They will drift.
- Proposed fix: Make generated-vs-legacy.md the SSOT, keep a 5-line summary in readme and link; remove version literals from m1/m2.
- Child task: yes (combine with D-1 and D-2 as a "docs restructure" task)

### D-5: migration.md covers only MediatR to 13.x; no 13.x to 14.x guidance
- Severity: debt
- Area: migration.md:3-26
- Evidence: migration.md:24-25 pins `Version="13.0.0"`; the only 14.x content is the pointer at migration.md:6-8. No document lists what a 13.0.0 consumer must change to adopt `AddGeneratedMediator` (add Generators package, `[assembly: MediatorAssembly]`, handler discovery rules, streams and typed exception handlers unsupported, `NoHandlerException`).
- Impact: Users on 13.0.0 have no step-by-step upgrade path; unsupported features (streams, exception cascade: m1-generated-mediator.md:50-56, emitter throws NotSupportedException at mediator-emitter.cs:383,388) are only discoverable in a "Deferred" table.
- Proposed fix: Add a "13.x reflection to 14.x generated" migration section with a supported/unsupported feature matrix.
- Child task: yes

### D-6: documentation/overview.md is stale and describes only two packages and a different workflow
- Severity: bug
- Area: documentation/overview.md:1-99
- Evidence: Lines 3,7 name only TimeWarp.Mediator and Contracts and `source/timewarp-mediator`/`-contracts`; lines 9,68 reference `.github/workflows/ci-cd.yml` but the workflow is `.github/workflows/workflow.yml` (workflow.yml:3 says the pipeline is `dev workflow`); lines 18-27,51-62 use example versions 12.x and MinVer `v` tags and "Create GitHub release" while Directory.Build.props:25 has a literal `<Version>14.0.0-beta.4</Version>` and the repo has `tw-release`/`dev release` flow; line 41 says prereleases go to MyGet. agent.md ("Release Process") points here.
- Impact: The documented release process is wrong for the current repo (four-plus packages, different workflow file, different release mechanism); an agent or human following it will fail.
- Proposed fix: Rewrite to describe the dev-cli `workflow`/`release` flow, all five packages, and Directory.Build.props versioning, or replace with a link to the tw-release skill doc.
- Child task: yes

### D-7: analysis/ and .agents/collaboration/ are one-off design scratch with live links from product docs
- Severity: debt
- Area: analysis/*.md (6 files), .agents/collaboration/**
- Evidence: m1-generated-mediator.md:3 and m2-named-pipelines.md:3 cite `analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14/§9.1 as the "Design SSOT". Three sibling analysis files are marked superseded (analysis/2026-06-17-source-gen-aot-rewrite.md:3). .agents/collaboration/readme.md:3-20 describes a layout and links to `Analysis/` (capitalised; actual dir is `analysis/`), and the only thread is two cross-agent feedback files. nuget-publishing-analysis.md and release-process-analysis.md are unrelated to the spec.
- Impact: Mixed live/stale design material at repo root; the capitalised `Analysis/` link is broken on Linux.
- Proposed fix: Keep the spec (rename `documentation/design/`), move the three superseded analyses plus agent threads to an `archive/` folder, and fix the `Analysis/` link.
- Child task: yes

### D-8: Root-level fork-migration leftovers and empty-value artifacts
- Severity: nit
- Area: file-mapping.csv, files-to-modify.txt, files-to-modify-filtered.txt, original-fork-files.txt
- Evidence: `git ls-files` lists all four at repo root; file-mapping.csv rows reference original MediatR paths like `.github/workflows/ci.yml` with `"False"`. Nothing in readme/agent.md references them.
- Impact: Dead one-time-migration tracking files clutter the root and the repo.
- Proposed fix: Delete (history keeps them) or move under analysis/archive.
- Child task: no

### D-9: Blip post names a 13.0.0 install command without a version and has no index
- Severity: nit
- Area: documentation/posts/blips/2025-08-04-release-announcement.md
- Evidence: Only one post exists; there is no 14.x announcement and the post is not linked from any doc.
- Impact: Negligible; orphan marketing content.
- Proposed fix: Leave or relocate with release notes in D-1.
- Child task: no

### D-10: Idempotent contracts documented in readme/m1 but not in main usage docs
- Severity: nit
- Area: readme.md:110-112, m1-generated-mediator.md:34
- Evidence: IIdempotent/IIdempotentCommand exist (source/timewarp-mediator-contracts/i-idempotent.cs, i-idempotent-command.cs) and are mentioned as one bullet in readme.md:110 and m1:34. There is no usage example, and no sample or test dedicated to them in the runtime suite beyond tests/timewarp-mediator-tests/send-idempotent-command-tests.cs (2 tests).
- Impact: Feature is discoverable but thinly documented; docs correctly say "types only, no dedup", which is accurate to source.
- Proposed fix: Add a short example to the Contracts section; no code change needed.
- Child task: no

### D-11: Samples are net8.0/net6.0/netstandard2.1 and several are legacy-stack only
- Severity: debt
- Area: samples/*/*.csproj
- Evidence: Windsor sample targets `net6.0` (timewarp-mediator-examples-windsor.csproj:5); the other 9 reflection samples target net8.0; the shared `timewarp-mediator-examples` library targets `netstandard2.1` (csproj:5). net6.0 and net8.0 are both out of support by 2026-10. Samples all use ProjectReference (aot csproj:21-23, named-pipelines csproj:16-18), never PackageReference, so they do not prove the published nupkg layout works.
- Impact: Samples do not validate the shipped package, and Windsor pins an EOL runtime.
- Proposed fix: Bump samples to a supported TFM (net10.0 is used by dev-cli/host) and add one sample that consumes the packed nupkg from a local feed.
- Child task: yes

### D-12: Eight DI-container samples only exercise legacy reflection `AddMediator`
- Severity: debt
- Area: samples/timewarp-mediator-examples-{autofac,dry-ioc,lamar,light-inject,simple-injector,stashbox,windsor,asp-net-core}
- Evidence: Each references only `..\timewarp-mediator-examples\timewarp-mediator-examples.csproj` (shared library) plus the container package; none references Generators. Only aot and named-pipelines samples (csproj:21-23 / 16-18) use the generated stack, and `AddGeneratedMediator` is MS.DI only.
- Impact: Maintenance cost (8 container package pins in Directory.Packages.props) for samples that demonstrate the legacy stack that 14.x positions as not-the-future. Windsor even has a net6 target.
- Proposed fix: Decide whether container samples stay; if so, label them "legacy reflection runtime" in a samples/readme; consider dropping the least-used containers.
- Child task: yes

### D-13: Samples build is in `dev build` via slnx, and `verify-samples` is a second redundant build
- Severity: nit
- Area: tools/dev-cli/endpoints/verify-samples-command.cs:7-10, timewarp-mediator.slnx:2-14
- Evidence: verify-samples comments say samples are already in the slnx; it rebuilds each csproj individually. It only compiles; no sample is ever run, so the sample `Main` return codes (e.g. named-pipelines program.cs returns 1 on failure, aot program.cs) are asserting nothing in CI.
- Impact: Samples can compile but be broken at runtime, which for the AOT and named-pipelines samples is exactly what they are meant to prove.
- Proposed fix: Have verify-samples (or a test) `dotnet run` the aot and named-pipelines samples and check exit code 0.
- Child task: yes

### D-14: Samples and tests disable warnings-as-errors and Nullable; banned APIs not enforced there
- Severity: debt
- Area: samples/Directory.Build.props:6-12, tests/Directory.Build.props:6-12, BannedSymbols.txt
- Evidence: Both set `TreatWarningsAsErrors=false`, `Nullable=disable`, `Features` empty, no strong-name signing. BannedSymbols.txt bans `System.Console` and `ProcessStartInfo` but samples use Console.WriteLine (e.g. samples/timewarp-mediator-examples-named-pipelines/program.cs:55-56), so RS0030 warnings are produced as non-fatal noise. The test csproj re-enables `<Nullable>enable</Nullable>` (timewarp-mediator-tests.csproj:6) only to have Directory.Build.props override-order determine the result.
- Impact: Warning noise hides real warnings; the ban looks unenforced in samples and tests. Intent of the test project Nullable setting is unclear.
- Proposed fix: Add `<NoWarn>RS0030</NoWarn>` for samples/benchmarks explicitly (they are demos), or reference ITerminal; fix the Nullable conflict.
- Child task: no

### D-15: Samples reference an upstream MediatR GitHub issue; otherwise no leftover MediatR naming
- Severity: nit
- Area: samples/timewarp-mediator-examples-autofac/program.cs:40
- Evidence: Only match for "mediatr" across samples/tests/benchmarks sources is the issue link `jbogard/MediatR/issues/462`; the test assembly copyright string (obj output) is intentional attribution.
- Impact: None; attribution is appropriate.
- Proposed fix: None.
- Child task: no

### D-16: Runtime test suite uses wall-clock timing assertions (flaky)
- Severity: risk
- Area: tests/timewarp-mediator-tests/notification-publisher-tests.cs:18-23,38-68
- Evidence: Should_handle_sequentially_by_default measures `Stopwatch` for sequential (500+250 ms) vs `TaskWhenAllPublisher` and asserts `sequentialElapsed.ShouldBeGreaterThan(parallelElapsed)`. The test name says "sequentially" but the assertion is a relative timing comparison; the first run also includes JIT, DI scan and first-call wrapper creation.
- Impact: Timing and cold-start sensitivity can produce intermittent failures on loaded CI; also does not assert ordering.
- Proposed fix: Replace delays with TaskCompletionSource-gated handlers and assert call order (sequential) and concurrency (parallel) deterministically.
- Child task: yes

### D-17: Cancellation/timeout tests rely on real 2 s delays
- Severity: debt
- Area: tests/timewarp-mediator-tests/send-tests.cs:184,198 and microsoft-extensions-di/base-generic-request-handler-tests.cs:16
- Evidence: `await Task.Delay(2000, cancellationToken)` in TimeoutRequestHandler and TimeoutRequest2Handler; generic-handler registration timeout assembly is also built through Generate*Assembly helpers.
- Impact: Slow tests and timing sensitivity; low flakiness since cancellation is token driven, but cost scales.
- Proposed fix: Use `Task.Delay(Timeout.Infinite, ct)` and cancel via CancellationTokenSource.CancelAfter(small) or cancel directly.
- Child task: no

### D-18: Static mutable state in a test
- Severity: nit
- Area: tests/timewarp-mediator-tests/microsoft-extensions-di/pipe-line-multi-call-to-constructor-test.cs:37-41
- Evidence: `private static volatile object _lockObject` and `private static int _constructorCallCount` shared across tests in a class that xUnit may run in parallel with other classes' instances.
- Impact: Low risk of cross-test interference if another test constructs the same type; `volatile` on a lock object is meaningless.
- Proposed fix: Make the counter instance-scoped or reset in the constructor; drop `volatile`.
- Child task: no

### D-19: Two Lamar open-generic tests skipped permanently
- Severity: debt
- Area: tests/timewarp-mediator-tests/stream-pipeline-tests.cs:318, tests/timewarp-mediator-tests/pipeline-tests.cs:435
- Evidence: `[Fact(Skip = "Lamar does not mix concrete and open generics. Use constraints instead.")]`. The suite also references Lamar as a primary container (timewarp-mediator-tests.csproj, `<PackageReference Include="Lamar" />`) rather than only MS.DI, which is the shipped integration.
- Impact: Skips never run and the same behavior is likely covered by the MS.DI tests in microsoft-extensions-di/pipeline-tests.cs (32 tests); dead tests mislead.
- Proposed fix: Delete the two skipped tests or port them to MS.DI; decide whether Lamar remains a test dependency.
- Child task: no

### D-20: Runtime test suite targets only legacy reflection runtime; coverage gaps vs documented behavior
- Severity: risk
- Area: tests/timewarp-mediator-tests/**
- Evidence: Project references only `timewarp-mediator.csproj` (timewarp-mediator-tests.csproj:11) and no generators; all tests use `AddMediator`. A grep for variance/contravariance/covariance across the test project returns no matches, although readme.md:12 advertises dispatch via C# generic variance. Publish strategies have one timing test (notification-publisher-tests.cs:27-68) plus microsoft-extensions-di/notification-publisher-tests.cs. GetPipelineInfo is covered (microsoft-extensions-di/service-collection-extensions-tests.cs:8,22).
- Impact: Generic-variance dispatch, a headline readme feature, is not explicitly regression-tested; generated-stack behavior lives only in other test projects (out of my scope).
- Proposed fix: Add contravariant notification/request handler dispatch tests and a deterministic publisher-order test; confirm generator test project covers the same scenarios as a parity matrix.
- Child task: yes

### D-21: Test naming and xunit conventions mixed
- Severity: nit
- Area: tests/timewarp-mediator-tests
- Evidence: Files are kebab-case as agent.md requires, but files nest in `microsoft-extensions-di/` and `pipeline/` with duplicates of the same names at two levels (`pipeline-tests.cs` vs `microsoft-extensions-di/pipeline-tests.cs`, `stream-pipeline-tests.cs` vs `microsoft-extensions-di/stream-pipeline-tests.cs`, `notification-publisher-tests.cs` twice). Test methods use `Should_...` snake style; class files use 4-space indentation in tests but dev-cli uses 2 spaces (tools/dev-cli/endpoints/verify-samples-command.cs).
- Impact: Hard to navigate; indentation inconsistent across the repo (agent.md states no explicit rule, tw-csharp expects 2 spaces).
- Proposed fix: Rename for distinct names; apply `dotnet format`/editorconfig to tests and samples if 2-space is the standard.
- Child task: no

### D-22: Benchmarks: Windows-only diagnoser package, no stored results, no CI
- Severity: debt
- Area: tests/timewarp-mediator-benchmarks/timewarp-mediator-benchmarks.csproj, tests/timewarp-mediator-benchmarks/dot-trace-diagnoser.cs
- Evidence: csproj references `BenchmarkDotNet.Diagnostics.Windows` on a project built on Linux CI via slnx; sets `<Configuration>Release</Configuration>` inside a PropertyGroup (csproj, same in the comparison csproj), which does not reliably force Release. `dot-trace-diagnoser.cs` is a custom DotTrace attribute requiring JetBrains tooling. No BenchmarkDotNet.Artifacts or results directory is tracked, and nothing in dev-cli runs benchmarks (grep of tools/dev-cli for "benchmark" returned nothing).
- Impact: Benchmarks only compile in CI; results are unreproducible/unrecorded other than the hand-copied table in documentation/m1-benchmark-gap.md.
- Proposed fix: Add a `dev benchmark` command and store a results baseline; remove the Configuration property and Windows-only diagnostics package behind a condition.
- Child task: no

### D-23: Comparison benchmark doc is hard-coded numbers and a hard-coded host
- Severity: nit
- Area: documentation/m1-benchmark-gap.md:3-22, tests/timewarp-mediator-benchmarks-comparison/*.csproj
- Evidence: m1-benchmark-gap.md:3 gives Ryzen 9 7950X3D, BDN 0.13.10 and ".NET 10.0.11" while the project targets net8.0 with `RollForward=LatestMajor` (comparison csproj:5-6). The doc's heading "(004-001)" refers to an internal kanban epic id, and it is not linked from readme or any other doc.
- Impact: Numbers cannot be reproduced or traced to a commit; the doc is orphaned. The comparison project is correctly scoped (legacy vs generated vs CallSiteInlining vs martinothamar), and it imports the generators props directly (csproj:13), a good coverage of purpose.
- Proposed fix: Link from generated-vs-legacy.md, add commit SHA and date to the doc, remove epic-id jargon.
- Child task: no

### D-24: Internal task/epic ids leak into user-facing docs
- Severity: nit
- Area: documentation/m1-generated-mediator.md:1,18,67; m2-named-pipelines.md; m1-benchmark-gap.md:1
- Evidence: "004-001", "004-002", "epic 005", "OQ-B", "ServiceGen (Nuru 444)" appear in headings and tables without definition (m1:1, m1:56-60, m1-benchmark-gap.md:15). Also the generator's runtime exception message embeds "(004-001)"/"(004-002)" (mediator-emitter.cs:383,388).
- Impact: End users see meaningless internal ids in documentation and in a thrown exception message.
- Proposed fix: Replace with milestone names or issue links; reword exception text.
- Child task: no

## Summary

Counts by severity: bug 1, risk 3, debt 10, nit 10.

The documentation is largely accurate against source: every API name checked (AddGeneratedMediator, AddGeneratedMediator<TScope>, ISender<TScope>, MediatorScope, MediatorBehavior, IIdempotent*, GetPipelineInfo, registration methods) exists. The real problems are structural: the same two-stack block is copied into five documents with a hard-coded beta version, overview.md describes a release workflow and file that no longer exist, there are no 14.x release notes, migration only covers the 13.x rename, and readme.md is reused verbatim as every NuGet package readme with relative links that break on nuget.org. Samples compile in CI via the solution and verify-samples but are never run, all use ProjectReference rather than the packed packages, and target net6/net8 plus a large legacy container-sample fan-out. The runtime test suite is a ported MediatR suite that is only for the reflection runtime, with one timing-based flaky test, two permanent skips, and thin coverage of GetPipelineInfo and variance. Overall health of this area is fair: code-facing docs for the generated stack are good, but release, migration and packaging docs need a focused restructure.
