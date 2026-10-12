# Core runtime + contracts review (source/timewarp-mediator, source/timewarp-mediator-contracts)

Paths are relative to the task-013 worktree root. Read-only review; nothing was built or run, so behavioral claims come from reading the code.

### C-1: Registration timeout of 0 cancels immediately instead of disabling
- Severity: bug
- Area: source/timewarp-mediator/registration, microsoft-extensions-di
- Evidence: source/timewarp-mediator/registration/service-registrar.cs:22 builds `new CancellationTokenSource(RegistrationTimeout)`; mediator-service-configuration.cs:79 documents "To Disable this constraint, set the value to 0". A zero delay cancels the source at once, so the next `ThrowIfCancellationRequested` (service-registrar.cs:272, 306) throws, which line 28-30 turns into `TimeoutException`.
- Impact: Anyone who follows the documented way to disable the timeout while using `RegisterGenericHandlers` gets a spurious TimeoutException.
- Proposed fix: Use `Timeout.InfiniteTimeSpan` (or no CTS) when the value is 0 or less. Pass the inner exception to `TimeoutException`.
- Child task: yes

### C-2: Timeout token is only passed to two of the six scan passes
- Severity: risk
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:40-41 pass `cancellationToken`; lines 42-45 and 49-50 call `ConnectImplementationsToTypesClosing` without it, and the default token is used by `AddAllConcretionsThatClose` (line 159).
- Impact: Generic notification handlers, stream handlers, exception handlers and processors are not covered by `RegistrationTimeout` or the registration limits' cancellation.
- Proposed fix: Thread the token through every call. Fold into the C-1 task.
- Child task: no (group with C-1)

### C-3: Registration-count guard checks the wrong setting
- Severity: bug
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:264 reads `if (MaxGenericTypeParameters > 0 && totalCombinations > MaxGenericTypeRegistrations)`. The first operand should be `MaxGenericTypeRegistrations > 0`. `totalCombinations` is a `long` multiplied unchecked in the loop at 261-266.
- Impact: Setting MaxGenericTypeParameters to 0 silently disables the registrations cap. Setting MaxGenericTypeRegistrations to 0 ("disable", per config line 74) makes every generic registration throw because the count exceeds 0. Large products can overflow.
- Proposed fix: Guard on `MaxGenericTypeRegistrations > 0` and use a checked or saturating multiply.
- Child task: yes (can share with C-1 as "generic registration limits")

### C-4: ServiceRegistrar keeps configuration in mutable public static state
- Severity: risk
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:7-10 are non-readonly statics written by `SetGenericRequestHandlerRegistrationLimitations` (12-18), called from service-collection-extensions.cs:46, and read by `GenerateCombinations` (245-266). The class and those methods are `public` (lines 5, 12, 20, 35, 245, 381).
- Impact: Two `AddMediator` calls on different threads (parallel tests, multi-host apps) can overwrite each other's limits. Calling the public `GenerateCombinations` directly uses zeros, meaning unlimited.
- Proposed fix: Pass the configuration object into `GenerateCombinations` instead of reading statics. Make `ServiceRegistrar` internal; only `AddMediator` needs to be public.
- Child task: yes

### C-5: Generic handler with a non-generic request type throws at registration
- Severity: risk
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:232-237. When the handler's first interface type argument is a closed non-generic type (for example `class H<T> : IRequestHandler<Ping, Pong>`), `IsGenericParameter` is false so the early return at 234 is skipped, and `requestType.GetGenericTypeDefinition()` at 237 throws InvalidOperationException. The expensive type scan at 221-230 also runs before the early return.
- Impact: With `RegisterGenericHandlers = true`, one such handler aborts all registration with a confusing error.
- Proposed fix: Check `requestType.IsGenericType` (and move the early returns above the scan). Add a test; I did not execute one.
- Child task: no (group with C-4/C-7 under registrar hardening)

### C-6: Name-based interface lookup can throw AmbiguousMatchException
- Severity: risk
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:179 calls `handlerType.GetInterface(handlerInterface.Name)`. `Type.GetInterface(string)` throws when more than one interface has that name. This path runs when two or more concretions match one handler interface (lines 139-142).
- Impact: Duplicate handlers where one class implements `IRequestHandler<,>` for several requests fail with an unhelpful reflection exception instead of "first wins".
- Proposed fix: Compare against `GetInterfaces()` by generic definition and arguments instead of name.
- Child task: no

