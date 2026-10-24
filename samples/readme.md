# Samples

Every sample targets `net10.0`. `./bin/dev verify-samples` builds each one and runs every
executable sample; a non-zero exit code fails the command, and `dev workflow` runs it on every
pull request.

Reference `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators`. Registration is
`AddGeneratedMediator()`.

| Sample | Shows | Verified output |
|--------|-------|-----------------|
| `timewarp-mediator-examples-aot` | `Aot` profile: static-field dispatch, trim and AOT analyzers clean, no DI container | `aot-pong` |
| `timewarp-mediator-examples-named-pipelines` | `ISender<TScope>` named pipelines with disjoint handlers and behaviors | `client:hello`, `server:hello` |

## Packed-nupkg consumer

The samples use `ProjectReference`. The consumer that restores the real nupkgs is
`tests/timewarp-mediator-package-smoke`, which `./bin/dev pack` builds and runs against
`artifacts/packages`.
