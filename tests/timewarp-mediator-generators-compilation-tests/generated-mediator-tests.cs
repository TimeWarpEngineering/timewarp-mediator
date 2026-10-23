#region Purpose
// Generated mediator behavior beyond visibility: membership attributes, profile/namespace overrides,
// TWM001–TWM003 from the generator, notification order and base-type delivery, guards (013-012, G-23).
#endregion

#region Design
// Runtime cases use the Aot profile so the emitted Mediator has a parameterless constructor and needs
// no service provider. Each test compiles and loads its own collectible assembly, so the static Log
// in the snippet is never shared between tests.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class GeneratedMediatorTests
{
  [Fact]
  public void InterfaceSend_ReturnsDispatchWithoutBoxing_AndBehaviorsUseCachedDelegates()
  {
    HostCompilation host = HostCompiler.Compile("DispatchShape", StampBehaviorSource + PingSource);
    Problems(host).ShouldBeEmpty();
    string generated = string.Join(
        "\n",
        host.Compilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains(".g.cs", StringComparison.Ordinal))
            .Select(tree => tree.GetText().ToString()));
    generated.ShouldContain("Unsafe.As<");
    generated.ShouldContain("typeof(TResponse) == typeof(");
    generated.ShouldContain("AdaptResponse<");
    generated.ShouldNotContain("SendCore");
    generated.ShouldNotContain("ThreadStatic");
    generated.ShouldContain("private readonly global::TimeWarp.Mediator.MessageHandlerDelegate<");
    generated.ShouldContain("[assembly: global::TimeWarp.Mediator.GeneratedMediatorAttribute(typeof(global::");
    string registration = host.Compilation.SyntaxTrees
        .Single(tree => tree.FilePath.Contains("MediatorServiceCollectionExtensions.g.cs", StringComparison.Ordinal))
        .GetText()
        .ToString();
    registration.ShouldNotContain("IRequestHandler");
    registration.ShouldNotContain("IPipelineBehavior");
    registration.ShouldNotContain("INotificationHandler");
    registration.ShouldContain(".Chain_");
    registration.ShouldContain("namespace TimeWarp.Mediator.Generated;");
    registration.ShouldContain("class TimeWarpMediatorGeneratedServiceCollectionExtensions");
    host.Manifest().Request("App.Ping").Kind.ShouldBe("Request");
  }

  [Fact]
  public void ReferencedLibrary_JoinsTheGraphOnlyWithMediatorAssembly()
  {
    MetadataReference member = HostCompiler.CompileLibrary("MemberLibrary", MemberLibraryAttribute + ModuleLibrarySource);
    MetadataReference moduleOnly = HostCompiler.CompileLibrary("ModuleOnlyLibrary", ModuleLibrarySource);

    HostCompilation linked = HostCompiler.Compile("MemberHost", PingSource, "Host", member);
    HostCompilation unlinked = HostCompiler.Compile("ModuleOnlyHost", PingSource, "Host", moduleOnly);

    Problems(linked).ShouldBeEmpty();
    ManifestDocument manifest = linked.Manifest();
    manifest.Request("Orders.PlaceOrder").Handler.ShouldBe("Orders.PlaceOrderHandler");
    manifest.Membership.ShouldBe(new[] { "MemberHost", "MemberLibrary" });
    Problems(unlinked).ShouldBeEmpty();
    unlinked.Manifest().Requests.Select(r => r.Request).ShouldBe(new[] { "App.Ping" });
    unlinked.Manifest().Membership.ShouldBe(new[] { "ModuleOnlyHost" });
  }

  [Fact]
  public async Task ReferencedMemberHandler_IsDispatched()
  {
    byte[] image = HostCompiler.CompileLibraryImage("RuntimeModuleLibrary", MemberLibraryAttribute + ModuleLibrarySource);
    HostCompilation host = HostCompiler.Compile(
        "RuntimeModuleHost",
        PingSource,
        "Aot",
        MetadataReference.CreateFromImage(image));
    Assembly assembly = Load(host, "RuntimeModuleHost", image);
    IMediator mediator = CreateMediator(assembly);
    Assembly library = AssemblyLoadContext.GetLoadContext(assembly)!.Assemblies.Single(a => a.GetName().Name == "RuntimeModuleLibrary");

    object? response = await mediator.Send(Activator.CreateInstance(library.GetType("Orders.PlaceOrder", true)!)!);

    response.ShouldBe(42);
  }

  [Theory]
  [InlineData("Host")]
  [InlineData("Aot")]
  [InlineData("Link")]
  public void ProfileAndNamespaceOverrides_AreHonored(string profile)
  {
    HostCompilation host = HostCompiler.CompileWithReferences(
        "Override" + profile,
        PingSource,
        profile,
        HostCompiler.GetBaseReferences(),
        "Custom.Generated");

    Problems(host).ShouldBeEmpty();
    host.GetType("Custom.Generated.Mediator");
    host.Compilation.Assembly.GetTypeByMetadataName(HostCompiler.DefaultNamespace + ".Mediator").ShouldBeNull();
    ManifestDocument manifest = host.Manifest("Custom.Generated");
    manifest.Version.ShouldBe(1);
    manifest.Profile.ShouldBe(profile);
    manifest.Namespace.ShouldBe("Custom.Generated");
  }

  [Fact]
  public void Twm001FromTheGenerator_IsAtTheRequest()
  {
    HostCompilation host = HostCompiler.Compile("GeneratorTwm001", PingSource + OrphanSource);

    Diagnostic diagnostic = Problems(host).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM001");
    diagnostic.GetMessage().ShouldBe("Request 'App.Orphan' has no handler");
    Span(diagnostic).ShouldBe("Orphan");
  }

  [Fact]
  public void Twm002FromTheGenerator_IsAtTheSecondHandler()
  {
    HostCompilation host = HostCompiler.Compile("GeneratorTwm002", PingSource + DuplicateSource);

    Diagnostic diagnostic = Problems(host).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM002");
    diagnostic.GetMessage().ShouldBe("Request 'App.Ping' has multiple handlers: App.PingHandler, App.SecondPingHandler");
    Span(diagnostic).ShouldBe("SecondPingHandler");
  }

  [Fact]
  public void Twm003FromTheGenerator_IsAtTheHandler()
  {
    HostCompilation host = HostCompiler.Compile("GeneratorTwm003", PingSource + ScopeMismatchSource);

    Diagnostic diagnostic = Problems(host).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM003");
    diagnostic.GetMessage().ShouldBe(
        "Handler 'App.ScopedHandler' is assigned to pipeline 'App.ServerPipeline' but message 'App.Scoped' is assigned to pipeline 'App.ClientPipeline'");
    Span(diagnostic).ShouldBe("ScopedHandler");
  }

  [Fact]
  public async Task Publish_DeliversToBaseTypeHandlers_MostDerivedFirst_InDisplayNameOrder()
  {
    HostCompilation host = HostCompiler.Compile("PublishOrder", NotificationSource, "Aot");
    Assembly assembly = Load(host, "PublishOrder");
    IMediator mediator = CreateMediator(assembly);

    await mediator.Publish(Activator.CreateInstance(assembly.GetType("Notes.Renamed", true)!)!);
    string[] renamed = Events(assembly);
    Clear(assembly);
    await mediator.Publish(Activator.CreateInstance(assembly.GetType("Notes.Changed", true)!)!);
    string[] changed = Events(assembly);

    renamed.ShouldBe(new[] { "Renamed:RenamedHandler", "Changed:AuditHandler", "Changed:CacheHandler" });
    changed.ShouldBe(new[] { "Changed:AuditHandler", "Changed:CacheHandler" });
  }

  [Fact]
  public async Task StructRequest_IsDispatched()
  {
    HostCompilation host = HostCompiler.Compile("StructDispatch", StructSource, "Aot");
    Assembly assembly = Load(host, "StructDispatch");
    IMediator mediator = CreateMediator(assembly);

    object? response = await mediator.Send(Activator.CreateInstance(assembly.GetType("App.Measure", true)!)!);

    response.ShouldBe(7);
  }

  [Fact]
  public async Task NullArguments_ThrowArgumentNullException()
  {
    IMediator mediator = CreateMediator(HostCompiler.Compile("NullGuards", PingSource, "Aot"));

    await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send((object) null!));
    await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send((IRequest<string>) null!));
    await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Publish((object) null!));
    await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Publish((INotification) null!));
  }

  [Fact]
  public async Task UnknownRequest_ThrowsNoHandlerException()
  {
    IMediator mediator = CreateMediator(HostCompiler.Compile("UnknownRequest", PingSource, "Aot"));

    await Should.ThrowAsync<NoHandlerException>(async () => await mediator.Send(new object()));
  }

  [Fact]
  public void AddGeneratedMediatorWithUnknownScope_ThrowsArgumentException()
  {
    HostCompilation host = HostCompiler.Compile("UnknownScope", PingSource + ScopedSource, "Host");
    Problems(host).ShouldBeEmpty();
    Assembly assembly = Load(host, "UnknownScope");
    MethodInfo addScoped = assembly
        .GetType("TimeWarp.Mediator.Generated.TimeWarpMediatorGeneratedServiceCollectionExtensions", true)!
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(m => m.Name == "AddGeneratedMediator" && m.IsGenericMethodDefinition);
    Microsoft.Extensions.DependencyInjection.ServiceCollection services = new();

    TargetInvocationException known = Should.Throw<TargetInvocationException>(
        () => addScoped.MakeGenericMethod(typeof(string)).Invoke(null, new object[] { services }));
    addScoped.MakeGenericMethod(assembly.GetType("App.ClientPipeline", true)!).Invoke(null, new object[] { services });

    ArgumentException exception = known.InnerException.ShouldBeOfType<ArgumentException>();
    exception.ParamName.ShouldBe("TScope");
    exception.Message.ShouldContain("System.String");
    services.ShouldNotBeEmpty();
  }

  private static Diagnostic[] Problems(HostCompilation host)
  {
    return host.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ToArray();
  }

  private static string Span(Diagnostic diagnostic)
  {
    diagnostic.Location.SourceTree.ShouldNotBeNull();
    return diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
  }

  private static IMediator CreateMediator(HostCompilation host)
  {
    return CreateMediator(Load(host, host.Compilation.AssemblyName!));
  }

  private static IMediator CreateMediator(Assembly assembly)
  {
    return (IMediator) Activator.CreateInstance(assembly.GetType(HostCompiler.DefaultNamespace + ".Mediator", true)!)!;
  }

  private static string[] Events(Assembly assembly)
  {
    return ((List<string>) assembly.GetType("Notes.Log", true)!.GetField("Events")!.GetValue(null)!).ToArray();
  }

  private static void Clear(Assembly assembly)
  {
    ((List<string>) assembly.GetType("Notes.Log", true)!.GetField("Events")!.GetValue(null)!).Clear();
  }

  // Dependencies load into the same context first, so the host resolves its member libraries.
  private static Assembly Load(HostCompilation host, string assemblyName, params byte[][] dependencies)
  {
    using MemoryStream stream = new();
    EmitResult result = host.Compilation.Emit(stream);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
    stream.Position = 0;
    AssemblyLoadContext context = new(assemblyName, isCollectible: true);
    foreach (byte[] dependency in dependencies)
    {
      using MemoryStream image = new(dependency);
      context.LoadFromStream(image);
    }

    return context.LoadFromStream(stream);
  }

  private const string StampBehaviorSource = @"
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