### C-7: Swallowed exceptions and dead branches in registrar
- Severity: debt
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:194-196 has an empty `catch (Exception)` around `MakeGenericType`. Lines 356-360 (`else if` for non-interface templates) are unreachable because every caller passes an interface. Line 229 uses `GetTypes()` while the rest uses `DefinedTypes`, and it rescans all assemblies once per generic parameter.
- Impact: Constraint violations are hidden, dead code misleads maintainers, and a ReflectionTypeLoadException is possible.
- Proposed fix: Catch only ArgumentException with a comment, delete the dead branch, and compute the type list once.
- Child task: no

### C-8: Handler wrapper cache is keyed only by request type
- Severity: risk
- Area: source/timewarp-mediator (mediator.cs)
- Evidence: mediator.cs:42, 60, 77 and 155, 175 use `request.GetType()` as the sole key in the static dictionaries (lines 13-15). `Send<TResponse>` builds `RequestHandlerWrapperImpl<,>(requestType, typeof(TResponse))` and casts the cached value to `RequestHandlerWrapper<TResponse>`. `IRequest<out TResponse>` is covariant.
- Impact: If `Send(object)` first caches the wrapper for response type X, a later `Send<object>(IRequest<object>)` for the same request type hits an InvalidCastException. The reverse order registers a wrapper for a handler that does not exist.
- Proposed fix: Key on (requestType, responseType) or resolve the response type from the request's interface in all paths.
- Child task: yes (low priority; document if not fixed)

### C-9: Stray `$` in Publish(object) error message and missing paramName
- Severity: bug
- Area: source/timewarp-mediator (mediator.cs)
- Evidence: mediator.cs:122 `$"{nameof(notification)} does not implement ${nameof(INotification)}"`. The extra `$` prints a literal dollar sign. The `ArgumentException` has no `paramName` argument. Line 87 and 180 use `nameof(request)` as paramName inside a static cache factory, which is fine.
- Impact: The message reads "notification does not implement $INotification".
- Proposed fix: Remove the `$` and pass `nameof(notification)` as paramName.
- Child task: no

### C-10: Constructor does not validate its arguments
- Severity: nit
- Area: source/timewarp-mediator (mediator.cs)
- Evidence: mediator.cs:29-33 stores `serviceProvider` and `publisher` without null checks. The first overload (21) also allocates a new `ForeachAwaitPublisher` per Mediator instance.
- Impact: A null surfaces later as a NullReferenceException inside Send or Publish.
- Proposed fix: Add `ArgumentNullException` guards; reuse a static default publisher.
- Child task: no

### C-11: Missing null checks across DI configuration and extension methods
- Severity: nit
- Area: microsoft-extensions-di, entities
- Evidence: service-collection-extensions.cs:22-29 and 38-41 never check `services` or `configuration`. mediator-service-configuration.cs:101-114, 156-185, 193-214 and similar never check `type`, `assembly`, `implementationType`. open-behavior.cs:41 does `throw new ArgumentNullException($"Open behavior type can not be null.")`, which passes the message as the paramName.
- Impact: Misleading exceptions (a "parameter" named with a full sentence, or NullReferenceException).
- Proposed fix: Use `ArgumentNullException.ThrowIfNull` equivalents (or explicit guards for netstandard2.0) and `nameof(...)`.
- Child task: no

### C-12: AddOpenBehavior family accepts closed generic types
- Severity: risk
- Area: microsoft-extensions-di
- Evidence: mediator-service-configuration.cs:195, 311, 397, 481 check `IsGenericType`, not `IsGenericTypeDefinition`. A closed type like `Foo<int,int>` passes, then registers an open service type against a closed implementation (210, 326, 412, 496).
- Impact: The failure comes later from the container as an ArgumentException, not at the configuration call.
- Proposed fix: Test `IsGenericTypeDefinition`. Extract the four near-identical open-registration blocks (lines 193-214, 309-330, 395-416, 479-500) into one helper.
- Child task: no

