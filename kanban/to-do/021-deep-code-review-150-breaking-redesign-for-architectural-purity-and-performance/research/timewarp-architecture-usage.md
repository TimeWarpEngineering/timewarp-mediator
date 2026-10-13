# TimeWarp.Architecture's use of TimeWarp.Mediator

The architecture already runs on the 14.x generated stack (`14.0.0-beta.5`) with no reflection dispatcher and no MediatR. The 15.0 break therefore lands on a codebase that is already generator-first. The paths below are relative to `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-architecture/master`.

## 1. Packages and versions
- **Central pins.** `Directory.Packages.props:120-122` pins `TimeWarp.Mediator.Contracts`, `.Generators` and `.Analyzers` at `14.0.0-beta.5`.
- **No reflection package.** `TimeWarp.Mediator` itself is not referenced anywhere. Nothing calls `AddMediator`, `MediatorServiceConfiguration`, `RegisterServicesFromAssembly` or `AddOpenBehavior`.
- **Contracts and Analyzers** are referenced by foundation-contracts, foundation-server, web-contracts, web-application, web-server, api-contracts, api-application, api-server and `tests/common/timewarp-testing`.
- **Generators** are referenced only by the three host assemblies that own a generated mediator, each with `PrivateAssets="all"`:
  - `source/container-apps/web/projects/web-application/web-application.csproj:14`
  - `source/container-apps/web/projects/web-spa/web-spa.csproj:75`
  - `source/container-apps/api/projects/api-server/api-server.csproj:24`
- **Related pins.**
  - TimeWarp.State `12.0.0-beta.13` (lines 171-173). It supplies the SPA `IAction`/`ClientPipeline` scope and owns orders 100-400 in that pipeline.
  - OneOf `3.0.271` (line 66).
  - FastEndpoints `8.3.0` (line 123).
- **Template.** `timewarp-templates/source/timewarp-architecture-template/Directory.Build.props:10` only mentions TimeWarp.Mediator in the package tags. It has no separate pin.

## 2. Inventory of mediator types actually used

| Type or API | Where |
|---|---|
| `IRequest<OneOf<Response, SharedProblemDetails>>` | 68 contract files, e.g. `source/container-apps/web/features/profile/update-profile/update-profile-contracts.cs:26` |
| `IRequestHandler<Command, OneOf<...>>` | 62 handlers, e.g. `.../update-profile-handler-application.cs:102` |
| `ISender` (unscoped) | `source/foundation/foundation-server/base/base-fast-endpoint.cs:24-32`, `web/features/chat/chat-hub-server.cs:27`, `.../sign-out-browser-session-endpoint-server.cs:44`, `.../download-feedback-attachment-endpoint-server.cs:39` |
| `IPublisher<ClientPipeline>` | SPA only: `web-spa/features/base/api-handler.cs:30`, `default-api-handler.cs:25`, `file-response-api-handler.cs:23`, and about 40 state handlers |
| `ISender<TScope>` | Not used directly. TWA0022 bans SPA code from calling `Send` on it (`source/analyzers/timewarp-architecture-convention-analyzers/spa-mediator-send-analyzer.cs:26,173`). Only TimeWarp.State's generated ActionSets send. |
| `IAction` | `web-spa/features/base/i-base-action.cs:11` (`IBaseAction : IBaseRequest, IAction`), plus the constraint on SPA behaviors |
| `IPipelineBehavior<,>` | See section 3 |
| `INotification` / `INotificationHandler<>` | SPA only: `PrePipelineNotification`, `PostPipelineNotification`, `OutcomeNotification`, `ProblemDetailsNotification`, `ExceptionNotification`, and their handlers under `web-spa/features/notification/` and `web-spa/features/counter/notification/` |
| `[assembly: MediatorAssembly]` | `foundation-contracts/mediator-assembly.cs`, `web-application/mediator-assembly.cs` |
| `[assembly: MediatorScope(typeof(ClientPipeline))]` | `web-spa/mediator-behaviors.cs:45` |
| `[assembly: MediatorBehavior(..., order:, Scope=)]` | `web-application/mediator-behaviors.cs:11`, `api-server/mediator-behaviors.cs:100-101`, `web-spa/mediator-behaviors.cs:46-49` |
| `AddGeneratedMediator()` | `api-server/program.cs:109`, `web-application/web-application-mediator-registration.cs:33` |
| `AddGeneratedMediator<ClientPipeline>()` | `web-spa/spa-generated-mediator.cs:65` |
| `IStreamRequest<>` / `CreateStream` | Only in the test wrapper `tests/common/timewarp-testing/scoped-sender.cs:21-30` and `test-server-application.cs:79-85`. Both are TODO stubs or pass-throughs, and no production code streams. |
| `ISender` implemented by hand | `ScopedSender : ISender` in `tests/common/timewarp-testing/scoped-sender.cs:10`. It wraps each `Send` in a fresh `IServiceScope` and implements the full interface surface (`Send<TResponse>`, `Send<TRequest>`, `Send(object)`, `CreateStream` x2). |

