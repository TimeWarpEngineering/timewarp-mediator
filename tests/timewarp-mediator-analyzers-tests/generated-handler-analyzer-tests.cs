#region Purpose
// TWM012 ownership: a library does not demand the generated handler, a host does.
#endregion

#region Design
// The analyzer sees the compilation after generators in a real host. These tests run the
// analyzer alone, so a missing name is still missing. generatorHost skips TWM001 and still
// reports TWM012. A library suppresses TWM001 when the attribute is present and does not
// report a missing type, because the host is where that type is generated.
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

public sealed class GeneratedHandlerAnalyzerTests
{
  [Fact]
  public async Task Library_MissingDeclaredHandler_IsSilent()
  {
    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(LibrarySource);
    diagnostics.ShouldBeEmpty();
  }

  [Fact]
  public async Task Host_MissingDeclaredHandler_IsTwm012_OnTheAttribute()
  {
    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(HostSource, isHost: true);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM012");
    diagnostic.GetMessage().ShouldBe(
        "Request 'Ping' names generated handler 'MissingHandler', which does not exist after generation");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("Reducer");
  }

  [Fact]
  public async Task GeneratorHost_MissingDeclaredHandler_IsTwm012()
  {
    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(
        HostSource,
        generatorHost: true);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM012");
  }

  [Fact]
  public async Task SameDeclaredName_AsTheHandlerClass_IsClean()
  {
    const string source = """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;

        public sealed class ReducerAttribute : GeneratedHandlerAttribute
        {
            public override string HandlerTypeName => "PingHandler";
        }

        [Reducer]
        public sealed class Ping : IRequest<int> { }

        public sealed class PingHandler : IRequestHandler<Ping, int>
        {
            public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
        }
        """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public async Task DifferentDeclaredName_IsTwm002()
  {
    const string source = """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;

        public sealed class ReducerAttribute : GeneratedHandlerAttribute
        {
            public override string HandlerTypeName => "Side";
        }

        [Reducer]
        public sealed class Ping : IRequest<int> { }

        public sealed class PingHandler : IRequestHandler<Ping, int>
        {
            public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
        }

        public sealed class Side : IRequestHandler<Ping, int>
        {
            public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(2);
        }
        """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM002");
    diagnostic.GetMessage().ShouldContain("PingHandler");
    diagnostic.GetMessage().ShouldContain("Side");
  }

  [Fact]
  public async Task Library_UnreadableName_IsTwm012_WithoutTwm001()
  {
    const string source = """
        using TimeWarp.Mediator;

        [assembly: MediatorAssembly]

        public sealed class WeirdAttribute : GeneratedHandlerAttribute
        {
            public override string HandlerTypeName => System.DateTime.UtcNow.Second.ToString();
        }

        [Weird]
        public sealed class Ping : IRequest<int> { }
        """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM012");
    diagnostic.GetMessage().ShouldContain("not a constant string");
  }

  [Fact]
  public async Task DeclaredType_ThatIsNotAHandler_IsTwm012()
  {
    const string source = """
        using TimeWarp.Mediator;

        public sealed class ReducerAttribute : GeneratedHandlerAttribute
        {
            public override string HandlerTypeName => "Side";
        }

        [Reducer]
        public sealed class Ping : IRequest<int> { }

        public sealed class Side
        {
        }
        """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM012");
    diagnostic.GetMessage().ShouldBe("Request 'Ping' names 'Side', which is not an IRequestHandler<Ping, int>");
  }

  [Fact]
  public async Task NotATypeName_IsTwm012_WithoutTheMissingTypeSentence()
  {
    const string source = """
        using TimeWarp.Mediator;

        [assembly: MediatorAssembly]

        public sealed class WeirdAttribute : GeneratedHandlerAttribute
        {
            public override string HandlerTypeName => "Not A Type";
        }

        [Weird]
        public sealed class Ping : IRequest<int> { }
        """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM012");
    diagnostic.GetMessage().ShouldContain("not a type name");
    diagnostic.GetMessage().ShouldNotContain("does not exist after generation");
  }

  private const string LibrarySource = """
      using TimeWarp.Mediator;

      [assembly: MediatorAssembly]

      public sealed class ReducerAttribute : GeneratedHandlerAttribute
      {
          public override string HandlerTypeName => "MissingHandler";
      }

      [Reducer]
      public sealed class Ping : IRequest<int> { }
      """;

  private const string HostSource = """
      using TimeWarp.Mediator;

      public sealed class ReducerAttribute : GeneratedHandlerAttribute
      {
          public override string HandlerTypeName => "MissingHandler";
      }

      [Reducer]
      public sealed class Ping : IRequest<int> { }
      """;
}