### C-13: Exception handler marked Handled with a null response rethrows the original exception
- Severity: risk
- Area: source/timewarp-mediator/pipeline
- Evidence: request-exception-processor-behavior.cs:56-66. After `state.Handled` is true, `if (state.Response is null) throw;` rethrows. `RequestExceptionHandlerState.SetHandled(TResponse)` (request-exception-handler-state.cs:24) accepts a null for reference-type responses, and the comment at line 66 says the value "cannot be null if Handled".
- Impact: A handler that deliberately returns null (valid for `string?`/nullable responses) sees its exception re-thrown with no explanation.
- Proposed fix: Return `state.Response!` when Handled, or document and reject null in `SetHandled`.
- Child task: no

### C-14: Request exception / pipeline behaviors rely on unannotated reflection
- Severity: debt
- Area: source/timewarp-mediator/pipeline, wrappers, mediator.cs
- Evidence: request-exception-processor-behavior.cs:80-96 and request-exception-action-processor-behavior.cs:64-82 use `MakeGenericType`, `GetMethod` and `MethodInfo.Invoke` per exception. mediator.cs:44, 62, 90, 95 use `MakeGenericType` plus `Activator.CreateInstance`. A grep for `RequiresUnreferencedCode`, `RequiresDynamicCode`, `IsAotCompatible` and `DynamicallyAccessedMembers` across source/ finds nothing.
- Impact: The product pitches an AOT/source-generated path, but the runtime package gives trimming and AOT analyzers no signal. Reflection invoke per exception is also slow.
- Proposed fix: Annotate the legacy types with `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]` (guarded for netstandard2.0) or document them as not AOT-safe. Cache MethodInfo per (request, response, exception) type.
- Child task: yes

### C-15: Unobserved-exception behavior in TaskWhenAllPublisher
- Severity: risk
- Area: source/timewarp-mediator/notification-publishers
- Evidence: task-when-all-publisher.cs:19-23 runs `Select(...).ToArray()`, which invokes each handler synchronously until one throws before returning a task. A handler that throws synchronously aborts the loop, so later handlers never start and earlier tasks are left unobserved. The class doc at lines 7-12 shows `handler.Handle(...)`, which is not the real code.
- Impact: Behavior differs from "all handlers run" and the XML sample is outdated.
- Proposed fix: Wrap each call with `Task.Run`-free try/catch that converts a synchronous throw into `Task.FromException`, then update the doc comment.
- Child task: no

### C-16: Stream handler resolution is deferred until enumeration
- Severity: nit
- Area: source/timewarp-mediator/wrappers
- Evidence: stream-request-handler-wrapper.cs:30-54 is an `async IAsyncEnumerable` method, so `GetRequiredService` (line 34-36) runs on first `MoveNextAsync`, not in `CreateStream`.
- Impact: A missing stream handler throws on first iteration while `Send` throws on the call, so error timing differs between request kinds.
- Proposed fix: Resolve the handler and behaviors eagerly in a non-iterator method and have a private iterator do only the `await foreach`.
- Child task: no

### C-17: Inconsistent ConfigureAwait in void-request wrapper
- Severity: nit
- Area: source/timewarp-mediator/wrappers
- Evidence: request-handler-wrapper.cs:56 awaits the handler without `ConfigureAwait(false)`, while lines 28, 49 and every pipeline behavior use it.
- Impact: Minor consistency and a small perf difference in a library.
- Proposed fix: Add `.ConfigureAwait(false)`.
- Child task: no

### C-18: Leftover and incorrect text in XML docs ("med", duplicate cref, typos)
- Severity: nit
- Area: contracts, microsoft-extensions-di, pipeline
- Evidence: "med" appears as a broken word-replacement of "mediator" at i-mediator.cs:5, i-sender.cs:6, i-publisher.cs:6, service-collection-extensions.cs:17 and :33. service-collection-extensions.cs:11 lists `RequestPreProcessorBehavior` twice (second should be Post) and line 9 says the mediator is registered as transient although `Lifetime` is configurable. i-request-pre-processor.cs:6 "Defined a" and request-pre-processor-behavior.cs:8-9 empty typeparam text. mediator-service-configuration.cs:84 "controlls", "containg". request-exception-handler-state.cs:16 uses `<code>true</code>` instead of `<c>`. unit.cs:79 and 87 have a missing `<returns>` tag. i-sender.cs:36-51 and i-publisher.cs:24 have empty or missing param/returns/typeparam docs.
- Impact: Public API docs read as unfinished.
- Proposed fix: One doc-cleanup pass.
- Child task: yes (single "XML doc cleanup" task, with C-19)