Not used anywhere: `IMediator`, `ICommand`, `IQuery`, `IIdempotentCommand`, `IRequest<Unit>` and non-generic `IRequest`, `IPublisher` (unscoped), `IRequestPreProcessor`, `IRequestPostProcessor`, `IRequestExceptionHandler`, `NoHandlerException`, `MediatorModule`, and `Unit` as a mediator type.

## 3. Prescribed CQRS pattern

1. **Endpoint-centric contract.** Each use case is a static partial class, such as `UpdateProfile`, holding nested `Command` or `Query`, `Response` and `Validator`. A contract sits beside its handler in one folder per use case. The filename grammar is `<name>-contracts.cs` and `<name>-handler-application.cs` (`skills/tw-feature-placement/SKILL.md`).
2. **Request shape.** Every request is a mutable sealed class implementing `IApiRequest` (or `IAuthApiRequest`) plus `IRequest<OneOf<Response, SharedProblemDetails>>` (`skills/tw-web-api-contracts/SKILL.md:54,127,203`). `IApiRequest` carries `GetRoute()` and `GetHttpVerb()` for the generic API client (`foundation-contracts/base/i-api-request.cs`).
3. **Marker hierarchy, not mediator-derived.**
   - `IBaseMessage` and `IBaseRequest` are plain marker interfaces.
   - `BaseMessage`, `BaseRequest` and `BasePagedRequest` are marker base classes.
   - Contracts mostly use the interfaces and avoid the base classes.
4. **Results.** There are no exceptions for expected failures. Handlers return `OneOf<Response, SharedProblemDetails>`. Cancellation maps to a synthetic 499 problem on the client.
5. **Validation.** FluentValidation lives in the contract as a nested `Validator`. One `FluentValidationBehavior<,>` in `foundation-contracts/behaviors/fluent-validation-behavior.cs` runs it for every request. It builds the failure arm by reflection (`FromT1`, lines 81-83), which requires every response to be a two-arm OneOf. Handlers do not re-validate, and FastEndpoints' own validation is disabled (`web-server/program.cs:29,298`).
6. **Handlers.**
   - They are plain nested `IRequestHandler` classes with constructor-injected ports such as `IProfileStore` and `ICurrentPrincipalAccessor`.
   - There is no server-side `BaseHandler`.
   - The only handler base classes are in the SPA, and they derive from TimeWarp.State's `StateActionHandler<TAction>`: `BaseHandler<TAction>` (`web-spa/features/base/base-handler.cs`) and the `ApiHandler<TAction,TRequest,TResponse>` family.
