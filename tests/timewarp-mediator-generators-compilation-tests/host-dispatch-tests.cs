#region Purpose
// Dispatch with no behaviors: the scoped cache throws once the scope is disposed.
#endregion

#region Design
// The generators test assembly weaves open behaviors onto every request, so the no-behavior
// path is compiled here. HandlerCache is scoped and reused by the dispatcher instance. Each
// handler slot is filled on the first send of that request, so a transient registration is
// resolved once per cache and an unresolvable handler does not block the others. Synchronous
// throws stay synchronous; 021-007 records that.
#endregion

using Microsoft.Extensions.DependencyInjection;

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class HostDispatchTests
{
  [Fact]
  public void NoBehaviorDispatch_DoesNotCacheOnTheThread()
  {
    HostCompilation host = HostCompiler.Compile("NoBehaviorShape", TokenSource);
    Problems(host).ShouldBeEmpty();
    string generated = string.Join(
        "\n",
        host.Compilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains("Mediator.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.GetText().ToString()));

    generated.ShouldNotContain("ThreadStatic");
    generated.ShouldNotContain("CachedHandler_");
    generated.ShouldContain("class HandlerCache");
    generated.ShouldContain("??= global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
    int cacheClass = generated.IndexOf("sealed class HandlerCache", StringComparison.Ordinal);
    int constructor = generated.IndexOf("public HandlerCache", cacheClass, StringComparison.Ordinal);
    int scopeAssignment = generated.IndexOf("Scope = scope;", constructor, StringComparison.Ordinal);
    generated[constructor..scopeAssignment].ShouldNotContain("GetRequiredService");
  }

  [Fact]
  public async Task ScopedHandler_StaysInsideTheScope_AndDisposedScopeThrows()
  {
    Assembly assembly = Load(HostCompiler.Compile("ScopedLifetime", TokenSource));
    await using ServiceProvider provider = Provider(assembly);
    using IServiceScope first = provider.CreateScope();
    using IServiceScope second = provider.CreateScope();
    ISender firstSender = Sender(first, assembly);
    ISender secondSender = Sender(second, assembly);
    IRequest<string> request = Request(assembly);

    string firstToken = await firstSender.Send(request);
    string firstAgain = await firstSender.Send(request);
    string secondToken = await secondSender.Send(request);
    first.Dispose();

    firstAgain.ShouldBe(firstToken);
    secondToken.ShouldNotBe(firstToken);
    Should.Throw<ObjectDisposedException>(() => firstSender.Send(request));
    (await secondSender.Send(request)).ShouldBe(secondToken);
  }

  [Fact]
  public async Task TransientHandler_IsResolvedOncePerCache()
  {
    Assembly assembly = Load(HostCompiler.Compile("TransientHandler", CountingSource));
    Type handler = assembly.GetType("App.CountingHandler", true)!;
    ServiceCollection services = new();
    AddGenerated(assembly, services);
    services.AddTransient(handler);
    await using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = Sender(scope, assembly);
    IRequest<int> request = (IRequest<int>) Activator.CreateInstance(assembly.GetType("App.CountPing", true)!)!;

    int first = await sender.Send(request);
    int second = await sender.Send(request);

    first.ShouldBe(1);
    second.ShouldBe(1);
    handler.GetField("Count")!.GetValue(null).ShouldBe(1);
  }

  [Fact]
  public async Task UnresolvableHandler_DoesNotBlockTheOtherRequest()
  {
    Assembly assembly = Load(HostCompiler.Compile("LazyHandlers", TwoHandlerSource));
    await using ServiceProvider provider = Provider(assembly);
    using IServiceScope scope = provider.CreateScope();
    ISender sender = Sender(scope, assembly);
    Type goodHandler = assembly.GetType("App.GoodHandler", true)!;
    Type badHandler = assembly.GetType("App.BadHandler", true)!;
    IRequest<string> good = (IRequest<string>)Activator.CreateInstance(assembly.GetType("App.GoodPing", true)!)!;
    IRequest<int> bad = (IRequest<int>)Activator.CreateInstance(assembly.GetType("App.BadPing", true)!)!;

    string result = await sender.Send(good);

    result.ShouldBe("ok");
    goodHandler.GetField("Count")!.GetValue(null).ShouldBe(1);
    badHandler.GetField("Count")!.GetValue(null).ShouldBe(0);
    Should.Throw<InvalidOperationException>(() => sender.Send(bad));
    badHandler.GetField("Count")!.GetValue(null).ShouldBe(0);
    (await sender.Send(good)).ShouldBe("ok");
    goodHandler.GetField("Count")!.GetValue(null).ShouldBe(1);
  }

  [Fact]
  public void SynchronousHandlerThrow_EscapesSend()
  {
    Assembly assembly = Load(HostCompiler.Compile("SyncThrow", SyncThrowSource));
    using ServiceProvider provider = Provider(assembly);
    using IServiceScope scope = provider.CreateScope();
    ISender sender = Sender(scope, assembly);
    IRequest<string> request = (IRequest<string>) Activator.CreateInstance(assembly.GetType("App.Boom", true)!)!;

    Should.Throw<InvalidOperationException>(() => sender.Send(request));
  }

  private static Diagnostic[] Problems(HostCompilation host)
  {
    return host.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ToArray();
  }

  private static ServiceProvider Provider(Assembly assembly)
  {
    ServiceCollection services = new();
    AddGenerated(assembly, services);
    return services.BuildServiceProvider();
  }

  private static void AddGenerated(Assembly assembly, IServiceCollection services)
  {
    MethodInfo add = assembly
        .GetType("TimeWarp.Mediator.Generated.TimeWarpMediatorGeneratedServiceCollectionExtensions", true)!
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == "AddGeneratedMediator" && !method.IsGenericMethod);
    add.Invoke(null, new object[] { services });
  }

  private static ISender Sender(IServiceScope scope, Assembly assembly)
  {
    Type mediator = assembly.GetType(HostCompiler.DefaultNamespace + ".Mediator", true)!;
    return (ISender) scope.ServiceProvider.GetRequiredService(mediator);
  }

  private static IRequest<string> Request(Assembly assembly)
  {
    return (IRequest<string>) Activator.CreateInstance(assembly.GetType("App.TokenPing", true)!)!;
  }

  private static Assembly Load(HostCompilation host)
  {
    using MemoryStream stream = new();
    EmitResult result = host.Compilation.Emit(stream);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
    stream.Position = 0;
    AssemblyLoadContext context = new(host.Compilation.AssemblyName, isCollectible: true);
    return context.LoadFromStream(stream);
  }

  private const string TokenSource = @"
namespace App
{
    public sealed class TokenPing : TimeWarp.Mediator.IRequest<string>
    {
    }

    public sealed class TokenHandler : TimeWarp.Mediator.IRequestHandler<TokenPing, string>
    {
        public string Token { get; } = System.Guid.NewGuid().ToString();

        public System.Threading.Tasks.ValueTask<string> Handle(TokenPing request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(Token);
    }
}
";

  private const string CountingSource = @"
namespace App
{
    public sealed class CountPing : TimeWarp.Mediator.IRequest<int>
    {
    }

    public sealed class CountingHandler : TimeWarp.Mediator.IRequestHandler<CountPing, int>
    {
        public static int Count;

        public CountingHandler()
        {
            System.Threading.Interlocked.Increment(ref Count);
        }

        public System.Threading.Tasks.ValueTask<int> Handle(CountPing request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(Count);
    }
}
";

  private const string TwoHandlerSource = @"
namespace App
{
    public sealed class GoodPing : TimeWarp.Mediator.IRequest<string>
    {
    }

    public sealed class GoodHandler : TimeWarp.Mediator.IRequestHandler<GoodPing, string>
    {
        public static int Count;

        public GoodHandler()
        {
            System.Threading.Interlocked.Increment(ref Count);
        }

        public System.Threading.Tasks.ValueTask<string> Handle(GoodPing request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(""ok"");
    }

    public sealed class MissingDep
    {
    }

    public sealed class BadPing : TimeWarp.Mediator.IRequest<int>
    {
    }

    public sealed class BadHandler : TimeWarp.Mediator.IRequestHandler<BadPing, int>
    {
        public static int Count;

        public BadHandler(MissingDep missing)
        {
            _ = missing;
            System.Threading.Interlocked.Increment(ref Count);
        }

        public System.Threading.Tasks.ValueTask<int> Handle(BadPing request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(Count);
    }
}
";

  private const string SyncThrowSource = @"
namespace App
{
    public sealed class Boom : TimeWarp.Mediator.IRequest<string>
    {
    }

    public sealed class BoomHandler : TimeWarp.Mediator.IRequestHandler<Boom, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(Boom request, System.Threading.CancellationToken cancellationToken)
            => throw new System.InvalidOperationException(""sync"");
    }
}
";
}