### C-19: Public API without XML docs, hidden by CS1591 suppression
- Severity: debt
- Area: whole runtime package
- Evidence: Root Directory.Build.props:14 sets `GenerateDocumentationFile` but :34 adds `CS1591` to NoWarn. Undocumented public members: Mediator.cs:35, 52, 70, 106, 117, 148, 168; notification-handler-executor.cs:5; i-notification-publisher.cs:5-8; both publishers' `Publish`; every type in wrappers/*.cs; ServiceRegistrar; RequestExceptionActionProcessorStrategy (request-exception-action-processor-strategy.cs:4-8); MediatorServiceConfiguration class declaration (line 3); constructors and `Handle` methods of all five pipeline behaviors.
- Impact: IntelliSense shows nothing for core entry points, and the compiler will not flag new gaps.
- Proposed fix: Document the public surface (or use `<inheritdoc/>` for interface implementations) and drop CS1591 from NoWarn for the two shipped runtime projects.
- Child task: yes (same task as C-18)

### C-20: Wrapper and registrar types are unnecessarily public
- Severity: debt
- Area: source/timewarp-mediator/wrappers, registration
- Evidence: request-handler-wrapper.cs:5-68 and notification-handler-wrapper.cs:5-25 declare `public` abstract bases and Impl classes, while stream-request-handler-wrapper.cs:5-70 declares the equivalent types `internal`. `ServiceRegistrar` (C-4) is also public. notification-handler-wrapper.cs:7 and :15 name the `IServiceProvider` parameter `serviceFactory`, a leftover from the old `ServiceFactory` delegate.
- Impact: Implementation details are locked into the public API surface and cannot be changed without a breaking release.
- Proposed fix: Make them `internal` in the next major or beta bump (a breaking change; check the generator package and tests first). Rename the parameter.
- Child task: yes (decide before 14.0.0 stable)

### C-21: ServiceFactory delegate is dead public API
- Severity: debt
- Area: source/timewarp-mediator
- Evidence: service-factory.cs:8 declares `public delegate object ServiceFactory(Type)`. A grep of source/ and tests/ finds no use. The only mention is a stale comment at service-registrar.cs:383, and tests/timewarp-mediator-tests/service-factory-tests.cs only tests a missing handler, not the delegate.
- Impact: Dead MediatR-era public type.
- Proposed fix: Mark `[Obsolete]` or remove it; fix the comment at service-registrar.cs:383 and rename the test.
- Child task: no (include in C-20)

### C-22: Type forwarding list is missing IStreamRequest<>
- Severity: risk
- Area: source/timewarp-mediator
- Evidence: type-forwardings.cs:3-41 forwards all moved contracts types except `IStreamRequest<>` (the handler `IStreamRequestHandler<,>` at line 19 is forwarded). i-stream-request.cs:8 defines it in the Contracts assembly. Grep for `IStreamRequest<>` in the file returns nothing.
- Impact: Consumers compiled against an older TimeWarp.Mediator assembly that held this interface fail to bind at runtime.
- Proposed fix: Add `[assembly: TypeForwardedTo(typeof(IStreamRequest<>))]`. Consider a test enumerating public contracts types against the forwarding list.
- Child task: yes