7. **HTTP ingress.** `BaseFastEndpoint<TRequest,TResponse>` (`foundation-server/base/base-fast-endpoint.cs:20`) is the only HTTP bridge, and a source generator emits the subclasses. It takes `ISender` from `HttpContext.RequestServices` and writes the OneOf manually. Success is bare JSON and failure is `application/problem+json`. It constrains the request to `IRequest<OneOf<TResponse, SharedProblemDetails>>`.
8. **SPA dispatch.** The SPA never calls `Send`. Components and handlers call generated TimeWarp.State ActionSet methods, and TWA0022 enforces this. State actions are `IAction` and go through the `ClientPipeline` scope.
9. **Behavior registration is assembly attributes only.** A host registers behaviors with `[assembly: MediatorBehavior(typeof(X<,>), order: N)]`. Library behaviors own orders 100-400 and app behaviors start at 500. There is no DI `AddOpenBehavior`.
10. **Three generated mediators coexist.** web-application (unscoped), api-server (unscoped) and web-spa (scoped to `ClientPipeline`). Uniquely named wrapper methods exist because two generators in one compilation make `AddGeneratedMediator` ambiguous (CS0121, "Mediator issue 63"). See `web-application-mediator-registration.cs:21-26` and `spa-generated-mediator.cs:54-58`.
11. **Cross-assembly membership.** Handlers in web-application join the web-server graph via `[assembly: MediatorAssembly]`. foundation-contracts is also marked, so the host graph picks up `FluentValidationBehavior`.
12. **Notifications are SPA-only.** `INotification` is used for UI observers. Pre and post pipeline notifications are published by behaviors via `IPublisher<ClientPipeline>`. Server code never publishes.
13. **Behaviors are `IPipelineBehavior<,>` by design.** The code comments say this is so the generated mediator can weave them (`pre-pipeline-notification-request-pre-processor.cs:7-9`). They note the parameter must be named `next`. Classic pre and post processors are deliberately avoided.
14. **Generic type-argument shape.** Behaviors must be public open generics, because generated closed types reference them by name. They are constrained either `where TRequest : notnull` or `where TRequest : notnull, IAction`.

## 4. Collisions with Mediator 14 contracts
- **`IBaseRequest`** is `TimeWarp.Foundation.Features.IBaseRequest`, an empty marker. It does not collide with `IRequest`. In the SPA it is fused with `IAction` via `IBaseAction`, so a rename of Mediator's marker would need only a one-line change.
- **`ICommand` and `IQuery`** are not defined or used by the architecture. Contracts are named `Command` and `Query` as nested classes but implement `IRequest<OneOf<...>>`. There are no `IBaseHandler`, `BaseHandler<T>` (server) or `ICommand<T>` abstractions to clash with.
- **Name overlap risk.** The SPA's internal `BaseHandler<TAction>` is TimeWarp.State's concept, not Mediator's.
- **Hand-implemented `ISender`.** `ScopedSender` implements the whole interface surface. Any change to `ISender`'s shape, such as removing `Send(object)` or `CreateStream`, breaks this test helper directly.

## 5. Documentation and wishes about the mediator
- `AGENTS.md:21-22` says "Mediator calls use TimeWarp.Mediator: `IRequest<OneOf<Response, SharedProblemDetails>>`" and bans MediatR.
- `skills/tw-web-api-contracts/SKILL.md` is the primary spec (lines 25, 54, 57, 127, 139, 203, 346, 406, 434-435). `skills/tw-feature-placement/SKILL.md` covers handler placement and filename grammar.
- `source/container-apps/api/projects/api-server/generic-pipeline-behavior.cs:3-14` is the logging exemplar. It says "replace with real concerns (metrics, transactions) rather than stacking more demo behaviors".
- Design comments state friction points the 15.0 design could fix:
  - **CS0121 ambiguity.** Multiple generators produce the same `AddGeneratedMediator` extension in one compilation (`web-application-mediator-registration.cs`, `spa-generated-mediator.cs`).
  - **Reflection in the validation behavior.** `FluentValidationBehavior` builds `OneOf` via `GetMethod("FromT1").Invoke`, which hurts trimming and AOT.
  - **Behavior parameter naming.** The `next` parameter name constraint is undocumented in the contract and appears in comments only.
