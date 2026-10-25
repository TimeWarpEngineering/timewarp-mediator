# Round 1 — general
**Date:** 2026-10-26
**Scope reviewed:** branch vs merge-base `24488ac`, all 23 changed files
**Inputs:** the orchestrator's foreground checks (Issues 1–4), plus two read-only fact-check subagents: one on architecture and migration against the source, one on release notes, readmes, and agent docs. The orchestrator re-verified Issues 5–9 against the source before recording them. Subagent claims that could not be verified were dropped. Those include the claims about the State, Architecture, and Nuru repos, and `AddGeneratedMediator<TScope>` being absent when there are no scopes, which is minor.

## Summary

The change replaces the two-stack docs with one architecture page, a 15.0 migration page with
State, Architecture and Nuru sections, and a `15.0.0.md` break list. It also rewrites
`agent.md`, the tw-mediator skill, the root and package readmes, and the overview. Most
technical claims match the code: the contract shapes, `ValueTask` signatures, registration
lifetimes, the extension class name, manifest version 2 and kind constants, diagnostic
ownership, build-property defaults, benchmark numbers, finding numbers against the parent
review, and the 13/14 tag links. Every relative link resolves.

The risk is in what a consumer acts on. The `using static Unit` explanation gets the C# lookup
rules wrong. The nuget.org install lines now resolve to the 14.0.0 reflection runtime while
15.0 is a prerelease. The `TimeWarp.Mediator` package readme misstates what that id was in
14.0.0.

## Issues

### Issue 1 — Severity: bug
- File: documentation/migration.md:66 (also :170–172; documentation/release-notes/15.0.0.md:34; skills/tw-mediator/SKILL.md:220–222)
- Description: The docs say that under `using static TimeWarp.Mediator.Unit` "the name `ValueTask` binds to the property `Unit.ValueTask` and hides `System.Threading.Tasks.ValueTask`". C# lookup does not work that way. In type position, `ValueTask<int>` still binds to the System type, because a using-static directive imports only nested types into type lookup. In an expression, a bare `ValueTask` is ambiguous and the build fails with CS0229. Probe (net10.0, ImplicitUsings on, a `Unit` struct with the shipped `ValueTask` property, `using static TimeWarp.Mediator.Unit;`): `public static ValueTask<int> M() => default;` compiles. `ValueTask.CompletedTask` gives `error CS0229: Ambiguity between 'Unit.ValueTask' and 'ValueTask'`. A reader who believes the "hides" text may rewrite every `ValueTask<T>` return type for no reason. The parenthetical says the removed `Unit.Task` "used to hide" `Task`, and that is the same mistake. The wording came from the task Notes (021-002 review M2), so the implementer copied it faithfully, but it is still wrong.
- Suggestion: State the actual rule. `ValueTask<T>` in a signature is unaffected. A bare `ValueTask` in an expression (`ValueTask.CompletedTask`, `ValueTask.FromResult`) is ambiguous (CS0229). Write `Unit.ValueTask` for the cached value and `System.Threading.Tasks.ValueTask.…` for the type's static members. `Task` is no longer in conflict because `Unit.Task` is gone. Apply the same wording in migration (both places), the 15.0.0 F8 row, and the skill gotcha.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-mediator/readme.md:8 (also source/timewarp-mediator-generators/readme.md:8–9, source/timewarp-mediator-analyzers/readme.md:19, readme.md:34–35)
- Description: The diff removed `--version` from every install line, so they read `dotnet add package TimeWarp.Mediator` and `dotnet add package TimeWarp.Mediator.Generators`. Without `--version` or `--prerelease`, `dotnet add package` resolves the latest stable version. While 15.0 ships as `15.0.0-beta.1`, that is 14.0.0, and `TimeWarp.Mediator` 14.0.0 is the reflection runtime (release-notes-v14.0.0.md:22–23, :147). The two packed readmes are the nuget.org pages for 15.0.0-beta.1. A reader who follows them gets 14.0.0 packages that do not match the `ValueTask` examples on the same page.
- Suggestion: Pin the version the packages ship at (`--version 15.0.0-beta.1`, kept in step with `<Version>` at release), or add `--prerelease` with a one-line note to drop it at 15.0.0 stable. The skill already pins `15.0.0-beta.1` in `Directory.Packages.props`. Make the readmes consistent with it.
- Status: open