### C-23: Package metadata inconsistencies
- Severity: debt
- Area: Directory.Build.props, source/Directory.Build.props, csproj files
- Evidence:
  - The version is set in two places: Directory.Build.props:25 and source/Directory.Build.props:7 (both 14.0.0-beta.4). They can drift.
  - `Authors` conflicts: root:7 "Steven T. Cramer", source:8 "TimeWarpEngineering". `PackageProjectUrl` conflicts: root:10 GitHub, source:9 timewarp.software.
  - `PackageIcon`, `PackageReadmeFile`, license, repository fields are repeated in source/Directory.Build.props:12-14 and root.
  - timewarp-mediator.csproj:5 description "Simple, unambitious mediator implementation in .NET" is the MediatR tagline and says nothing about source generation.
  - `ContinuousIntegrationBuild` is set only in timewarp-mediator.csproj:7, not in the contracts csproj or the other packages (grep shows no other location).
  - `LangVersion 10.0` is repeated in both csproj files (runtime :8, contracts :34) although the root props already sets it.
  - Both csproj files start with `<!-- Modified by Steven T. Cramer -->`.
- Impact: Contracts, Analyzers and Generators packages are not built with deterministic CI paths. The nuspec shows whichever Authors/URL wins, depending on import order.
- Proposed fix: Keep each property in one file (move `ContinuousIntegrationBuild` to the shared props), remove duplicates, rewrite descriptions to mention the generated path.
- Child task: yes

### C-24: Target frameworks
- Severity: nit
- Area: timewarp-mediator.csproj, timewarp-mediator-contracts.csproj
- Evidence: Runtime targets `netstandard2.0;net6.0` (csproj:6). No `#if NET*` or `NETSTANDARD` code exists in either project (grep). net6.0 reached end of support in 2024. Contracts targets `netstandard2.0` only (csproj:30).
- Impact: The net6.0 asset only changes dependency resolution (Bcl.AsyncInterfaces is dropped); it adds a build leg and EOL-framework target with no code benefit.
- Proposed fix: Drop net6.0 or replace it with a supported TFM only if a feature needs it. This is a packaging decision, so confirm with the maintainer.
- Child task: no

### C-25: Coding-convention mismatches (indent, underscores, headers)
- Severity: debt
- Area: whole scope
- Evidence: agent.md/global conventions call for 2-space indentation and no underscore prefixes. Every .cs file in scope uses 4 spaces, and .editorconfig:14-17 pins `indent_size=4` for `*.cs`. Underscore fields: mediator.cs:11-15, request-post-processor-behavior.cs:13, request-pre-processor-behavior.cs:13, request-exception-processor-behavior.cs:14, request-exception-action-processor-behavior.cs:14, unit.cs:10. `// Modified by Steven T. Cramer` headers appear in about 40 files. Trailing whitespace at mediator.cs:21, 132, service-registrar.cs:10, 102, 220, 271. Doubled blank lines at mediator.cs:146-147, 166-167 and mediator-service-configuration.cs:501-502.
- Impact: Style drift from the stated rules and noisy diffs.
- Proposed fix: Decide which rule wins (the .editorconfig says 4 spaces), reconcile the doc and the file, then apply one mechanical formatting pass. Do not mix with behavior changes.
- Child task: yes

### C-26: Stale comment in Mediator.Send(object)
- Severity: nit
- Area: source/timewarp-mediator
- Evidence: mediator.cs:102 says "call via dynamic dispatch to avoid calling through reflection", but the code at 103 is an ordinary virtual call.
- Impact: Misleading comment.
- Proposed fix: Delete it.
- Child task: no

### C-27: Behavior-ordering helper may be non-transitive
- Severity: nit
- Area: source/timewarp-mediator/internal
- Evidence: object-details.cs:42 chains three comparison rules with `??`, and `CompareByNamespace` returns 0 for null locations (86-89). `Array.Sort` (handlers-orderer.cs:19) requires a consistent total order. `Compare(null, null)` returns 1 (lines 32-35).
- Impact: Ordering with mixed assemblies/namespaces is not guaranteed stable. Inherited from MediatR; not known to fail.
- Proposed fix: Return 0 for two nulls and add property-style tests for ordering.
- Child task: no

