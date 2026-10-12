#region Purpose
// TWM003 for notifications and TWM004 receiver/argument hardening (013-010 G-14, G-18), plus descriptor metadata (G-17).
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

public class SendScopeAnalyzerTests
{
  private const string Pipelines =
      """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;
        [assembly: MediatorAssembly]
        public sealed class ClientPipeline { }
        public sealed class ServerPipeline { }
        [MediatorScope(typeof(ClientPipeline))]
        public sealed class ClientPing : IRequest<string> { }
        [MediatorScope(typeof(ClientPipeline))]
        public sealed class ClientPingHandler : IRequestHandler<ClientPing, string>
        {
            public Task<string> Handle(ClientPing request, CancellationToken cancellationToken) => Task.FromResult("c");
        }
        [MediatorScope(typeof(ServerPipeline))]
        public sealed class ServerPing : IRequest<int> { }
        [MediatorScope(typeof(ServerPipeline))]
        public sealed class ServerPingHandler : IRequestHandler<ServerPing, int>
        {
            public Task<int> Handle(ServerPing request, CancellationToken cancellationToken) => Task.FromResult(2);
        }
        """;

  [Fact]
  public async Task Twm003_NotificationHandlerInAnotherPipeline_IsError()
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
            public sealed class Note : INotification { }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class NoteHandler : INotificationHandler<Note>
            {
                public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM003");
    diagnostic.GetMessage().ShouldContain("message 'Note'");
  }

  [Fact]
  public async Task Twm003_OnlyOneSideScoped_IsClean()
  {
    const string source =
        """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class ClientPipeline { }
            public sealed class Ping : IRequest<int> { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class Note : INotification { }
            public sealed class NoteHandler : INotificationHandler<Note>
            {
                public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
    diagnostics.ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_ConditionalAccessSend_IsError()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int>? Go(ISender<ClientPipeline>? sender) => sender?.Send(new ServerPing());
            }
            """;

    Diagnostic diagnostic = (await AnalyzerTestHelper.GetDiagnosticsAsync(source)).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM004");
    diagnostic.GetMessage().ShouldContain("'ISender<ClientPipeline>'");
  }

  [Fact]
  public async Task Twm004_ObjectSend_IsNotReported()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<object?> Go(ISender<ClientPipeline> sender) => sender.Send((object)new ServerPing());
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_UnrelatedMethodNamedSend_IsNotReported()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Send(this ISender<ClientPipeline> sender, ServerPing request) => Task.FromResult(0);
                public static Task<int> Go(ISender<ClientPipeline> sender, Outbox outbox)
                {
                    outbox.Send(new ServerPing());
                    return Use.Send(sender, new ServerPing());
                }
            }
            public sealed class Outbox
            {
                public void Send(ServerPing request) { }
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_UnscopedSenderWithScopedRequest_IsErrorInHost()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(IMediator mediator) => mediator.Send(new ServerPing());
            }
            """;

    Diagnostic diagnostic = (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM004");
    diagnostic.GetMessage().ShouldContain("'ServerPipeline'");
    diagnostic.GetMessage().ShouldContain("'ISender (unscoped)'");
  }

  [Fact]
  public async Task Twm004_UnscopedSenderWithScopedRequest_IsErrorInGeneratorHost()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(IMediator mediator) => mediator.Send(new ServerPing());
            }
            """;

    Diagnostic diagnostic = (await AnalyzerTestHelper.GetDiagnosticsAsync(source, generatorHost: true)).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM004");
  }

  [Fact]
  public async Task Twm004_UnscopedSenderWithScopedRequest_IsNotReportedInLibrary()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(ISender sender) => sender.Send(new ServerPing());
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_InterfaceTypedRequestOnlyInAnotherPipeline_IsErrorInHost()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(ISender<ClientPipeline> sender, IRequest<int> request) => sender.Send(request);
            }
            """;

    Diagnostic diagnostic = (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe("TWM004");
    diagnostic.GetMessage().ShouldContain("IRequest<int>");
    diagnostic.GetMessage().ShouldContain("'ServerPipeline'");
  }

  [Fact]
  public async Task Twm004_InterfaceTypedRequestWithAMatchInThePipeline_IsNotReported()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<string> Go(ISender<ClientPipeline> sender, IRequest<string> request) => sender.Send(request);
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_InterfaceTypedRequest_IsNotReportedInLibrary()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(ISender<ClientPipeline> sender, IRequest<int> request) => sender.Send(request);
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source)).ShouldBeEmpty();
  }

  [Fact]
  public async Task Twm004_MatchingScopedSend_IsClean()
  {
    string source = Pipelines +
        """
            public static class Use
            {
                public static Task<int> Go(ISender<ServerPipeline> sender) => sender.Send(new ServerPing());
            }
            """;

    (await AnalyzerTestHelper.GetDiagnosticsAsync(source, isHost: true)).ShouldBeEmpty();
  }

  [Fact]
  public void Descriptors_HaveHelpLinks_AndCompilationEndTagsForEndActions()
  {
    RequestHandlerAnalyzer analyzer = new();

    foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
    {
      descriptor.HelpLinkUri.ShouldEndWith("diagnostics.md#" + descriptor.Id.ToLowerInvariant());
      bool compilationEnd = descriptor.CustomTags.Contains(WellKnownDiagnosticTags.CompilationEnd);
      compilationEnd.ShouldBe(descriptor.Id != "TWM004", descriptor.Id);
    }
  }
}