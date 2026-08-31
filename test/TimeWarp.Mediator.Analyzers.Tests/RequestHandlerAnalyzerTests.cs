#region Purpose
// TWM001/TWM002 fire in a library that references the analyzer but not the generator.
#endregion

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace TimeWarp.Mediator.Analyzers.Tests;

public class RequestHandlerAnalyzerTests
{
    [Fact]
    public async Task Twm001_RequestWithNoHandler_IsError()
    {
        const string Source =
            """
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Orphan : IRequest<int> { }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(Source);
        Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("TWM001");
        diagnostic.GetMessage().ShouldContain("Orphan");
    }

    [Fact]
    public async Task Twm002_DuplicateHandler_IsError()
    {
        const string Source =
            """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Ping : IRequest<int> { }
            public sealed class PingHandlerA : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            public sealed class PingHandlerB : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(2);
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(Source);
        diagnostics.Any(d => d.Id == "TWM002").ShouldBeTrue();
    }

    [Fact]
    public async Task NoMembership_DoesNotDiagnose()
    {
        const string Source =
            """
            using TimeWarp.Mediator;
            public sealed class Orphan : IRequest<int> { }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(Source);
        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandlerPresent_IsClean()
    {
        const string Source =
            """
            using System.Threading;
            using System.Threading.Tasks;
            using TimeWarp.Mediator;
            [assembly: MediatorAssembly]
            public sealed class Ping : IRequest<int> { }
            public sealed class PingHandler : IRequestHandler<Ping, int>
            {
                public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(1);
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(Source);
        diagnostics.ShouldBeEmpty();
    }
}