### C-28: IIdempotent contracts: the "types only" caveat is not in the public XML docs
- Severity: debt
- Area: source/timewarp-mediator-contracts (i-idempotent.cs, i-idempotent-command.cs, i-query.cs)
- Evidence: i-idempotent.cs:2 says "Types only; no key enforcement or dedup store", and i-idempotent-command.cs:2 says dispatch is identical to `ICommand`, but both are `#region Purpose` comments, not `<remarks>`. The public docs (i-idempotent.cs:7-10, i-idempotent-command.cs:7-12) say a command "is safe to retry" with nothing indicating the library does nothing to make it so. `IQuery<TResponse>` now inherits `IIdempotent` (i-query.cs:11), which changes its public inheritance chain, but the XML for `IQuery` only says "idempotent by definition". The handler interfaces (i-idempotent-command.cs:26-39) have no remarks. readme.md:110-112 does state the caveat.
- Impact: Package consumers reading IntelliSense can believe the library enforces idempotency.
- Proposed fix: Add a `<remarks>` to `IIdempotent` and `IIdempotentCommand` stating that nothing is enforced, and what the caller or a behavior must provide. There is no `IIdempotentAction` equivalent; note that as intentional if it is.
- Child task: no (fold into the C-18/C-19 doc task)

### C-29: Contract nits
- Severity: nit
- Area: source/timewarp-mediator-contracts
- Evidence:
  - pipeline-behavior delegate parameter is named `t` (i-pipeline-behavior.cs:10); the wrappers treat a default token as "use the outer token" (request-handler-wrapper.cs:34, 40, 57, 66), which is undocumented.
  - mediator-assemblies-attribute.cs:19 silently accepts null while sibling attributes throw ArgumentNullException (mediator-behavior-attribute.cs:29, mediator-module-attribute.cs:21, mediator-scope-attribute.cs:28).
  - no-handler-exception.cs:18 builds its message before the null check at line 20 and offers no standard exception constructors.
  - `Unit._value` (unit.cs:10) uses an underscore.
  - i-stream-pipeline-behavior.cs:26 returns text says "Awaitable task" for an `IAsyncEnumerable`.
  - mediator-behavior-attribute.cs:6, 17 mention "MediatR registration order" in public XML docs, which is a legitimate comparison but is the only remaining MediatR reference; no leftover `MediatR` namespaces or names exist in either project (grep).
- Impact: Small API polish gaps.
- Proposed fix: Handle in the doc/convention passes.
- Child task: no

### C-30: Pipeline processors are only enabled by registrations present at AddMediator time
- Severity: nit
- Area: source/timewarp-mediator/registration
- Evidence: service-registrar.cs:429-443 (`RegisterBehaviorIfImplementationsExist`) inspects `services` only for `ImplementationType` registrations that exist when `AddMediator` runs (factory or instance registrations and anything added later are ignored). service-registrar.cs:395-404 registers the same two behaviors in either branch, with the order swapped.
- Impact: Exception handlers registered after `AddMediator` never run and nothing warns.
- Proposed fix: Document the ordering requirement in the AddMediator XML docs, or always register the behaviors.
- Child task: no

## Summary

Counts by severity: bug 3 (C-1, C-3, C-9), risk 9 (C-2, C-4, C-5, C-6, C-8, C-12, C-13, C-15, C-22), debt 8 (C-7, C-14, C-19, C-20, C-21, C-23, C-25, C-28), nit 10 (C-10, C-11, C-16, C-17, C-18, C-24, C-26, C-27, C-29, C-30). Total 30.

Child tasks recommended: C-1 with C-2 and C-3 (generic registration limits and timeout), C-4 with C-5/C-6/C-7 (registrar hardening and internalization), C-8, C-14, C-20 with C-21, C-22, C-23, C-25, and a single XML-doc cleanup task covering C-18, C-19, C-28 and C-29.

Overall health: The core dispatch path (Mediator, wrappers, pipeline behaviors, publishers) is a small, mostly faithful MediatR port and has no serious defect on the common path. The real bugs sit in the legacy assembly-scanning registrar, whose limits and timeout settings do not behave as documented and whose configuration lives in public static state. The contracts are clean, small and consistent, and the 14.0.0-beta.2 idempotency types are correct but under-documented. The biggest hygiene gaps are the suppressed CS1591 warning (large undocumented public surface), implementation types left public, package metadata duplicated across three props/csproj files, and a stated style (2-space, no underscores) that the code and .editorconfig do not follow.
