# Samples

Every sample targets `net10.0`. `./bin/dev verify-samples` builds each one and runs every
executable sample. A non-zero exit code fails the command, and `dev workflow` runs it on every
pull request.

Reference `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators`. Registration is
`AddGeneratedMediator()` from the generated namespace. The host supplies `IServiceProvider`.
Behaviors are woven.

| Sample | Shows | Verified output |
|--------|-------|-----------------|
| `timewarp-mediator-examples-aot` | NativeAOT publish with trim and AOT analyzers as errors, `AddGeneratedMediator()`, and a behavior that runs on both sends | `aot-pong` |
| `timewarp-mediator-examples-named-pipelines` | `ISender<TScope>` named pipelines with disjoint handlers and behaviors | `client:hello`, `server:hello` |

## Packed-nupkg consumer

The samples use `ProjectReference`. The consumer that restores the real nupkgs is
`tests/timewarp-mediator-package-smoke`, which `./bin/dev pack` builds and runs against
`artifacts/packages`.
