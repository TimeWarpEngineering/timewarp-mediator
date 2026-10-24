# Migration

## Generated host

A host references `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` at the same
version and calls `AddGeneratedMediator()`.

Libraries that declare messages and handlers, and do not run the generator, reference Contracts
and `TimeWarp.Mediator.Analyzers`, and opt in with `[assembly: MediatorAssembly]`.

- **Behaviors.** Register with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. The optional
  `Scope` argument selects one named pipeline.
- **Registration.** `AddGeneratedMediator()` is `internal` to the host project. Call it from the
  host. Handlers and behaviors are scoped.
- **Named pipelines.** Mark requests with `[MediatorScope(typeof(ClientPipeline))]` and call
  `AddGeneratedMediator<ClientPipeline>()`. See
  [m2-named-pipelines.md](./documentation/m2-named-pipelines.md).
- **Unsupported kinds.** Streams, pre/post processors, and the exception cascade are not
  dispatched. The generator reports **TWM009**. Move that logic into a request handler or an
  `IPipelineBehavior`.
- **AOT.** Set `<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>`. Profile limits are in
  [m1-generated-mediator.md](./documentation/m1-generated-mediator.md).
- **From MediatR.** Change `using MediatR;` to `using TimeWarp.Mediator;`. The host calls
  `AddGeneratedMediator()`.

Contracts targets net10.0. Generators and Analyzers are netstandard2.0 and need Roslyn 4.8 or
later.

Every diagnostic is described in [documentation/diagnostics.md](./documentation/diagnostics.md).

## Support

For issues or questions about the migration:

- GitHub Issues: https://github.com/TimeWarpEngineering/timewarp-mediator/issues
