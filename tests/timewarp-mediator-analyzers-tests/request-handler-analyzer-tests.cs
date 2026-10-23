#region Purpose
// TWM001/TWM002 fire in a library that references the analyzer but not the generator.
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

public class RequestHandlerAnalyzerTests
{
  [Fact]
  public async Task Twm001_RequestWithNoHandler_IsError()
  {
    const string source =
        """
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Orphan : IRequest<int> { }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM001");
    diagnostic.GetMessage().ShouldBe("Request 'Orphan' has no handler");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("Orphan");
  }

  [Fact]
  public async Task GeneratorHost_LeavesTwm001ToTheGenerator()
  {
    const string source =
        """
            using TimeWarp.Mediator;
            public sealed class Orphan : IRequest<int> { }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true, generatorHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm002_DuplicateHandler_IsError()
  {
    const string source =
        """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Ping : IRequest<int> { }
            public sealed class PingHandlerA : IRequestHandler<Ping, int>
            {
                public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
            }
            public sealed class PingHandlerB : IRequestHandler<Ping, int>
            {
                public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(2);
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM002");
    diagnostic.GetMessage().ShouldBe("Request 'Ping' has multiple handlers: PingHandlerA, PingHandlerB");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("PingHandlerB");
  }

  [Fact]
  public async Task NoMembership_DoesNotDiagnose()
  {
    const string source =
        """
            using TimeWarp.Mediator;
            public sealed class Orphan : IRequest<int> { }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    diagnostics.ShouldBeEmpty();
  }

  [Fact]
  public async Task HandlerPresent_IsClean()
  {
    const string source =
        """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Ping : IRequest<int> { }
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    diagnostics.ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm003_HandlerAndRequestDifferentScopes_IsError()
  {
    const string source =
        """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class ClientPipeline { }
            public sealed class ServerPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class Ping : IRequest<int> { }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM003");
    diagnostic.GetMessage().ShouldBe("Handler 'PingHandler' is assigned to pipeline 'ServerPipeline' but message 'Ping' is assigned to pipeline 'ClientPipeline'");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("PingHandler");
  }

  [Fact]
  public async Task Twm004_WrongScopeSend_IsError()
  {
    const string source =
        """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class ClientPipeline { }
            public sealed class ServerPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class ClientPing : IRequest<int> { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class ClientPingHandler : IRequestHandler<ClientPing, int>
            {
                public ValueTask<int> Handle(ClientPing request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
            }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class ServerPing : IRequest<int> { }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class ServerPingHandler : IRequestHandler<ServerPing, int>
            {
                public ValueTask<int> Handle(ServerPing request, CancellationToken cancellationToken) => ValueTask.FromResult(2);
            }
            public static class Use
            {
                public static ValueTask<int> Go(ISender<ClientPipeline> sender)
                {
                    return sender.Send(new ServerPing());
                }
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM004");
    diagnostic.GetMessage().ShouldBe("Request 'ServerPing' belongs to pipeline 'ServerPipeline' and cannot be sent through 'ISender<ClientPipeline>'");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("sender.Send(new ServerPing())");
  }
}