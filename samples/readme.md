# Samples

Every sample targets `net10.0` (the 14.x floor). `./bin/dev verify-samples` builds each one and
runs every executable sample; a non-zero exit code fails the command, and `dev workflow` runs it
on every pull request. Which dispatcher each sample uses is explained in
[generated-vs-legacy.md](../documentation/generated-vs-legacy.md).

## Generated stack (`AddGeneratedMediator`)

Reference `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` only. Start here for
new code.

| Sample | Shows | Verified output |
|--------|-------|-----------------|
| `timewarp-mediator-examples-aot` | `Aot` profile: static-field dispatch, trim and AOT analyzers clean, no DI container | `aot-pong` |
| `timewarp-mediator-examples-named-pipelines` | `ISender<TScope>` named pipelines with disjoint handlers and behaviors | `client:hello`, `server:hello` |

## Legacy reflection stack (`AddMediator` / `Mediator`)

The 13.x MediatR-fork runtime in `TimeWarp.Mediator`. These samples show that the reflection
`Mediator` still resolves through third-party containers; they are kept, not extended. Each one
runs the shared `timewarp-mediator-examples` scenario and prints a Y/N feature matrix. An `N`
is a container limitation (open generics, contravariance, exception-handler lookup) and does not
fail the run; an unhandled exception does.

| Sample | Container |
|--------|-----------|
| `timewarp-mediator-examples` | Shared library: requests, notifications, streams and behaviors used by the samples below (not runnable) |
| `timewarp-mediator-examples-asp-net-core` | Microsoft.Extensions.DependencyInjection via `AddMediator` |
| `timewarp-mediator-examples-autofac` | Autofac |
| `timewarp-mediator-examples-dry-ioc` | DryIoc |
| `timewarp-mediator-examples-lamar` | Lamar |
| `timewarp-mediator-examples-light-inject` | LightInject |
| `timewarp-mediator-examples-simple-injector` | SimpleInjector |
| `timewarp-mediator-examples-stashbox` | Stashbox |
| `timewarp-mediator-examples-windsor` | Castle Windsor |
| `timewarp-mediator-examples-publish-strategies` | Custom `INotificationPublisher` strategies on Microsoft DI |

## Packed-nupkg consumer

The samples use `ProjectReference`. The consumer that restores the real nupkgs is
`tests/timewarp-mediator-package-smoke`, which `./bin/dev pack` builds and runs against
`artifacts/packages`.
