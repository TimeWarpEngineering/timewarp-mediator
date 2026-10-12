#region Purpose
// Near-miss shapes for TWM001–TWM004 that must not be diagnosed, plus location checks for the
// diagnostics that sit beside them (013-012, G-23).
#endregion

#region Design
// Each negative is a member assembly ([assembly: MediatorAssembly]) and also runs as an MSBuild host,
// so a rule that only misfires in one membership mode still fails.
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

public class AnalyzerNegativeTests
{
  private const string Usings =
      """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;
        [assembly: MediatorAssembly]

        """;

  public static IEnumerable<object[]> Negatives()
  {
    foreach (string name in NegativeSources.Keys)
    {
      yield return new object[] { name, false };
      yield return new object[] { name, true };
    }
  }

  [Theory]
  [MemberData(nameof(Negatives))]
  public async Task NearMiss_IsNotDiagnosed(string name, bool isHost)
  {
    ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(Usings + NegativeSources[name], isHost: isHost);

    diagnostics.ShouldBeEmpty(string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString())));
  }

  [Fact]
  public async Task Twm001_EachOrphan_IsReportedAtItsOwnDeclaration()
  {
    const string Source =
        Usings +
        """
            public sealed class First : IRequest<int> { }
            public sealed class Handled : IRequest<int> { }
            public sealed class HandledHandler : IRequestHandler<Handled, int>
            {
                public Task<int> Handle(Handled request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            public static class Outer
            {
                public sealed class Second : IRequest { }
            }
            """;

    Diagnostic[] diagnostics = (await AnalyzerTestHelper.GetDiagnosticsAsync(Source))
        .OrderBy(d => d.Location.SourceSpan.Start)
        .ToArray();

    diagnostics.Select(d => d.Id).ShouldBe(new[] { "TWM001", "TWM001" });
    diagnostics.Select(AnalyzerTestHelper.LocatedText).ShouldBe(new[] { "First", "Second" });
    diagnostics[1].GetMessage().ShouldBe("Request 'Outer.Second' has no handler");
  }

  [Fact]
  public async Task MediatorModule_MakesTheDeclaringLibraryAMember()
  {
    const string Source =
        """
            using TimeWarp.Mediator;
            [MediatorModule("Orders")]
            public sealed class Orphan : IRequest<int> { }
            """;

    Diagnostic diagnostic = (await AnalyzerTestHelper.GetDiagnosticsAsync(Source)).ShouldHaveSingleItem();

    diagnostic.Id.ShouldBe("TWM001");
    AnalyzerTestHelper.LocatedText(diagnostic).ShouldBe("Orphan");
  }

  [Fact]
  public async Task Twm004_IsReportedAtEachWrongSendOnly()
  {
    const string Source =
        Usings +
        """
            public sealed class ClientPipeline { }
            public sealed class ServerPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class ClientPing : IRequest<int> { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class ClientPingHandler : IRequestHandler<ClientPing, int>
            {
                public Task<int> Handle(ClientPing request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class ServerPing : IRequest<int> { }
            [MediatorScope(typeof(ServerPipeline))]
            public sealed class ServerPingHandler : IRequestHandler<ServerPing, int>
            {
                public Task<int> Handle(ServerPing request, CancellationToken cancellationToken) => Task.FromResult(2);
            }
            public static class Use
            {
                public static async Task Go(ISender<ClientPipeline> client, ISender<ServerPipeline> server)
                {
                    await client.Send(new ClientPing());
                    await server.Send(new ServerPing());
                    await client.Send(new ServerPing());
                    await server.Send(new ClientPing());
                }
            }
            """;

    Diagnostic[] diagnostics = (await AnalyzerTestHelper.GetDiagnosticsAsync(Source))
        .OrderBy(d => d.Location.SourceSpan.Start)
        .ToArray();

    diagnostics.Select(d => d.Id).ShouldBe(new[] { "TWM004", "TWM004" });
    diagnostics.Select(AnalyzerTestHelper.LocatedText)
        .ShouldBe(new[] { "client.Send(new ServerPing())", "server.Send(new ClientPing())" });
  }

  private static readonly Dictionary<string, string> NegativeSources = new()
  {
    // TWM001: only concrete requests need a handler.
    ["AbstractRequest"] =
          """
            public abstract class RequestBase : IRequest<int> { }
            """,
    ["OpenGenericRequest"] =
          """
            public sealed class Box<T> : IRequest<T> { }
            """,
    ["RequestInterface"] =
          """
            public interface IAuditedRequest : IRequest<int> { }
            """,
    ["NotificationWithNoHandler"] =
          """
            public sealed class Note : INotification { }
            """,
    ["HandlerInBaseClass"] =
          """
            public sealed class Ping : IRequest<int> { }
            public abstract class PingHandlerBase : IRequestHandler<Ping, int>
            {
                public abstract Task<int> Handle(Ping request, CancellationToken cancellationToken);
            }
            public sealed class PingHandler : PingHandlerBase
            {
                public override Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            """,

    // TWM002: one handler per request, but one class may handle several requests and a
    // notification may have many handlers.
    ["OneClassHandlesTwoRequests"] =
          """
            public sealed class First : IRequest<int> { }
            public sealed class Second : IRequest<string> { }
            public sealed class BothHandler : IRequestHandler<First, int>, IRequestHandler<Second, string>
            {
                public Task<int> Handle(First request, CancellationToken cancellationToken) => Task.FromResult(1);
                public Task<string> Handle(Second request, CancellationToken cancellationToken) => Task.FromResult("2");
            }
            """,
    ["NotificationWithManyHandlers"] =
          """
            public sealed class Note : INotification { }
            public sealed class AuditHandler : INotificationHandler<Note>
            {
                public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            public sealed class CacheHandler : INotificationHandler<Note>
            {
                public Task Handle(Note notification, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """,

    // TWM003: matching markers, or a marker inherited from the containing type.
    ["SameScopeOnBothSides"] =
          """
            public sealed class ClientPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class Ping : IRequest<int> { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            """,
    ["ScopeFromContainingType"] =
          """
            public sealed class ClientPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public static class Feature
            {
                public sealed class Ping : IRequest<int> { }
                public sealed class PingHandler : IRequestHandler<Ping, int>
                {
                    public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
                }
            }
            """,

    // TWM004: the matching scoped sender, and Send(object) which is a runtime NoHandlerException.
    ["MatchingScopedSendAndObjectSend"] =
          """
            public sealed class ClientPipeline { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class Ping : IRequest<int> { }
            [MediatorScope(typeof(ClientPipeline))]
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            public sealed class Pong : IRequest<int> { }
            public sealed class PongHandler : IRequestHandler<Pong, int>
            {
                public Task<int> Handle(Pong request, CancellationToken cancellationToken) => Task.FromResult(2);
            }
            public static class Use
            {
                public static async Task Go(ISender<ClientPipeline> client, ISender sender)
                {
                    await client.Send(new Ping());
                    await sender.Send(new Pong());
                    await client.Send((object)new Pong());
                }
            }
            """,
  };
}