- Decisions are recorded in `kanban/done/037-migrate-from-mediatr-to-timewarp-mediator.md` and `kanban/done/004-002-remove-mediatr-from-api.md`. Task 215 covers `TrackEventBehavior`.
- There is no `documentation/` ADR on the mediator. That directory only has `developer/`.

## 6. Analyzer rules
- **TWA0022** (`SpaMediatorSendAnalyzer`) is the only rule that targets the mediator directly. It bans `ISender`, `ISender<TScope>` and `IMediator` `.Send` in Blazor WASM code, and it covers method-group conversions. It matches the full names `TimeWarp.Mediator.ISender` and `ISender`1`.
- **TWA0006** (endpoint coverage) requires every `[ApiRoute]` contract to have a server endpoint, or an explicit `[ClientOnlyContract]` opt-out.
- **TWA0013, TWA0014, TWA0020 and TWA0024** concern endpoint authorization on contracts.
- **TWA0015 and TWA0016** (filename grammar) require `*-handler-application.cs` for handlers.
- **TWA0009** (slice isolation) and **TWA0026** (components dispatch actions instead of calling the API directly) touch handler placement and dispatch indirectly.
- There is no rule on `IRequest<OneOf<...>>`, which the skill and `BaseFastEndpoint`'s generic constraint enforce instead.

## 7. Conclusions for 15.0

**The architecture needs:**
- `IRequest<TResponse>` and `IRequestHandler<,>` with arbitrary generic responses. OneOf-wrapped responses must keep working.
- Unscoped `ISender`, injected from DI or `RequestServices`, for FastEndpoints and hubs.
- `ISender<TScope>` and `IPublisher<TScope>`, plus `AddGeneratedMediator<TScope>()`. The SPA is built around scoped pipelines.
- `INotification` and `INotificationHandler<>` for in-process pub/sub, at least in the scoped SPA.
- `IPipelineBehavior<,>` with open-generic registration, ordering, per-scope assignment, and a constraint on `IAction`. Behaviors must be able to construct and return a response of arbitrary type. The OneOf failure arm should become reflection-free.
- `[MediatorAssembly]`, `[MediatorScope]` and `[MediatorBehavior(order:, Scope=)]`.
- `IAction` as the SPA state-action contract, since TimeWarp.State depends on it.
- Multiple generators in one compilation graph without CS0121. This is a real pain point today.
- An `ISender` surface that is cheap to wrap. `ScopedSender` and `TestServerApplication` wrap and forward every method.

**The architecture does not use:**
- The reflection dispatcher (`AddMediator`, assembly scanning, `MediatorServiceConfiguration`).
- Pre and post processors, `IRequestExceptionHandler`, and `IMediator`.
- `ICommand`, `IQuery`, `IIdempotentCommand`, `IRequest<Unit>` and the non-generic `IRequest`.
- Streams in production, only test-wrapper stubs.
- Server-side notifications.
- Mediator-provided validation, logging, authorization, transaction or caching behaviors. The architecture ships its own `FluentValidationBehavior` and `GenericPipelineBehavior`, and the SPA's `StateTransactionBehavior` comes from TimeWarp.State.

**Risk if 15.0 introduces mandatory `ICommand` and `IQuery` markers.** About 68 contracts and 62 handlers would need regeneration or a mechanical change. Contracts are the only place such a marker could live, because the architecture's own interfaces (`IApiRequest`, `IAuthApiRequest`) are the natural carriers. Handlers and `BaseFastEndpoint`'s generic constraint would also change.

**Limits of this survey.** I did not open the SPA and server test projects beyond `tests/common/timewarp-testing`. I did not check whether `TimeWarp.State` 12.x itself references `TimeWarp.Mediator` internally (likely, via `StateActionHandler` and `IAction`). That needs a separate look in the State repo. I counted handlers and contracts with `grep -l`, so the 62 and 68 figures are file counts, not type counts.
