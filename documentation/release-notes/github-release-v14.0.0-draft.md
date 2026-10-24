# DRAFT — do not publish

Paste this over the body `dev release` generates for tag `v14.0.0`. Publish only after all four
packages are on nuget.org and indexed, and only after Steven says to. Nothing in this folder
has been posted.

Headline: **TimeWarp Mediator 14.0: The Rewrite That Pays You Back**

The version is 14.0.0, not 1.0. 13.0.0 was the MediatR rename. 14.0.0 adds a second dispatcher.

---

## TimeWarp Mediator 14.0.0

14.0.0 is the stable source-generated mediator, shipped as four packages at one version:

| Package | Role |
|---|---|
| `TimeWarp.Mediator.Contracts` | Requests, commands, queries, notifications, handlers, pipeline contracts, membership attributes. net10.0. |
| `TimeWarp.Mediator.Generators` | Compile-time mediator and `AddGeneratedMediator()`. Bundles the analyzers. netstandard2.0. |
| `TimeWarp.Mediator.Analyzers` | TWM001–TWM004 for libraries that do not run the generator. netstandard2.0. |
| `TimeWarp.Mediator` | Reflection runtime and `AddMediator()`. net10.0. Not required by a generated-only host. |

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

```csharp
public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}

services.AddGeneratedMediator();
string reply = await provider.GetRequiredService<ISender>().Send(new Ping());
```

`AddGeneratedMediator()` is internal to the host. Call it from the host project.

On the comparison baseline recorded 2026-10-12 (commit `d478415`, BenchmarkDotNet 0.15.8,
.NET 10.0.11, AMD Ryzen 9 7950X3D, Ping/Pong, no pipeline behaviors), generated `Send` is
**29.9 ns and 96 B** against reflection `Send` at **69.0 ns and 224 B** (ratio **0.43** for
both time and bytes). The same run shows martinothamar's generator at 8.8 ns and 24 B. This
release does not claim to beat that. Numbers live in
[comparison-baseline.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/benchmarks/comparison-baseline.md).

The generated Host profile is trim and NativeAOT safe. The reflection `AddMediator()` path is
not: it is annotated `RequiresUnreferencedCode` and `RequiresDynamicCode` (IL2026 / IL3050).

## Breaking changes

14.0.0 is not a drop-in for 13.0.0.

- `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` target **net10.0 only**. The
  netstandard2.0 and net6.0 assets are gone. Projects that cannot move stay on 13.0.0.
- `AddMediator()` does **not** register the generated mediator. Source generation, named
  pipelines, and AOT-clean dispatch need `AddGeneratedMediator()` /
  `AddGeneratedMediator<TScope>()`.
- Generated dispatch does not run streams (`CreateStream` / `IStreamRequest`), pre/post
  processors, or the exception cascade. Those report TWM009. Hosts that need them stay on
  `AddMediator()`, or move that work into a handler or an `IPipelineBehavior` first.
- On the Aot and Link profiles, `[assembly: MediatorBehavior]` is not woven (TWM005), and
  there is no `AddGeneratedMediator()`. Handlers are `new Handler()` singletons.
- `ServiceFactory` is removed; resolve through `IServiceProvider`. The handler wrappers and
  `ServiceRegistrar` are now internal. Call `ISender.Send`, `IPublisher.Publish`, and
  `services.AddMediator(...)`.
- `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` declare `Unlicense AND Apache-2.0`.
  Generators and Analyzers stay `Unlicense`. Every package packs `notice` and `unlicense`.

The beta line (beta.1 through the work merged after beta.4) is folded into this tag: idempotent
command markers, struct requests, `ICommand<Unit>` and array responses, and internal generated
types so two hosts in one compilation do not collide (CS0436 / CS0121). Detail is in
[release-notes-v14.0.0.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md).

## Migration

Full matrix and steps:
[migration.md — 13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

1. Reference `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` at 14.0.0. Drop
   `TimeWarp.Mediator` when nothing calls `AddMediator()`. A handler library that does not run
   the generator references Contracts and `TimeWarp.Mediator.Analyzers`.
2. Add `[assembly: MediatorAssembly]` on every library whose handlers the host should see.
   The host that references Generators is a member already.
3. Replace `cfg.AddBehavior` / `cfg.AddOpenBehavior` with
   `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`.
4. Replace `services.AddMediator(...)` with `services.AddGeneratedMediator()` in the host.
5. Build and fix every TWM009. Diagnostics:
   [diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md).
6. Optional: `[MediatorScope(typeof(ClientPipeline))]` and
   `AddGeneratedMediator<ClientPipeline>()` for a named pipeline.
7. Optional: `<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>` and the Aot limits in
   [generated-vs-legacy.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/documentation/generated-vs-legacy.md).

From MediatR rather than from 13.x: `using MediatR;` becomes `using TimeWarp.Mediator;`, and
`AddMediatR` becomes `AddMediator`. That rename is the 13.x guide at the top of the same file.
Doing it does not put you on the generated stack.

## Tag-day doc flips

These pages still describe 14.0.0 as a prerelease, which is true until this tag is the stable
package. Update them in the version-bump commit, not after the post goes out:

- `documentation/generated-vs-legacy.md` ("14.0.0-beta is not a drop-in", issue
  [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays open until
  the stable package exists)
- `documentation/release-notes/release-notes-v14.0.0.md` (the "Unreleased" and "in beta" lines)
- `<Version>` in `source/Directory.Build.props` (still `14.0.0-beta.4` when this draft was written)

## Not in this post

No NuGet push, no `gh release create`, no blog stub, no X post, no Nostr post. The launch
order after Steven's OK: GitHub Release once nuget.org has indexed 14.0.0, then the blog, then
X and Nostr around 20:00–22:00 Asia/Bangkok. The follow-up example post is 2–5 days later.