### Issue 3 — Severity: bug
- File: source/timewarp-mediator/readme.md:11 (also :35; skills/tw-mediator/SKILL.md:57–59; documentation/migration.md:14–17)
- Description: This readme is the nuget.org page for the id `TimeWarp.Mediator`. It says "14.0.0 on nuget.org is the previous surface (`TimeWarp.Mediator.Contracts` plus a separate analyzers package)" and that the reflection runtime "used this id in 13.x". Both are wrong for this id. `TimeWarp.Mediator` 14.0.0 on nuget.org *is* the reflection runtime with `AddMediator()`. 14.0.0 shipped four packages: Contracts, the `TimeWarp.Mediator` runtime, Generators, and Analyzers (release-notes-v14.0.0.md:22–23, :140–147). The runtime used this id through 14.0.0, not only in 13.x. The skill's "previous surface" list leaves out the runtime. The migration package table maps Contracts to `TimeWarp.Mediator` and marks `TimeWarp.Mediator` (runtime) removed, but it never says the id is reused. A 14.x host that referenced both packages can misread that table.
- Suggestion: On the package readme, say: "Through 14.0.0 this id was the reflection runtime (`AddMediator`). The 14.0.0 contracts were `TimeWarp.Mediator.Contracts`. 15.0 reuses this id for the contracts and the analyzer." Change the 13.x clause to "through 14.0.0". Add the runtime to the skill's previous-surface list. Add one sentence under the migration table saying that the 15.0 `TimeWarp.Mediator` id is the old Contracts content, not the old runtime.
- Status: open

### Issue 4 — Severity: suggestion
- File: documentation/benchmarks/comparison-baseline.md:3–5 (and :13)
- Description: This file is generated. `./bin/dev benchmark --record` rewrites the header from `tools/dev-cli/endpoints/benchmark-command.cs:161–165`, and the header still says "Do not edit by hand". The diff hand-edits that header: it rewords the instruction, adds a prose paragraph that repeats the numbers, and adds a `- Task: 021-003` line. The next `--record` silently erases all three, and the paragraph's numbers go stale against the table under it. The architecture page and the 15.0.0 notes already carry the 021-003 provenance and commit, which is the "benchmarks page points at the 021-003 re-record" requirement.
- Suggestion: Revert the hand edits to the generated header. Keep the 021-003 provenance in architecture.md, 15.0.0.md, and agent.md, which already have it. If the baseline must name the task, have the recorder emit that line instead.
- Status: open

### Issue 5 — Severity: bug
- File: documentation/architecture.md:154–157, :172, :176 (also documentation/migration.md:184–185; agent.md:108–109; skills/tw-mediator/SKILL.md:211–215)
- Description: The docs say `TimeWarpMediatorAssembly=false` keeps a library that references Generators from emitting its own mediator. migration.md tells Nuru to keep it `false` for that reason, and the architecture table heading calls these "Build properties the generator reads". The generator never reads that property:
  - `MediatorGenerator` passes `MembershipOptions` to `CandidateGraph.Create`.
  - `CandidateGraph.Create` always adds the compilation as a member and calls `MediatorEmitter.Emit` / `ManifestEmitter.Emit` unconditionally (`candidate-graph.cs:52–55`, `:333–334`). The only early exit is TWM008.
  - `IsMsBuildMember` is read only by the analyzer's `Membership.TryCreate` (`membership.cs:81`, `:111`).
  - `TimeWarpMediatorGenerator` is analyzer-only too. Only `TimeWarpMediatorNamespace` reaches emission (`candidate-graph.cs:327`).

  So a Nuru-style library with `false` still gets an internal `Mediator`, `MediatorManifest`, and `AddGeneratedMediator`. The Nuru dogfood that gates beta.1 would follow this advice.
- Suggestion: Decide which side is wrong. If `false` should suppress emission (Nuru relies on it), that is a generator fix and the docs stay. Otherwise, state the actual behavior: the property only affects analyzer membership; a library avoids emitting a mediator by not referencing Generators; and the table is the build properties the *package* sets. Then rewrite the Nuru bullet to match.
- Status: open

### Issue 6 — Severity: bug
- File: documentation/architecture.md:165–166 (also :92)
- Description: The page says "The first attribute is outermost. `Order` is the tie-break; a lower value wraps the rest." The code does the reverse. `Order` is the primary sort key and source position is the tie-break (`candidate-graph.cs:179–180` comment and `CompareOrdered` at `:855`; `message-graph-builder.cs` sorts the same way). An attribute declared later with a lower `Order` is outermost. Separately, :92 says a multi-interface handler class is "registered once per interface". It is *bound* once per interface and *registered* once per concrete type: `AddScoped<Concrete>` is de-duplicated through `registered` at `mediator-emitter.cs:985–993`. :115 on the same page states this correctly.
- Suggestion: Write "A lower `Order` is outermost. Declaration order breaks ties (the first attribute is outermost among equal `Order` values)." Change :92 to "bound once per interface; the concrete type is registered once."
- Status: open

