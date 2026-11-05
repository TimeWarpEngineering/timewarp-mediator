#region Purpose
// A request in source names a handler another generator emits, and a missing name is TWM012.
// Post-initialization output is visible to this generator, so that test covers dedupe.
// RegisterSourceOutput is not visible, so that test covers the declared-name path.
#endregion

#region Design
// The mediator generator cannot see another generator's RegisterSourceOutput. A post-initialization
// source is visible, and a real IRequestHandler of the same type must stay one handler.
// The final compilation contains both, so Send runs. The metadata-only test is the reference
// assembly a ProjectReference passes to csc. The name comes from a constructor parameter named
// handlerTypeName, including its default. A getter-only image is TWM012. A source handler of the
// same name would bind through IRequestHandler and hide a failed read. A missing name under an
// existing namespace is CS0234 in Mediator.g.cs; TWM012 is the diagnostic on the attribute.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class GeneratedHandlerTests
{
  [Fact]
  public async Task DeclaredHandler_FromPostInitializationOutput_DedupesTheVisibleHandler()
  {
    const string handler = """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;

        namespace App
        {
            public sealed class PingHandler : IRequestHandler<Ping, string>
            {
                public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
                    ValueTask.FromResult("pong");
            }
        }
        """;

    HostCompilation host = HostCompiler.Compile(
        "DeclaredSyntax",
        SyntaxHost,
        new EmittedHandler(handler));

    Problems(host).ShouldBeEmpty();
    MediatorSource(host).ShouldContain("global::App.PingHandler");
    Assembly assembly = Load(host);
    string result = await Send(assembly, "App.Ping");
    result.ShouldBe("pong");
  }

  [Fact]
  public async Task DeclaredHandler_FromMetadataOnlyConstructorArgument_IsDispatched()
  {
    byte[] referenceImage = HostCompiler.CompileLibraryImage("StateLib", AttributeLibrary, metadataOnly: true);
    byte[] runtimeImage = HostCompiler.CompileLibraryImage("StateLib", AttributeLibrary);
    HostCompilation host = HostCompiler.Compile(
        "DeclaredMetadata",
        MetadataHost,
        new EmittedHandler(MetadataHandler),
        MetadataReference.CreateFromImage(referenceImage));

    Problems(host).ShouldBeEmpty();
    MediatorSource(host).ShouldContain("global::App.PingHandler");
    Assembly assembly = Load(host, runtimeImage);
    string result = await Send(assembly, "App.Ping");
    result.ShouldBe("pong");
  }

  [Fact]
  public void DeclaredHandler_FromMetadataOnlyGetter_IsTwm012()
  {
    byte[] library = HostCompiler.CompileLibraryImage("StateLib", GetterOnlyLibrary, metadataOnly: true);
    HostCompilation host = HostCompiler.Compile(
        "GetterOnlyMetadata",
        MetadataHost,
        MetadataReference.CreateFromImage(library));

    Diagnostic twm012 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM012").ShouldHaveSingleItem();
    twm012.GetMessage().ShouldContain("not a constant string");
    twm012.GetMessage().ShouldContain("handlerTypeName");
    MediatorSource(host).ShouldNotContain("PingHandler");
  }

  [Fact]
  public async Task DeclaredHandler_FromRegisterSourceOutput_IsDispatched()
  {
    HostCompilation host = HostCompiler.Compile(
        "InvisibleHandler",
        NestedSourceHost,
        new SourceOutputHandler(NestedHandler));

    Problems(host).ShouldBeEmpty();
    MediatorSource(host).ShouldContain("global::App.CounterState.Handler");
    Assembly assembly = Load(host);
    string result = await Send(assembly, "App.CounterState+Increment");
    result.ShouldBe("pong");
  }

  [Fact]
  public async Task DeclaredHandler_UnrelatedConstructorString_UsesTheGetter()
  {
    HostCompilation host = HostCompiler.Compile("UnrelatedArgument", UnrelatedArgumentHost);

    Problems(host).ShouldBeEmpty();
    MediatorSource(host).ShouldContain("global::App.PingHandler");
    Assembly assembly = Load(host);
    string result = await Send(assembly, "App.Ping");
    result.ShouldBe("pong");
  }

  [Fact]
  public void DeclaredHandler_ThatIsNotARequestHandler_IsTwm012()
  {
    HostCompilation host = HostCompiler.Compile("NotAHandler", NotAHandlerHost);

    Diagnostic twm012 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM012").ShouldHaveSingleItem();
    twm012.GetMessage().ShouldBe(
        "Request 'App.Ping' names 'App.Side', which is not an IRequestHandler<App.Ping, string>");
  }

  [Fact]
  public void NestedRelativeName_BindsTheContainingType_WithoutADuplicate()
  {
    HostCompilation host = HostCompiler.Compile("NestedDeclared", NestedHost);

    host.Diagnostics.ShouldNotContain(diagnostic =>
        diagnostic.Id == "TWM001" || diagnostic.Id == "TWM002" || diagnostic.Id == "TWM012");
    Problems(host).ShouldBeEmpty();
    MediatorSource(host).ShouldContain("global::App.CounterState.Handler");
  }

  [Fact]
  public void MissingDeclaredHandler_IsTwm012_OnTheAttribute()
  {
    HostCompilation host = HostCompiler.Compile("MissingDeclared", MissingHost);

    Diagnostic twm012 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM012").ShouldHaveSingleItem();
    twm012.GetMessage().ShouldBe(
        "Request 'App.Ping' names generated handler 'App.MissingHandler', which does not exist after generation");
    twm012.Location.SourceTree!.GetText().ToString(twm012.Location.SourceSpan).ShouldBe("Reducer");
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM001" || diagnostic.Id == "TWM002");
    host.Diagnostics.ShouldContain(diagnostic =>
        (diagnostic.Id == "CS0234" || diagnostic.Id == "CS0246")
        && diagnostic.GetMessage().Contains("MissingHandler"));
    MediatorSource(host).ShouldContain("global::App.MissingHandler");
  }

  [Fact]
  public void UnreadableHandlerName_IsTwm012_AndDoesNotEmitAType()
  {
    HostCompilation host = HostCompiler.Compile("UnreadableDeclared", UnreadableHost);

    Diagnostic twm012 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM012").ShouldHaveSingleItem();
    twm012.GetMessage().ShouldContain("not a constant string");
    twm012.GetMessage().ShouldContain("handlerTypeName");
    twm012.Location.SourceTree!.GetText().ToString(twm012.Location.SourceSpan).ShouldBe("Weird");
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM001" || diagnostic.Id == "CS0246");
  }

  private static string MediatorSource(HostCompilation host)
  {
    return string.Join(
        "\n",
        host.Compilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains("Mediator.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.GetText().ToString()));
  }

  private static Diagnostic[] Problems(HostCompilation host)
  {
    return host.Diagnostics
        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        .ToArray();
  }

  private static async Task<string> Send(Assembly assembly, string requestTypeName)
  {
    await using ServiceProvider provider = Provider(assembly);
    using IServiceScope scope = provider.CreateScope();
    ISender sender = Sender(scope, assembly);
    IRequest<string> request = (IRequest<string>)Activator.CreateInstance(assembly.GetType(requestTypeName, true)!)!;
    return await sender.Send(request);
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
    return (ISender)scope.ServiceProvider.GetRequiredService(mediator);
  }

  private static Assembly Load(HostCompilation host, byte[]? companion = null)
  {
    using MemoryStream stream = new();
    EmitResult result = host.Compilation.Emit(stream);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
    stream.Position = 0;
    AssemblyLoadContext context = new(host.Compilation.AssemblyName, isCollectible: true);
    Assembly? companionAssembly = null;
    if (companion is not null)
    {
      companionAssembly = context.LoadFromStream(new MemoryStream(companion));
      context.Resolving += (_, name) =>
          companionAssembly.GetName().Name == name.Name ? companionAssembly : null;
    }

    return context.LoadFromStream(stream);
  }

  private sealed class EmittedHandler : IIncrementalGenerator
  {
    private readonly string Source;

    public EmittedHandler(string source)
    {
      Source = source;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      string source = Source;
      context.RegisterPostInitializationOutput(output => output.AddSource("EmittedHandler.g.cs", source));
    }
  }

  private sealed class SourceOutputHandler : IIncrementalGenerator
  {
    private readonly string Source;

    public SourceOutputHandler(string source)
    {
      Source = source;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      string source = Source;
      context.RegisterSourceOutput(
          context.CompilationProvider,
          (production, _) => production.AddSource("EmittedHandler.g.cs", source));
    }
  }

  private const string SyntaxHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "PingHandler";
          }

          [Reducer]
          public sealed class Ping : IRequest<string> { }
      }
      """;

  private const string AttributeLibrary = """
      using TimeWarp.Mediator;

      namespace State
      {
          public sealed class ReducerAttribute(string handlerTypeName = "PingHandler") : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => handlerTypeName;
          }
      }
      """;

  private const string GetterOnlyLibrary = """
      using TimeWarp.Mediator;

      namespace State
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "PingHandler";
          }
      }
      """;

  private const string MetadataHost = """
      using State;
      using TimeWarp.Mediator;

      namespace App
      {
          [Reducer]
          public sealed class Ping : IRequest<string> { }
      }
      """;

  private const string MetadataHandler = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class PingHandler : IRequestHandler<Ping, string>
          {
              public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
                  ValueTask.FromResult("pong");
          }
      }
      """;

  private const string NestedSourceHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "Handler";
          }

          public sealed partial class CounterState
          {
              [Reducer]
              public sealed class Increment : IRequest<string> { }
          }
      }
      """;

  private const string NestedHandler = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed partial class CounterState
          {
              public sealed class Handler : IRequestHandler<Increment, string>
              {
                  public ValueTask<string> Handle(Increment request, CancellationToken cancellationToken) =>
                      ValueTask.FromResult("pong");
              }
          }
      }
      """;

  private const string UnrelatedArgumentHost = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public ReducerAttribute(string description)
              {
              }

              public override string HandlerTypeName => "PingHandler";
          }

          [Reducer("increments the counter")]
          public sealed class Ping : IRequest<string> { }

          public sealed class PingHandler : IRequestHandler<Ping, string>
          {
              public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
                  ValueTask.FromResult("pong");
          }
      }
      """;

  private const string NotAHandlerHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "Side";
          }

          [Reducer]
          public sealed class Ping : IRequest<string> { }

          public sealed class Side
          {
          }
      }
      """;

  private const string NestedHost = """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "Handler";
          }

          public sealed class CounterState
          {
              [Reducer]
              public sealed class Increment : IAction { }

              public sealed class Handler : IRequestHandler<Increment>
              {
                  public ValueTask Handle(Increment request, CancellationToken cancellationToken) => default;
              }
          }
      }
      """;

  private const string MissingHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class ReducerAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => "MissingHandler";
          }

          [Reducer]
          public sealed class Ping : IRequest<string> { }
      }
      """;

  private const string UnreadableHost = """
      using TimeWarp.Mediator;

      namespace App
      {
          public sealed class WeirdAttribute : GeneratedHandlerAttribute
          {
              public override string HandlerTypeName => System.DateTime.UtcNow.Second.ToString();
          }

          [Weird]
          public sealed class Ping : IRequest<string> { }
      }
      """;
}