[assembly: MediatorBehavior(typeof(App.StampBehavior<,>))]

namespace App
{
    public sealed class StampBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public ValueTask<TResponse> Handle(
            TRequest request,
            MessageHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken)
        {
            return next(request, cancellationToken);
        }
    }
}
";

  private const string PingSource = @"
namespace App
{
    public sealed class Ping : TimeWarp.Mediator.IRequest<string>
    {
    }

    public sealed class PingHandler : TimeWarp.Mediator.IRequestHandler<Ping, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(Ping request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(""pong"");
    }
}
";

  private const string OrphanSource = @"
namespace App
{
    public sealed class Orphan : TimeWarp.Mediator.IRequest<int>
    {
    }
}
";

  private const string DuplicateSource = @"
namespace App
{
    public sealed class SecondPingHandler : TimeWarp.Mediator.IRequestHandler<Ping, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(Ping request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(""second"");
    }
}
";

  private const string ScopeMismatchSource = @"
namespace App
{
    public sealed class ClientPipeline
    {
    }

    public sealed class ServerPipeline
    {
    }

    [TimeWarp.Mediator.MediatorScope(typeof(ClientPipeline))]
    public sealed class Scoped : TimeWarp.Mediator.IRequest<int>
    {
    }

    [TimeWarp.Mediator.MediatorScope(typeof(ServerPipeline))]
    public sealed class ScopedHandler : TimeWarp.Mediator.IRequestHandler<Scoped, int>
    {
        public System.Threading.Tasks.ValueTask<int> Handle(Scoped request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(1);
    }
}
";

