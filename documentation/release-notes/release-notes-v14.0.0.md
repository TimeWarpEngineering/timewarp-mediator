# TimeWarp.Mediator 14.0.0 release notes

14.0.0 is in beta. This file collects changes across the 14.0.0 betas; task 013-017 will add the
beta.1 to beta.4 history. **14.0.0-beta is not a drop-in for 13.0.0** (see
[generated-vs-legacy.md](../generated-vs-legacy.md)).

## Breaking changes

### Smaller public API in the `TimeWarp.Mediator` runtime package

Types that only the runtime uses are no longer public. Code that only uses `IMediator`, `ISender`,
`IPublisher`, `AddMediator(...)`, `MediatorServiceConfiguration` and the pipeline contracts needs
no change. `TimeWarp.Mediator.Contracts`, `.Generators` and `.Analyzers` are unchanged.

| Type | Change | Replacement |
|---|---|---|
| `ServiceFactory` delegate | Removed. Nothing in the library used it. | Resolve through `IServiceProvider`. If a container sample needs a resolver delegate, declare it in your own code. |
| `TimeWarp.Mediator.Wrappers.RequestHandlerBase`, `RequestHandlerWrapper`, `RequestHandlerWrapper<TResponse>`, `RequestHandlerWrapperImpl<TRequest>`, `RequestHandlerWrapperImpl<TRequest, TResponse>` | Now `internal` | Call `ISender.Send(...)`. |
| `TimeWarp.Mediator.Wrappers.NotificationHandlerWrapper`, `NotificationHandlerWrapperImpl<TNotification>` | Now `internal` | Call `IPublisher.Publish(...)`. To change how handlers run, implement `INotificationPublisher` or override `Mediator.PublishCore`. |
| `TimeWarp.Mediator.Registration.ServiceRegistrar` (including `AddMediatorClassesWithTimeout`, `AddMediatorClasses`, `GenerateCombinations`, `AddRequiredServices`) | Now `internal` | Call `services.AddMediator(cfg => ...)`. |

These types stay public because consumers use them directly: `Mediator`, `INotificationPublisher`,
`NotificationHandlerExecutor` (part of the `INotificationPublisher` and `PublishCore` signatures),
`ForeachAwaitPublisher` and `TaskWhenAllPublisher` (values for
`MediatorServiceConfiguration.NotificationPublisher`), `MediatorServiceConfiguration`,
`OpenBehavior`, `RequestExceptionActionProcessorStrategy`, `RequestExceptionHandlerState<TResponse>`,
the pre/post/exception processor interfaces, and the four built-in behaviors. Containers other
than Microsoft.Extensions.DependencyInjection register those behaviors by hand.