### Issue 7 — Severity: suggestion
- File: documentation/release-notes/15.0.0.md:26, :29
- Description: The requirement is "every breaking change with its finding number". Two citations miss the finding that introduced the change. The typed-`Send` row cites F3 and F7, but the non-boxing type switch that returns the `Dispatch_*` `ValueTask` directly, which is the cause of the synchronous-throw change, is F2. The `[GeneratedMediator]`-replaces-`build_property.TimeWarpMediatorProfile` row cites only F11, but the parent review proposes that marker under F10 (review :311–315).
- Suggestion: Cite F2 and F3 (F7 optional) on the `Send` row, and F10 and F11 on the `[GeneratedMediator]` row.
- Status: open

### Issue 8 — Severity: suggestion
- File: readme.md:41–58 (also documentation/m2-named-pipelines.md:30–42)
- Description: The rewritten quick start declares types and *then* top-level statements, which is CS8803. It also resolves `ISender` from the root `provider` (:53), and `provider` is never declared. Line :58 says "Resolve `ISender` from a scope: handlers are scoped." The m2 snippet puts `[assembly: …]` attributes after type declarations, which is CS1730. The ordering predates this diff, but both blocks were rewritten here, and the skill and m2 already show the `CreateScope()` form.
- Suggestion: Put the statements first: build the provider, call `CreateScope()`, resolve `ISender` from `scope.ServiceProvider`, then declare the types. Move the `[assembly: …]` lines above the type declarations in m2.
- Status: open

### Issue 9 — Severity: nit
- File: source/timewarp-mediator-analyzers/readme.md:40 (also skills/tw-mediator/SKILL.md:3; documentation/release-notes/15.0.0.md:19)
- Description: These are stale phrases:
  - The analyzer readme still says a missing handler fails the host "even when the library does not reference `TimeWarp.Mediator`". In 15.0 a library that declares `IRequest` must reference `TimeWarp.Mediator`, which carries the analyzer.
  - The skill description's range "TWM007-TWM011" includes the retired TWM009.
  - 15.0.0.md:19 calls the reflection runtime "the 14.x dispatcher". In 14.x the generated mediator was primary, and the runtime was the legacy dispatcher.
- Suggestion: Drop the analyzer readme sentence's first clause. List "TWM007, TWM008, TWM010, TWM011" in the skill description. Say "the 13.x/14.x reflection dispatcher".
- Status: open

## Verified (no finding)

- `.gitignore:263–264` repeats the line-252 comment header over `stacked-task-set*.resume.json`. The task Results say that block mirrors what the audit writer would add. Under the audit-fix policy this is not raised.

- `ISender` / `IPublisher` / `IPipelineBehavior` / `MessageHandlerDelegate` / `ActionHandler<T>` signatures and the marker inheritance (`IQuery<T> : IRequest<T>, IIdempotent`, the `IIdempotentCommand` pair, `IAction : IRequest`) match `source/timewarp-mediator/*.cs`. `Unit` has no `IComparable`.
- Registration: the dispatcher is transient plus `IMediator`/`ISender`/`IPublisher`; handlers, behaviors, chains, and `HandlerCache` are scoped; an unknown `TScope` throws `ArgumentException`; the class name is `TimeWarpMediatorGeneratedServiceCollectionExtensions`; after disposal the cache throws `ObjectDisposedException` (`mediator-emitter.cs`).
- Manifest `Version = 2`, the kind constants, and `Request(string)` throwing `ArgumentException` (`manifest-emitter.cs`).
- buildTransitive props defaults for `TimeWarpMediatorAssembly`, `TimeWarpMediatorGenerator`, and `TimeWarpMediatorNamespace`.
- Benchmark rows in architecture.md and 15.0.0.md match `comparison-baseline.md`.
- The 15.0.0.md finding numbers match the parent review §3 and §5.
- `v13.0.0:migration.md` and `v14.0.0:migration.md` exist, including the `#13x-reflection-to-14x-generated` anchor.
- Every relative link in the changed docs resolves. The packed readmes use absolute links only.
- `roslyn-floor-tests.cs` sits in the compilation tests. The AOT sample sets `TreatWarningsAsErrors=true`. Dependabot ignores `Microsoft.CodeAnalysis.CSharp*`.
- Stale references to Contracts, the profiles, `CreateStream`, and `AddMediator` remain only in the archive, the design docs, posts, and the 13/14 release notes. All of those are historical.

Not re-run here: `./bin/dev workflow`. The task Results record it green on 2026-10-26. `artifacts/package-smoke/packages/*/14.0.0/readme.md` already contains the new readme text, which shows pack ran over these readmes.