  private const string ScopedSource = @"
namespace App
{
    public sealed class ClientPipeline
    {
    }

    [TimeWarp.Mediator.MediatorScope(typeof(ClientPipeline))]
    public sealed class Scoped : TimeWarp.Mediator.IRequest<int>
    {
    }

    [TimeWarp.Mediator.MediatorScope(typeof(ClientPipeline))]
    public sealed class ScopedHandler : TimeWarp.Mediator.IRequestHandler<Scoped, int>
    {
        public System.Threading.Tasks.ValueTask<int> Handle(Scoped request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(1);
    }
}
";

  private const string StructSource = @"
namespace App
{
    public readonly struct Measure : TimeWarp.Mediator.IRequest<int>
    {
    }

    public sealed class MeasureHandler : TimeWarp.Mediator.IRequestHandler<Measure, int>
    {
        public System.Threading.Tasks.ValueTask<int> Handle(Measure request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(7);
    }
}
";

  private const string MemberLibraryAttribute = "[assembly: TimeWarp.Mediator.MediatorAssembly]";

  // A referenced assembly joins a host only with [assembly: MediatorAssembly].
  private const string ModuleLibrarySource = @"
namespace Orders
{
    public sealed class PlaceOrder : TimeWarp.Mediator.IRequest<int>
    {
    }

    public sealed class PlaceOrderHandler : TimeWarp.Mediator.IRequestHandler<PlaceOrder, int>
    {
        public System.Threading.Tasks.ValueTask<int> Handle(PlaceOrder request, System.Threading.CancellationToken cancellationToken)
            => System.Threading.Tasks.ValueTask.FromResult(42);
    }
}
";

  private const string NotificationSource = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace Notes
{
    public static class Log
    {
        public static readonly List<string> Events = new List<string>();
    }

    public class Changed : INotification
    {
    }

    public sealed class Renamed : Changed
    {
    }

    public sealed class CacheHandler : INotificationHandler<Changed>
    {
        public ValueTask Handle(Changed notification, CancellationToken cancellationToken)
        {
            Log.Events.Add(""Changed:CacheHandler"");
            return default;
        }
    }

    public sealed class AuditHandler : INotificationHandler<Changed>
    {
        public ValueTask Handle(Changed notification, CancellationToken cancellationToken)
        {
            Log.Events.Add(""Changed:AuditHandler"");
            return default;
        }
    }

    public sealed class RenamedHandler : INotificationHandler<Renamed>
    {
        public ValueTask Handle(Renamed notification, CancellationToken cancellationToken)
        {
            Log.Events.Add(""Renamed:RenamedHandler"");
            return default;
        }
    }
}
";
}